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

                    // Cascade delete: assignments -> active items -> KOT -> completed orders -> table states -> section
                    public async Task<bool> DeleteAsync(int id, int restaurantId)
                    {
                        using var con = new NpgsqlConnection(_conn);
                        await con.OpenAsync();
                        using var tx = await con.BeginTransactionAsync();

                        try
                        {
                            // 1) Ensure section exists and get its Name (used to find tables like "SectionName-T1")
                            string? sectionName;
                            using (var getCmd = new NpgsqlCommand(@"
                                SELECT ""Name"" FROM ""TablePreferences""
                                WHERE ""Id""=@id AND ""RestaurantId""=@rid
                            ", con, tx))
                            {
                                getCmd.Parameters.AddWithValue("@id", id);
                                getCmd.Parameters.AddWithValue("@rid", restaurantId);
                                var result = await getCmd.ExecuteScalarAsync();
                                if (result == null || result == DBNull.Value)
                                {
                                    await tx.RollbackAsync();
                                    return false;
                                }
                                sectionName = Convert.ToString(result);
                            }

                            var tablePattern = sectionName + "-%"; // matches "MainHall-T1", "MainHall-T2", etc.

                            // 2) Delete assignments (explicit)
                            using (var cmd = new NpgsqlCommand(@"
                                DELETE FROM ""UserTableAssignments""
                                WHERE ""RestaurantId""=@rid AND ""TablePreferenceId""=@id
                            ", con, tx))
                            {
                                cmd.Parameters.AddWithValue("@rid", restaurantId);
                                cmd.Parameters.AddWithValue("@id", id);
                                await cmd.ExecuteNonQueryAsync();
                            }

                            // 3) Delete active table items for all tables in section
                            using (var cmd = new NpgsqlCommand(@"
                                DELETE FROM ""ActiveTableItems""
                                WHERE ""RestaurantId""=@rid AND ""TableId"" LIKE @pattern
                            ", con, tx))
                            {
                                cmd.Parameters.AddWithValue("@rid", restaurantId);
                                cmd.Parameters.AddWithValue("@pattern", tablePattern);
                                await cmd.ExecuteNonQueryAsync();
                            }

                            // 4) Delete KOT snapshots (KotSnapshotItems cascade via FK)
                            using (var cmd = new NpgsqlCommand(@"
                                DELETE FROM ""KotSnapshots""
                                WHERE ""RestaurantId""=@rid AND ""TableId"" LIKE @pattern
                            ", con, tx))
                            {
                                cmd.Parameters.AddWithValue("@rid", restaurantId);
                                cmd.Parameters.AddWithValue("@pattern", tablePattern);
                                await cmd.ExecuteNonQueryAsync();
                            }

                            // 5) Delete completed orders (CompletedOrderItems cascade via FK)
                            using (var cmd = new NpgsqlCommand(@"
                                DELETE FROM ""CompletedOrders""
                                WHERE ""RestaurantId""=@rid AND ""TableId"" LIKE @pattern
                            ", con, tx))
                            {
                                cmd.Parameters.AddWithValue("@rid", restaurantId);
                                cmd.Parameters.AddWithValue("@pattern", tablePattern);
                                await cmd.ExecuteNonQueryAsync();
                            }

                            // 6) Delete table states for those tables
                            using (var cmd = new NpgsqlCommand(@"
                                DELETE FROM ""TableStates""
                                WHERE ""RestaurantId""=@rid AND ""TableId"" LIKE @pattern
                            ", con, tx))
                            {
                                cmd.Parameters.AddWithValue("@rid", restaurantId);
                                cmd.Parameters.AddWithValue("@pattern", tablePattern);
                                await cmd.ExecuteNonQueryAsync();
                            }

                            // 7) Finally delete the section
                            int rowsAffected;
                            using (var cmd = new NpgsqlCommand(@"
                                DELETE FROM ""TablePreferences""
                                WHERE ""Id""=@id AND ""RestaurantId""=@rid
                            ", con, tx))
                            {
                                cmd.Parameters.AddWithValue("@id", id);
                                cmd.Parameters.AddWithValue("@rid", restaurantId);
                                rowsAffected = await cmd.ExecuteNonQueryAsync();
                            }

                            await tx.CommitAsync();
                            Console.WriteLine($"Cascade delete completed for section '{sectionName}' (Id={id}) in restaurant {restaurantId}.");
                            return rowsAffected > 0;
                        }
                        catch
                        {
                            await tx.RollbackAsync();
                            throw;
                        }
                    }

                    // Cascade delete for all sections in a restaurant
                    public async Task<bool> DeleteAllAsync(int restaurantId)
                    {
                        using var con = new NpgsqlConnection(_conn);
                        await con.OpenAsync();
                        using var tx = await con.BeginTransactionAsync();

                        try
                        {
                            // 1) Delete all assignments for restaurant
                            using (var cmd = new NpgsqlCommand(@"
                                DELETE FROM ""UserTableAssignments""
                                WHERE ""RestaurantId""=@rid
                            ", con, tx))
                            {
                                cmd.Parameters.AddWithValue("@rid", restaurantId);
                                await cmd.ExecuteNonQueryAsync();
                            }

                            // 2) Delete all active table items for restaurant
                            using (var cmd = new NpgsqlCommand(@"
                                DELETE FROM ""ActiveTableItems""
                                WHERE ""RestaurantId""=@rid
                            ", con, tx))
                            {
                                cmd.Parameters.AddWithValue("@rid", restaurantId);
                                await cmd.ExecuteNonQueryAsync();
                            }

                            // 3) Delete all KOT snapshots for restaurant (items cascade)
                            using (var cmd = new NpgsqlCommand(@"
                                DELETE FROM ""KotSnapshots""
                                WHERE ""RestaurantId""=@rid
                            ", con, tx))
                            {
                                cmd.Parameters.AddWithValue("@rid", restaurantId);
                                await cmd.ExecuteNonQueryAsync();
                            }

                            // 4) Delete all completed orders for restaurant (items cascade)
                            using (var cmd = new NpgsqlCommand(@"
                                DELETE FROM ""CompletedOrders""
                                WHERE ""RestaurantId""=@rid
                            ", con, tx))
                            {
                                cmd.Parameters.AddWithValue("@rid", restaurantId);
                                await cmd.ExecuteNonQueryAsync();
                            }

                            // 5) Delete all table states for restaurant
                            using (var cmd = new NpgsqlCommand(@"
                                DELETE FROM ""TableStates""
                                WHERE ""RestaurantId""=@rid
                            ", con, tx))
                            {
                                cmd.Parameters.AddWithValue("@rid", restaurantId);
                                await cmd.ExecuteNonQueryAsync();
                            }

                            // 6) Finally delete all section records
                            int rows;
                            using (var cmd = new NpgsqlCommand(@"
                                DELETE FROM ""TablePreferences""
                                WHERE ""RestaurantId""=@rid
                            ", con, tx))
                            {
                                cmd.Parameters.AddWithValue("@rid", restaurantId);
                                rows = await cmd.ExecuteNonQueryAsync();
                            }

                            await tx.CommitAsync();
                            Console.WriteLine($"Cascade delete completed for all sections in restaurant {restaurantId}.");
                            return rows > 0;
                        }
                        catch
                        {
                            await tx.RollbackAsync();
                            throw;
                        }
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
