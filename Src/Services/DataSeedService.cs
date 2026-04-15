namespace SinaMN75U.Services;

public interface IDataSeedService {
	Task SeedUsers();
}

public class DataSeedService(DbContext db) : IDataSeedService {
	public async Task SeedUsers() {
		Guid id = Guid.CreateVersion7();
		UserEntity superAdmin = new() {
			Id = id,
			CreatedAt = DateTime.UtcNow,
			UserName = "SinaMN75",
			Password = "SinaMN75",
			RefreshToken = "",
			PhoneNumber = "+989351902721",
			NationalCode = "0019246935",
			Email = "sinamn75@gmail.com",
			FirstName = "Sina",
			LastName = "MohammadZadeh",
			Bio = "BIO",
			Birthdate = new DateTime(1996, 7, 21),
			Tags = [TagUser.Male, TagUser.Female],
			JsonData = new UserJson {
				FcmToken = "",
				FatherName = "Davoud",
				Weight = 180,
				Height = 90
			},
			Extra = new UserExtraEntity {
				UserId = id,
				Id = id,
				CreatedAt = DateTime.UtcNow,
				Tags = [],
				JsonData = new GeneralJsonData()
			}
		};

		await db.Set<UserEntity>().AddRangeAsync(superAdmin);
	}
}