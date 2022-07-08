using System;
using System.Security.Cryptography;
using System.Text;

namespace QuartersSDK.Services
{
    public class PCKE
    {
        public string CodeVerifier;

        public PCKE()
        {
            this.CodeVerifier = GenerateCodeVerifier();
        }

        private string GenerateCodeVerifier()
        {
            System.Random rand = new System.Random();
            string codeVerifier = RandomString(rand.Next(43, 129));
            return codeVerifier;
        }

        private static readonly char[] padding = { '=' };

        public string CodeChallenge()
        {
            byte[] hash = Sha256(CodeVerifier);

            string codeChallenge = Convert.ToBase64String(hash);
            string result = codeChallenge.TrimEnd(padding).Replace('+', '-').Replace('/', '_');

            return result;
        }

        public string RandomString(int length)
        {
            const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890_-";
            StringBuilder res = new StringBuilder();
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                byte[] uintBuffer = new byte[sizeof(uint)];

                while (length-- > 0)
                {
                    rng.GetBytes(uintBuffer);
                    uint num = BitConverter.ToUInt32(uintBuffer, 0);
                    res.Append(valid[(int)(num % (uint)valid.Length)]);
                }
            }
            return res.ToString();
        }

        public byte[] Sha256(string randomString)
        {
            var crypt = new SHA256Managed();
            byte[] crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(randomString));
            return crypto;
        }
    }
}