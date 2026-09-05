namespace SinaMN75U.Data.Params;

// SystemAdmin-only database console params. Schema defaults to "public"; identifiers are always quoted server side.
public sealed class DbAdminTablesParams : BaseParams {
	public string Schema { get; set; } = "public";
}

public sealed class DbAdminTableSchemaParams : BaseParams {
	[UValidationRequired("tableIsRequired")]
	public required string Table { get; set; }

	public string Schema { get; set; } = "public";
}

public sealed class DbAdminRowsParams : BaseParams {
	[UValidationRequired("tableIsRequired")]
	public required string Table { get; set; }

	public string Schema { get; set; } = "public";
	public int PageSize { get; set; } = 100;
	public int PageNumber { get; set; } = 1;
	public string? OrderByColumn { get; set; }
	public bool Descending { get; set; }

	// Raw SQL condition appended as WHERE (SystemAdmin-only, full-power console). Null/empty = no filter.
	public string? Where { get; set; }

	// When false, the total row count is not recomputed (used while paging/sorting so large tables aren't re-scanned).
	public bool WithCount { get; set; } = true;
}

public sealed class DbAdminQueryParams : BaseParams {
	[UValidationRequired("sqlIsRequired")]
	public required string Sql { get; set; }

	// Cap on the number of rows returned for SELECT-like statements.
	public int MaxRows { get; set; } = 500;
}

public sealed class DbAdminUpdateRowParams : BaseParams {
	[UValidationRequired("tableIsRequired")]
	public required string Table { get; set; }

	public string Schema { get; set; } = "public";

	[UValidationRequired("primaryKeyIsRequired")]
	public required string PrimaryKeyColumn { get; set; }

	[UValidationRequired("primaryKeyIsRequired")]
	public required string PrimaryKeyValue { get; set; }

	public Dictionary<string, JsonElement> Values { get; set; } = new();
}

public sealed class DbAdminInsertRowParams : BaseParams {
	[UValidationRequired("tableIsRequired")]
	public required string Table { get; set; }

	public string Schema { get; set; } = "public";

	public Dictionary<string, JsonElement> Values { get; set; } = new();
}

public sealed class DbAdminDeleteRowParams : BaseParams {
	[UValidationRequired("tableIsRequired")]
	public required string Table { get; set; }

	public string Schema { get; set; } = "public";

	[UValidationRequired("primaryKeyIsRequired")]
	public required string PrimaryKeyColumn { get; set; }

	[UValidationRequired("primaryKeyIsRequired")]
	public required string PrimaryKeyValue { get; set; }
}
