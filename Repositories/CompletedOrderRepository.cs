using BillByte.Model;
using Billbyte_BE.Repositories.Interface;
using Npgsql;

namespace Billbyte_BE.Repositories
{
    public class CompletedOrderRepository : ICompletedOrderRepository
    {
        private readonly string _conn;

        public CompletedOrderRepository(IConfiguration cfg)
        {
            _conn = cfg.GetConnectionString("DBConn");
        }

        public async Task AddOrderAsync(CompletedOrder order)
        {
            using var con = new NpgsqlConnection(_conn);
            await con.OpenAsync();

            using var tx = await con.BeginTransactionAsync();

            try
            {
                using var cmd = new NpgsqlCommand(@"
                    INSERT INTO ""CompletedOrders""
                    (""TableId"", ""OrderType"", ""Subtotal"", ""Tax"", ""Discount"",
                     ""Total"", ""PaymentMode"", ""TableTimeMinutes"", ""CreatedDate"")
                    VALUES
                    (@tableId, @type, @subtotal, @tax, @discount,
                     @total, @payment, @time, @date)
                    RETURNING ""Id"";
                ", con, tx);

                cmd.Parameters.AddWithValue("@tableId", order.TableId);
                cmd.Parameters.AddWithValue("@type", order.OrderType);
                cmd.Parameters.AddWithValue("@subtotal", order.Subtotal);
                cmd.Parameters.AddWithValue("@tax", order.Tax);
                cmd.Parameters.AddWithValue("@discount", order.Discount);
                cmd.Parameters.AddWithValue("@total", order.Total);
                cmd.Parameters.AddWithValue("@payment", order.PaymentMode);
                cmd.Parameters.AddWithValue("@time", order.TableTimeMinutes);
                cmd.Parameters.AddWithValue("@date", order.CreatedDate);

                order.Id = Convert.ToInt32(await cmd.ExecuteScalarAsync());

                foreach (var item in order.Items)
                {
                    using var itemCmd = new NpgsqlCommand(@"
                        INSERT INTO ""CompletedOrderItems""
                        (""CompletedOrderId"", ""ItemName"", ""Price"", ""Qty"")
                        VALUES (@oid, @name, @price, @qty)
                    ", con, tx);

                    itemCmd.Parameters.AddWithValue("@oid", order.Id);
                    itemCmd.Parameters.AddWithValue("@name", item.ItemName);
                    itemCmd.Parameters.AddWithValue("@price", item.Price);
                    itemCmd.Parameters.AddWithValue("@qty", item.Qty);

                    await itemCmd.ExecuteNonQueryAsync();
                }

                await tx.CommitAsync();
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        public async Task<List<CompletedOrder>> GetAllAsync()
        {
            var orders = new List<CompletedOrder>();

            using var con = new NpgsqlConnection(_conn);
            await con.OpenAsync();

            // 1️⃣ Load Orders
            using (var cmd = new NpgsqlCommand(@"SELECT * FROM ""CompletedOrders"" ORDER BY ""CreatedDate"" DESC", con))
            using (var dr = await cmd.ExecuteReaderAsync())
            {
                while (await dr.ReadAsync())
                {
                    orders.Add(new CompletedOrder
                    {
                        Id = dr.GetInt32(dr.GetOrdinal("Id")),
                        TableId = dr["TableId"].ToString(),
                        OrderType = dr["OrderType"].ToString(),
                        Subtotal = dr.GetDecimal(dr.GetOrdinal("Subtotal")),
                        Tax = dr.GetDecimal(dr.GetOrdinal("Tax")),
                        Discount = dr.GetDecimal(dr.GetOrdinal("Discount")),
                        Total = dr.GetDecimal(dr.GetOrdinal("Total")),
                        PaymentMode = dr["PaymentMode"].ToString(),
                        TableTimeMinutes = dr.GetInt32(dr.GetOrdinal("TableTimeMinutes")),
                        CreatedDate = dr.GetDateTime(dr.GetOrdinal("CreatedDate")),
                        Items = new List<CompletedOrderItem>()
                    });
                }
            }

            if (!orders.Any())
                return orders;

            // 2️⃣ Load Items
            var orderIds = string.Join(",", orders.Select(o => o.Id));

            using var itemCmd = new NpgsqlCommand($@"
        SELECT * FROM ""CompletedOrderItems""
        WHERE ""CompletedOrderId"" IN ({orderIds})
    ", con);

            using var itemDr = await itemCmd.ExecuteReaderAsync();

            var lookup = orders.ToDictionary(o => o.Id);

            while (await itemDr.ReadAsync())
            {
                var item = new CompletedOrderItem
                {
                    Id = itemDr.GetInt32(itemDr.GetOrdinal("Id")),
                    CompletedOrderId = itemDr.GetInt32(itemDr.GetOrdinal("CompletedOrderId")),
                    ItemName = itemDr["ItemName"].ToString(),
                    Price = itemDr.GetDecimal(itemDr.GetOrdinal("Price")),
                    Qty = itemDr.GetInt32(itemDr.GetOrdinal("Qty"))
                };

                lookup[item.CompletedOrderId].Items.Add(item);
            }

            return orders;
        }


        public async Task<List<CompletedOrder>> GetOrdersByDateRangeAsync(DateTime from, DateTime to)
        {
            var list = new List<CompletedOrder>();

            using var con = new NpgsqlConnection(_conn);
            using var cmd = new NpgsqlCommand(@"
        SELECT * FROM ""CompletedOrders""
        WHERE ""CreatedDate"" >= @from
          AND ""CreatedDate"" <= @to
        ORDER BY ""CreatedDate"" DESC
    ", con);

            cmd.Parameters.AddWithValue("@from", from);
            cmd.Parameters.AddWithValue("@to", to);

            await con.OpenAsync();
            using var dr = await cmd.ExecuteReaderAsync();

            while (await dr.ReadAsync())
            {
                list.Add(new CompletedOrder
                {
                    Id = dr.GetInt32(dr.GetOrdinal("Id")),
                    TableId = dr["TableId"].ToString(),
                    OrderType = dr["OrderType"].ToString(),
                    Subtotal = dr.GetDecimal(dr.GetOrdinal("Subtotal")),
                    Tax = dr.GetDecimal(dr.GetOrdinal("Tax")),
                    Discount = dr.GetDecimal(dr.GetOrdinal("Discount")),
                    Total = dr.GetDecimal(dr.GetOrdinal("Total")),
                    PaymentMode = dr["PaymentMode"].ToString(),
                    TableTimeMinutes = dr.GetInt32(dr.GetOrdinal("TableTimeMinutes")),
                    CreatedDate = dr.GetDateTime(dr.GetOrdinal("CreatedDate")),
                    Items = new List<CompletedOrderItem>()
                });
            }

            return list;
        }

    }
}
