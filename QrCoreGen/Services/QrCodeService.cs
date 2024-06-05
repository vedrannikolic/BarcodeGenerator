namespace QRCodeGen.Services;

using Microsoft.Extensions.Options;
using QrCoreGen;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Runtime.InteropServices;
using System.Text.Json;
using ZXing;
using ZXing.QrCode;

public class QrCodeService : IQrCodeService
{
    private readonly IOptions<QrCoreGenOptions> _configuration;

    public QrCodeService(IOptions<QrCoreGenOptions> configuration)
    {
        _configuration = configuration;
    }

    public byte[] GenerateQrCode(string token, string guid)
    {
        var config = _configuration.Value;

        var data = new
        {
            //Token = token,
            Guid = guid
        };

        string json = JsonSerializer.Serialize(data);

        var writer = new BarcodeWriterPixelData
        {
            Format = Enum.Parse<BarcodeFormat>(config.Format),
            Options = new QrCodeEncodingOptions
            {
                Height = config.Height,
                Width = config.Width,
                Margin = config.Margin
            }
        };

        var pixelData = writer.Write(json);


        using (var image = new Image<Rgba32>(pixelData.Width, pixelData.Height))
        {
            image.ProcessPixelRows(accessor =>
            {
                for (int y = 0; y < pixelData.Height; y++)
                {
                    Span<Rgba32> pixelRowSpan = accessor.GetRowSpan(y);
                    Span<byte> byteRowSpan = pixelData.Pixels.AsSpan(y * pixelData.Width * 4, pixelData.Width * 4);
                    MemoryMarshal.Cast<byte, Rgba32>(byteRowSpan).CopyTo(pixelRowSpan);
                }
            });

            using (var ms = new MemoryStream())
            {
                image.SaveAsPng(ms);
                return ms.ToArray();
            }
        }
    }
}
