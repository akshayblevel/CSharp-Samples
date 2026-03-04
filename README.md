 Task<(List<T1>, List<T2>)> ExecuteMultipleResultSetsAsync<T1, T2>(string storedProcedure, params SqlParameter[] parameters) where T1 : class, new() where T2 : class, new();
 
 public async Task<(List<T1>, List<T2>)> ExecuteMultipleResultSetsAsync<T1, T2>(string storedProcedure,params SqlParameter[] parameters)where T1 : class, new() where T2 : class, new()
    {
        using var connection = _dbContext.Database.GetDbConnection();
        using var command = connection.CreateCommand();
        command.CommandText = storedProcedure;
        command.CommandType = CommandType.StoredProcedure;

        if (parameters?.Length > 0)
            command.Parameters.AddRange(parameters);

        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync();

        using var reader = await command.ExecuteReaderAsync();

        var result1 = await MapResultSet<T1>(reader);

        await reader.NextResultAsync();

        var result2 = await MapResultSet<T2>(reader);

        return (result1, result2);
    }

    private async Task<List<T>> MapResultSet<T>(DbDataReader reader) where T : class, new()
    {
        var entities = new List<T>();
        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        var columnNames = Enumerable.Range(0, reader.FieldCount)
            .Select(reader.GetName)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        while (await reader.ReadAsync())
        {
            var entity = new T();
            foreach (var prop in properties)
            {
                if (!prop.CanWrite || !columnNames.Contains(prop.Name))
                    continue;

                var value = reader[prop.Name];
                if (value != DBNull.Value)
                    prop.SetValue(entity, value);
            }
            entities.Add(entity);
        }

        return entities;
}
