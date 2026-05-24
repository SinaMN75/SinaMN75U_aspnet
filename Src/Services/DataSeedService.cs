using System.Collections.ObjectModel;

namespace SinaMN75U.Services;

public interface IDataSeedService {
	Task<UResponse> SeedUsers();
	Task<UResponse> SeedVerificationProcess(); // اضافه شد
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

	public async Task<UResponse> SeedVerificationProcess() {
		// 1. اول فرآیند اصلی رو بساز
		ProcessEntity process = new() {
			Id = Guid.NewGuid(),
			CreatedAt = DateTime.UtcNow,
			CreatorId = Core.App.Users.SystemAdmin.Id,
			Title = "فرآیند احراز هویت",
			JsonData = new BaseJson {
				Detail1 = "احراز هویت کاربران جدید",
				Detail2 = "نسخه 1.0"
			},
			Tags = new Collection<TagProcess> { TagProcess.Active }
		};

		// 2. مراحل رو به ترتیب بساز
		List<ProcessStepEntity> steps = new();

		// مرحله 1: کارت ملی
		steps.Add(new ProcessStepEntity {
			Id = Guid.NewGuid(),
			CreatedAt = DateTime.UtcNow,
			CreatorId = Core.App.Users.SystemAdmin.Id,
			StepNumber = 1,
			Instruction = "لطفاً تصویر روی کارت ملی خود را آپلود کنید. (فرمت JPEG یا PNG)",
			ProcessId = process.Id,
			JsonData = new BaseJson(),
			Tags = new Collection<TagProcessStep>()
		});

		// مرحله 2: پشت کارت ملی
		steps.Add(new ProcessStepEntity {
			Id = Guid.NewGuid(),
			CreatedAt = DateTime.UtcNow,
			CreatorId = Core.App.Users.SystemAdmin.Id,
			StepNumber = 2,
			Instruction = "لطفاً تصویر پشت کارت ملی خود را آپلود کنید.",
			ProcessId = process.Id,
			JsonData = new BaseJson(),
			Tags = new Collection<TagProcessStep>()
		});

		// مرحله 3: صفحه اول شناسنامه
		steps.Add(new ProcessStepEntity {
			Id = Guid.NewGuid(),
			CreatedAt = DateTime.UtcNow,
			CreatorId = Core.App.Users.SystemAdmin.Id,
			StepNumber = 3,
			Instruction = "لطفاً تصویر صفحه اول شناسنامه خود را آپلود کنید.",
			ProcessId = process.Id,
			JsonData = new BaseJson(),
			Tags = new Collection<TagProcessStep>()
		});

		// مرحله 4: ویدیو احراز هویت
		steps.Add(new ProcessStepEntity {
			Id = Guid.NewGuid(),
			CreatedAt = DateTime.UtcNow,
			CreatorId = Core.App.Users.SystemAdmin.Id,
			StepNumber = 4,
			Instruction = "لطفاً یک ویدیوی 5 ثانیه‌ای از خودتان در حال خواندن یک جمله تصادفی آپلود کنید.",
			ProcessId = process.Id,
			JsonData = new BaseJson(),
			Tags = new Collection<TagProcessStep>()
		});

		// مرحله 5: امضای دیجیتال
		steps.Add(new ProcessStepEntity {
			Id = Guid.NewGuid(),
			CreatedAt = DateTime.UtcNow,
			CreatorId = Core.App.Users.SystemAdmin.Id,
			StepNumber = 5,
			Instruction = "لطفاً امضای دیجیتال خود را آپلود کنید.",
			ProcessId = process.Id,
			JsonData = new BaseJson(),
			Tags = new Collection<TagProcessStep>()
		});

		// 3. همه رو به دیتابیس اضافه کن
		await db.Set<ProcessEntity>().AddAsync(process);
		await db.Set<ProcessStepEntity>().AddRangeAsync(steps);
		await db.SaveChangesAsync();

		return new UResponse();
	}
}