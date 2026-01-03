using System.Security.Cryptography;
using System.Text;

namespace BillByte.Helpers
{
    public static class PasswordGenerator
    {
        public static string Generate(int length = 8)
        {
            const string chars =
                "ABCDEFGHJKLMNPQRSTUVWXYZ" +
                "abcdefghijkmnopqrstuvwxyz" +
                "23456789"; // no confusing chars

            var bytes = new byte[length];
            RandomNumberGenerator.Fill(bytes);

            var result = new StringBuilder(length);
            foreach (var b in bytes)
            {
                result.Append(chars[b % chars.Length]);
            }

            return result.ToString();
        }
    }
}
