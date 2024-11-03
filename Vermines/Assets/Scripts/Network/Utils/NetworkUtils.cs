using System.Linq;

public static class NetworkUtils
{
    public static string GenerateRandomCode(string keyStringCodeGeneration, int length, System.Random random)
    {
        if (length <= 0)
        {
            return string.Empty;
        }

        return new string(Enumerable.Repeat(keyStringCodeGeneration, length).Select(s =>
            s[random.Next(0, keyStringCodeGeneration.Length)]).ToArray());
    }
}

