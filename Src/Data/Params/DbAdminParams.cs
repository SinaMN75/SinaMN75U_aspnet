namespace SinaMN75U.Data.Params;

// SystemAdmin-only database console params. Schema defaults to "public"; identifiers are always quoted server side.
public sealed class DbAdminTablesParams : BaseParams {
	public string Schema { get; set; } = "public";
}

public sealed class DbAdminTableSchemaParams : BaseParams {
	[UValidationRequired("TableRequired")]
	public required string Table { get; set; }

	public string Schema { get; set; } = "public";
}

public sealed class DbAdminRowsParams : BaseParams {
	[UValidationRequired("TableRequired")]
	public required string Table { get; set; }

	public string Schema { get; set; } = "public";
	public int PageSize { get; set; } = 100;
	public int PageNumber { get; set; } = 1;
	public string? OrderByColumn { get; set; }
	public bool Descending { get; set; }
}

public sealed class DbAdminQueryParams : BaseParams {
	[UValidationRequired("SqlRequired")]
	public required string Sql { get; set; }

	// Cap on the number of rows returned for SELECT-like statements.
	public int MaxRows { get; set; } = 500;
}

public sealed class DbAdminUpdateRowParams : BaseParams {
	[UValidationRequired("TableRequired")]
	public required string Table { get; set; }

	public string Schema { get; set; } = "public";

	[UValidationRequired("PrimaryKeyRequired")]
	public required string PrimaryKeyColumn { get; set; }

	[UValidationRequired("PrimaryKeyRequired")]
	public required string PrimaryKeyValue { get; set; }

	public Dictionary<string, JsonElement> Values { get; set; } = new();
}

public sealed class DbAdminInsertRowParams : BaseParams {
	[UValidationRequired("TableRequired")]
	public required string Table { get; set; }

	public string Schema { get; set; } = "public";

	public Dictionary<string, JsonElement> Values { get; set; } = new();
}

public sealed class DbAdminDeleteRowParams : BaseParams {
	[UValidationRequired("TableRequired")]
	public required string Table { get; set; }

	public string Schema { get; set; } = "public";

	[UValidationRequired("PrimaryKeyRequired")]
	public required string PrimaryKeyColumn { get; set; }

	[UValidationRequired("PrimaryKeyRequired")]
	public required string PrimaryKeyValue { get; set; }
}
