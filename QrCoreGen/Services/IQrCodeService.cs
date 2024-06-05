namespace QRCodeGen.Services;

public interface IQrCodeService
{
    byte[] GenerateQrCode(string token, string guid);
}
