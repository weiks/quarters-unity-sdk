using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using Random = UnityEngine.Random;

public class PCKE {

    public string CodeVerifier;

    public PCKE() {
        this.CodeVerifier = GenerateCodeVerifier();
    }
    
    private string GenerateCodeVerifier() {
        string codeVerifier = RandomString(Random.Range(43, 129));
        return codeVerifier;
    }

    static readonly char[] padding = { '=' };

    public string CodeChallenge() {

        byte[] hash = Sha256(CodeVerifier);

        string codeChallenge = Convert.ToBase64String(hash);
        string result = codeChallenge.TrimEnd(padding).Replace('+', '-').Replace('/', '_');
        
        return result;
    }
    
    
    
    
    
    static string RandomString(int length) {
        const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890_-";
        StringBuilder res = new StringBuilder();
        using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider()) {
            byte[] uintBuffer = new byte[sizeof(uint)];

            while (length-- > 0) {
                rng.GetBytes(uintBuffer);
                uint num = BitConverter.ToUInt32(uintBuffer, 0);
                res.Append(valid[(int)(num % (uint)valid.Length)]);
            }
        }

        return res.ToString();
    }
    
    static byte[] Sha256(string randomString) {
        var crypt = new SHA256Managed();
        byte[] crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(randomString));
        return crypto;
    }
}
