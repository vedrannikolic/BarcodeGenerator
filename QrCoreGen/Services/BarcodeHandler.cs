using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using QRCodeGen.Services;
using QrCoreGen.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QrCoreGen.Services
{
    public class BarcodeHandler : IBarcodeHandler
    {
        private readonly IQrCodeService _qrCodeService;
        private readonly ITokenService _tokenService;
        private readonly IMemoryCache _memoryCache;
        private readonly IOptions<QrCoreGenOptions> _configuration;

        public BarcodeHandler(
            IOptions<QrCoreGenOptions> configuration,
            IQrCodeService qrCodeService,
            IMemoryCache memoryCache,
            ITokenService tokenService)
        {
            _configuration = configuration;
            _qrCodeService = qrCodeService;
            _memoryCache = memoryCache;
            _tokenService = tokenService;
        }

        public string Generate2DBarcode(string userToken = "")
        {
            var guid = Guid.NewGuid().ToString(); // Generiranje jedinstvenog identifikatora

            var config = _configuration.Value;

            // Spremanje JWT tokena u memoriju
            _memoryCache.Set(
                guid,
                userToken,
                TimeSpan.FromMinutes(config.UserTokenTimeout));


            // Generiranje QR koda s JWT tokenom i GUID-om
            var qrCodeImage = _qrCodeService.GenerateQrCode(
                userToken,
                guid);

            var base64String = Convert.ToBase64String(qrCodeImage);

            // Vraćanje QR koda kao Base64 stringa
            return base64String;
        }

        public QrCoreGenTokens Verify2DBarcodeData(string? guid, string? token)
        {
            Console.WriteLine($"Received: guid={guid}, token={token}");

            if (string.IsNullOrEmpty(guid))
                return null;

            var config = _configuration.Value;

            // Provjera da li GUID postoji u memoriji
            if (!_memoryCache.TryGetValue(guid, out string storedToken))
            {
                Console.WriteLine($"Invalid or expired GUID: {guid}");
                return null;
            }

            // Generirajte novi JWT i refresh token za web klijenta
            var newJwtToken = _tokenService.GenerateToken();
            var newRefreshToken = _tokenService.GenerateRefreshToken();

            // Pohranite refresh token u memoriju s vremenskim ograničenjem
            _memoryCache.Set(newRefreshToken, guid,
                TimeSpan.FromMinutes(config.MemCacheTimeout));

            var tokens = new QrCoreGenTokens
            {
                JwtToken = newJwtToken,
                RefreshToken = newRefreshToken,
            };

            return tokens;
        }

        public QrCoreGenTokens RefreshToken(string refreshToken)
        {
            // Provjera valjanosti refresh tokena (ovdje možete implementirati vlastitu logiku provjere)
            bool isValidRefreshToken = _tokenService.ValidateRefreshToken(refreshToken);
            if (!isValidRefreshToken)
                return null;

            // Generirajte novi JWT token
            var newJwtToken = _tokenService.GenerateToken();
            var newRefreshToken = _tokenService.GenerateRefreshToken();

            return
                new QrCoreGenTokens
                {
                    JwtToken = newJwtToken,
                    RefreshToken = newRefreshToken
                };
        }
    }
}
