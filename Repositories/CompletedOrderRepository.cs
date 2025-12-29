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
                // 🔹 1. Generate Invoice No
                order.InvoiceNo = await GenerateInvoiceNo(con, order.RestaurantId, tx);

                // 🔹 2. Calculate Table Time Minutes (from TableStates)
                int tableMinutes = 0;

                if (!string.IsNullOrEmpty(order.TableId))
                {
                    using var timeCmd = new NpgsqlCommand(@"
                SELECT ""StartTime""
                FROM ""TableStates""
                WHERE ""RestaurantId"" = @rid
                  AND ""TableId"" = @tableId
            ", con, tx);

                    timeCmd.Parameters.AddWithValue("@rid", order.RestaurantId);
                    timeCmd.Parameters.AddWithValue("@tableId", order.TableId);

                    var startTimeObj = await timeCmd.ExecuteScalarAsync();

                    if (startTimeObj != null && startTimeObj != DBNull.Value)
                    {
                        var startTime = (DateTime)startTimeObj;
                        tableMinutes = (int)(DateTime.UtcNow - startTime).TotalMinutes;
                    }
                }

                // 🔹 3. Insert Completed Order
                using var cmd = new NpgsqlCommand(@"
            INSERT INTO ""CompletedOrders""
            (""RestaurantId"", ""InvoiceNo"", ""TableId"", ""OrderType"",
             ""Subtotal"", ""Tax"", ""Discount"", ""Total"",
             ""PaymentMode"", ""TableTimeMinutes"", ""CreatedDate"")
            VALUES
            (@rid, @invoice, @tableId, @type,
             @subtotal, @tax, @discount, @total,
             @payment, @time, @date)
            RETURNING ""Id"";
        ", con, tx);

                cmd.Parameters.AddWithValue("@rid", order.RestaurantId);
                cmd.Parameters.AddWithValue("@invoice", order.InvoiceNo);
                cmd.Parameters.AddWithValue("@tableId", order.TableId ?? "");
                cmd.Parameters.AddWithValue("@type", order.OrderType);
                cmd.Parameters.AddWithValue("@subtotal", order.Subtotal);
                cmd.Parameters.AddWithValue("@tax", order.Tax);
                cmd.Parameters.AddWithValue("@discount", order.Discount);
                cmd.Parameters.AddWithValue("@total", order.Total);
                cmd.Parameters.AddWithValue("@payment", order.PaymentMode);
                cmd.Parameters.AddWithValue("@time", tableMinutes);
                cmd.Parameters.AddWithValue("@date", DateTime.UtcNow);

                order.Id = Convert.ToInt32(await cmd.ExecuteScalarAsync());

                // 🔹 4. Insert Order Items
                foreach (var item in order.Items)
                {
                    using var itemCmd = new NpgsqlCommand(@"
                INSERT INTO ""CompletedOrderItems""
                (""CompletedOrderId"", ""ItemName"", ""Price"", ""Qty"")
                VALUES (@oid, @name, @price, @qty);
            ", con, tx);

                    itemCmd.Parameters.AddWithValue("@oid", order.Id);
                    itemCmd.Parameters.AddWithValue("@name", item.ItemName);
                    itemCmd.Parameters.AddWithValue("@price", item.Price);
                    itemCmd.Parameters.AddWithValue("@qty", item.Qty);

                    await itemCmd.ExecuteNonQueryAsync();
                }

                // 🔹 5. Commit
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
                InvoiceNo = dr["InvoiceNo"].ToString(), // ✅
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


        private async Task<string> GenerateInvoiceNo(
    NpgsqlConnection con,
    int restaurantId,
    NpgsqlTransaction tx)
        {
            var year = DateTime.UtcNow.Year;

            using var cmd = new NpgsqlCommand(@"
        SELECT COUNT(*) 
        FROM ""CompletedOrders""
        WHERE ""RestaurantId"" = @rid
          AND EXTRACT(YEAR FROM ""CreatedDate"") = @year
    ", con, tx);

            cmd.Parameters.AddWithValue("@rid", restaurantId);
            cmd.Parameters.AddWithValue("@year", year);

            var seq = Convert.ToInt32(await cmd.ExecuteScalarAsync()) + 1;

            return $"INV-{restaurantId}-{year}-{seq:D5}";
        }

        public async Task<CompletedOrder?> GetByInvoiceAsync(
    int restaurantId,
    string invoiceNo)
        {
            using var con = new NpgsqlConnection(_conn);
            await con.OpenAsync();

            CompletedOrder? order = null;

            using (var cmd = new NpgsqlCommand(@"
        SELECT * FROM ""CompletedOrders""
        WHERE ""RestaurantId""=@rid
          AND ""InvoiceNo""=@inv
    ", con))
            {
                cmd.Parameters.AddWithValue("@rid", restaurantId);
                cmd.Parameters.AddWithValue("@inv", invoiceNo);

                using var dr = await cmd.ExecuteReaderAsync();
                if (await dr.ReadAsync())
                    order = MapOrder(dr);
            }

            if (order == null) return null;

            await LoadItemsAsync(con, new List<CompletedOrder> { order });
            return order;
        }


    }
}
