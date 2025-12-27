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

        public async Task<List<MenuItem>> GetAllAsync(int restaurantId)
        {
            var list = new List<MenuItem>();

            using var con = new NpgsqlConnection(_conn);
            using var cmd = new NpgsqlCommand(@"
                SELECT * FROM ""MenuItems""
                WHERE ""RestaurantId""=@rid
                ORDER BY ""Name""", con);

            cmd.Parameters.AddWithValue("@rid", restaurantId);

            await con.OpenAsync();
            using var dr = await cmd.ExecuteReaderAsync();

            while (await dr.ReadAsync())
                list.Add(Map(dr));

            return list;
        }

        public async Task<MenuItem?> GetByIdAsync(string id, int restaurantId)
        {
            using var con = new NpgsqlConnection(_conn);
            using var cmd = new NpgsqlCommand(@"
                SELECT * FROM ""MenuItems""
                WHERE ""MenuId""=@id AND ""RestaurantId""=@rid", con);

            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@rid", restaurantId);

            await con.OpenAsync();
            using var dr = await cmd.ExecuteReaderAsync();

            return await dr.ReadAsync() ? Map(dr) : null;
        }

        public async Task<MenuItem> AddAsync(MenuItem item)
        {
            using var con = new NpgsqlConnection(_conn);
            using var cmd = new NpgsqlCommand(@"
                INSERT INTO ""MenuItems""
                (""MenuId"", ""RestaurantId"", ""Name"", ""Type"", ""VegType"",
                 ""Status"", ""Price"", ""ImageUrl"", ""CreatedBy"", ""CreatedDate"")
                VALUES
                (@id, @rid, @name, @type, @veg, @status, @price, @img, @createdBy, @date)", con);

            cmd.Parameters.AddWithValue("@id", item.MenuId);
            cmd.Parameters.AddWithValue("@rid", item.RestaurantId);
            cmd.Parameters.AddWithValue("@name", item.Name);
            cmd.Parameters.AddWithValue("@type", item.Type);
            cmd.Parameters.AddWithValue("@veg", item.VegType);
            cmd.Parameters.AddWithValue("@status", item.Status);
            cmd.Parameters.AddWithValue("@price", item.Price);
            cmd.Parameters.AddWithValue("@img", (object?)item.ImageUrl ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@createdBy", item.CreatedBy);
            cmd.Parameters.AddWithValue("@date", item.CreatedDate);

            await con.OpenAsync();
            await cmd.ExecuteNonQueryAsync();

            return item;
        }

        public async Task<MenuItem> UpdateAsync(MenuItem item)
        {
            using var con = new NpgsqlConnection(_conn);
            using var cmd = new NpgsqlCommand(@"
                UPDATE ""MenuItems""
                SET ""Name""=@name,
                    ""Type""=@type,
                    ""VegType""=@veg,
                    ""Status""=@status,
                    ""Price""=@price,
                    ""ImageUrl""=@img
                WHERE ""MenuId""=@id AND ""RestaurantId""=@rid", con);

            cmd.Parameters.AddWithValue("@id", item.MenuId);
            cmd.Parameters.AddWithValue("@rid", item.RestaurantId);
            cmd.Parameters.AddWithValue("@name", item.Name);
            cmd.Parameters.AddWithValue("@type", item.Type);
            cmd.Parameters.AddWithValue("@veg", item.VegType);
            cmd.Parameters.AddWithValue("@status", item.Status);
            cmd.Parameters.AddWithValue("@price", item.Price);
            cmd.Parameters.AddWithValue("@img", (object?)item.ImageUrl ?? DBNull.Value);

            await con.OpenAsync();
            await cmd.ExecuteNonQueryAsync();

            return item;
        }

        public async Task<bool> DeleteAsync(string id, int restaurantId)
        {
            using var con = new NpgsqlConnection(_conn);
            using var cmd = new NpgsqlCommand(@"
                DELETE FROM ""MenuItems""
                WHERE ""MenuId""=@id AND ""RestaurantId""=@rid", con);

            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@rid", restaurantId);

            await con.OpenAsync();
            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        private MenuItem Map(NpgsqlDataReader dr)
        {
            return new MenuItem
            {
                MenuId = dr["MenuId"].ToString()!,
                RestaurantId = Convert.ToInt32(dr["RestaurantId"]),
                Name = dr["Name"].ToString()!,
                Type = dr["Type"].ToString()!,
                VegType = dr["VegType"].ToString()!,
                Status = dr["Status"].ToString()!,
                Price = dr.GetDecimal(dr.GetOrdinal("Price")),
                ImageUrl = dr["ImageUrl"] == DBNull.Value ? null : dr["ImageUrl"].ToString(),
                CreatedBy = Convert.ToInt32(dr["CreatedBy"]),
                CreatedDate = dr.GetDateTime(dr.GetOrdinal("CreatedDate"))
            };
        }
    }
}
