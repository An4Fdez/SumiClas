using System.Security.Cryptography;
using System.Text;
using SumiClas.Sessions.Domain;

namespace SumiClas.Sessions.Infrastructure;

public sealed class RefreshTokenGenerator : IRefreshTokenGenerator
{
    private const int RawTokenBytes = 32;

    public string Generate() => System.Buffers.Text.Base64Url.EncodeToString(RandomNumberGenerator.GetBytes(RawTokenBytes));

    public string Hash(string refreshToken) => Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken)));
}