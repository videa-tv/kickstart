using System;
using System.Security.Cryptography;
using System.Text;

namespace Kickstart.Utility
{
    public static class ConsistentGuid
    {
        public static Guid Generate(string id)
        {
            //use MD5 hash to get a 16-byte hash of the string: 

            var provider = new MD5CryptoServiceProvider();

            var inputBytes = Encoding.Default.GetBytes(id);

            var hashBytes = provider.ComputeHash(inputBytes);

            //generate a guid from the hash: 

            var hashGuid = new Guid(hashBytes);

            return hashGuid;
        }
    }
}