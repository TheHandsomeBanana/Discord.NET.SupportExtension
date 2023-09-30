using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord.NET.SupportExtension.Mef {
    public static class PackageImageIds {
        public const string DiscordMonikerString = "d53d7256-d44d-4245-bdd2-bfd22943659c";
        public static Guid DiscordMoniker = new Guid(DiscordMonikerString);
        public const int Discord = 0x0001;

    }
}
