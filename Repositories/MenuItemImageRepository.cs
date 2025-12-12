using BillByte.Interface;
using BillByte.Model;
using Npgsql;

namespace BillByte.Repository
{
    public class MenuItemImageRepository : IMenuItemImagesRepository
    {
        private readonly string _conn;

        public MenuItemImageRepository(IConfiguration cfg)
        {
            _conn = cfg.GetConnectionString("DBConn");
        }

        public async Task<List<MenuItemImgs>> GetAllAsync()
        {
            var list = new List<MenuItemImgs>();

            using var con = new NpgsqlConnection(_conn);
            using var cmd = new NpgsqlCommand(@"SELECT * FROM ""MenuItemImgs"" ORDER BY ""Id""", con);

            await con.OpenAsync();
            using var dr = await cmd.ExecuteReaderAsync();

            while (await dr.ReadAsync())
                list.Add(Map(dr));

            return list;
        }

        public async Task<MenuItemImgs?> GetByIdAsync(int id)
        {
            using var con = new NpgsqlConnection(_conn);
            using var cmd = new NpgsqlCommand(@"SELECT * FROM ""MenuItemImgs"" WHERE ""Id""=@id", con);

            cmd.Parameters.AddWithValue("@id", id);

            await con.OpenAsync();
            using var dr = await cmd.ExecuteReaderAsync();

            return await dr.ReadAsync() ? Map(dr) : null;
        }

        public async Task<MenuItemImgs> AddAsync(MenuItemImgs img)
        {
            using var con = new NpgsqlConnection(_conn);
            using var cmd = new NpgsqlCommand(@"
        INSERT INTO ""MenuItemImgs""(""ItemName"", ""ItemImage"", ""CreatedDate"") 
        VALUES(@name, @img, @date)
        RETURNING ""Id"";
    ", con);

            cmd.Parameters.AddWithValue("@name", img.ItemName);
            cmd.Parameters.AddWithValue("@img", img.ItemImage);
            cmd.Parameters.AddWithValue("@date", img.CreatedDate.ToUniversalTime());

            await con.OpenAsync();
            img.Id = Convert.ToInt32(await cmd.ExecuteScalarAsync());

            return img;
        }


        public async Task<MenuItemImgs> UpdateAsync(MenuItemImgs img)
        {
            using var con = new NpgsqlConnection(_conn);
            using var cmd = new NpgsqlCommand(@"
        UPDATE ""MenuItemImgs""
        SET ""ItemName""=@name, ""ItemImage""=@img
        WHERE ""Id""=@id;
    ", con);

            cmd.Parameters.AddWithValue("@id", img.Id);
            cmd.Parameters.AddWithValue("@name", img.ItemName);
            cmd.Parameters.AddWithValue("@img", img.ItemImage);

            await con.OpenAsync();
            await cmd.ExecuteNonQueryAsync();

            return img;
        }


        public async Task<bool> DeleteAsync(int id)
        {
            using var con = new NpgsqlConnection(_conn);
            using var cmd = new NpgsqlCommand(@"DELETE FROM ""MenuItemImgs"" WHERE ""Id""=@id", con);

            cmd.Parameters.AddWithValue("@id", id);

            await con.OpenAsync();
            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        private MenuItemImgs Map(NpgsqlDataReader dr)
        {
            return new MenuItemImgs
            {
                Id = Convert.ToInt32(dr["Id"]),
                ItemName = dr["ItemName"].ToString() ?? "",
                ItemImage = dr["ItemImage"].ToString() ?? "",
                CreatedDate = dr.GetDateTime(dr.GetOrdinal("CreatedDate"))
            };
        }

    }
}
