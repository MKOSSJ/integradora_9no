using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Serialization;

namespace Plandi.Dto.Utils
{
    public class Utils
    {
        public static string EncryptPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                var builder = new StringBuilder();
                foreach (var b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }

        

        public static string GenerateCode()
        {
            return Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        }

        public static string HashToken(string token) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));



        /*
        public static List<PairSettingsDto> DeserializeSettings(string xml)
        {
            var xmlRoot = new XmlRootAttribute("PairSettings");
            var serializer = new XmlSerializer(typeof(List<PairSettingsDto>), xmlRoot);

            using (var stringReader = new StringReader(xml))
            {
                return (List<PairSettingsDto>)serializer.Deserialize(stringReader);
            }
        }
        */
    }
}
