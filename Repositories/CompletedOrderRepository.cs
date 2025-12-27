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
                    (""RestaurantId"", ""TableId"", ""OrderType"", ""Subtotal"", ""Tax"",
                     ""Discount"", ""Total"", ""PaymentMode"", ""TableTimeMinutes"", ""CreatedDate"")
                    VALUES
                    (@rid, @tableId, @type, @subtotal, @tax,
                     @discount, @total, @payment, @time, @date)
                    RETURNING ""Id"";
                ", con, tx);

                cmd.Parameters.AddWithValue("@rid", order.RestaurantId);
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

        public async Task<List<CompletedOrder>> GetAllAsync(int restaurantId)
        {
            var orders = new List<CompletedOrder>();

            using var con = new NpgsqlConnection(_conn);
            await con.OpenAsync();

            using (var cmd = new NpgsqlCommand(@"
                SELECT * FROM ""CompletedOrders""
                WHERE ""RestaurantId""=@rid
                ORDER BY ""CreatedDate"" DESC
            ", con))
            {
                cmd.Parameters.AddWithValue("@rid", restaurantId);

                using var dr = await cmd.ExecuteReaderAsync();
                while (await dr.ReadAsync())
                {
                    orders.Add(MapOrder(dr));
                }
            }

            if (!orders.Any())
                return orders;

            await LoadItemsAsync(con, orders);
            return orders;
        }

        public async Task<List<CompletedOrder>> GetOrdersByDateRangeAsync(
            int restaurantId,
            DateTime from,
            DateTime to)
        {
            var orders = new List<CompletedOrder>();

            using var con = new NpgsqlConnection(_conn);
            await con.OpenAsync();

            using (var cmd = new NpgsqlCommand(@"
                SELECT * FROM ""CompletedOrders""
                WHERE ""RestaurantId""=@rid
                  AND ""CreatedDate"" BETWEEN @from AND @to
                ORDER BY ""CreatedDate"" DESC
            ", con))
            {
                cmd.Parameters.AddWithValue("@rid", restaurantId);
                cmd.Parameters.AddWithValue("@from", from);
                cmd.Parameters.AddWithValue("@to", to);

                using var dr = await cmd.ExecuteReaderAsync();
                while (await dr.ReadAsync())
                {
                    orders.Add(MapOrder(dr));
                }
            }

            if (!orders.Any())
                return orders;

            await LoadItemsAsync(con, orders);
            return orders;
        }

        private async Task LoadItemsAsync(
            NpgsqlConnection con,
            List<CompletedOrder> orders)
        {
            var orderIds = string.Join(",", orders.Select(o => o.Id));
            var lookup = orders.ToDictionary(o => o.Id);

            using var cmd = new NpgsqlCommand($@"
                SELECT * FROM ""CompletedOrderItems""
                WHERE ""CompletedOrderId"" IN ({orderIds})
            ", con);

            using var dr = await cmd.ExecuteReaderAsync();

            while (await dr.ReadAsync())
            {
                var item = new CompletedOrderItem
                {
                    Id = dr.GetInt32(dr.GetOrdinal("Id")),
                    CompletedOrderId = dr.GetInt32(dr.GetOrdinal("CompletedOrderId")),
                    ItemName = dr["ItemName"].ToString(),
                    Price = dr.GetDecimal(dr.GetOrdinal("Price")),
                    Qty = dr.GetInt32(dr.GetOrdinal("Qty"))
                };

                lookup[item.CompletedOrderId].Items.Add(item);
            }
        }

        private CompletedOrder MapOrder(NpgsqlDataReader dr)
        {
            return new CompletedOrder
            {
                Id = dr.GetInt32(dr.GetOrdinal("Id")),
                RestaurantId = dr.GetInt32(dr.GetOrdinal("RestaurantId")),
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
            };
        }
    }
}
