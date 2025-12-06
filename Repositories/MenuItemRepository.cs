using BillByte.Interface;
using BillByte.Model;
using Npgsql;

namespace BillByte.Repository
{
    public class MenuItemRepository : IMenuItemRepository
    {
        private readonly string _conn;

        public MenuItemRepository(IConfiguration cfg)
        {
            _conn = cfg.GetConnectionString("DBConn");
        }

        public async Task<List<MenuItem>> GetAllAsync()
        {
            var list = new List<MenuItem>();

            using var con = new NpgsqlConnection(_conn);
            using var cmd = new NpgsqlCommand(@"SELECT * FROM ""MenuItems"" ORDER BY ""ItemName""", con);

            await con.OpenAsync();
            using var dr = await cmd.ExecuteReaderAsync();
            while (await dr.ReadAsync())
            {
                list.Add(Map(dr));
            }

            return list;
        }

        public async Task<MenuItem?> GetByIdAsync(int id)
        {
            using var con = new NpgsqlConnection(_conn);
            using var cmd = new NpgsqlCommand(@"SELECT * FROM ""MenuItems"" WHERE ""ItemId""=@id", con);

            cmd.Parameters.AddWithValue("@id", id);

            await con.OpenAsync();
            using var dr = await cmd.ExecuteReaderAsync();

            return await dr.ReadAsync() ? Map(dr) : null;
        }

        public async Task<MenuItem> AddAsync(MenuItem item)
        {
            using var con = new NpgsqlConnection(_conn);
            using var cmd = new NpgsqlCommand(@"
                INSERT INTO ""MenuItems"" 
                (""ItemName"", ""FoodTypeId"", ""ItemCost"",
                 ""GSTPercentage"", ""CGSTPercentage"", ""ImageUrl"",
                 ""CreatedBy"", ""CreatedDate"")
                VALUES 
                (@name, @type, @cost, @gst, @cgst, @img, @createdBy, @date)
                RETURNING ""ItemId"";", con);

            cmd.Parameters.AddWithValue("@name", item.ItemName);
            cmd.Parameters.AddWithValue("@type", item.FoodTypeId);
            cmd.Parameters.AddWithValue("@cost", item.ItemCost);
            cmd.Parameters.AddWithValue("@gst", (object?)item.GSTPercentage ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@cgst", (object?)item.CGSTPercentage ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@img", (object?)item.ImageUrl ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@createdBy", item.CreatedBy ?? "System");
            cmd.Parameters.AddWithValue("@date", DateTime.UtcNow);

            await con.OpenAsync();
            item.ItemId = Convert.ToInt32(await cmd.ExecuteScalarAsync());

            return item;
        }

        public async Task<MenuItem> UpdateAsync(MenuItem item)
        {
            using var con = new NpgsqlConnection(_conn);

            using var cmd = new NpgsqlCommand(@"
                UPDATE ""MenuItems""
                SET ""ItemName""=@name, ""FoodTypeId""=@type, ""ItemCost""=@cost,
                    ""GSTPercentage""=@gst, ""CGSTPercentage""=@cgst, ""ImageUrl""=@img
                WHERE ""ItemId""=@id", con);

            cmd.Parameters.AddWithValue("@id", item.ItemId);
            cmd.Parameters.AddWithValue("@name", item.ItemName);
            cmd.Parameters.AddWithValue("@type", item.FoodTypeId);
            cmd.Parameters.AddWithValue("@cost", item.ItemCost);
            cmd.Parameters.AddWithValue("@gst", (object?)item.GSTPercentage ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@cgst", (object?)item.CGSTPercentage ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@img", (object?)item.ImageUrl ?? DBNull.Value);

            await con.OpenAsync();
            await cmd.ExecuteNonQueryAsync();

            return item;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var con = new NpgsqlConnection(_conn);
            using var cmd = new NpgsqlCommand(@"DELETE FROM ""MenuItems"" WHERE ""ItemId""=@id", con);

            cmd.Parameters.AddWithValue("@id", id);

            await con.OpenAsync();
            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        public async Task<List<MenuItem>> GetByFoodTypeAsync(int typeId)
        {
            var list = new List<MenuItem>();

            using var con = new NpgsqlConnection(_conn);
            using var cmd = new NpgsqlCommand(@"SELECT * FROM ""MenuItems"" WHERE ""FoodTypeId""=@t", con);

            cmd.Parameters.AddWithValue("@t", typeId);

            await con.OpenAsync();
            using var dr = await cmd.ExecuteReaderAsync();
            while (await dr.ReadAsync())
            {
                list.Add(Map(dr));
            }

            return list;
        }

        // Helper mapper
        private MenuItem Map(NpgsqlDataReader dr)
        {
            return new MenuItem
            {
                ItemId = dr.GetInt32(dr.GetOrdinal("ItemId")),
                ItemName = dr["ItemName"].ToString(),
                FoodTypeId = dr.GetInt32(dr.GetOrdinal("FoodTypeId")),
                ItemCost = dr.GetDecimal(dr.GetOrdinal("ItemCost")),
                GSTPercentage = dr["GSTPercentage"] == DBNull.Value ? null : dr.GetDecimal(dr.GetOrdinal("GSTPercentage")),
                CGSTPercentage = dr["CGSTPercentage"] == DBNull.Value ? null : dr.GetDecimal(dr.GetOrdinal("CGSTPercentage")),
                ImageUrl = dr["ImageUrl"]?.ToString(),
                CreatedBy = dr["CreatedBy"]?.ToString(),
                CreatedDate = dr.GetDateTime(dr.GetOrdinal("CreatedDate"))
            };
        }
    }
}
