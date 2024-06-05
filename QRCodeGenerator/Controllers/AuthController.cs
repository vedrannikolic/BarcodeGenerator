using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using QRCodeGenerator.Dto;
using QRCodeGenerator.Hubs;
using QrCoreGen.Services;

namespace QRCodeGenerator.Controllers
{
    public class AuthController : Controller
    {
        private readonly IHubContext<AuthenticationHub> _hubContext;
        private readonly IBarcodeHandler _barcodeHandler;

        public AuthController(
            IHubContext<AuthenticationHub> hubContext, 
            IBarcodeHandler barcodeHandler
        )
        {
            _hubContext = hubContext;
            _barcodeHandler = barcodeHandler;
        }

        [HttpGet]
        public IActionResult Generate2DBarcode(string userToken = "")
            => Ok(_barcodeHandler.Generate2DBarcode(userToken));

        [HttpPost("refresh")]
        public IActionResult RefreshToken([FromBody] RefreshTokenDto refreshTokenDto)
            => Ok(_barcodeHandler.RefreshToken(refreshTokenDto.RefreshToken));

        // Napomena: ova rutina šalje cookie i browseru koji je zakačen na SignalR!
        [HttpPost]
        public async Task<IActionResult> Verify2DBarcodeData([FromBody]VerifyTokenDto verifyTokenDto)
        {
            Console.WriteLine($"Received payload: {JsonConvert.SerializeObject(verifyTokenDto)}");
            if (verifyTokenDto == null)
            {
                Console.WriteLine("verifyTokenDto was null.");
                return BadRequest("Invalid data");
            }

            var tokens =
                _barcodeHandler.Verify2DBarcodeData(verifyTokenDto.Guid, verifyTokenDto.Token);
            if (tokens == null)
            {
                return BadRequest("Token validation failed");
            }

            // TODO: ovo staviti u README.md kao primjer kako korisnik treba 
            // kreirati cookie i poslati ga putem SignalR ili nekog drugog libraryja
            // koji radi takvu stvar (npr. SSE)
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = false, // Set this to true if using HTTPS
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddMinutes(30) 
            };
            Response.Cookies.Append("jwtToken", tokens.JwtToken, cookieOptions);

            // Pošaljite tokene web klijentu preko SignalR
            await _hubContext.Clients
                             .Group(verifyTokenDto.Guid)
                             .SendAsync(
                                "ReceiveTokens", 
                                tokens.JwtToken, 
                                tokens.RefreshToken);

            return Ok();
        }
    }
}
