namespace SinaMN75U.Services;

public interface IDashboardService {
	Task<SystemMetricsResponse> ReadSystemMetrics();
	Task<DashboardResponse> ReadDashboardData(CancellationToken ct);
	Task<UResponse<FinancialOpsDashboardResponse?>> ReadFinancialOpsDashboard(DashboardRangeParams p, CancellationToken ct);
	Task<UResponse<PropertyDashboardResponse?>> ReadPropertyDashboard(DashboardRangeParams p, CancellationToken ct);
}

public class DashboardService(
	DbContext db,
	ICategoryService categoryService,
	ICommentService commentService,
	IContentService contentService,
	IMediaService mediaService,
	IProductService productService,
	IUserService userService,
	ITokenService ts,
	ILocalizationService ls
) : IDashboardService {
	// Wallet-txn tags that represent spending/consumption (mirrors AccountingService).
	private static readonly TagWalletTxn[] SpendingTags = [
		TagWalletTxn.MobileAndNationalCodeVerification, TagWalletTxn.ZipCodeToAddressDetail,
		TagWalletTxn.VehicleViolationsDetail, TagWalletTxn.DrivingLicenceStatus, TagWalletTxn.LicencePlateDetail,
		TagWalletTxn.DrivingLicenceNegativePoint, TagWalletTxn.IBanToBankAccountDetail, TagWalletTxn.FreewayTolls,
		TagWalletTxn.MerchantCreationFee, TagWalletTxn.ChargeSimPin, TagWalletTxn.ChargeSimTopup, TagWalletTxn.InternetSim
	];
	public async Task<SystemMetricsResponse> ReadSystemMetrics() {
		string[] linuxMemInfoKeys = ["MemTotal:", "MemAvailable:"];
		const double bytesToGb = 1024.0 * 1024 * 1024;
		double memUsage = 0, totalMem = 0, freeMem = 0, cpuUsage = 0, diskUsage = 0, totalDisk = 0, freeDisk = 0;
		if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
			try {
				(ulong user, ulong nice, ulong system, ulong total) firstSample = await GetLinuxCpuSample();
				await Task.Delay(1000);
				(ulong user, ulong nice, ulong system, ulong total) secondSample = await GetLinuxCpuSample();
				ulong used = secondSample.user + secondSample.nice + secondSample.system - (firstSample.user + firstSample.nice + firstSample.system);
				ulong total = secondSample.total - firstSample.total;
				cpuUsage = (double)used / total * 100;
				string[] memLines = await File.ReadAllLinesAsync("/proc/meminfo");
				double[] memInfo = memLines.Take(3)
					.Where(l => linuxMemInfoKeys.Any(l.StartsWith))
					.Select(l => double.Parse(l.Split(' ', StringSplitOptions.RemoveEmptyEntries)[1]))
					.ToArray();

				totalMem = memInfo[0] / (1024 * 1024);
				freeMem = memInfo[1] / (1024 * 1024);
				memUsage = 100 - freeMem / totalMem * 100;
			}
			catch {
				// Fallback values remain at 0
			}
		}
		else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
			try {
				using Process? cpuProcess = Process.Start(new ProcessStartInfo {
					FileName = "top",
					Arguments = "-l 1 -n 0 -stats cpu",
					RedirectStandardOutput = true,
					UseShellExecute = false
				});

				if (cpuProcess != null) {
					string cpuOutput = await cpuProcess.StandardOutput.ReadToEndAsync();
					await cpuProcess.WaitForExitAsync();

					string? cpuLine = cpuOutput.Split('\n')
						.FirstOrDefault(l => l.Trim().StartsWith("CPU usage:"));

					if (cpuLine != null) {
						string percent = cpuLine.Split(':')[1].Trim().Split(' ')[0].TrimEnd('%');
						cpuUsage = double.Parse(percent);
					}
				}

				// Memory metrics
				using Process? memProcess = Process.Start(new ProcessStartInfo {
					FileName = "vm_stat",
					RedirectStandardOutput = true,
					UseShellExecute = false
				});

				if (memProcess != null) {
					string memOutput = await memProcess.StandardOutput.ReadToEndAsync();
					await memProcess.WaitForExitAsync();

					string[] lines = memOutput.Split('\n');
					const double pageSize = 4096.0;
					double free = double.Parse(lines[1].Split(':')[1].Trim().Split(' ')[0]) * pageSize;
					double active = double.Parse(lines[2].Split(':')[1].Trim().Split(' ')[0]) * pageSize;
					double inactive = double.Parse(lines[3].Split(':')[1].Trim().Split(' ')[0]) * pageSize;
					double wired = double.Parse(lines[4].Split(':')[1].Trim().Split(' ')[0]) * pageSize;

					double used = (active + inactive + wired) / bytesToGb;
					totalMem = used + free / bytesToGb;
					freeMem = free / bytesToGb;
					memUsage = used / totalMem * 100;
				}
			}
			catch {
				// Fallback values remain at 0
			}
		}

		try {
			DriveInfo drive = DriveInfo.GetDrives()
				.First(d => d is { IsReady: true, Name: "/" or "C:\\" });

			totalDisk = drive.TotalSize / bytesToGb;
			freeDisk = drive.AvailableFreeSpace / bytesToGb;
			diskUsage = 100 - freeDisk / totalDisk * 100;
		}
		catch {
			// Fallback values remain at 0
		}

		return new SystemMetricsResponse(
			Math.Round(cpuUsage, 1),
			Math.Round(memUsage, 1),
			Math.Round(diskUsage, 1),
			Math.Round(totalMem, 6),
			Math.Round(freeMem, 6),
			Math.Round(totalDisk, 6),
			Math.Round(freeDisk, 6),
			DateTime.UtcNow
		);
	}

	public async Task<DashboardResponse> ReadDashboardData(CancellationToken ct) {
		UResponse<IEnumerable<UserResponse>?> newUsers = await userService.Read(new UserReadParams { PageSize = 5 }, ct);
		UResponse<IEnumerable<CategoryResponse>?> newCategories = await categoryService.Read(new CategoryReadParams { PageSize = 5 }, ct);
		UResponse<IEnumerable<CommentResponse>?> newComments = await commentService.Read(new CommentReadParams { PageSize = 5 }, ct);
		UResponse<IEnumerable<ContentResponse>?> newContents = await contentService.Read(new ContentReadParams { PageSize = 5 }, ct);
		UResponse<IEnumerable<MediaResponse>?> newMedia = await mediaService.Read(new BaseReadParams<TagMedia> { PageSize = 5 }, ct);
		UResponse<IEnumerable<ProductResponse>?> newProducts = await productService.Read(new ProductReadParams { PageSize = 5 }, ct);

		return new DashboardResponse {
			Categories = await db.Set<CategoryEntity>().CountAsync(ct),
			Comments = await db.Set<CommentEntity>().CountAsync(ct),
			Contents = await db.Set<ContentEntity>().CountAsync(ct),
			Media = await db.Set<MediaEntity>().CountAsync(ct),
			Products = await db.Set<ProductEntity>().CountAsync(ct),
			Users = await db.Set<UserEntity>().CountAsync(ct),
			NewUsers = newUsers.Result ?? [],
			NewCategories = newCategories.Result ?? [],
			NewComments = newComments.Result ?? [],
			NewContents = newContents.Result ?? [],
			NewMedia = newMedia.Result ?? [],
			NewProducts = newProducts.Result ?? []
		};
	}

	private static async Task<(ulong user, ulong nice, ulong system, ulong total)> GetLinuxCpuSample() {
		string[] lines = await File.ReadAllLinesAsync("/proc/stat");
		string[] values = lines.First(l => l.StartsWith("cpu "))
			.Split(' ', StringSplitOptions.RemoveEmptyEntries);

		ulong user = ulong.Parse(values[1]);
		ulong nice = ulong.Parse(values[2]);
		ulong system = ulong.Parse(values[3]);
		ulong idle = ulong.Parse(values[4]);
		ulong total = user + nice + system + idle;

		return (user, nice, system, total);
	}

	// ===================== Financial / Operations Dashboard =====================

	public async Task<UResponse<FinancialOpsDashboardResponse?>> ReadFinancialOpsDashboard(DashboardRangeParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<FinancialOpsDashboardResponse?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));
		if (!userData.IsAdmin) return new UResponse<FinancialOpsDashboardResponse?>(null, Usc.Forbidden, ls.Get("YouDoNotHaveClearanceToDoThisAction"));

		DateTime to = p.ToDate ?? DateTime.UtcNow;
		DateTime from = p.FromDate ?? to.AddDays(-30);

		int usersCount = await db.Set<UserEntity>().CountAsync(ct);
		int newUsersCount = await db.Set<UserEntity>().CountAsync(x => x.CreatedAt >= from && x.CreatedAt <= to, ct);

		int merchantsCount = await db.Set<MerchantEntity>().CountAsync(ct);
		int newMerchantsCount = await db.Set<MerchantEntity>().CountAsync(x => x.CreatedAt >= from && x.CreatedAt <= to, ct);

		int terminalsCount = await db.Set<TerminalEntity>().CountAsync(ct);
		int terminalsAssignedCount = await db.Set<TerminalEntity>().CountAsync(x => x.MerchantId != null, ct);

		int txnCount = await db.Set<TxnEntity>().CountAsync(ct);
		int newTxnCount = await db.Set<TxnEntity>().CountAsync(x => x.CreatedAt >= from && x.CreatedAt <= to, ct);

		int walletsCount = await db.Set<WalletEntity>().CountAsync(ct);
		decimal totalWalletBalance = await db.Set<WalletEntity>().SumAsync(x => (decimal?)x.Balance, ct) ?? 0;

		List<WalletTxnEntity> walletRows = await db.Set<WalletTxnEntity>().AsNoTracking()
			.Where(x => x.CreatedAt >= from && x.CreatedAt <= to)
			.ToListAsync(ct);

		decimal totalIn = walletRows.Where(x => x.Tags.Contains(TagWalletTxn.Charge)).Sum(x => x.Amount);
		decimal totalOut = walletRows.Where(x => x.Tags.Any(t => SpendingTags.Contains(t))).Sum(x => x.Amount);

		List<TxnEntity> txnRows = await db.Set<TxnEntity>().AsNoTracking()
			.Where(x => x.CreatedAt >= from && x.CreatedAt <= to)
			.ToListAsync(ct);

		List<AccountingBreakdownItem> txnByStatus = txnRows
			.SelectMany(x => x.Tags.Where(t => t is TagTxn.Pending or TagTxn.Paid or TagTxn.Failed or TagTxn.Refunded).Select(t => (Tag: t, x.Amount)))
			.GroupBy(x => x.Tag)
			.Select(g => new AccountingBreakdownItem { Tag = (int)g.Key, TagName = g.Key.ToString(), Amount = g.Sum(i => i.Amount), Count = g.Count() })
			.OrderByDescending(x => x.Amount).ToList();

		List<AccountingBreakdownItem> txnByMethod = txnRows
			.SelectMany(x => x.Tags.Where(t => t is TagTxn.CreditCard or TagTxn.Cash).Select(t => (Tag: t, x.Amount)))
			.GroupBy(x => x.Tag)
			.Select(g => new AccountingBreakdownItem { Tag = (int)g.Key, TagName = g.Key.ToString(), Amount = g.Sum(i => i.Amount), Count = g.Count() })
			.OrderByDescending(x => x.Amount).ToList();

		List<TerminalEntity> terminalTagRows = await db.Set<TerminalEntity>().AsNoTracking().ToListAsync(ct);

		List<AccountingBreakdownItem> terminalsByType = terminalTagRows
			.SelectMany(x => x.Tags)
			.GroupBy(t => t)
			.Select(g => new AccountingBreakdownItem { Tag = (int)g.Key, TagName = g.Key.ToString(), Amount = 0, Count = g.Count() })
			.OrderByDescending(x => x.Count).ToList();

		List<AccountingTimelineItem> dailyTimeline = walletRows
			.GroupBy(x => x.CreatedAt.Date)
			.Select(g => new AccountingTimelineItem {
				Date = g.Key,
				In = g.Where(x => x.Tags.Contains(TagWalletTxn.Charge)).Sum(x => x.Amount),
				Out = g.Where(x => x.Tags.Any(t => SpendingTags.Contains(t))).Sum(x => x.Amount)
			})
			.OrderBy(x => x.Date).ToList();

		List<TopMerchantItem> topMerchants = await db.Set<MerchantEntity>().AsNoTracking()
			.OrderByDescending(x => x.Terminals.Count)
			.Take(5)
			.Select(x => new TopMerchantItem { Id = x.Id, Title = x.Title, City = x.CityCode, TerminalCount = x.Terminals.Count, CreatedAt = x.CreatedAt })
			.ToListAsync(ct);

		List<TxnEntity> recentTxnEntities = await db.Set<TxnEntity>().AsNoTracking().Include(x => x.User)
			.OrderByDescending(x => x.CreatedAt).Take(10).ToListAsync(ct);
		List<RecentTxnItem> recentTransactions = recentTxnEntities.Select(x => new RecentTxnItem {
			Id = x.Id, Amount = x.Amount, TrackingNumber = x.TrackingNumber, UserName = x.User.UserName,
			Tags = x.Tags.Select(t => t.ToString()).ToList(), CreatedAt = x.CreatedAt
		}).ToList();

		List<MerchantEntity> recentMerchantEntities = await db.Set<MerchantEntity>().AsNoTracking()
			.OrderByDescending(x => x.CreatedAt).Take(5).ToListAsync(ct);
		List<RecentMerchantItem> recentMerchants = recentMerchantEntities.Select(x => new RecentMerchantItem {
			Id = x.Id, Title = x.Title, CityCode = x.CityCode, TerminalCount = 0, CreatedAt = x.CreatedAt
		}).ToList();

		List<UserEntity> recentUserEntities = await db.Set<UserEntity>().AsNoTracking()
			.OrderByDescending(x => x.CreatedAt).Take(5).ToListAsync(ct);
		List<RecentUserItem> recentUsers = recentUserEntities.Select(x => new RecentUserItem {
			Id = x.Id, DisplayName = $"{x.FirstName} {x.LastName}".Trim() is { Length: > 0 } n ? n : x.UserName,
			UserName = x.UserName, PhoneNumber = x.PhoneNumber, CreatedAt = x.CreatedAt
		}).ToList();

		return new UResponse<FinancialOpsDashboardResponse?>(new FinancialOpsDashboardResponse {
			GeneratedAt = DateTime.UtcNow,
			FromDate = from,
			ToDate = to,
			UsersCount = usersCount,
			NewUsersCount = newUsersCount,
			MerchantsCount = merchantsCount,
			NewMerchantsCount = newMerchantsCount,
			TerminalsCount = terminalsCount,
			TerminalsAssignedCount = terminalsAssignedCount,
			TerminalsUnassignedCount = terminalsCount - terminalsAssignedCount,
			TxnCount = txnCount,
			NewTxnCount = newTxnCount,
			WalletsCount = walletsCount,
			TotalWalletBalance = totalWalletBalance,
			TotalIn = totalIn,
			TotalOut = totalOut,
			Net = totalIn - totalOut,
			TxnByStatus = txnByStatus,
			TxnByMethod = txnByMethod,
			TerminalsByType = terminalsByType,
			DailyTimeline = dailyTimeline,
			TopMerchants = topMerchants,
			RecentTransactions = recentTransactions,
			RecentMerchants = recentMerchants,
			RecentUsers = recentUsers
		});
	}

	// ===================== Property (Hotels/Dorms) Dashboard =====================

	public async Task<UResponse<PropertyDashboardResponse?>> ReadPropertyDashboard(DashboardRangeParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<PropertyDashboardResponse?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));
		if (!userData.IsAdmin) return new UResponse<PropertyDashboardResponse?>(null, Usc.Forbidden, ls.Get("YouDoNotHaveClearanceToDoThisAction"));

		DateTime now = DateTime.UtcNow;
		DateTime to = p.ToDate ?? now;
		DateTime from = p.FromDate ?? to.AddDays(-30);
		DateTime soon = now.AddDays(30);

		int usersCount = await db.Set<UserEntity>().CountAsync(ct);
		int newUsersCount = await db.Set<UserEntity>().CountAsync(x => x.CreatedAt >= from && x.CreatedAt <= to, ct);

		int hotelsCount = await db.Set<HotelEntity>().CountAsync(ct);
		int hotelRoomsCount = await db.Set<HotelRoomEntity>().CountAsync(ct);
		int hotelRoomsAvailableCount = await db.Set<HotelRoomEntity>().CountAsync(x => x.IsAvailable, ct);
		int hotelRoomsOccupiedCount = hotelRoomsCount - hotelRoomsAvailableCount;

		int dormsCount = await db.Set<DormEntity>().CountAsync(ct);
		int dormRoomsCount = await db.Set<DormRoomEntity>().CountAsync(ct);
		int dormBedsCount = await db.Set<DormBedEntity>().CountAsync(ct);
		int dormBedsAvailableCount = await db.Set<DormBedEntity>().CountAsync(x => x.IsAvailable, ct);
		int dormBedsOccupiedCount = dormBedsCount - dormBedsAvailableCount;

		int contractsCount = await db.Set<DormBedContractEntity>().CountAsync(ct);
		int activeContractsCount = await db.Set<DormBedContractEntity>().CountAsync(x => x.StartDate <= now && x.EndDate >= now, ct);
		int upcomingContractsCount = await db.Set<DormBedContractEntity>().CountAsync(x => x.StartDate > now, ct);
		int expiredContractsCount = await db.Set<DormBedContractEntity>().CountAsync(x => x.EndDate < now, ct);
		int expiringSoonContractsCount = await db.Set<DormBedContractEntity>().CountAsync(x => x.EndDate >= now && x.EndDate <= soon, ct);

		int invoicesCount = await db.Set<DormBedInvoiceEntity>().CountAsync(ct);
		int unpaidInvoicesCount = await db.Set<DormBedInvoiceEntity>().CountAsync(x => x.Tags.Contains(TagDormBedInvoice.NotPaid), ct);
		int paidInvoicesCount = invoicesCount - unpaidInvoicesCount;
		int overdueInvoicesCount = await db.Set<DormBedInvoiceEntity>().CountAsync(x => x.Tags.Contains(TagDormBedInvoice.NotPaid) && x.DueDate < now, ct);

		decimal totalDebt = await db.Set<DormBedInvoiceEntity>().SumAsync(x => (decimal?)x.DebtAmount, ct) ?? 0;
		decimal totalPaid = await db.Set<DormBedInvoiceEntity>().SumAsync(x => (decimal?)x.PaidAmount, ct) ?? 0;
		decimal totalPenalty = await db.Set<DormBedInvoiceEntity>().SumAsync(x => (decimal?)x.PenaltyAmount, ct) ?? 0;

		List<DormBedInvoiceEntity> recentInvoices = await db.Set<DormBedInvoiceEntity>().AsNoTracking()
			.Where(x => x.CreatedAt >= now.AddMonths(-12))
			.ToListAsync(ct);
		List<DormBedInvoiceChartResponse> monthlyRevenue = recentInvoices
			.GroupBy(x => new { x.CreatedAt.Year, x.CreatedAt.Month })
			.Select(g => new DormBedInvoiceChartResponse {
				Month = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy"),
				TotalDebt = g.Sum(x => x.DebtAmount),
				TotalPaid = g.Sum(x => x.PaidAmount),
				TotalPenalty = g.Sum(x => x.PenaltyAmount),
				TotalRemaining = g.Sum(x => x.DebtAmount - x.PaidAmount),
				InvoiceCount = g.Count()
			})
			.OrderBy(x => x.Month).ToList();

		List<DormBedContractEntity> expiringEntities = await db.Set<DormBedContractEntity>().AsNoTracking()
			.Include(x => x.User).Include(x => x.Bed).ThenInclude(x => x.Room).ThenInclude(x => x.Dorm)
			.Where(x => x.EndDate >= now && x.EndDate <= soon)
			.OrderBy(x => x.EndDate).Take(10).ToListAsync(ct);
		List<ExpiringContractItem> expiringContracts = expiringEntities.Select(x => new ExpiringContractItem {
			Id = x.Id, UserName = x.User.UserName, BedTitle = x.Bed.Title, DormTitle = x.Bed.Room.Dorm.Title, EndDate = x.EndDate, Rent = x.Rent
		}).ToList();

		List<DormBedInvoiceEntity> overdueEntities = await db.Set<DormBedInvoiceEntity>().AsNoTracking()
			.Include(x => x.Contract).ThenInclude(x => x!.User)
			.Where(x => x.Tags.Contains(TagDormBedInvoice.NotPaid) && x.DueDate < now)
			.OrderBy(x => x.DueDate).Take(10).ToListAsync(ct);
		List<OverdueInvoiceItem> overdueInvoices = overdueEntities.Select(x => new OverdueInvoiceItem {
			Id = x.Id, UserName = x.Contract?.User.UserName, DebtAmount = x.DebtAmount, PaidAmount = x.PaidAmount,
			PenaltyAmount = x.PenaltyAmount, DueDate = x.DueDate, DaysOverdue = Math.Max(0, (now - x.DueDate).Days)
		}).ToList();

		List<DormBedContractEntity> recentContractEntities = await db.Set<DormBedContractEntity>().AsNoTracking()
			.Include(x => x.User).Include(x => x.Bed).ThenInclude(x => x.Room).ThenInclude(x => x.Dorm)
			.OrderByDescending(x => x.CreatedAt).Take(5).ToListAsync(ct);
		List<RecentContractItem> recentContracts = recentContractEntities.Select(x => new RecentContractItem {
			Id = x.Id, UserName = x.User.UserName, BedTitle = x.Bed.Title, DormTitle = x.Bed.Room.Dorm.Title,
			StartDate = x.StartDate, EndDate = x.EndDate, Rent = x.Rent, CreatedAt = x.CreatedAt
		}).ToList();

		List<UserEntity> recentUserEntities = await db.Set<UserEntity>().AsNoTracking()
			.OrderByDescending(x => x.CreatedAt).Take(5).ToListAsync(ct);
		List<RecentUserItem> recentUsers = recentUserEntities.Select(x => new RecentUserItem {
			Id = x.Id, DisplayName = $"{x.FirstName} {x.LastName}".Trim() is { Length: > 0 } n ? n : x.UserName,
			UserName = x.UserName, PhoneNumber = x.PhoneNumber, CreatedAt = x.CreatedAt
		}).ToList();

		List<PropertyBreakdownItem> hotelsByCity = await db.Set<HotelEntity>().AsNoTracking()
			.GroupBy(x => x.City)
			.Select(g => new PropertyBreakdownItem { Name = g.Key, Count = g.Count() })
			.OrderByDescending(x => x.Count).Take(10).ToListAsync(ct);

		List<PropertyBreakdownItem> dormsByCity = await db.Set<DormEntity>().AsNoTracking()
			.GroupBy(x => x.City)
			.Select(g => new PropertyBreakdownItem { Name = g.Key, Count = g.Count() })
			.OrderByDescending(x => x.Count).Take(10).ToListAsync(ct);

		return new UResponse<PropertyDashboardResponse?>(new PropertyDashboardResponse {
			GeneratedAt = DateTime.UtcNow,
			UsersCount = usersCount,
			NewUsersCount = newUsersCount,
			HotelsCount = hotelsCount,
			HotelRoomsCount = hotelRoomsCount,
			HotelRoomsAvailableCount = hotelRoomsAvailableCount,
			HotelRoomsOccupiedCount = hotelRoomsOccupiedCount,
			HotelOccupancyRate = hotelRoomsCount == 0 ? 0 : Math.Round(hotelRoomsOccupiedCount * 100.0 / hotelRoomsCount, 1),
			DormsCount = dormsCount,
			DormRoomsCount = dormRoomsCount,
			DormBedsCount = dormBedsCount,
			DormBedsAvailableCount = dormBedsAvailableCount,
			DormBedsOccupiedCount = dormBedsOccupiedCount,
			DormOccupancyRate = dormBedsCount == 0 ? 0 : Math.Round(dormBedsOccupiedCount * 100.0 / dormBedsCount, 1),
			ContractsCount = contractsCount,
			ActiveContractsCount = activeContractsCount,
			UpcomingContractsCount = upcomingContractsCount,
			ExpiredContractsCount = expiredContractsCount,
			ExpiringSoonContractsCount = expiringSoonContractsCount,
			InvoicesCount = invoicesCount,
			PaidInvoicesCount = paidInvoicesCount,
			UnpaidInvoicesCount = unpaidInvoicesCount,
			OverdueInvoicesCount = overdueInvoicesCount,
			TotalDebt = totalDebt,
			TotalPaid = totalPaid,
			TotalPenalty = totalPenalty,
			TotalOutstanding = totalDebt + totalPenalty - totalPaid,
			MonthlyRevenue = monthlyRevenue,
			ExpiringContracts = expiringContracts,
			OverdueInvoices = overdueInvoices,
			RecentContracts = recentContracts,
			RecentUsers = recentUsers,
			HotelsByCity = hotelsByCity,
			DormsByCity = dormsByCity
		});
	}
}