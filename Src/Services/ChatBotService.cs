namespace SinaMN75U.Services;

public interface IChatBotService {
	Task<UResponse<ChatBotResponse?>> Create(ChatBotCreateParams p, CancellationToken ct);
	Task<UResponse<IEnumerable<ChatBotEntity>?>> Read(ChatBotReadParams p, CancellationToken ct);
}

public class ChatBotService(
	DbContext db,
	ILocalizationService ls,
	ITokenService ts,
	IHttpClientService http
) : IChatBotService {
	public async Task<UResponse<ChatBotResponse?>> Create(ChatBotCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<ChatBotResponse?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		ChatBotEntity? e = await db.Set<ChatBotEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == p.ChatId, ct);
		if (e != null) {
			string response = await http.Post("https://ai.ittalie.ir/sheldon/api/chat", new {
					user_input = p.Message,
					chat_history = e.JsonData.History.Select(x => new {
							user = x.User,
							assistant = x.Bot
						}
					)
				}
			);
			string responseValue = JsonDocument.Parse(response).RootElement.GetProperty("response").GetString()!;
			e.JsonData.History.Add(new ChatBotHistoryItem {
				User = p.Message,
				Bot = responseValue,
			});
			db.Set<ChatBotEntity>().Update(e);
			await db.SaveChangesAsync(ct);
			return new UResponse<ChatBotResponse?>(e.MapToResponse());
		}
		else {
			string response = await http.Post("https://ai.ittalie.ir/sheldon/api/chat", new {
					user_input = p.Message,
					chat_history = new List<string>()
				}
			);
			string responseValue = JsonDocument.Parse(response).RootElement.GetProperty("response").GetString()!;
			EntityEntry<ChatBotEntity> newE = await db.Set<ChatBotEntity>().AddAsync(new ChatBotEntity {
				CreatorId = userData.Id,
				JsonData = new ChatBotJsonData {
					History = [
						new ChatBotHistoryItem {
							User = p.Message,
							Bot = responseValue,
						}
					]
				},
				Tags = [TagChatBot.DrHana]
			}, ct);
			await db.SaveChangesAsync(ct);
			return new UResponse<ChatBotResponse?>(newE.Entity.MapToResponse());
		}
	}

	public async Task<UResponse<IEnumerable<ChatBotEntity>?>> Read(ChatBotReadParams p, CancellationToken ct) {
		IQueryable<ChatBotEntity> q = db.Set<ChatBotEntity>().Where(x => x.CreatorId == p.UserId);
		return await q.ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}
}