using BillByte.Model;
using Billbyte_BE.Repositories.Interface;
using Npgsql;

namespace Billbyte_BE.Repositories
{
    public class KotRepository : IKotRepository
    {
        private readonly string _conn;

        public KotRepository(IConfiguration cfg)
        {
            _conn = cfg.GetConnectionString("DBConn");
        }

        // =========================
        // CREATE KOT
        // =========================
        public async Task<KotSnapshot> CreateKotAsync(KotSnapshot kot)
        {
            using var con = new NpgsqlConnection(_conn);
            await con.OpenAsync();

            using var tx = await con.BeginTransactionAsync();

            try
            {
                kot.BusinessDate = DateTime.UtcNow.Date;

                var (kotNo, kotNumber) =
                    await GenerateKotNumberAsync(con, kot.RestaurantId, kot.BusinessDate, tx);

                kot.KotNo = kotNo;
                kot.KotNumber = kotNumber;

                using var cmd = new NpgsqlCommand(@"
            INSERT INTO ""KotSnapshots""
            (""RestaurantId"", ""TableId"", ""BusinessDate"",
             ""KotNo"", ""KotNumber"", ""CreatedAt"")
            VALUES
            (@rid, @table, @date, @kotNo, @kotNumber, NOW())
            RETURNING ""Id"", ""CreatedAt"";
        ", con, tx);

                cmd.Parameters.AddWithValue("@rid", kot.RestaurantId);
                cmd.Parameters.AddWithValue("@table", kot.TableId);
                cmd.Parameters.AddWithValue("@date", kot.BusinessDate);
                cmd.Parameters.AddWithValue("@kotNo", kot.KotNo);
                cmd.Parameters.AddWithValue("@kotNumber", kot.KotNumber);

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    kot.Id = reader.GetInt32(0);
                    kot.CreatedAt = reader.GetDateTime(1); // ✅ FIXED
                }
                reader.Close();

                foreach (var item in kot.Items)
                {
                    using var itemCmd = new NpgsqlCommand(@"
                INSERT INTO ""KotSnapshotItems""
                (""KotSnapshotId"", ""ItemName"", ""Qty"", ""SpecialNote"")
                VALUES (@kid, @name, @qty, @note);
            ", con, tx);

                    itemCmd.Parameters.AddWithValue("@kid", kot.Id);
                    itemCmd.Parameters.AddWithValue("@name", item.ItemName);
                    itemCmd.Parameters.AddWithValue("@qty", item.Qty);
                    itemCmd.Parameters.AddWithValue("@note", item.SpecialNote ?? "");

                    await itemCmd.ExecuteNonQueryAsync();
                }

                await tx.CommitAsync();
                return kot; // ✅ REQUIRED
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }


        // =========================
        // KOT NUMBER GENERATION
        // =========================
        private async Task<(int kotNo, string kotNumber)> GenerateKotNumberAsync(
           NpgsqlConnection con,
           int restaurantId,
           DateTime businessDate,
           NpgsqlTransaction tx)
        {
            using var cmd = new NpgsqlCommand(@"
        SELECT COALESCE(MAX(""KotNo""), 0)
        FROM ""KotSnapshots""
        WHERE ""RestaurantId"" = @rid
          AND ""BusinessDate"" = @date
    ", con, tx);

            cmd.Parameters.AddWithValue("@rid", restaurantId);
            cmd.Parameters.AddWithValue("@date", businessDate);

            var lastNo = Convert.ToInt32(await cmd.ExecuteScalarAsync());
            var nextNo = lastNo + 1;

            var kotNumber =
                $"KOT-{restaurantId}-{businessDate:yyyy-MM-dd}-{nextNo:D3}";

            return (nextNo, kotNumber);
        }


        // =========================
        // GET TODAY KOTS
        // =========================
        public async Task<List<KotSnapshot>> GetTodayKotsAsync(int restaurantId)
        {
            var list = new List<KotSnapshot>();
            var today = DateTime.UtcNow.Date;

            using var con = new NpgsqlConnection(_conn);
            await con.OpenAsync();

            using var cmd = new NpgsqlCommand(@"
                SELECT *
                FROM ""KotSnapshots""
                WHERE ""RestaurantId"" = @rid
                  AND ""BusinessDate"" = @date
                ORDER BY ""KotNo"";
            ", con);

            cmd.Parameters.AddWithValue("@rid", restaurantId);
            cmd.Parameters.AddWithValue("@date", today);

            using var dr = await cmd.ExecuteReaderAsync();
            while (await dr.ReadAsync())
            {
                list.Add(new KotSnapshot
                {
                    Id = dr.GetInt32(dr.GetOrdinal("Id")),
                    KotNo = dr.GetInt32(dr.GetOrdinal("KotNo")),
                    KotNumber = dr["KotNumber"].ToString(),
                    TableId = dr["TableId"].ToString(),
                    CreatedAt = dr.GetDateTime(dr.GetOrdinal("CreatedAt"))
                });
            }

            return list;
        }
    }
}
