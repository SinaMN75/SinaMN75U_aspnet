using System.Collections.ObjectModel;

namespace SinaMN75U.Services;

public interface IDataSeedService {
	Task<UResponse> SeedUsers();
}

public class DataSeedService(DbContext db) : IDataSeedService {
	public async Task<UResponse> SeedUsers() {
		await db.Set<UserEntity>().AddRangeAsync(
			Core.App.Users.SystemAdmin,
			Core.App.Users.ITHub,
			Core.App.Users.AvaPlus,
			Core.App.Users.Mobtakeran
		);
		await db.SaveChangesAsync();
		return new UResponse();
	}
}