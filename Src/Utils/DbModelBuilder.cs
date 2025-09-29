namespace SinaMN75U.Utils;

public static class DbModelBuilder {
	public static void SetupModelBuilder(this ModelBuilder builder) {
		foreach (IMutableEntityType entityType in builder.Model.GetEntityTypes())
		foreach (IMutableForeignKey foreignKey in entityType.GetForeignKeys())
			foreignKey.DeleteBehavior = DeleteBehavior.Cascade;

		builder.Entity<CategoryEntity>().OwnsOne(e => e.JsonData, b => RelationalOwnedNavigationBuilderExtensions.ToJson(b));
		builder.Entity<MediaEntity>().OwnsOne(e => e.JsonData, b => RelationalOwnedNavigationBuilderExtensions.ToJson(b));
		builder.Entity<ContentEntity>().OwnsOne(e => e.JsonData, b => RelationalOwnedNavigationBuilderExtensions.ToJson(b));
		builder.Entity<FollowEntity>().OwnsOne(e => e.JsonData, b => RelationalOwnedNavigationBuilderExtensions.ToJson(b));
		builder.Entity<UserEntity>().OwnsOne(e => e.JsonData, b => {
			RelationalOwnedNavigationBuilderExtensions.ToJson(b);
			b.OwnsMany(i => i.UserAnswerJson).OwnsMany(i => i.Results).OwnsOne(i => i.Answer);
			b.OwnsMany(i => i.VisitCounts);
		});
		builder.Entity<ProductEntity>().OwnsOne(e => e.JsonData, b => {
			RelationalOwnedNavigationBuilderExtensions.ToJson(b);
			b.OwnsMany(i => i.VisitCounts);
			b.OwnsMany(i => i.PointCounts);
		});
		builder.Entity<CommentEntity>().OwnsOne(e => e.JsonData, b => {
			RelationalOwnedNavigationBuilderExtensions.ToJson(b);
			b.OwnsMany(i => i.Reacts);
		});
		builder.Entity<ExamEntity>().OwnsOne(e => e.JsonData, b => {
			RelationalOwnedNavigationBuilderExtensions.ToJson(b);
			b.OwnsMany(i => i.Questions).OwnsMany(i => i.Options);
			b.OwnsMany(i => i.ScoreDetails);
		});
	}
}