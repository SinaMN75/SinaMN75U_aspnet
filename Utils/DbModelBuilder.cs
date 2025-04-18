namespace SinaMN75U.Utils;

using Microsoft.EntityFrameworkCore.Metadata;

public static class DbModelBuilder {
	public static void SetupModelBuilder(this ModelBuilder builder) {
		foreach (IMutableEntityType entityType in builder.Model.GetEntityTypes()) {
			foreach (IMutableForeignKey foreignKey in entityType.GetForeignKeys()) {
				foreignKey.DeleteBehavior = DeleteBehavior.Cascade;
			}
		}
		
		builder.Entity<UserEntity>().OwnsOne(e => e.JsonData, b => {
			b.ToJson();
			b.OwnsMany(i => i.UserAnswerJson).OwnsMany(i => i.Results).OwnsOne(i => i.Answer);
		});
		builder.Entity<CategoryEntity>().OwnsOne(e => e.JsonData, b => b.ToJson());
		builder.Entity<MediaEntity>().OwnsOne(e => e.JsonData, b => b.ToJson());
		builder.Entity<ContentEntity>().OwnsOne(e => e.JsonData, b => b.ToJson());
		builder.Entity<ProductEntity>().OwnsOne(e => e.JsonData, b => {
			b.ToJson();
			b.OwnsMany(i => i.VisitCounts);
		});
		builder.Entity<CommentEntity>().OwnsOne(e => e.JsonData, b => {
			b.ToJson();
			b.OwnsMany(i => i.Reacts);
		});
		builder.Entity<ExamEntity>().OwnsOne(e => e.JsonData, b => {
			b.ToJson();
			b.OwnsMany(i => i.Questions).OwnsMany(i => i.Options);
		});
	}
}