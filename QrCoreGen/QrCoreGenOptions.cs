using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QrCoreGen
{
    public class QrCoreGenOptions
    {
        public const string QrCoreGen = "QrCoreGen";

        public string Format { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int Margin { get; set; }
        public int UserTokenTimeout { get; set; }
        public int MemCacheTimeout { get; set; } = 7 * 24 * 60;
        public string JwtKey { get; set; }
        public string JwtIssuer { get; set; }
        public string JwtAudience { get; set; }
        public int JwtExpiresTimeout { get; set; } = 30;
    }
}
