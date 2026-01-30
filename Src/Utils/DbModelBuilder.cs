namespace SinaMN75U.Utils;

public static class DbModelBuilder {
	public static void SetupModelBuilder(this ModelBuilder builder) {
		foreach (IMutableEntityType entityType in builder.Model.GetEntityTypes())
		foreach (IMutableForeignKey foreignKey in entityType.GetForeignKeys())
			foreignKey.DeleteBehavior = DeleteBehavior.Cascade;

		builder.Entity<CategoryEntity>().OwnsOne(e => e.JsonData, b => b.ToJson());
		builder.Entity<MediaEntity>().OwnsOne(e => e.JsonData, b => b.ToJson());
		builder.Entity<ContentEntity>().OwnsOne(e => e.JsonData, b => b.ToJson());
		builder.Entity<FollowEntity>().OwnsOne(e => e.JsonData, b => b.ToJson());
		builder.Entity<ContractEntity>().OwnsOne(e => e.JsonData, b => b.ToJson());
		builder.Entity<InvoiceEntity>().OwnsOne(e => e.JsonData, b => b.ToJson());
		builder.Entity<TxnEntity>().OwnsOne(e => e.JsonData, b => b.ToJson());
		builder.Entity<TicketEntity>().OwnsOne(e => e.JsonData, b => b.ToJson());
		builder.Entity<ParkingEntity>().OwnsOne(e => e.JsonData, b => b.ToJson());
		builder.Entity<ParkingReportEntity>().OwnsOne(e => e.JsonData, b => b.ToJson());
		builder.Entity<VehicleEntity>().OwnsOne(e => e.JsonData, b => b.ToJson());
		builder.Entity<UserEntity>().OwnsOne(e => e.JsonData, b => {
			b.ToJson();
			b.OwnsMany(i => i.UserAnswerJson).OwnsMany(i => i.Results).OwnsOne(i => i.Answer);
			b.OwnsMany(i => i.VisitCounts);
		});
		builder.Entity<ProductEntity>().OwnsOne(e => e.JsonData, b => {
			b.ToJson();
			b.OwnsMany(i => i.VisitCounts);
			b.OwnsMany(i => i.PointCounts);
		});
		builder.Entity<CommentEntity>().OwnsOne(e => e.JsonData, b => {
			b.ToJson();
			b.OwnsMany(i => i.Reacts);
		});
		builder.Entity<ExamEntity>().OwnsOne(e => e.JsonData, b => {
			b.ToJson();
			b.OwnsMany(i => i.Questions).OwnsMany(i => i.Options);
			b.OwnsMany(i => i.ScoreDetails);
		});
		builder.Entity<ChatBotEntity>().OwnsOne(e => e.JsonData, b => {
			b.ToJson();
			b.OwnsMany(i => i.History);
		});
	}
}