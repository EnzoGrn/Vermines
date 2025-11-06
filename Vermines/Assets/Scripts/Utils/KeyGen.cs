using System.Security.Cryptography;
using System;

public static class KeyGen {

    public static string GenerateSecretKey(int byteLength = 16)
    {
        var bytes = new byte[byteLength];

        RandomNumberGenerator.Fill(bytes);

        string base64 = Convert.ToBase64String(bytes);
        string urlSafe = base64.TrimEnd('=').Replace('+', '-').Replace('/', '_');

        return urlSafe;
    }
}
