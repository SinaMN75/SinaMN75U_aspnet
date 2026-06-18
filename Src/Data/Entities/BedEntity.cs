namespace SinaMN75U.Data.Entities;

public class BedEntity: BaseEntity<TagBed, BaseJson> {
	public decimal Deposit { get; set; }
	public decimal Rent { get; set; }

	public Guid? ParentId { get; set; }
	public BedEntity? Parent { get; set; }

	[InverseProperty("Parent")]
	public ICollection<BedEntity> Children { get; set; } = [];
	
	public ICollection<ContractEntity> Contracts { get; set; } = [];
	public ICollection<MediaEntity> Media { get; set; } = [];
}