using System;
using System.Security.Cryptography;
using System.Text;
using Random = UnityEngine.Random;

public class PCKE {
    private static readonly char[] padding = {'='};

    public string CodeVerifier;

    public PCKE() {
        CodeVerifier = GenerateCodeVerifier();
    }

    private string GenerateCodeVerifier() {
        string codeVerifier = RandomString(Random.Range(43, 129));
        return codeVerifier;
    }

    public string CodeChallenge() {
        byte[] hash = Sha256(CodeVerifier);

        string codeChallenge = Convert.ToBase64String(hash);
        string result = codeChallenge.TrimEnd(padding).Replace('+', '-').Replace('/', '_');

        return result;
    }


    private static string RandomString(int length) {
        const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890_-";
        StringBuilder res = new StringBuilder();
        using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider()) {
            byte[] uintBuffer = new byte[sizeof(uint)];

            while (length-- > 0) {
                rng.GetBytes(uintBuffer);
                uint num = BitConverter.ToUInt32(uintBuffer, 0);
                res.Append(valid[(int) (num % (uint) valid.Length)]);
            }
        }

        return res.ToString();
    }

    private static byte[] Sha256(string randomString) {
        SHA256Managed crypt = new SHA256Managed();
        byte[] crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(randomString));
        return crypto;
    }
}