namespace QRCodeGen.Services;

public interface ITokenService
{
    string GenerateToken();
    bool ValidateToken(string token, string secretKey, string issuer, string audience);
    bool ValidateRefreshToken(string refreshToken);
    string GenerateRefreshToken();
}
