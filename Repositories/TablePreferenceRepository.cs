using BillByte.Interface;
using BillByte.Hubs;
using Billbyte_BE.Models;
using Microsoft.AspNetCore.SignalR;
using Npgsql;

namespace BillByte.Repository
{
    public class TablePreferenceRepository : ITablePreferenceRepository
    {
        private readonly string _conn;
        private readonly IHubContext<PosHub> _hub;

        public TablePreferenceRepository(IConfiguration cfg, IHubContext<PosHub> hub)
        {
            _conn = cfg.GetConnectionString("DBConn");
            _hub = hub;
        }

        public async Task<List<TablePreference>> GetAllAsync(int restaurantId)
        {
            var list = new List<TablePreference>();

            using var con = new NpgsqlConnection(_conn);
            using var cmd = new NpgsqlCommand(
                @"SELECT * FROM ""TablePreferences""
                  WHERE ""RestaurantId""=@rid
                  ORDER BY ""Name""", con);

            cmd.Parameters.AddWithValue("@rid", restaurantId);

            await con.OpenAsync();
            using var dr = await cmd.ExecuteReaderAsync();

            while (await dr.ReadAsync())
                list.Add(Map(dr));

            return list;
        }

        public async Task<TablePreference?> GetByIdAsync(int id, int restaurantId)
        {
            using var con = new NpgsqlConnection(_conn);
            using var cmd = new NpgsqlCommand(
                @"SELECT * FROM ""TablePreferences""
                  WHERE ""Id""=@id AND ""RestaurantId""=@rid", con);

            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@rid", restaurantId);

            await con.OpenAsync();
            using var dr = await cmd.ExecuteReaderAsync();

            return await dr.ReadAsync() ? Map(dr) : null;
        }

        public async Task AddRangeAsync(List<TablePreference> items)
        {
            using var con = new NpgsqlConnection(_conn);
            await con.OpenAsync();

            foreach (var item in items)
            {
                using var cmd = new NpgsqlCommand(@"
                    INSERT INTO ""TablePreferences""
                    (""RestaurantId"", ""Name"", ""TableCount"")
                    VALUES (@rid, @name, @count)", con);

                cmd.Parameters.AddWithValue("@rid", item.RestaurantId);
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
                WHERE ""Id""=@id AND ""RestaurantId""=@rid", con);

            cmd.Parameters.AddWithValue("@id", item.Id);
            cmd.Parameters.AddWithValue("@rid", item.RestaurantId);
            cmd.Parameters.AddWithValue("@name", item.Name);
            cmd.Parameters.AddWithValue("@count", item.TableCount);

            await con.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<bool> DeleteAsync(int id, int restaurantId)
        {
            using var con = new NpgsqlConnection(_conn);
            using var cmd = new NpgsqlCommand(
                @"DELETE FROM ""TablePreferences""
                  WHERE ""Id""=@id AND ""RestaurantId""=@rid", con);

            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@rid", restaurantId);

            await con.OpenAsync();
            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        public async Task<bool> DeleteAllAsync(int restaurantId)
        {
            using var con = new NpgsqlConnection(_conn);
            using var cmd = new NpgsqlCommand(
                @"DELETE FROM ""TablePreferences""
                  WHERE ""RestaurantId""=@rid", con);

            cmd.Parameters.AddWithValue("@rid", restaurantId);

            await con.OpenAsync();
            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        private TablePreference Map(NpgsqlDataReader dr)
        {
            return new TablePreference
            {
                Id = Convert.ToInt32(dr["Id"]),
                RestaurantId = Convert.ToInt32(dr["RestaurantId"]),
                Name = dr["Name"].ToString()!,
                TableCount = Convert.ToInt32(dr["TableCount"])
            };
        }

        public async Task NotifyAssignmentChangedAsync(int restaurantId, int userId, int tablePreferenceId)
        {
            // Frontend dashboard (which uses table-preferences) should listen to this event
            var group = restaurantId.ToString();
            var payload = new
            {
                userId,
                tablePreferenceId
            };

            await _hub.Clients.Group(group)
                .SendAsync("ASSIGNED_TABLES_CHANGED", payload);

            Console.WriteLine($"SignalR (TablePreferenceRepository): Sent ASSIGNED_TABLES_CHANGED to group {group} -> user {userId} / section {tablePreferenceId}");
        }
    }
}
