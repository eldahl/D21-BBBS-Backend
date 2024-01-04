using Org.BouncyCastle.Utilities;
using System.Text;

namespace BBBSBackend
{
    public static class Util
    {
        public static string CreateCancelationToken(Guid id, string email) {
            byte[] bytes = Encoding.UTF8.GetBytes(id.ToString() + " | " + email);
            string token = Convert.ToBase64String(bytes);
            return token;
        }
    }
}
