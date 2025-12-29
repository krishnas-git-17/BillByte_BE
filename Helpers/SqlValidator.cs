namespace Billbyte_BE.Helpers
{
    public static class SqlValidator
    {
        public static bool IsSafe(string sql)
        {
            var s = sql.ToLower();

            if (!s.StartsWith("select"))
                return false;

            string[] blocked =
            {
        "insert ", "update ", "delete ",
        "drop ", "alter ", "truncate ",
        "create ", ";"
    };

            return !blocked.Any(s.Contains) && s.Contains("limit");
        }
    }
}
