using QrCoreGen.Models;

namespace QrCoreGen.Services
{
    public interface IBarcodeHandler
    {
        string Generate2DBarcode(string userToken = "");
        QrCoreGenTokens Verify2DBarcodeData(string? guid, string? token);
        QrCoreGenTokens RefreshToken(string refreshToken);
    }
}
