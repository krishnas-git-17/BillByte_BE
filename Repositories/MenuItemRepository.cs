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

        // ---------------------- GET ALL ----------------------
        public async Task<List<MenuItem>> GetAllAsync()
        {
            var list = new List<MenuItem>();

            using var con = new NpgsqlConnection(_conn);
            using var cmd = new NpgsqlCommand(@"SELECT * FROM ""MenuItems"" ORDER BY ""Name""", con);

            await con.OpenAsync();
            using var dr = await cmd.ExecuteReaderAsync();

            while (await dr.ReadAsync())
                list.Add(Map(dr));

            return list;
        }

        // ---------------------- GET BY ID ----------------------
        public async Task<MenuItem?> GetByIdAsync(string id)
        {
            using var con = new NpgsqlConnection(_conn);
            using var cmd = new NpgsqlCommand(@"SELECT * FROM ""MenuItems"" WHERE ""MenuId""=@id", con);

            cmd.Parameters.AddWithValue("@id", id);

            await con.OpenAsync();
            using var dr = await cmd.ExecuteReaderAsync();

            return await dr.ReadAsync() ? Map(dr) : null;
        }

        // ---------------------- ADD ----------------------
        public async Task<MenuItem> AddAsync(MenuItem item)
        {
            using var con = new NpgsqlConnection(_conn);

            using var cmd = new NpgsqlCommand(@"
                INSERT INTO ""MenuItems""
                (""MenuId"", ""Name"", ""Type"", ""VegType"", ""Status"", ""Price"", ""ImageUrl"", ""CreatedBy"", ""CreatedDate"")
                VALUES
                (@id, @name, @type, @veg, @status, @price, @img, @createdBy, @date);
            ", con);

            // values
            cmd.Parameters.AddWithValue("@id", item.MenuId);
            cmd.Parameters.AddWithValue("@name", item.Name);
            cmd.Parameters.AddWithValue("@type", item.Type);
            cmd.Parameters.AddWithValue("@veg", item.VegType);
            cmd.Parameters.AddWithValue("@status", item.Status);
            cmd.Parameters.AddWithValue("@price", item.Price);
            cmd.Parameters.AddWithValue("@img", (object?)item.ImageUrl ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@createdBy", item.CreatedBy ?? "System");
            cmd.Parameters.AddWithValue("@date", item.CreatedDate);

            await con.OpenAsync();
            await cmd.ExecuteNonQueryAsync();

            return item;
        }

        // ---------------------- UPDATE ----------------------
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
                WHERE ""MenuId""=@id
            ", con);

            cmd.Parameters.AddWithValue("@id", item.MenuId);
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

        // ---------------------- DELETE ----------------------
        public async Task<bool> DeleteAsync(string id)
        {
            using var con = new NpgsqlConnection(_conn);
            using var cmd = new NpgsqlCommand(@"DELETE FROM ""MenuItems"" WHERE ""MenuId""=@id", con);

            cmd.Parameters.AddWithValue("@id", id);

            await con.OpenAsync();
            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        // ---------------------- MAPPER ----------------------
        private MenuItem Map(NpgsqlDataReader dr)
        {
            return new MenuItem
            {
                MenuId = dr["MenuId"].ToString(),
                Name = dr["Name"].ToString(),
                Type = dr["Type"].ToString(),
                VegType = dr["VegType"].ToString(),
                Status = dr["Status"].ToString(),
                Price = dr.GetDecimal(dr.GetOrdinal("Price")),
                ImageUrl = dr["ImageUrl"]?.ToString(),
                CreatedBy = dr["CreatedBy"]?.ToString(),
                CreatedDate = dr.GetDateTime(dr.GetOrdinal("CreatedDate"))
            };
        }
    }
}
