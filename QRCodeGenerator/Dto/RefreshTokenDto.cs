namespace QRCodeGenerator.Dto
{
    // TODO: možda se ovo može maknuti, ima QrCoreGenTokens
    public class RefreshTokenDto
    {
        public string JwtToken { get; set; }
        public string RefreshToken { get; set; }
    }
}
