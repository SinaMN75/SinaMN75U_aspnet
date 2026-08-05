namespace SinaMN75U.Services;

public interface IDbAdminService {
	Task<UResponse<List<DbAdminTableResponse>?>> Tables(DbAdminTablesParams p, CancellationToken ct);
	Task<UResponse<DbAdminTableSchemaResponse?>> Schema(DbAdminTableSchemaParams p, CancellationToken ct);
	Task<UResponse<DbAdminQueryResultResponse?>> Rows(DbAdminRowsParams p, CancellationToken ct);
	Task<UResponse<DbAdminQueryResultResponse?>> Query(DbAdminQueryParams p, CancellationToken ct);
	Task<UResponse<DbAdminQueryResultResponse?>> UpdateRow(DbAdminUpdateRowParams p, CancellationToken ct);
	Task<UResponse<DbAdminQueryResultResponse?>> InsertRow(DbAdminInsertRowParams p, CancellationToken ct);
	Task<UResponse> DeleteRow(DbAdminDeleteRowParams p, CancellationToken ct);
}

// SystemAdmin-only PgAdmin-style console. Every statement runs against the live connection with full SQL power,
// so access is gated by GuardAdmin and identifiers are always quoted server side.
public class DbAdminService(DbContext db, ITokenService ts, ILocalizationService ls) : IDbAdminService {
	public async Task<UResponse<List<DbAdminTableResponse>?>> Tables(DbAdminTablesParams p, CancellationToken ct) {
		UResponse<List<DbAdminTableResponse>?>? guard = GuardAdmin<List<DbAdminTableResponse>?>(p.Token);
		if (guard != null) return guard;

		NpgsqlConnection conn = await OpenConnection(ct);
		const string sql = """
			SELECT c.relname AS name,
			       c.reltuples::bigint AS estimated_rows,
			       pg_size_pretty(pg_total_relation_size(c.oid)) AS size,
			       (SELECT count(*) FROM information_schema.columns col WHERE col.table_schema = @schema AND col.table_name = c.relname) AS column_count
			FROM pg_class c
			JOIN pg_namespace n ON n.oid = c.relnamespace
			WHERE n.nspname = @schema AND c.relkind = 'r'
			ORDER BY c.relname;
			""";
		await using NpgsqlCommand cmd = new(sql, conn);
		cmd.Parameters.AddWithValue("schema", p.Schema);
		List<DbAdminTableResponse> tables = [];
		await using (NpgsqlDataReader reader = await cmd.ExecuteReaderAsync(ct)) {
			while (await reader.ReadAsync(ct))
				tables.Add(new DbAdminTableResponse {
					Schema = p.Schema,
					Name = reader.GetString(0),
					EstimatedRows = reader.IsDBNull(1) ? 0 : reader.GetInt64(1),
					Size = reader.IsDBNull(2) ? null : reader.GetString(2),
					ColumnCount = reader.IsDBNull(3) ? 0 : Convert.ToInt32(reader.GetValue(3))
				});
		}

		return new UResponse<List<DbAdminTableResponse>?>(tables);
	}

	public async Task<UResponse<DbAdminTableSchemaResponse?>> Schema(DbAdminTableSchemaParams p, CancellationToken ct) {
		UResponse<DbAdminTableSchemaResponse?>? guard = GuardAdmin<DbAdminTableSchemaResponse?>(p.Token);
		if (guard != null) return guard;

		NpgsqlConnection conn = await OpenConnection(ct);
		DbAdminTableSchemaResponse result = new() { Schema = p.Schema, Table = p.Table };

		List<string> pks = await PrimaryKeys(conn, p.Schema, p.Table, ct);
		result.PrimaryKeys = pks;

		const string colSql = """
			SELECT column_name, data_type, udt_name, is_nullable, column_default, ordinal_position
			FROM information_schema.columns
			WHERE table_schema = @schema AND table_name = @table
			ORDER BY ordinal_position;
			""";
		await using (NpgsqlCommand cmd = new(colSql, conn)) {
			cmd.Parameters.AddWithValue("schema", p.Schema);
			cmd.Parameters.AddWithValue("table", p.Table);
			await using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync(ct);
			while (await reader.ReadAsync(ct)) {
				string dataType = reader.GetString(1);
				string udt = reader.GetString(2);
				string name = reader.GetString(0);
				result.Columns.Add(new DbAdminColumnResponse {
					Name = name,
					DataType = dataType is "ARRAY" or "USER-DEFINED" ? udt : dataType,
					IsNullable = reader.GetString(3) == "YES",
					IsPrimaryKey = pks.Contains(name),
					Default = reader.IsDBNull(4) ? null : reader.GetString(4),
					OrdinalPosition = reader.GetInt32(5)
				});
			}
		}

		const string idxSql = "SELECT indexname, indexdef FROM pg_indexes WHERE schemaname = @schema AND tablename = @table ORDER BY indexname;";
		await using (NpgsqlCommand cmd = new(idxSql, conn)) {
			cmd.Parameters.AddWithValue("schema", p.Schema);
			cmd.Parameters.AddWithValue("table", p.Table);
			await using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync(ct);
			while (await reader.ReadAsync(ct)) {
				string def = reader.GetString(1);
				string name = reader.GetString(0);
				result.Indexes.Add(new DbAdminIndexResponse {
					Name = name,
					Definition = def,
					IsUnique = def.Contains("UNIQUE INDEX", StringComparison.OrdinalIgnoreCase),
					IsPrimary = name.EndsWith("_pkey", StringComparison.OrdinalIgnoreCase)
				});
			}
		}

		const string fkSql = """
			SELECT kcu.column_name, ccu.table_name AS ref_table, ccu.column_name AS ref_column, tc.constraint_name
			FROM information_schema.table_constraints tc
			JOIN information_schema.key_column_usage kcu ON kcu.constraint_name = tc.constraint_name AND kcu.table_schema = tc.table_schema
			JOIN information_schema.constraint_column_usage ccu ON ccu.constraint_name = tc.constraint_name AND ccu.table_schema = tc.table_schema
			WHERE tc.constraint_type = 'FOREIGN KEY' AND tc.table_schema = @schema AND tc.table_name = @table;
			""";
		await using (NpgsqlCommand cmd = new(fkSql, conn)) {
			cmd.Parameters.AddWithValue("schema", p.Schema);
			cmd.Parameters.AddWithValue("table", p.Table);
			await using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync(ct);
			while (await reader.ReadAsync(ct))
				result.ForeignKeys.Add(new DbAdminForeignKeyResponse {
					Column = reader.GetString(0),
					ReferencesTable = reader.GetString(1),
					ReferencesColumn = reader.GetString(2),
					ConstraintName = reader.GetString(3)
				});
		}

		return new UResponse<DbAdminTableSchemaResponse?>(result);
	}

	public async Task<UResponse<DbAdminQueryResultResponse?>> Rows(DbAdminRowsParams p, CancellationToken ct) {
		UResponse<DbAdminQueryResultResponse?>? guard = GuardAdmin<DbAdminQueryResultResponse?>(p.Token);
		if (guard != null) return guard;

		NpgsqlConnection conn = await OpenConnection(ct);
		int pageSize = p.PageSize < 1 ? 100 : p.PageSize;
		int pageNumber = p.PageNumber < 1 ? 1 : p.PageNumber;
		string relation = $"{Quote(p.Schema)}.{Quote(p.Table)}";

		string orderBy = "";
		if (!string.IsNullOrWhiteSpace(p.OrderByColumn)) {
			HashSet<string> columns = await ColumnNames(conn, p.Schema, p.Table, ct);
			if (columns.Contains(p.OrderByColumn)) orderBy = $" ORDER BY {Quote(p.OrderByColumn)} {(p.Descending ? "DESC" : "ASC")}";
		}

		long total;
		await using (NpgsqlCommand countCmd = new($"SELECT count(*) FROM {relation};", conn))
			total = Convert.ToInt64(await countCmd.ExecuteScalarAsync(ct) ?? 0L);

		string sql = $"SELECT * FROM {relation}{orderBy} LIMIT @limit OFFSET @offset;";
		Stopwatch sw = Stopwatch.StartNew();
		DbAdminQueryResultResponse result;
		await using (NpgsqlCommand cmd = new(sql, conn)) {
			cmd.Parameters.AddWithValue("limit", pageSize);
			cmd.Parameters.AddWithValue("offset", (pageNumber - 1) * pageSize);
			await using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync(ct);
			result = await ReadReader(reader, pageSize, ct);
		}
		sw.Stop();
		result.ExecutionMs = sw.ElapsedMilliseconds;
		List<string> pks = await PrimaryKeys(conn, p.Schema, p.Table, ct);
		result.PrimaryKeyColumn = pks.FirstOrDefault();

		return new UResponse<DbAdminQueryResultResponse?>(result) {
			TotalCount = (int)total,
			PageSize = pageSize,
			PageCount = (int)Math.Ceiling(total / (decimal)pageSize)
		};
	}

	public async Task<UResponse<DbAdminQueryResultResponse?>> Query(DbAdminQueryParams p, CancellationToken ct) {
		UResponse<DbAdminQueryResultResponse?>? guard = GuardAdmin<DbAdminQueryResultResponse?>(p.Token);
		if (guard != null) return guard;

		int maxRows = p.MaxRows < 1 ? 500 : p.MaxRows;
		NpgsqlConnection conn = await OpenConnection(ct);
		Stopwatch sw = Stopwatch.StartNew();
		try {
			await using NpgsqlCommand cmd = new(p.Sql, conn);
			await using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync(ct);
			DbAdminQueryResultResponse result;
			if (reader.FieldCount > 0) {
				result = await ReadReader(reader, maxRows, ct);
			}
			else {
				result = new DbAdminQueryResultResponse { AffectedRows = reader.RecordsAffected };
			}

			sw.Stop();
			result.ExecutionMs = sw.ElapsedMilliseconds;
			return new UResponse<DbAdminQueryResultResponse?>(result);
		}
		catch (PostgresException e) {
			sw.Stop();
			return new UResponse<DbAdminQueryResultResponse?>(null, Usc.BadRequest, e.MessageText);
		}
		catch (Exception e) {
			sw.Stop();
			return new UResponse<DbAdminQueryResultResponse?>(null, Usc.BadRequest, e.Message);
		}
	}

	public async Task<UResponse<DbAdminQueryResultResponse?>> UpdateRow(DbAdminUpdateRowParams p, CancellationToken ct) {
		UResponse<DbAdminQueryResultResponse?>? guard = GuardAdmin<DbAdminQueryResultResponse?>(p.Token);
		if (guard != null) return guard;

		NpgsqlConnection conn = await OpenConnection(ct);
		Dictionary<string, string> udt = await ColumnUdtMap(conn, p.Schema, p.Table, ct);
		List<string> assignments = [];
		await using NpgsqlCommand cmd = new() { Connection = conn };
		int i = 0;
		foreach ((string column, JsonElement value) in p.Values) {
			if (!udt.TryGetValue(column, out string? type)) continue;
			(bool isNull, string? text) = JsonToText(value);
			if (isNull) {
				assignments.Add($"{Quote(column)} = NULL");
				continue;
			}

			string param = $"p{i++}";
			assignments.Add($"{Quote(column)} = @{param}::{Quote(type)}");
			cmd.Parameters.AddWithValue(param, text ?? (object)DBNull.Value);
		}

		if (assignments.Count == 0) return new UResponse<DbAdminQueryResultResponse?>(null, Usc.BadRequest, ls.Get("NoValuesToUpdate"));

		cmd.Parameters.AddWithValue("pk", p.PrimaryKeyValue);
		cmd.CommandText = $"UPDATE {Quote(p.Schema)}.{Quote(p.Table)} SET {string.Join(", ", assignments)} WHERE {Quote(p.PrimaryKeyColumn)}::text = @pk RETURNING *;";

		Stopwatch sw = Stopwatch.StartNew();
		try {
			await using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync(ct);
			DbAdminQueryResultResponse result = await ReadReader(reader, 1, ct);
			sw.Stop();
			result.ExecutionMs = sw.ElapsedMilliseconds;
			result.AffectedRows = result.RowCount;
			return new UResponse<DbAdminQueryResultResponse?>(result, Usc.Success, ls.Get("Updated"));
		}
		catch (PostgresException e) {
			return new UResponse<DbAdminQueryResultResponse?>(null, Usc.BadRequest, e.MessageText);
		}
	}

	public async Task<UResponse<DbAdminQueryResultResponse?>> InsertRow(DbAdminInsertRowParams p, CancellationToken ct) {
		UResponse<DbAdminQueryResultResponse?>? guard = GuardAdmin<DbAdminQueryResultResponse?>(p.Token);
		if (guard != null) return guard;

		NpgsqlConnection conn = await OpenConnection(ct);
		Dictionary<string, string> udt = await ColumnUdtMap(conn, p.Schema, p.Table, ct);
		List<string> columns = [];
		List<string> valuesSql = [];
		await using NpgsqlCommand cmd = new() { Connection = conn };
		int i = 0;
		foreach ((string column, JsonElement value) in p.Values) {
			if (!udt.TryGetValue(column, out string? type)) continue;
			(bool isNull, string? text) = JsonToText(value);
			columns.Add(Quote(column));
			if (isNull) {
				valuesSql.Add("NULL");
				continue;
			}

			string param = $"p{i++}";
			valuesSql.Add($"@{param}::{Quote(type)}");
			cmd.Parameters.AddWithValue(param, text ?? (object)DBNull.Value);
		}

		if (columns.Count == 0) return new UResponse<DbAdminQueryResultResponse?>(null, Usc.BadRequest, ls.Get("NoValuesToUpdate"));

		cmd.CommandText = $"INSERT INTO {Quote(p.Schema)}.{Quote(p.Table)} ({string.Join(", ", columns)}) VALUES ({string.Join(", ", valuesSql)}) RETURNING *;";
		try {
			await using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync(ct);
			DbAdminQueryResultResponse result = await ReadReader(reader, 1, ct);
			result.AffectedRows = result.RowCount;
			return new UResponse<DbAdminQueryResultResponse?>(result, Usc.Created, ls.Get("Created"));
		}
		catch (PostgresException e) {
			return new UResponse<DbAdminQueryResultResponse?>(null, Usc.BadRequest, e.MessageText);
		}
	}

	public async Task<UResponse> DeleteRow(DbAdminDeleteRowParams p, CancellationToken ct) {
		UResponse? guard = GuardAdmin(p.Token);
		if (guard != null) return guard;

		NpgsqlConnection conn = await OpenConnection(ct);
		await using NpgsqlCommand cmd = new($"DELETE FROM {Quote(p.Schema)}.{Quote(p.Table)} WHERE {Quote(p.PrimaryKeyColumn)}::text = @pk;", conn);
		cmd.Parameters.AddWithValue("pk", p.PrimaryKeyValue);
		try {
			int affected = await cmd.ExecuteNonQueryAsync(ct);
			return new UResponse(affected > 0 ? Usc.Success : Usc.NotFound, ls.Get(affected > 0 ? "Deleted" : "NotFound"));
		}
		catch (PostgresException e) {
			return new UResponse(Usc.BadRequest, e.MessageText);
		}
	}

	// ===== Helpers =====

	private async Task<NpgsqlConnection> OpenConnection(CancellationToken ct) {
		NpgsqlConnection conn = (NpgsqlConnection)db.Database.GetDbConnection();
		if (conn.State != System.Data.ConnectionState.Open) await conn.OpenAsync(ct);
		return conn;
	}

	private static async Task<DbAdminQueryResultResponse> ReadReader(NpgsqlDataReader reader, int maxRows, CancellationToken ct) {
		DbAdminQueryResultResponse result = new();
		if (reader.FieldCount == 0) return result;

		for (int i = 0; i < reader.FieldCount; i++) {
			result.Columns.Add(reader.GetName(i));
			result.ColumnTypes.Add(reader.GetDataTypeName(i));
		}

		while (await reader.ReadAsync(ct)) {
			if (result.Rows.Count >= maxRows) {
				result.Truncated = true;
				break;
			}

			Dictionary<string, string?> row = new();
			for (int i = 0; i < reader.FieldCount; i++)
				row[result.Columns[i]] = Normalize(reader.IsDBNull(i) ? null : reader.GetValue(i));
			result.Rows.Add(row);
		}

		result.RowCount = result.Rows.Count;
		return result;
	}

	private static async Task<List<string>> PrimaryKeys(NpgsqlConnection conn, string schema, string table, CancellationToken ct) {
		const string sql = """
			SELECT a.attname
			FROM pg_index i
			JOIN pg_attribute a ON a.attrelid = i.indrelid AND a.attnum = ANY (i.indkey)
			WHERE i.indrelid = format('%I.%I', @schema, @table)::regclass AND i.indisprimary
			ORDER BY array_position(i.indkey, a.attnum);
			""";
		List<string> pks = [];
		await using NpgsqlCommand cmd = new(sql, conn);
		cmd.Parameters.AddWithValue("schema", schema);
		cmd.Parameters.AddWithValue("table", table);
		await using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync(ct);
		while (await reader.ReadAsync(ct)) pks.Add(reader.GetString(0));
		return pks;
	}

	private static async Task<HashSet<string>> ColumnNames(NpgsqlConnection conn, string schema, string table, CancellationToken ct) {
		HashSet<string> set = new(StringComparer.Ordinal);
		await using NpgsqlCommand cmd = new("SELECT column_name FROM information_schema.columns WHERE table_schema = @schema AND table_name = @table;", conn);
		cmd.Parameters.AddWithValue("schema", schema);
		cmd.Parameters.AddWithValue("table", table);
		await using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync(ct);
		while (await reader.ReadAsync(ct)) set.Add(reader.GetString(0));
		return set;
	}

	private static async Task<Dictionary<string, string>> ColumnUdtMap(NpgsqlConnection conn, string schema, string table, CancellationToken ct) {
		Dictionary<string, string> map = new(StringComparer.Ordinal);
		await using NpgsqlCommand cmd = new("SELECT column_name, udt_name FROM information_schema.columns WHERE table_schema = @schema AND table_name = @table;", conn);
		cmd.Parameters.AddWithValue("schema", schema);
		cmd.Parameters.AddWithValue("table", table);
		await using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync(ct);
		while (await reader.ReadAsync(ct)) map[reader.GetString(0)] = reader.GetString(1);
		return map;
	}

	private static (bool isNull, string? text) JsonToText(JsonElement e) => e.ValueKind switch {
		JsonValueKind.Null or JsonValueKind.Undefined => (true, null),
		JsonValueKind.String => (false, e.GetString()),
		JsonValueKind.Number => (false, e.GetRawText()),
		JsonValueKind.True => (false, "true"),
		JsonValueKind.False => (false, "false"),
		_ => (false, e.GetRawText())
	};

	private static string? Normalize(object? v) {
		switch (v) {
			case null or DBNull: return null;
			case string s: return s;
			case bool b: return b ? "true" : "false";
			case DateTime dt: return dt.ToString("o", CultureInfo.InvariantCulture);
			case DateTimeOffset dto: return dto.ToString("o", CultureInfo.InvariantCulture);
			case Guid g: return g.ToString();
			case byte[] bytes: return "\\x" + Convert.ToHexString(bytes);
			case IEnumerable when v is not string: return JsonSerializer.Serialize(v);
			default: return Convert.ToString(v, CultureInfo.InvariantCulture);
		}
	}

	private static string Quote(string identifier) => $"\"{identifier.Replace("\"", "\"\"")}\"";

	private UResponse? GuardAdmin(string? token) {
		JwtClaimData? userData = ts.ExtractClaims(token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("AuthorizationRequired"));
		if (!userData.Tags.Contains(TagUser.SystemAdmin)) return new UResponse(Usc.Forbidden, ls.Get("YouDoNotHaveClearanceToDoThisAction"));
		return null;
	}

	private UResponse<T>? GuardAdmin<T>(string? token) {
		JwtClaimData? userData = ts.ExtractClaims(token);
		if (userData == null) return new UResponse<T>(default!, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));
		if (userData.IsExpired) return new UResponse<T>(default!, Usc.ExpiredToken, ls.Get("TokenExpired"));
		if (!userData.Tags.Contains(TagUser.SystemAdmin)) return new UResponse<T>(default!, Usc.Forbidden, ls.Get("YouDoNotHaveClearanceToDoThisAction"));
		return null;
	}
}
