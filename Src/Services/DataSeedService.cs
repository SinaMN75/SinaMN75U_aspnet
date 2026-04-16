namespace SinaMN75U.Services;

public interface IDataSeedService {
	Task SeedUsers();
}

public class DataSeedService(DbContext db) : IDataSeedService {
	public async Task SeedUsers() {
		await db.Set<UserEntity>().AddRangeAsync(Core.App.Users.SystemAdmin, Core.App.Users.ITHub, Core.App.Users.AvaPlus);
		await db.SaveChangesAsync();
	}
}