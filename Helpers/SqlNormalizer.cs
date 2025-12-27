using System.Text.RegularExpressions;

namespace Billbyte_BE.Helpers
{
    public static class SqlNormalizer
    {
        public static string Normalize(string sql)
        {
            // Quote ONLY table names (word boundaries)
            sql = Regex.Replace(sql, @"\bCompletedOrders\b", "\"CompletedOrders\"");
            sql = Regex.Replace(sql, @"\bCompletedOrderItems\b", "\"CompletedOrderItems\"");
            sql = Regex.Replace(sql, @"\bTableStates\b", "\"TableStates\"");
            sql = Regex.Replace(sql, @"\bMenuItems\b", "\"MenuItems\"");

            return sql;
        }
    }
}
