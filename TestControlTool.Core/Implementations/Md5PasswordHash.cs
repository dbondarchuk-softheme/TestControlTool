using System.Security.Cryptography;
using System.Text;
using TestControlTool.Core.Contracts;

namespace TestControlTool.Core.Implementations
{
    /// <summary>
    /// Describes md5 hash provider
    /// </summary>
    public class Md5PasswordHash : IPasswordHash
    {
        /// <summary>
        /// Gets md5 hash from the string
        /// </summary>
        /// <param name="stringToHash">String to hash</param>
        /// <returns>Hash</returns>
        public string GetHash(string stringToHash)
        {
            var md5 = new MD5CryptoServiceProvider();

            var tmpSource = Encoding.ASCII.GetBytes(stringToHash);
            var tmpHash = md5.ComputeHash(tmpSource);

            var hash = new StringBuilder(tmpHash.Length);

            for (var i = 0; i < tmpHash.Length; i++)
            {
                hash.Append(tmpHash[i].ToString("X2"));
            }

            return hash.ToString();
        }
    }
}
