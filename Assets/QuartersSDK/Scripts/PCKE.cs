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
        
        // RNGCryptoServiceProvider
        string codeVerifier = RandomString(Random.Range(43, 129));
        return codeVerifier;
    }

    static readonly char[] padding = { '=' };

    public string GenerateCodeChallenge() {

        string hash = Sha256(GenerateCodeVerifier());
        
        string codeChallenge = Convert.ToBase64String(Encoding.ASCII.GetBytes(hash)).TrimEnd(padding).Replace('+', '-').Replace('/', '_');
        
        return codeChallenge;
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
    
    static string Sha256(string randomString) {
        var crypt = new SHA256Managed();
        string hash = String.Empty;
        byte[] crypto = crypt.ComputeHash(Encoding.ASCII.GetBytes(randomString));
        foreach (byte theByte in crypto) {
            hash += theByte.ToString("x2");
        }
        return hash;
    }
}
