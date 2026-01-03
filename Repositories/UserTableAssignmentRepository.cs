                using BillByte.Repositories.Interface;
                using BillByte.Interface;
                using Billbyte_BE.Models;
                using Npgsql;

                namespace BillByte.Repositories
                {
                    public class UserTableAssignmentRepository : IUserTableAssignmentRepository
                    {
                        private readonly string _conn;
                        private readonly ITablePreferenceRepository _tablePrefRepo;

                        public UserTableAssignmentRepository(IConfiguration cfg, ITablePreferenceRepository tablePrefRepo)
                        {
                            _conn = cfg.GetConnectionString("DBConn");
                            _tablePrefRepo = tablePrefRepo;
                        }

                        public async Task AssignAsync(UserTableAssignment item)
                        {
                            using var con = new NpgsqlConnection(_conn);
                            using var cmd = new NpgsqlCommand(@"
                                INSERT INTO ""UserTableAssignments""
                                (""RestaurantId"", ""UserId"", ""TablePreferenceId"")
                                VALUES (@rid, @uid, @tpid)
                                ON CONFLICT DO NOTHING", con);

                            cmd.Parameters.AddWithValue("@rid", item.RestaurantId);
                            cmd.Parameters.AddWithValue("@uid", item.UserId);
                            cmd.Parameters.AddWithValue("@tpid", item.TablePreferenceId);

                            await con.OpenAsync();
                            var rows = await cmd.ExecuteNonQueryAsync();

                            // If a new assignment was created, notify table-preferences subscribers
                            if (rows > 0)
                            {
                                await _tablePrefRepo.NotifyAssignmentChangedAsync(item.RestaurantId, item.UserId, item.TablePreferenceId);
                            }
                        }

                        public async Task<List<TablePreference>> GetSectionsForUserAsync(
                            int restaurantId,
                            int userId)
                        {
                            var list = new List<TablePreference>();

                            using var con = new NpgsqlConnection(_conn);
                            using var cmd = new NpgsqlCommand(@"
                                SELECT tp.*
                                FROM ""UserTableAssignments"" uta
                                JOIN ""TablePreferences"" tp
                                  ON tp.""Id"" = uta.""TablePreferenceId""
                                WHERE uta.""RestaurantId""=@rid
                                  AND uta.""UserId""=@uid
                                ORDER BY tp.""Name""", con);

                            cmd.Parameters.AddWithValue("@rid", restaurantId);
                            cmd.Parameters.AddWithValue("@uid", userId);

                            await con.OpenAsync();
                            using var dr = await cmd.ExecuteReaderAsync();

                            while (await dr.ReadAsync())
                            {
                                list.Add(new TablePreference
                                {
                                    Id = Convert.ToInt32(dr["Id"]),
                                    RestaurantId = Convert.ToInt32(dr["RestaurantId"]),
                                    Name = dr["Name"].ToString()!,
                                    TableCount = Convert.ToInt32(dr["TableCount"])
                                });
                            }

                            return list;
                        }

                        public async Task<List<int>> GetAssignedSectionIdsAsync(
                          int restaurantId,
                          int userId)
                        {
                            var list = new List<int>();

                            using var con = new NpgsqlConnection(_conn);
                            using var cmd = new NpgsqlCommand(@"
                                SELECT ""TablePreferenceId""
                                FROM ""UserTableAssignments""
                                WHERE ""RestaurantId""=@rid
                                  AND ""UserId""=@uid", con);

                            cmd.Parameters.AddWithValue("@rid", restaurantId);
                            cmd.Parameters.AddWithValue("@uid", userId);

                            await con.OpenAsync();
                            using var dr = await cmd.ExecuteReaderAsync();

                            while (await dr.ReadAsync())
                            {
                                list.Add(Convert.ToInt32(dr["TablePreferenceId"]));
                            }

                            return list;
                        }

                        public async Task<bool> HasAccessAsync(
                            int restaurantId,
                            int userId,
                            int tablePreferenceId)
                        {
                            using var con = new NpgsqlConnection(_conn);
                            using var cmd = new NpgsqlCommand(@"
                                SELECT 1
                                FROM ""UserTableAssignments""
                                WHERE ""RestaurantId""=@rid
                                  AND ""UserId""=@uid
                                  AND ""TablePreferenceId""=@tpid
                                LIMIT 1", con);

                            cmd.Parameters.AddWithValue("@rid", restaurantId);
                            cmd.Parameters.AddWithValue("@uid", userId);
                            cmd.Parameters.AddWithValue("@tpid", tablePreferenceId);

                            await con.OpenAsync();
                            var result = await cmd.ExecuteScalarAsync();
                            return result != null;
                        }
                    }
                }
