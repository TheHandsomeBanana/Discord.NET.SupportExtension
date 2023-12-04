using HB.NETF.Services.Data.Identifier;
using HB.NETF.Services.Security.Cryptography.Keys;

namespace Discord.NET.SupportExtension.Models.VMModels {
    internal class KeyEntryModel {
        public string Name { get; set; }
        public Identifier<AesKey> Key { get; set; }
        public bool IsCanceled { get; set; } = false;

        public KeyEntryModel(string name) {
            this.Name = name;
        }

        public bool ContainsKey() => Key != null;
    }
}
