namespace SinaMN75U.Data.Responses;

public class DbAdminTableResponse {
	public required string Schema { get; set; }
	public required string Name { get; set; }
	public long EstimatedRows { get; set; }
	public int ColumnCount { get; set; }
	public string? Size { get; set; }
}

public class DbAdminColumnResponse {
	public required string Name { get; set; }
	public required string DataType { get; set; }
	public bool IsNullable { get; set; }
	public bool IsPrimaryKey { get; set; }
	public string? Default { get; set; }
	public int OrdinalPosition { get; set; }
}

public class DbAdminIndexResponse {
	public required string Name { get; set; }
	public required string Definition { get; set; }
	public bool IsUnique { get; set; }
	public bool IsPrimary { get; set; }
}

public class DbAdminForeignKeyResponse {
	public required string Column { get; set; }
	public required string ReferencesTable { get; set; }
	public required string ReferencesColumn { get; set; }
	public required string ConstraintName { get; set; }
}

public class DbAdminTableSchemaResponse {
	public required string Schema { get; set; }
	public required string Table { get; set; }
	public List<DbAdminColumnResponse> Columns { get; set; } = [];
	public List<DbAdminIndexResponse> Indexes { get; set; } = [];
	public List<DbAdminForeignKeyResponse> ForeignKeys { get; set; } = [];
	public List<string> PrimaryKeys { get; set; } = [];
}

public class DbAdminQueryResultResponse {
	// Ordered column names for a result set (empty for statements that return no rows).
	public List<string> Columns { get; set; } = [];
	public List<string?> ColumnTypes { get; set; } = [];

	// Each row is a column -> stringified value map (null preserved).
	public List<Dictionary<string, string?>> Rows { get; set; } = [];

	public int RowCount { get; set; }
	public int? AffectedRows { get; set; }
	public long ExecutionMs { get; set; }
	public bool Truncated { get; set; }
	public string? PrimaryKeyColumn { get; set; }
}
