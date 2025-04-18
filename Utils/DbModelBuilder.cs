namespace SinaMN75U.Utils;

using Microsoft.EntityFrameworkCore.Metadata;

public static class DbModelBuilder {
	public static void SetupModelBuilder(this ModelBuilder builder) {
		foreach (IMutableEntityType entityType in builder.Model.GetEntityTypes()) {
			foreach (IMutableForeignKey foreignKey in entityType.GetForeignKeys()) {
				foreignKey.DeleteBehavior = DeleteBehavior.Cascade;
			}
		}

		builder.Entity<UserEntity>().OwnsOne(e => e.Json, b => {
			b.ToJson();
			b.OwnsMany(i => i.UserAnswerJson).OwnsMany(i => i.Results);
		});
		builder.Entity<CategoryEntity>().OwnsOne(e => e.Json, b => b.ToJson());
		builder.Entity<MediaEntity>().OwnsOne(e => e.Json, b => b.ToJson());
		builder.Entity<ContentEntity>().OwnsOne(e => e.Json, b => b.ToJson());
		builder.Entity<ProductEntity>().OwnsOne(e => e.Json, b => {
			b.ToJson();
			b.OwnsMany(i => i.VisitCounts);
		});
		builder.Entity<CommentEntity>().OwnsOne(e => e.Json, b => {
			b.ToJson();
			b.OwnsMany(i => i.Reacts);
		});
		builder.Entity<ExamEntity>().OwnsOne(e => e.Json, b => {
			b.ToJson();
			b.OwnsMany(i => i.Questions).OwnsMany(i => i.Options);
		});
	}
}