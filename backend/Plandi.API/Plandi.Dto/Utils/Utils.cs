using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Serialization;

namespace Plandi.Dto.Utils
{
    public class Utils
    {
        private const string Alphanumeric = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        private static readonly Random _random = new();
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
            int length = 6;
            return new string(Enumerable.Repeat(Alphanumeric, length)
                .Select(s => s[_random.Next(s.Length)]).ToArray());
        }



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
