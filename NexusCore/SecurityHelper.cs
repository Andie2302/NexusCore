using System.Security.Cryptography;
using System.Text;

namespace NexusCore;

public static class SecurityHelper
{
    public static string CalculateSha256(string input)
    {
        var inputBytes = Encoding.UTF8.GetBytes(input);
        var hashBytes = SHA256.HashData(inputBytes);
        return Convert.ToHexString(hashBytes);
    }
}