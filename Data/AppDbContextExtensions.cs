using Microsoft.EntityFrameworkCore;
using System.Data.Common;

namespace Billbyte_BE.Data
{
    public static class AppDbContextExtensions
    {
        public static async Task<List<Dictionary<string, object>>> ExecuteSqlAsync(
            this AppDbContext db, string sql)
        {
            using var cmd = db.Database.GetDbConnection().CreateCommand();
            cmd.CommandText = sql;

            if (cmd.Connection!.State != System.Data.ConnectionState.Open)
                await cmd.Connection.OpenAsync();

            using var reader = await cmd.ExecuteReaderAsync();
            var result = new List<Dictionary<string, object>>();

            while (await reader.ReadAsync())
            {
                var row = new Dictionary<string, object>();

                for (int i = 0; i < reader.FieldCount; i++)
                {
                    row[reader.GetName(i)] = reader.GetValue(i);
                }

                result.Add(row);
            }

            return result;
        }
    }
}
