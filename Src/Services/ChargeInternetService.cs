namespace SinaMN75U.Services;

public interface IChargeInternetService {
	Task<UResponse<ChargeInternetReserveResponse?>> Pin(ReserveChargeParams p, CancellationToken ct);
	Task<UResponse<ChargeInternetReserveResponse?>> Topup(TopupChargeParams p, CancellationToken ct);
	Task<UResponse<InternetPackageResponse?>> InternetList(InternetListParams p, CancellationToken ct);
	Task<UResponse<InternetPackageResponse?>> MciTopOffer(MCITopOfferParams p, CancellationToken ct);
	Task<UResponse<GetStatusResponse?>> GetStatus(GetStatusParams p, CancellationToken ct);
	Task<UResponse<GetBalanceResponse?>> GetBalance(BaseParams p, CancellationToken ct);
	Task<UResponse<EchoResponse?>> Echo(CancellationToken ct);
	Task<UResponse<ChargeInternetReserveResponse?>> InternetReserve(InternetReserveParams p, CancellationToken ct);
}

// Mobtakeran charge system. Every purchase is two steps: "Reserve" (Pin / Topup / Internet) and then "Approve".
// The user's wallet is charged only after the operator confirms, so a failed purchase never touches the wallet.
public class ChargeInternetService(
	DbContext db,
	IHttpClientService httpClient,
	ILocalizationService ls,
	ITokenService ts,
	IWalletService walletService,
	IVasService vs,
	IServiceScopeFactory scopeFactory
) : IChargeInternetService {
	private const int MobtakeranOk = 1;
	private const int InvalidTokenCode = 33;
	private const string Mci = "1";
	private const string Rightel = "3";

	// "device" table of the document: 5 = Mobile Application.
	private const string Device = "5";

	// These codes mean the result is not known yet (747 = "unknown", the others = "in progress"): the charge may or may not
	// have been sent, so nothing may be decided until GetStatus gives a final answer.
	private static readonly HashSet<int> PendingCodes = [28, 70, 71, 73, 75, 306, 747];

	// Codes that say nothing about the transaction itself (our token, their servers, rate limit, a bad status request),
	// so a GetStatus that returns one of them is simply asked again later.
	private static readonly HashSet<int> RetryableCodes = [0, 3, 9, 15, 16, 19, 27, 33, 60, 64, 84, 100, 401, 511, 611, 711, 750];

	// Product types ("type" in Topup/Reserve) that can be bought per operator. MCI "4" (custom amount) and Irancell "2"
	// (postpaid bill) are left out because the app only sells the predefined amounts.
	private static readonly Dictionary<string, string[]> TopupTypes = new() {
		{ "1", ["0", "2", "3"] }, // MCI: normal, youth, women
		{ "2", ["0", "1"] }, // Irancell: normal, amazing
		{ "3", ["0", "1"] }, // Rightel: normal, exciting (shoorangiz)
		{ "5", ["0"] } // Shatel: normal
	};

	// After an unknown Approve, GetStatus is asked a few times before answering the user...
	private static readonly TimeSpan[] QuickChecks = [TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(5)];

	// ...and if it is still unknown, a background check keeps asking and refunds the wallet if the charge finally failed.
	private static readonly TimeSpan[] BackgroundChecks = [
		TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(2), TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(10), TimeSpan.FromMinutes(20), TimeSpan.FromMinutes(30)
	];

	// One login token is shared by all requests instead of logging in before every call.
	private static readonly SemaphoreSlim TokenLock = new(1, 1);
	private static string? _token;
	private static DateTime _tokenExpiresAt;

	// A user can run only one purchase at a time, so a double tap (or two parallel requests) can't buy twice.
	private static readonly ConcurrentDictionary<Guid, byte> UsersInPurchase = new();

	// The document recommends calling Echo about once a minute, so the result is shared for that long.
	private static EchoResponse? _echo;
	private static DateTime _echoExpiresAt;

	private static long _reserveCounter;

	// States kept in VasEntity.JsonData.Detail2 for a Mobtakeran purchase.
	private const string VasDone = "mobtakeran:done";
	private const string VasPending = "mobtakeran:pending";
	private const string VasRefunded = "mobtakeran:refunded";
	private const string VasUnknown = "mobtakeran:unknown";

	private enum TxnState { Success, Failed, Unknown }

	// What the user pays for a predefined amount (amount + ChargeInternetTaxPercent), or null when the amount is not one of the
	// operator's predefined amounts for that type. The amount itself is what is sent to Mobtakeran.
	public static decimal? PayableAmount(string operatorId, decimal amount, bool isPin, string type = "0") {
		ChargeInternet? op = Core.App.ChargeInternet.FirstOrDefault(x => ((int)x.Operator).ToString() == operatorId);
		List<ChargeInternetPreDefinedAmounts> amounts = (isPin ? op?.PinAmountsList : op?.TopupAmountsList) ?? [];
		int typeCode = int.TryParse(type, out int t) ? t : 0;
		List<ChargeInternetPreDefinedAmounts> forType = amounts.Where(x => x.Type == typeCode).ToList();
		if (forType.Count == 0) forType = amounts.Where(x => x.Type == 0).ToList();
		if (forType.All(x => x.Amount != amount)) return null;
		return Math.Round(amount * (100 + Core.App.ChargeInternetTaxPercent) / 100, 0, MidpointRounding.AwayFromZero);
	}

	public async Task<UResponse<ChargeInternetReserveResponse?>> Pin(ReserveChargeParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return Error<ChargeInternetReserveResponse>(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return Error<ChargeInternetReserveResponse>(Usc.ExpiredToken, ls.Get("authTokenIsExpired"));
		decimal? payableAmount = PayableAmount(p.SimType, p.Amount, true);
		if (payableAmount == null) return Error<ChargeInternetReserveResponse>(Usc.BadRequest, ls.Get("theSelectedChargeAmountIsNotOfferedByThisOperator"));

		return await Purchase(p, userData, new PurchaseRequest {
			ReservePath = "api/v2/Pin/Reserve",
			ReserveAttachments = new Dictionary<string, string> {
				{ "amount", p.Amount.ToIntString() },
				{ "operator_id", p.SimType },
				{ "device", Device }
			},
			OperatorId = p.SimType,
			Amount = payableAmount.Value,
			WalletTag = TagWalletTxn.ChargeSimPin,
			VasTag = TagVas.ChargePin,
			KeyValues = [new KeyValue { Key = ULocalizedConstants.Operator, Value = p.SimType }],
			SuccessMessage = ls.Get("chargePinPurchasedSuccessfully")
		}, ct);
	}

	public async Task<UResponse<ChargeInternetReserveResponse?>> Topup(TopupChargeParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return Error<ChargeInternetReserveResponse>(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return Error<ChargeInternetReserveResponse>(Usc.ExpiredToken, ls.Get("authTokenIsExpired"));
		string? subscriber = NormalizeMobile(p.PhoneNumber);
		if (subscriber == null) return Error<ChargeInternetReserveResponse>(Usc.BadRequest, ls.Get("theMobileNumberIsNotValid"));
		string type = TopupType(p.OperatorId, p.ChargeType);
		decimal? payableAmount = PayableAmount(p.OperatorId, p.Amount, false, type);
		if (payableAmount == null) return Error<ChargeInternetReserveResponse>(Usc.BadRequest, ls.Get("theSelectedChargeAmountIsNotOfferedByThisOperator"));

		return await Purchase(p, userData, new PurchaseRequest {
			ReservePath = "api/v2/Topup/Reserve",
			ReserveAttachments = new Dictionary<string, string> {
				{ "subscriber", subscriber },
				{ "amount", p.OperatorId == "2" ? (p.Amount / 10).ToIntString() : p.Amount.ToIntString() },
				{ "operator_id", p.OperatorId },
				{ "device", Device },
				{ "type", type }
			},
			OperatorId = p.OperatorId,
			Amount = payableAmount.Value,
			WalletTag = TagWalletTxn.ChargeSimTopup,
			VasTag = TagVas.ChargeTopup,
			KeyValues = [
				new KeyValue { Key = ULocalizedConstants.PhoneNumber, Value = subscriber },
				new KeyValue { Key = ULocalizedConstants.Operator, Value = p.OperatorId }
			],
			SuccessMessage = ls.Get("simCardChargedSuccessfully")
		}, ct);
	}

	public async Task<UResponse<ChargeInternetReserveResponse?>> InternetReserve(InternetReserveParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return Error<ChargeInternetReserveResponse>(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return Error<ChargeInternetReserveResponse>(Usc.ExpiredToken, ls.Get("authTokenIsExpired"));
		string? subscriber = NormalizeMobile(p.Subscriber);
		if (subscriber == null) return Error<ChargeInternetReserveResponse>(Usc.BadRequest, ls.Get("theMobileNumberIsNotValid"));

		// The price comes from Mobtakeran, never from the app, so the wallet is charged exactly what the package costs.
		(InternetPackageItem? package, string? error) = await FindPackage(p.OperatorId, subscriber, p.PackageId, ct);
		if (error != null) return Error<ChargeInternetReserveResponse>(Usc.ThirdPartyError, error);
		if (package == null) return Error<ChargeInternetReserveResponse>(Usc.BadRequest, ls.Get("theSelectedInternetPackageIsNoLongerAvailable"));
		if (package.Amount != p.Amount) return Error<ChargeInternetReserveResponse>(Usc.BadRequest, ls.Get("thePriceOfThisItemHasChangedPleaseTryAgain"));

		return await Purchase(p, userData, new PurchaseRequest {
			ReservePath = "api/v2/Internet/Reserve",
			ReserveAttachments = new Dictionary<string, string> {
				{ "subscriber", subscriber },
				{ "operator_id", p.OperatorId },
				{ "package_id", package.Id },
				{ "amount", package.Amount.ToString() },
				{ "device", Device }
			},
			OperatorId = p.OperatorId,
			Amount = package.Amount,
			WalletTag = TagWalletTxn.InternetSim,
			VasTag = TagVas.InternetPackage,
			KeyValues = [
				new KeyValue { Key = ULocalizedConstants.PhoneNumber, Value = subscriber },
				new KeyValue { Key = ULocalizedConstants.Operator, Value = p.OperatorId },
				new KeyValue { Key = ULocalizedConstants.InternetPackage, Value = package.Title.IsNotNullOrEmpty() ? package.Title : package.Id }
			],
			SuccessMessage = ls.Get("internetPackageActivatedSuccessfully")
		}, ct);
	}

	public Task<UResponse<InternetPackageResponse?>> InternetList(InternetListParams p, CancellationToken ct) =>
		PackageList("api/v2/Internet/getlist", new Dictionary<string, string> { { "operator_id", p.OperatorId } }, p.OperatorId, ct);

	// MCI "companion" packages (PackageDType 11) depend on the subscriber, so they have their own list.
	public async Task<UResponse<InternetPackageResponse?>> MciTopOffer(MCITopOfferParams p, CancellationToken ct) {
		string? subscriber = NormalizeMobile(p.Subscriber);
		if (subscriber == null) return Error<InternetPackageResponse>(Usc.BadRequest, ls.Get("theMobileNumberIsNotValid"));
		return await PackageList("api/v2/Internet/MCITopOffer", new Dictionary<string, string> { { "subscriber", subscriber } }, Mci, ct);
	}

	// Admin only: the answer contains the charge PIN of the transaction.
	public async Task<UResponse<GetStatusResponse?>> GetStatus(GetStatusParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return Error<GetStatusResponse>(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (!userData.IsAdmin) return Error<GetStatusResponse>(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		MobtakeranReply? reply = await Status(p.Reserve.IsNotNullOrEmpty() ? p.Reserve : NewReserve(), p.Reference);
		if (reply == null) return Error<GetStatusResponse>(Usc.ThirdPartyError, ls.Get("chargeServiceIsNotAvailable"));

		return new UResponse<GetStatusResponse?>(new GetStatusResponse {
			Reserve = reply.Long("reserve"),
			ServerDateTime = reply.RootText("serverDateTime"),
			Status = reply.RootBool("status"),
			Code = reply.Code,
			Message = reply.RootText("message"),
			Reference = reply.AttachmentLong("reference"),
			Subscriber = reply.Text("subscriber"),
			Serial = reply.Text("serial"),
			Pin = reply.Text("pin"),
			TxnTime = reply.Text("txn_time"),
			Help = reply.Text("help"),
			MessageSource = reply.Text("message_source"),
			ExtCode = reply.Text("ext_code")
		}, Usc.Success, reply.Ok ? "" : reply.UserMessage(null));
	}

	// Admin only: this is the company's balance at Mobtakeran.
	public async Task<UResponse<GetBalanceResponse?>> GetBalance(BaseParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return Error<GetBalanceResponse>(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (!userData.IsAdmin) return Error<GetBalanceResponse>(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		MobtakeranReply? reply = await Send("api/v2/GetBalance", NewReserve(), new Dictionary<string, string>(), ct);
		if (reply == null) return Error<GetBalanceResponse>(Usc.ThirdPartyError, ls.Get("chargeServiceIsNotAvailable"));

		return new UResponse<GetBalanceResponse?>(new GetBalanceResponse {
			Reserve = reply.Long("reserve"),
			ServerDateTime = reply.RootText("serverDateTime"),
			Status = reply.RootBool("status"),
			Code = reply.Code,
			Message = reply.RootText("message"),
			Balance = reply.AttachmentLong("balance"),
			Wallet = reply.AttachmentLong("wallet"),
			Credit = reply.AttachmentLong("credit"),
			Limit = reply.AttachmentLong("limit"),
			Help = reply.Text("help"),
			MessageSource = reply.Text("message_source"),
			ExtCode = reply.Text("ext_code")
		}, Usc.Success, reply.Ok ? "" : reply.UserMessage(null));
	}

	// Which operators are up right now. Cached for a minute, as the document recommends.
	public async Task<UResponse<EchoResponse?>> Echo(CancellationToken ct) {
		if (_echo != null && DateTime.UtcNow < _echoExpiresAt) return new UResponse<EchoResponse?>(_echo);

		MobtakeranReply? reply = await Send("api/v2/Echo", NewReserve(), new Dictionary<string, string>(), ct);
		if (reply == null) return Error<EchoResponse>(Usc.ThirdPartyError, ls.Get("chargeServiceIsNotAvailable"));

		EchoResponse echo = new() {
			Reserve = reply.Long("reserve"),
			ServerDateTime = reply.RootText("serverDateTime"),
			Status = reply.RootBool("status"),
			Code = reply.Code,
			Message = reply.RootText("message"),
			MciTopup = reply.AttachmentBool("mci_topup"),
			Mtn = reply.AttachmentBool("mtn"),
			Rightel = reply.AttachmentBool("rightel"),
			Shatel = reply.AttachmentBool("shatel"),
			MciInternet = reply.AttachmentBool("mci_internet")
		};
		_echo = echo;
		_echoExpiresAt = DateTime.UtcNow.AddMinutes(1);
		return new UResponse<EchoResponse?>(echo);
	}

	private sealed class PurchaseRequest {
		public required string ReservePath { get; init; }
		public required Dictionary<string, string> ReserveAttachments { get; init; }
		public required string OperatorId { get; init; }
		public required decimal Amount { get; init; }
		public required TagWalletTxn WalletTag { get; init; }
		public required TagVas VasTag { get; init; }
		public required List<KeyValue> KeyValues { get; init; }
		public required string SuccessMessage { get; init; }
	}

	private sealed record PendingCharge(Guid UserId, string Reserve, string Reference, decimal Amount, TagWalletTxn Tag, List<KeyValue> KeyValues);

	// Reserve -> Approve -> charge the wallet. Shared by PIN, top-up and internet packages.
	private async Task<UResponse<ChargeInternetReserveResponse?>> Purchase(BaseParams p, JwtClaimData userData, PurchaseRequest r, CancellationToken ct) {
		if (!UsersInPurchase.TryAdd(userData.Id, 0)) return Error<ChargeInternetReserveResponse>(Usc.TooManyRequests, ls.Get("yourPreviousPurchaseIsStillInProgress"));
		try {
			if (!await walletService.HasEnoughBalance(userData.Id, r.Amount, ct)) return Error<ChargeInternetReserveResponse>(Usc.BalanceIsLow, ls.Get("yourBalanceIsNotEnough"));

			// The same reserve (order number) must be used for Reserve, Approve and GetStatus of one purchase.
			string reserve = NewReserve();
			MobtakeranReply? reserveReply = await Send(r.ReservePath, reserve, r.ReserveAttachments, ct);
			if (reserveReply == null) return Error<ChargeInternetReserveResponse>(Usc.ThirdPartyError, ls.Get("chargeServiceIsNotAvailable"));
			if (!reserveReply.Ok) return Error<ChargeInternetReserveResponse>(Usc.ThirdPartyError, reserveReply.UserMessage(r.OperatorId));

			string? reference = reserveReply.Text("reference");
			if (!reference.IsNotNullOrEmpty()) return Error<ChargeInternetReserveResponse>(Usc.ThirdPartyError, ls.Get("chargeServiceIsNotAvailable"));

			// "affective_amount" is what Mobtakeran takes from our account; the user must pay the same, so a higher amount is
			// not approved (a reserve that is never approved simply expires).
			decimal? affectiveAmount = reserveReply.AttachmentNumber("affective_amount");
			if (affectiveAmount > r.Amount) {
				ULog.Error($"Mobtakeran reserve {reserve}: affective_amount {affectiveAmount} is more than the user price {r.Amount}; not approved.");
				return Error<ChargeInternetReserveResponse>(Usc.BadRequest, ls.Get("thePriceOfThisItemHasChangedPleaseTryAgain"));
			}

			// From here on the charge may reach the subscriber, so nothing below may be cancelled half way
			// (a cancelled request must still charge the wallet), hence CancellationToken.None.
			MobtakeranReply? approveReply = await Send("api/v2/Approve", reserve, ApproveAttachments(reference!, userData, r.OperatorId), CancellationToken.None);
			(TxnState state, MobtakeranReply? finalReply) = await Settle(approveReply, reserve, reference!);
			if (state == TxnState.Failed) return Error<ChargeInternetReserveResponse>(Usc.ThirdPartyError, finalReply?.UserMessage(r.OperatorId) ?? ls.Get("chargeServiceIsNotAvailable"));

			bool pending = state == TxnState.Unknown;
			string? pin = pending ? null : finalReply?.Text("pin");
			List<KeyValue> keyValues = [..r.KeyValues];
			if (r.WalletTag == TagWalletTxn.ChargeSimPin) keyValues.Add(new KeyValue { Key = ULocalizedConstants.Pin, Value = pin ?? "---" });
			keyValues.Add(new KeyValue { Key = ULocalizedConstants.Reference, Value = reference! });

			await Debit(p, userData.Id, r, reserve, keyValues);
			await SaveVas(p, userData.Id, r, reserve, reference!, pin, pending ? VasPending : VasDone);

			if (pending) {
				ULog.Error($"Mobtakeran reserve {reserve} / reference {reference}: result is unknown (code {finalReply?.Code}); wallet charged, watching it in the background.");
				Watch(new PendingCharge(userData.Id, reserve, reference!, r.Amount, r.WalletTag, keyValues));
			}

			return new UResponse<ChargeInternetReserveResponse?>(new ChargeInternetReserveResponse {
				Reserve = long.TryParse(reserve, out long reserveNumber) ? reserveNumber : null,
				ServerDateTime = finalReply?.RootText("serverDateTime") ?? reserveReply.RootText("serverDateTime"),
				Status = !pending,
				Code = pending ? 747 : MobtakeranOk,
				Message = pending ? ls.Get("yourTransactionIsBeingProcessed") : r.SuccessMessage,
				Reference = reference,
				TraceId = finalReply?.Text("trace_id") ?? reserveReply.Text("trace_id"),
				AffectiveAmount = affectiveAmount.HasValue ? (long)affectiveAmount.Value : null,
				Help = null,
				MessageSource = null,
				Pin = pin,
				Serial = pending ? null : finalReply?.Text("serial")
			}, Usc.Success, pending ? ls.Get("yourTransactionIsBeingProcessed") : r.SuccessMessage);
		}
		finally {
			UsersInPurchase.TryRemove(userData.Id, out _);
		}
	}

	// Approve answers 1 (sent), a failure code (not sent, Mobtakeran returns the money) or 747 (unknown). For "unknown"
	// (or no answer at all) GetStatus is asked a few times before giving up for now.
	private async Task<(TxnState, MobtakeranReply?)> Settle(MobtakeranReply? approveReply, string reserve, string reference) {
		TxnState state = approveReply?.Code is not { } approveCode || PendingCodes.Contains(approveCode) ? TxnState.Unknown
			: approveCode == MobtakeranOk ? TxnState.Success : TxnState.Failed;
		if (state != TxnState.Unknown) return (state, approveReply);

		foreach (TimeSpan delay in QuickChecks) {
			await Task.Delay(delay);
			MobtakeranReply? statusReply = await Status(reserve, reference);
			TxnState statusState = StateOf(statusReply);
			if (statusState != TxnState.Unknown) return (statusState, statusReply);
		}

		return (TxnState.Unknown, approveReply);
	}

	private static TxnState StateOf(MobtakeranReply? statusReply) {
		if (statusReply?.Code is not { } code || PendingCodes.Contains(code) || RetryableCodes.Contains(code)) return TxnState.Unknown;
		return code == MobtakeranOk ? TxnState.Success : TxnState.Failed;
	}

	private Task<MobtakeranReply?> Status(string reserve, string reference) =>
		Send("api/v2/GetStatus", reserve, new Dictionary<string, string> { { "reference", reference } }, CancellationToken.None);

	// The debit is linked to the reserve (Detail2), so the background check can find it and a refund happens at most once.
	private async Task Debit(BaseParams p, Guid userId, PurchaseRequest r, string reserve, List<KeyValue> keyValues) {
		UResponse<WalletTxnResponse?> result = await walletService.Transfer(new WalletTransferParams {
			ApiKey = p.ApiKey,
			Token = p.Token,
			SenderId = userId,
			ReceiverId = Core.App.Users.Mobtakeran.Id,
			Amount = r.Amount,
			Detail1 = r.WalletTag.ToString(),
			Detail2 = DebitKey(reserve),
			KeyValues = keyValues,
			TagWalletTxn = [r.WalletTag],
			AllowOverdraft = true
		}, CancellationToken.None);
		if (result.Status != Usc.Success) ULog.Error($"Mobtakeran reserve {reserve}: the charge was sent but the wallet debit of user {userId} failed: {result.Message}");
	}

	// The purchase record: Detail1 keeps the reserve and Detail2 the Mobtakeran state, so a purchase that is still unknown
	// when the server restarts is picked up again (see RecoverPending). A failure here must not turn a completed purchase
	// into an error for the user.
	private async Task SaveVas(BaseParams p, Guid userId, PurchaseRequest r, string reserve, string reference, string? pin, string state) {
		try {
			await vs.Create(new VasCreateParams {
				Id = Guid.CreateVersion7(),
				ApiKey = p.ApiKey,
				Token = p.Token,
				Tags = [r.VasTag],
				CreatorId = userId,
				Amount = r.Amount,
				AuthorizeCode = reference,
				ChargePin = pin,
				Detail1 = reserve,
				Detail2 = state
			}, CancellationToken.None);
		}
		catch (Exception e) {
			db.ChangeTracker.Clear();
			ULog.Error($"Mobtakeran reference {reference}: could not save the VAS record: {e.Message}");
		}
	}

	// Runs outside the request (the request's DbContext is gone by then), in its own scope.
	private void Watch(PendingCharge charge) => _ = Task.Run(async () => {
		try {
			using IServiceScope scope = scopeFactory.CreateScope();
			await ActivatorUtilities.CreateInstance<ChargeInternetService>(scope.ServiceProvider).WatchPending(charge);
		}
		catch (Exception e) {
			ULog.Error($"Mobtakeran reserve {charge.Reserve}: background check crashed, reconcile it manually. {e.Message}");
		}
	});

	private async Task WatchPending(PendingCharge charge) {
		foreach (TimeSpan delay in BackgroundChecks) {
			await Task.Delay(delay);
			MobtakeranReply? statusReply = await Status(charge.Reserve, charge.Reference);
			switch (StateOf(statusReply)) {
				case TxnState.Success:
					await CompleteDebit(charge, statusReply!);
					await SetVasState(charge.Reserve, VasDone, statusReply!.Text("pin"));
					return;
				case TxnState.Failed:
					await Refund(charge);
					await SetVasState(charge.Reserve, VasRefunded, null);
					return;
				case TxnState.Unknown:
				default:
					continue;
			}
		}

		await SetVasState(charge.Reserve, VasUnknown, null);
		ULog.Error($"Mobtakeran reserve {charge.Reserve} / reference {charge.Reference} of user {charge.UserId} ({charge.Amount}) is still unknown; reconcile it manually.");
	}

	// Called once at startup (ChargeInternetRecoveryService): the in-process checks die with the server, so every purchase
	// that was still unknown is watched again. Refunds are keyed by the reserve, so a purchase is never refunded twice.
	private async Task RecoverPending(CancellationToken ct) {
		DateTime since = DateTime.UtcNow.AddDays(-7);
		List<VasEntity> pending = await db.Set<VasEntity>().Where(x => x.JsonData.Detail2 == VasPending && x.CreatedAt > since).ToListAsync(ct);
		foreach (VasEntity e in pending) {
			TagWalletTxn tag = e.Tags.Contains(TagVas.ChargePin) ? TagWalletTxn.ChargeSimPin
				: e.Tags.Contains(TagVas.InternetPackage) ? TagWalletTxn.InternetSim
				: TagWalletTxn.ChargeSimTopup;
			List<KeyValue> keyValues = [new KeyValue { Key = ULocalizedConstants.Reference, Value = e.AuthorizeCode }];
			Watch(new PendingCharge(e.CreatorId, e.JsonData.Detail1, e.AuthorizeCode, e.Amount, tag, keyValues));
		}

		if (pending.Count > 0) ULog.Info($"Mobtakeran: watching {pending.Count} purchase(s) that were still unknown before the restart.");
	}

	private async Task SetVasState(string reserve, string state, string? pin) {
		VasEntity? e = await db.Set<VasEntity>().AsTracking().FirstOrDefaultAsync(x => x.JsonData.Detail1 == reserve && x.JsonData.Detail2 == VasPending);
		if (e == null) return;
		e.JsonData.Detail2 = state;
		if (pin.IsNotNullOrEmpty()) e.JsonData.ChargePin = pin;
		await db.SaveChangesAsync();
	}

	public static Task RecoverPendingOnStartup(IServiceProvider services, CancellationToken ct) =>
		ActivatorUtilities.CreateInstance<ChargeInternetService>(services).RecoverPending(ct);

	// A PIN bought while the result was unknown only arrives now, so it is written into the user's wallet transaction.
	private async Task CompleteDebit(PendingCharge charge, MobtakeranReply statusReply) {
		string? pin = statusReply.Text("pin");
		if (charge.Tag != TagWalletTxn.ChargeSimPin || !pin.IsNotNullOrEmpty()) return;

		string key = DebitKey(charge.Reserve);
		WalletTxnEntity? e = await db.Set<WalletTxnEntity>().AsTracking().FirstOrDefaultAsync(x => x.SenderId == charge.UserId && x.JsonData.Detail2 == key);
		KeyValue? pinKeyValue = e?.JsonData.KeyValues.FirstOrDefault(x => x.Key == ULocalizedConstants.Pin);
		if (pinKeyValue == null) return;
		pinKeyValue.Value = pin!;
		await db.SaveChangesAsync();
	}

	private async Task Refund(PendingCharge charge) {
		string key = $"{DebitKey(charge.Reserve)}:refund";
		Guid mobtakeranId = Core.App.Users.Mobtakeran.Id;
		if (await db.Set<WalletTxnEntity>().AnyAsync(x => x.SenderId == mobtakeranId && x.JsonData.Detail2 == key)) return;

		UResponse<WalletTxnResponse?> result = await walletService.Transfer(new WalletTransferParams {
			ApiKey = Core.App.ApiKey,
			SenderId = mobtakeranId,
			ReceiverId = charge.UserId,
			Amount = charge.Amount,
			Detail1 = TagWalletTxn.ChargeInternetRefund.ToString(),
			Detail2 = key,
			KeyValues = charge.KeyValues.Where(x => x.Key != ULocalizedConstants.Pin).ToList(),
			TagWalletTxn = [TagWalletTxn.ChargeInternetRefund],
			AllowOverdraft = true
		}, CancellationToken.None);
		if (result.Status == Usc.Success) ULog.Info($"Mobtakeran reserve {charge.Reserve}: failed, {charge.Amount} refunded to user {charge.UserId}.");
		else ULog.Error($"Mobtakeran reserve {charge.Reserve}: failed but the refund to user {charge.UserId} failed: {result.Message}");
	}

	private static string DebitKey(string reserve) => $"mobtakeran:{reserve}";

	private async Task<UResponse<InternetPackageResponse?>> PackageList(string path, Dictionary<string, string> attachments, string operatorId, CancellationToken ct) {
		MobtakeranReply? reply = await Send(path, NewReserve(), attachments, ct);
		if (reply == null) return Error<InternetPackageResponse>(Usc.ThirdPartyError, ls.Get("chargeServiceIsNotAvailable"));
		if (!reply.Ok) return Error<InternetPackageResponse>(Usc.ThirdPartyError, reply.UserMessage(operatorId));

		return new UResponse<InternetPackageResponse?>(new InternetPackageResponse {
			Status = true,
			Message = reply.RootText("message") ?? "",
			List = ParsePackages(reply.Attachment("list"))
		});
	}

	// Looks the package up in the operator list, and for MCI also in the subscriber's companion packages.
	// Returns an error message when the lists could not be read at all.
	private async Task<(InternetPackageItem?, string?)> FindPackage(string operatorId, string subscriber, string packageId, CancellationToken ct) {
		UResponse<InternetPackageResponse?> list = await InternetList(new InternetListParams { ApiKey = "", OperatorId = operatorId }, ct);
		if (list.Result == null) return (null, list.Message);
		InternetPackageItem? package = list.Result.List.FirstOrDefault(x => x.Id == packageId);
		if (package != null || operatorId != Mci) return (package, null);

		UResponse<InternetPackageResponse?> offers = await MciTopOffer(new MCITopOfferParams { ApiKey = "", Subscriber = subscriber }, ct);
		return (offers.Result?.List.FirstOrDefault(x => x.Id == packageId), null);
	}

	// "list" is documented as a string holding a JSON array; a plain array is accepted too.
	private static List<InternetPackageItem> ParsePackages(JsonElement? list) {
		try {
			JsonElement array = list is { ValueKind: JsonValueKind.String } s ? JsonSerializer.Deserialize<JsonElement>(s.GetString() ?? "[]") : list ?? default;
			if (array.ValueKind != JsonValueKind.Array) return [];

			return array.EnumerateArray()
				.Select(x => new InternetPackageItem {
					Id = Text(x, "Id") ?? "",
					Title = Text(x, "Title") ?? "",
					Amount = (long)(Number(x, "amount") ?? 0),
					Price = (long)(Number(x, "Price") ?? 0),
					SimType = (int)(Number(x, "SimType") ?? 0),
					Duration = NormalizeDuration(Text(x, "Duration")),
					OfferCode = Text(x, "OfferCode") ?? "",
					PackageDType = (int)(Number(x, "PackageDType") ?? 0),
					Capacity = Text(x, "Capacity") ?? ""
				})
				.Where(x => x.Id != "" && x.Amount > 0)
				.ToList();
		}
		catch (JsonException e) {
			ULog.Error($"Mobtakeran package list could not be read: {e.Message}");
			return [];
		}
	}

	// Approve needs the buyer's identity. MCI wants a national code (or card number); Rightel wants the mobile registered in
	// the app in "cardnumber" when the user pays from a wallet. Field names are case sensitive (all lower case).
	private static Dictionary<string, string> ApproveAttachments(string reference, JwtClaimData userData, string operatorId) {
		Dictionary<string, string> attachments = new() { { "reference", reference } };
		if (userData.NationalCode.IsNotNullOrEmpty()) attachments["nationalcode"] = userData.NationalCode!.Trim();
		if (operatorId == Rightel && NormalizeMobile(userData.PhoneNumber) is { } mobile) attachments["cardnumber"] = mobile;
		return attachments;
	}

	private static string TopupType(string operatorId, string? chargeType) =>
		chargeType != null && TopupTypes.TryGetValue(operatorId, out string[]? types) && types.Contains(chargeType) ? chargeType : "0";

	// Sends one request with the shared token. Mobtakeran answers errors with a JSON body too (even with a non-2xx status),
	// so the body is always read. A rejected token (code 33) is renewed and the request is sent once more.
	private async Task<MobtakeranReply?> Send(string path, string reserve, Dictionary<string, string> attachments, CancellationToken ct, bool isRetry = false) {
		string? token = await Token(isRetry);
		if (token == null) return null;

		HttpResponseMessage? response = await httpClient.Post(
			$"{Core.App.Mobtakeran.BaseUrl}{path}",
			new { apiKey = Core.App.Mobtakeran.ApiKey, reserve, localDateTime = LocalDateTime(), attachments },
			new Dictionary<string, string> { { "Authorization", $"Bearer {token}" }, { "Accept", "application/json" } }
		);
		MobtakeranReply? reply = await Parse(response, ct);
		if (reply == null) return null;

		// "Whenever a token is received in any response, it replaces the previous one."
		string? newToken = reply.Text("token");
		if (newToken.IsNotNullOrEmpty()) SaveToken(newToken!);

		if (reply.Code == InvalidTokenCode && !isRetry) return await Send(path, reserve, attachments, ct, true);
		return reply;
	}

	private async Task<string?> Token(bool forceNew) {
		await TokenLock.WaitAsync();
		try {
			if (!forceNew && _token != null && DateTime.UtcNow < _tokenExpiresAt) return _token;

			HttpResponseMessage? response = await httpClient.Post(
				uri: $"{Core.App.Mobtakeran.BaseUrl}api/v2/login",
				body: new {
					apiKey = Core.App.Mobtakeran.ApiKey,
					reserve = NewReserve(),
					localDateTime = LocalDateTime(),
					attachments = new { username = Core.App.Mobtakeran.UserName, password = Core.App.Mobtakeran.Password }
				}
			);
			MobtakeranReply? reply = await Parse(response, CancellationToken.None);
			string? token = reply?.Text("token");
			if (!token.IsNotNullOrEmpty()) {
				ULog.Error($"Mobtakeran login failed: code {reply?.Code} - {reply?.RootText("message")}");
				return null;
			}

			SaveToken(token!);
			return token;
		}
		finally {
			TokenLock.Release();
		}
	}

	// Kept until one minute before the JWT expires (or 20 minutes when the expiry can't be read).
	private static void SaveToken(string token) {
		DateTime expiresAt = DateTime.UtcNow.AddMinutes(20);
		try {
			DateTime validTo = new JwtSecurityTokenHandler().ReadJwtToken(token).ValidTo;
			if (validTo > DateTime.UtcNow.AddMinutes(2)) expiresAt = validTo.AddMinutes(-1);
		}
		catch (Exception) {
			// Not a readable JWT: keep the default lifetime.
		}

		_token = token;
		_tokenExpiresAt = expiresAt;
	}

	private static async Task<MobtakeranReply?> Parse(HttpResponseMessage? response, CancellationToken ct) {
		if (response == null) return null;
		try {
			JsonElement root = JsonSerializer.Deserialize<JsonElement>(await response.Content.ReadAsStringAsync(ct));
			if (root.ValueKind != JsonValueKind.Object) return null;
			return new MobtakeranReply { Root = root, Attachments = Prop(root, "attachments") ?? default };
		}
		catch (JsonException) {
			return null;
		}
	}

	// reserve is the client's unique order number (Mobtakeran does not check it, the client must). Time + a counter keeps it
	// unique and it still fits in a long, which is how Mobtakeran returns it.
	private static string NewReserve() => $"{DateTime.UtcNow:yyMMddHHmmssfff}{Interlocked.Increment(ref _reserveCounter) % 1000:D3}";

	// Mobtakeran refuses a request whose date is not its (Iran) working day, so the time is sent in Iran time with its offset
	// (Iran has no daylight saving since 2022). Before, server-local time was sent with a "Z", which on a UTC server gave
	// yesterday's date between 00:00 and 03:30 in Iran. InvariantCulture keeps the Gregorian calendar on a fa-IR server.
	private static string LocalDateTime() =>
		DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromMinutes(210)).ToString("yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture);

	// Accepts 0912..., 912..., +98912..., 0098912... and Persian / Arabic digits; returns 09XXXXXXXXX or null.
	private static string? NormalizeMobile(string? number) {
		if (string.IsNullOrWhiteSpace(number)) return null;
		StringBuilder digits = new();
		foreach (char ch in number) {
			if (ch is >= '0' and <= '9') digits.Append(ch);
			else if (ch is >= '۰' and <= '۹') digits.Append((char)('0' + (ch - '۰')));
			else if (ch is >= '٠' and <= '٩') digits.Append((char)('0' + (ch - '٠')));
		}

		string d = digits.ToString();
		if (d.StartsWith("0098")) d = d[4..];
		else if (d.StartsWith("98") && d.Length == 12) d = d[2..];
		if (d.Length == 10 && d.StartsWith('9')) d = "0" + d;
		return d.Length == 11 && d.StartsWith("09") ? d : null;
	}

	private static string NormalizeDuration(string? raw) {
		if (string.IsNullOrWhiteSpace(raw)) return "UNKNOWN";
		string v = raw.Trim().ToUpperInvariant().Replace(" ", "");
		if (int.TryParse(v, out int num)) return num <= 31 ? $"{num}D" : $"{Math.Max(1, (int)Math.Round(num / 30.0))}M";
		if (v.Length == 2 && (v[0] == 'W' || v[0] == 'M' || v[0] == 'D')) return $"{v[1]}{v[0]}";
		if (v == "W") return "1W";
		return v == "M" ? "1M" : v;
	}

	private static UResponse<T?> Error<T>(Usc status, string message) where T : class => new(null, status, message);

	// JSON helpers that don't care about the exact case of a property name or whether a number was sent as a string.
	private static JsonElement? Prop(JsonElement e, string name) {
		if (e.ValueKind != JsonValueKind.Object) return null;
		if (e.TryGetProperty(name, out JsonElement value)) return value;
		foreach (JsonProperty p in e.EnumerateObject())
			if (string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase))
				return p.Value;
		return null;
	}

	private static string? Text(JsonElement e, string name) => Prop(e, name) is not { } v ? null : v.ValueKind switch {
		JsonValueKind.String => v.GetString(),
		JsonValueKind.Number => v.GetRawText(),
		JsonValueKind.True => "true",
		JsonValueKind.False => "false",
		_ => null
	};

	private static decimal? Number(JsonElement e, string name) =>
		decimal.TryParse(Text(e, name), NumberStyles.Number, CultureInfo.InvariantCulture, out decimal d) ? d : null;

	private static bool? Bool(JsonElement e, string name) => Text(e, name)?.ToLowerInvariant() switch {
		"true" or "1" => true,
		"false" or "0" => false,
		_ => null
	};

	// Every Mobtakeran method answers with the same envelope: reserve, serverDateTime, status, code, message and an
	// "attachments" object with the method's own fields.
	private sealed class MobtakeranReply {
		public required JsonElement Root { get; init; }
		public required JsonElement Attachments { get; init; }

		public int? Code => Number(Root, "code") is { } code ? (int)code : null;
		public bool Ok => Code == MobtakeranOk;

		public string? RootText(string name) => ChargeInternetService.Text(Root, name);
		public bool? RootBool(string name) => ChargeInternetService.Bool(Root, name);
		public long? Long(string name) => Number(Root, name) is { } n ? (long)n : null;
		public JsonElement? Attachment(string name) => Prop(Attachments, name);
		public string? Text(string name) => ChargeInternetService.Text(Attachments, name);
		public decimal? AttachmentNumber(string name) => Number(Attachments, name);
		public long? AttachmentLong(string name) => Number(Attachments, name) is { } n ? (long)n : null;
		public bool? AttachmentBool(string name) => ChargeInternetService.Bool(Attachments, name);

		// Mobtakeran's "message" is written for the merchant, so the user gets a message built from the codes instead.
		public string UserMessage(string? operatorId) => ChargeInternetErrors.Message(Code, Text("ext_code"), Text("message_source"), operatorId);
	}
}

// Resumes the checks of purchases that were still unknown when the server stopped. Waits a minute so startup (migrations,
// seeding) is finished first.
public class ChargeInternetRecoveryService(IServiceScopeFactory scopeFactory) : BackgroundService {
	protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
		try {
			await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
			using IServiceScope scope = scopeFactory.CreateScope();
			await ChargeInternetService.RecoverPendingOnStartup(scope.ServiceProvider, stoppingToken);
		}
		catch (OperationCanceledException) {
			// The server is stopping.
		}
		catch (Exception e) {
			ULog.Error($"Mobtakeran: could not resume the unknown purchases, reconcile them manually. {e.Message}");
		}
	}
}

public class ChargeInternetServiceFake(
	ILocalizationService ls,
	ITokenService ts,
	IWalletService walletService,
	IVasService vs
) : IChargeInternetService {
	public bool SimulateUnauthorized { get; set; }
	public bool SimulateLowBalance { get; set; }
	public bool SimulateUpstreamFailure { get; set; }
	public string FakePin { get; set; } = "1234567890123456";
	public long FakeBalance { get; set; } = 5_000_000;

	public async Task<UResponse<ChargeInternetReserveResponse?>> Pin(ReserveChargeParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null || SimulateUnauthorized) return new UResponse<ChargeInternetReserveResponse?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<ChargeInternetReserveResponse?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));
		decimal? payableAmount = ChargeInternetService.PayableAmount(p.SimType, p.Amount, true);
		if (payableAmount == null) return new UResponse<ChargeInternetReserveResponse?>(null, Usc.BadRequest, ls.Get("theSelectedChargeAmountIsNotOfferedByThisOperator"));
		if (SimulateLowBalance || !await walletService.HasEnoughBalance(userData.Id, payableAmount.Value, ct)) return new UResponse<ChargeInternetReserveResponse?>(null, Usc.BalanceIsLow, ls.Get("yourBalanceIsNotEnough"));
		if (SimulateUpstreamFailure) return new UResponse<ChargeInternetReserveResponse?>(null, Usc.ThirdPartyError, ls.Get("chargeServiceIsNotAvailable"));

		string reference = Math.Abs(Guid.NewGuid().GetHashCode()).ToString();
		await walletService.Purchase(
			new WalletPurchaseParams { AllowOverdraft = true,
				ApiKey = p.ApiKey,
				Token = p.Token,
				Tag = TagWalletTxn.ChargeSimPin,
				Amount = payableAmount.Value,
				KeyValues = [
					new KeyValue { Key = ULocalizedConstants.Operator, Value = p.SimType },
					new KeyValue { Key = ULocalizedConstants.Pin, Value = FakePin },
					new KeyValue { Key = ULocalizedConstants.Reference, Value = reference }
				]
			}, ct);
		await vs.Create(new VasCreateParams {
			Id = Guid.CreateVersion7(),
			ApiKey = p.ApiKey,
			Token = p.Token,
			Tags = [TagVas.ChargePin],
			CreatorId = userData.Id,
			Amount = payableAmount.Value,
			AuthorizeCode = reference,
			ChargePin = FakePin
		}, ct);
		string message = ls.Get("chargePinPurchasedSuccessfully");
		return new UResponse<ChargeInternetReserveResponse?>(BuildReserve(payableAmount.Value, FakePin, reference, message), Usc.Success, message);
	}

	public async Task<UResponse<ChargeInternetReserveResponse?>> Topup(TopupChargeParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null || SimulateUnauthorized) return new UResponse<ChargeInternetReserveResponse?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<ChargeInternetReserveResponse?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));
		decimal? payableAmount = ChargeInternetService.PayableAmount(p.OperatorId, p.Amount, false, p.ChargeType);
		if (payableAmount == null) return new UResponse<ChargeInternetReserveResponse?>(null, Usc.BadRequest, ls.Get("theSelectedChargeAmountIsNotOfferedByThisOperator"));
		if (SimulateLowBalance || !await walletService.HasEnoughBalance(userData.Id, payableAmount.Value, ct)) return new UResponse<ChargeInternetReserveResponse?>(null, Usc.BalanceIsLow, ls.Get("yourBalanceIsNotEnough"));
		if (SimulateUpstreamFailure) return new UResponse<ChargeInternetReserveResponse?>(null, Usc.ThirdPartyError, ls.Get("chargeServiceIsNotAvailable"));

		string reference = Math.Abs(Guid.NewGuid().GetHashCode()).ToString();
		await walletService.Purchase(
			new WalletPurchaseParams { AllowOverdraft = true,
				ApiKey = p.ApiKey,
				Token = p.Token,
				Tag = TagWalletTxn.ChargeSimTopup,
				Amount = payableAmount.Value,
				KeyValues = [
					new KeyValue { Key = ULocalizedConstants.PhoneNumber, Value = p.PhoneNumber },
					new KeyValue { Key = ULocalizedConstants.Operator, Value = p.OperatorId },
					new KeyValue { Key = ULocalizedConstants.Reference, Value = reference }
				]
			}, ct);
		string message = ls.Get("simCardChargedSuccessfully");
		return new UResponse<ChargeInternetReserveResponse?>(BuildReserve(payableAmount.Value, null, reference, message), Usc.Success, message);
	}

	public async Task<UResponse<ChargeInternetReserveResponse?>> InternetReserve(InternetReserveParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null || SimulateUnauthorized) return new UResponse<ChargeInternetReserveResponse?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<ChargeInternetReserveResponse?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));
		if (SimulateLowBalance || !await walletService.HasEnoughBalance(userData.Id, p.Amount, ct)) return new UResponse<ChargeInternetReserveResponse?>(null, Usc.BalanceIsLow, ls.Get("yourBalanceIsNotEnough"));
		if (SimulateUpstreamFailure) return new UResponse<ChargeInternetReserveResponse?>(null, Usc.ThirdPartyError, ls.Get("chargeServiceIsNotAvailable"));

		string reference = Math.Abs(Guid.NewGuid().GetHashCode()).ToString();
		await walletService.Purchase(
			new WalletPurchaseParams { AllowOverdraft = true,
				ApiKey = p.ApiKey,
				Token = p.Token,
				Tag = TagWalletTxn.InternetSim,
				Amount = p.Amount,
				KeyValues = [
					new KeyValue { Key = ULocalizedConstants.PhoneNumber, Value = p.Subscriber },
					new KeyValue { Key = ULocalizedConstants.Operator, Value = p.OperatorId },
					new KeyValue { Key = ULocalizedConstants.InternetPackage, Value = p.PackageId },
					new KeyValue { Key = ULocalizedConstants.Reference, Value = reference }
				]
			}, ct);
		string message = ls.Get("internetPackageActivatedSuccessfully");
		return new UResponse<ChargeInternetReserveResponse?>(BuildReserve(p.Amount, null, reference, message), Usc.Success, message);
	}

	public Task<UResponse<InternetPackageResponse?>> InternetList(InternetListParams p, CancellationToken ct) {
		if (SimulateUpstreamFailure) return Task.FromResult(new UResponse<InternetPackageResponse?>(null, Usc.ThirdPartyError, ls.Get("chargeServiceIsNotAvailable")));
		return Task.FromResult(new UResponse<InternetPackageResponse?>(new InternetPackageResponse {
			Status = true,
			Message = "OK",
			List = [
				new InternetPackageItem { Id = "PKG-D1", Title = "روزانه ۱ گیگ", Amount = 30_000, Price = 27_273, SimType = 0, Duration = "1D", OfferCode = "OFFD1", PackageDType = 3, Capacity = "1GB" },
				new InternetPackageItem { Id = "PKG-W5", Title = "هفتگی ۵ گیگ", Amount = 90_000, Price = 81_818, SimType = 0, Duration = "1W", OfferCode = "OFFW5", PackageDType = 2, Capacity = "5GB" },
				new InternetPackageItem { Id = "PKG-M10", Title = "ماهانه ۱۰ گیگ", Amount = 200_000, Price = 181_818, SimType = 1, Duration = "1M", OfferCode = "OFFM10", PackageDType = 1, Capacity = "10GB" },
				new InternetPackageItem { Id = "PKG-M30", Title = "ماهانه ۳۰ گیگ", Amount = 450_000, Price = 409_091, SimType = 1, Duration = "1M", OfferCode = "OFFM30", PackageDType = 1, Capacity = "30GB" }
			]
		}));
	}

	public Task<UResponse<InternetPackageResponse?>> MciTopOffer(MCITopOfferParams p, CancellationToken ct) {
		if (SimulateUpstreamFailure) return Task.FromResult(new UResponse<InternetPackageResponse?>(null, Usc.ThirdPartyError, ls.Get("chargeServiceIsNotAvailable")));
		return Task.FromResult(new UResponse<InternetPackageResponse?>(new InternetPackageResponse {
			Status = true,
			Message = "OK",
			List = [
				new InternetPackageItem { Id = "PKG-TOP7", Title = "همراهی ۷ گیگ", Amount = 70_000, Price = 63_636, SimType = 0, Duration = "7D", OfferCode = "", PackageDType = 11, Capacity = "7GB" }
			]
		}));
	}

	public Task<UResponse<GetStatusResponse?>> GetStatus(GetStatusParams p, CancellationToken ct) {
		if (SimulateUpstreamFailure) return Task.FromResult(new UResponse<GetStatusResponse?>(null, Usc.ThirdPartyError, ls.Get("chargeServiceIsNotAvailable")));
		return Task.FromResult(new UResponse<GetStatusResponse?>(new GetStatusResponse {
			Reserve = Math.Abs(Guid.NewGuid().GetHashCode()),
			ServerDateTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
			Status = true,
			Code = 1,
			Message = "Success",
			Reference = Math.Abs(Guid.NewGuid().GetHashCode()),
			Subscriber = "09120000000",
			Serial = "SER-0001",
			Pin = FakePin,
			TxnTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
			Help = null,
			MessageSource = "fake",
			ExtCode = "0"
		}));
	}

	public Task<UResponse<GetBalanceResponse?>> GetBalance(BaseParams p, CancellationToken ct) =>
		Task.FromResult(new UResponse<GetBalanceResponse?>(new GetBalanceResponse {
			Reserve = Math.Abs(Guid.NewGuid().GetHashCode()),
			ServerDateTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
			Status = true,
			Code = 1,
			Message = "Success",
			Balance = FakeBalance,
			Wallet = FakeBalance,
			Credit = 0,
			Limit = 0,
			Help = null,
			MessageSource = "fake",
			ExtCode = "0"
		}));

	public Task<UResponse<EchoResponse?>> Echo(CancellationToken ct) =>
		Task.FromResult(new UResponse<EchoResponse?>(new EchoResponse {
			Reserve = Math.Abs(Guid.NewGuid().GetHashCode()),
			ServerDateTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
			Status = true,
			Code = 1,
			Message = "Success",
			MciTopup = true,
			Mtn = true,
			Rightel = true,
			Shatel = true,
			MciInternet = true
		}));

	private static ChargeInternetReserveResponse BuildReserve(decimal amount, string? pin, string reference, string message) => new() {
		Reserve = Math.Abs(Guid.NewGuid().GetHashCode()),
		ServerDateTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
		Status = true,
		Code = 1,
		Message = message,
		Reference = reference,
		TraceId = Guid.NewGuid().ToString("N"),
		AffectiveAmount = (long)amount,
		Help = null,
		MessageSource = "fake",
		Pin = pin,
		Serial = "SER-0001"
	};
}
