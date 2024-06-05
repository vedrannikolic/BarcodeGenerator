namespace QRCodeGen.Services;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using QrCoreGen;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;

public class TokenService : ITokenService
{
    private readonly IOptions<QrCoreGenOptions> _configuration;
    private readonly IMemoryCache _memoryCache;

    public TokenService(IOptions<QrCoreGenOptions> configuration, IMemoryCache memoryCache)
    {
        _configuration = configuration;
        _memoryCache = memoryCache;
    }

    public string GenerateToken()
    {
        var config = _configuration.Value;

        var key = Encoding.UTF8.GetBytes(config.JwtKey);
        var securityKey = new SymmetricSecurityKey(key);

        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: config.JwtIssuer,
            audience: config.JwtAudience,
            expires: DateTime.Now.AddMinutes(config.JwtExpiresTimeout),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public bool ValidateToken(string token, string secretKey, string issuer, string audience)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(secretKey);

        try
        {
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidIssuer = issuer,
                ValidAudience = audience,
                ClockSkew = TimeSpan.Zero
            };

            SecurityToken validatedToken;
            tokenHandler.ValidateToken(token, validationParameters, out validatedToken);

            return true;
        }
        catch (Exception ex)
        {
            // Logirajte grešku za daljnje ispitivanje
            Console.WriteLine($"Greška pri validaciji tokena: {ex.Message}");
            return false;
        }
    }

    public bool ValidateRefreshToken(string refreshToken)
    {
        // Provjerite da li refresh token postoji u memoriji
        if (_memoryCache.TryGetValue(refreshToken, out string storedGuid))
        {

            return true;
        }

        return false;
    }


    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }
}
