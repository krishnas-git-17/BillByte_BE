using BillByte.Interface;
using BillByte.Model;
using Billbyte_BE.Models;
using Npgsql;

namespace BillByte.Repository
{
    public class TablePreferenceRepository : ITablePreferenceRepository
    {
        private readonly string _conn;

        public TablePreferenceRepository(IConfiguration cfg)
        {
            _conn = cfg.GetConnectionString("DBConn");
        }

        // ---------------------- GET ALL ----------------------
        public async Task<List<TablePreference>> GetAllAsync()
        {
            var list = new List<TablePreference>();

            using var con = new NpgsqlConnection(_conn);
            using var cmd = new NpgsqlCommand(
                @"SELECT * FROM ""TablePreferences"" ORDER BY ""Name""", con);

            await con.OpenAsync();
            using var dr = await cmd.ExecuteReaderAsync();

            while (await dr.ReadAsync())
                list.Add(Map(dr));

            return list;
        }

        // ---------------------- GET BY ID ----------------------
        public async Task<TablePreference?> GetByIdAsync(int id)
        {
            using var con = new NpgsqlConnection(_conn);
            using var cmd = new NpgsqlCommand(
                @"SELECT * FROM ""TablePreferences"" WHERE ""Id""=@id", con);

            cmd.Parameters.AddWithValue("@id", id);

            await con.OpenAsync();
            using var dr = await cmd.ExecuteReaderAsync();

            return await dr.ReadAsync() ? Map(dr) : null;
        }

        public async Task AddAsync(TablePreference item)
        {
            using var con = new NpgsqlConnection(_conn);
            using var cmd = new NpgsqlCommand(@"
                INSERT INTO ""TablePreferences"" (""Name"", ""TableCount"")
                VALUES (@name, @count)
            ", con);

            cmd.Parameters.AddWithValue("@name", item.Name);
            cmd.Parameters.AddWithValue("@count", item.TableCount);

            await con.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task AddRangeAsync(List<TablePreference> items)
        {
            using var con = new NpgsqlConnection(_conn);
            await con.OpenAsync();

            foreach (var item in items)
            {
                using var cmd = new NpgsqlCommand(@"
                    INSERT INTO ""TablePreferences"" (""Name"", ""TableCount"")
                    VALUES (@name, @count)
                ", con);

                cmd.Parameters.AddWithValue("@name", item.Name);
                cmd.Parameters.AddWithValue("@count", item.TableCount);

                await cmd.ExecuteNonQueryAsync();
            }
        }

        public async Task UpdateAsync(TablePreference item)
        {
            using var con = new NpgsqlConnection(_conn);
            using var cmd = new NpgsqlCommand(@"
                UPDATE ""TablePreferences""
                SET ""Name""=@name,
                    ""TableCount""=@count
                WHERE ""Id""=@id
            ", con);

            cmd.Parameters.AddWithValue("@id", item.Id);
            cmd.Parameters.AddWithValue("@name", item.Name);
            cmd.Parameters.AddWithValue("@count", item.TableCount);

            await con.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        // ---------------------- DELETE BY ID ----------------------
        public async Task<bool> DeleteAsync(int id)
        {
            using var con = new NpgsqlConnection(_conn);
            using var cmd = new NpgsqlCommand(
                @"DELETE FROM ""TablePreferences"" WHERE ""Id""=@id", con);

            cmd.Parameters.AddWithValue("@id", id);

            await con.OpenAsync();
            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        // ---------------------- DELETE ALL ----------------------
        public async Task<bool> DeleteAllAsync()
        {
            using var con = new NpgsqlConnection(_conn);
            using var cmd = new NpgsqlCommand(
                @"DELETE FROM ""TablePreferences""", con);

            await con.OpenAsync();
            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        private TablePreference Map(NpgsqlDataReader dr)
        {
            return new TablePreference
            {
                Id = Convert.ToInt32(dr["Id"]),
                Name = dr["Name"].ToString(),
                TableCount = Convert.ToInt32(dr["TableCount"])
            };
        }
    }
}
