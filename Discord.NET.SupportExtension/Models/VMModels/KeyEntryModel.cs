using HB.NETF.Services.Data.Identifier;
using HB.NETF.Services.Security.Cryptography.Keys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
