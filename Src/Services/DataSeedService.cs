namespace SinaMN75U.Services;

public interface IDataSeedService {
	Task<UResponse> SeedUsers(BaseParams p);
}

public class DataSeedService(DbContext db) : IDataSeedService {
	public async Task<UResponse> SeedUsers(BaseParams p) {
		await db.Set<UserEntity>().AddRangeAsync(Core.App.Users.SystemAdmin, Core.App.Users.ITHub, Core.App.Users.AvaPlus);
		await db.SaveChangesAsync();
		return new UResponse();
	}
}