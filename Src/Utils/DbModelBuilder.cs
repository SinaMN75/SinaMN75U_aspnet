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
		builder.Entity<ContractEntity>().OwnsOne(e => e.JsonData, b => RelationalOwnedNavigationBuilderExtensions.ToJson(b));
		builder.Entity<InvoiceEntity>().OwnsOne(e => e.JsonData, b => RelationalOwnedNavigationBuilderExtensions.ToJson(b));
		builder.Entity<TxnEntity>().OwnsOne(e => e.JsonData, b => RelationalOwnedNavigationBuilderExtensions.ToJson(b));
		builder.Entity<TicketEntity>().OwnsOne(e => e.JsonData, b => RelationalOwnedNavigationBuilderExtensions.ToJson(b));
		builder.Entity<ParkingEntity>().OwnsOne(e => e.JsonData, b => RelationalOwnedNavigationBuilderExtensions.ToJson(b));
		builder.Entity<ParkingReportEntity>().OwnsOne(e => e.JsonData, b => RelationalOwnedNavigationBuilderExtensions.ToJson(b));
		builder.Entity<VehicleEntity>().OwnsOne(e => e.JsonData, b => RelationalOwnedNavigationBuilderExtensions.ToJson(b));
		builder.Entity<AddressEntity>().OwnsOne(e => e.JsonData, b => RelationalOwnedNavigationBuilderExtensions.ToJson(b));
		builder.Entity<WalletEntity>().OwnsOne(e => e.JsonData, b => RelationalOwnedNavigationBuilderExtensions.ToJson(b));
		builder.Entity<WalletTxnEntity>().OwnsOne(e => e.JsonData, b => RelationalOwnedNavigationBuilderExtensions.ToJson(b));
		builder.Entity<UserEntity>().OwnsOne(e => e.JsonData, b => {
			RelationalOwnedNavigationBuilderExtensions.ToJson(b);
			b.OwnsMany(i => i.VisitCounts);
		});
		builder.Entity<UserExtraEntity>().OwnsOne(e => e.JsonData, b => RelationalOwnedNavigationBuilderExtensions.ToJson(b));
		builder.Entity<ProductEntity>().OwnsOne(e => e.JsonData, b => {
			RelationalOwnedNavigationBuilderExtensions.ToJson(b);
			b.OwnsMany(i => i.VisitCounts);
			b.OwnsMany(i => i.PointCounts);
		});
		builder.Entity<CommentEntity>().OwnsOne(e => e.JsonData, b => {
			RelationalOwnedNavigationBuilderExtensions.ToJson(b);
			b.OwnsMany(i => i.Reacts);
		});
		builder.Entity<ChatBotEntity>().OwnsOne(e => e.JsonData, b => {
			RelationalOwnedNavigationBuilderExtensions.ToJson(b);
			b.OwnsMany(i => i.History);
		});
	}
}