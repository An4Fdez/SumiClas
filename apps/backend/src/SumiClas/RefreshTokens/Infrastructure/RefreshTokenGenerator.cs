using System.Security.Cryptography;
using System.Text;
using SumiClas.RefreshTokens.Domain;

namespace SumiClas.RefreshTokens.Infrastructure;

public sealed class RefreshTokenGenerator : IRefreshTokenGenerator
{
    private const int RawTokenBytes = 32;

    public (string Raw, string Hash) Generate()
    {
        var raw = System.Buffers.Text.Base64Url.EncodeToString(RandomNumberGenerator.GetBytes(RawTokenBytes));
        return (raw, Hash(raw));
    }

    public string Hash(string raw) => Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(raw)));
}
