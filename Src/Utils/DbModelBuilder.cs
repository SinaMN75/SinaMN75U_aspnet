namespace SinaMN75U.Utils;

public static class DbModelBuilder {
	public static void SetupModelBuilder(this ModelBuilder builder) {
		foreach (IMutableEntityType entityType in builder.Model.GetEntityTypes())
		foreach (IMutableForeignKey foreignKey in entityType.GetForeignKeys())
			foreignKey.DeleteBehavior = DeleteBehavior.Cascade;

		foreach (IMutableEntityType entityType in builder.Model.GetEntityTypes()) {
			if (entityType.IsOwned()) continue;
			AddIndexIfMissing(entityType, "CreatedAt");
			AddIndexIfMissing(entityType, "CreatorId");
			AddGinIndexIfMissing(entityType, "Tags");
		}

		builder.Entity<CategoryEntity>().OwnsOne(e => e.JsonData, b => RelationalOwnedNavigationBuilderExtensions.ToJson(b));
		builder.Entity<MediaEntity>().OwnsOne(e => e.JsonData, b => RelationalOwnedNavigationBuilderExtensions.ToJson(b));
		builder.Entity<FollowEntity>().OwnsOne(e => e.JsonData, b => RelationalOwnedNavigationBuilderExtensions.ToJson(b));
		builder.Entity<TxnEntity>().OwnsOne(e => e.JsonData, b => RelationalOwnedNavigationBuilderExtensions.ToJson(b));
		builder.Entity<TicketEntity>().OwnsOne(e => e.JsonData, b => RelationalOwnedNavigationBuilderExtensions.ToJson(b));
		builder.Entity<ParkingEntity>().OwnsOne(e => e.JsonData, b => RelationalOwnedNavigationBuilderExtensions.ToJson(b));
		builder.Entity<ParkingReportEntity>().OwnsOne(e => e.JsonData, b => RelationalOwnedNavigationBuilderExtensions.ToJson(b));
		builder.Entity<VehicleEntity>().OwnsOne(e => e.JsonData, b => RelationalOwnedNavigationBuilderExtensions.ToJson(b));
		builder.Entity<AddressEntity>().OwnsOne(e => e.JsonData, b => RelationalOwnedNavigationBuilderExtensions.ToJson(b));
		builder.Entity<WalletEntity>().OwnsOne(e => e.JsonData, b => RelationalOwnedNavigationBuilderExtensions.ToJson(b));
		builder.Entity<WalletTxnEntity>().OwnsOne(e => e.JsonData, b => RelationalOwnedNavigationBuilderExtensions.ToJson(b));
		builder.Entity<TerminalEntity>().OwnsOne(e => e.JsonData, b => RelationalOwnedNavigationBuilderExtensions.ToJson(b));
		builder.Entity<BankAccountEntity>().OwnsOne(e => e.JsonData, b => RelationalOwnedNavigationBuilderExtensions.ToJson(b));
		builder.Entity<SimCardEntity>().OwnsOne(e => e.JsonData, b => RelationalOwnedNavigationBuilderExtensions.ToJson(b));
		builder.Entity<InquiryHistoryEntity>().OwnsOne(e => e.JsonData, b => RelationalOwnedNavigationBuilderExtensions.ToJson(b));
		builder.Entity<NotificationEntity>().OwnsOne(e => e.JsonData, b => RelationalOwnedNavigationBuilderExtensions.ToJson(b));
		builder.Entity<UserEntity>().OwnsOne(e => e.JsonData, b => RelationalOwnedNavigationBuilderExtensions.ToJson(b));
		builder.Entity<ProductEntity>().OwnsOne(e => e.JsonData, b => RelationalOwnedNavigationBuilderExtensions.ToJson(b));
		builder.Entity<VasEntity>().OwnsOne(e => e.JsonData, b => RelationalOwnedNavigationBuilderExtensions.ToJson(b));
		builder.Entity<MerchantEntity>().OwnsOne(e => e.JsonData, b => RelationalOwnedNavigationBuilderExtensions.ToJson(b));
		builder.Entity<MoadiEntity>().OwnsOne(e => e.JsonData, b => RelationalOwnedNavigationBuilderExtensions.ToJson(b));
		builder.Entity<DormBedContractEntity>().OwnsOne(e => e.JsonData, b => RelationalOwnedNavigationBuilderExtensions.ToJson(b));
		builder.Entity<HotelEntity>().OwnsOne(e => e.JsonData, b => RelationalOwnedNavigationBuilderExtensions.ToJson(b));
		builder.Entity<HotelRoomEntity>().OwnsOne(e => e.JsonData, b => RelationalOwnedNavigationBuilderExtensions.ToJson(b));
		builder.Entity<HotelReservationEntity>().OwnsOne(e => e.JsonData, b => RelationalOwnedNavigationBuilderExtensions.ToJson(b));
		builder.Entity<HotelInvoiceEntity>().OwnsOne(e => e.JsonData, b => RelationalOwnedNavigationBuilderExtensions.ToJson(b));
		builder.Entity<DormEntity>().OwnsOne(e => e.JsonData, b => RelationalOwnedNavigationBuilderExtensions.ToJson(b));
		builder.Entity<DormBedEntity>().OwnsOne(e => e.JsonData, b => RelationalOwnedNavigationBuilderExtensions.ToJson(b));
		builder.Entity<DormRoomEntity>().OwnsOne(e => e.JsonData, b => RelationalOwnedNavigationBuilderExtensions.ToJson(b));
		builder.Entity<DormBedInvoiceEntity>().OwnsOne(e => e.JsonData, b => RelationalOwnedNavigationBuilderExtensions.ToJson(b));
		builder.Entity<ContentEntity>().OwnsOne(e => e.JsonData, b => {
			RelationalOwnedNavigationBuilderExtensions.ToJson(b);
			b.OwnsMany(i => i.Extra);
		});
		builder.Entity<CommentEntity>().OwnsOne(e => e.JsonData, b => {
			RelationalOwnedNavigationBuilderExtensions.ToJson(b);
			b.OwnsMany(i => i.Reacts);
		});
		builder.Entity<ApiLogEntity>().OwnsOne(e => e.JsonData, b => RelationalOwnedNavigationBuilderExtensions.ToJson(b));
		builder.Entity<BlogEntity>().OwnsOne(e => e.JsonData, b => RelationalOwnedNavigationBuilderExtensions.ToJson(b));
	}

	private static void AddIndexIfMissing(IMutableEntityType entityType, string propertyName) {
		IMutableProperty? property = entityType.FindProperty(propertyName);
		if (property == null) return;
		if (entityType.FindIndex(property) != null) return;
		entityType.AddIndex(property);
	}

	private static void AddGinIndexIfMissing(IMutableEntityType entityType, string propertyName) {
		IMutableProperty? property = entityType.FindProperty(propertyName);
		if (property == null) return;
		if (entityType.FindIndex(property) != null) return;
		IMutableIndex index = entityType.AddIndex(property);
		index.SetAnnotation("Npgsql:IndexMethod", "gin");
	}
}