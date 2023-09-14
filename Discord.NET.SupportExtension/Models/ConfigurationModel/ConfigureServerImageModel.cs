using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord.NET.SupportExtension.Models.ConfigurationModel {
    public class ConfigureServerImageModel {
        public bool SaveTokens { get; set; }
        public bool EncryptData { get; set; }
        public EncryptionMethod TokenEncryptionMethod { get; set; }
        public EncryptionMethod DataEncryptionMethod { get; set; }

        public string Test { get; set; } = "Test";
    }

    public enum EncryptionMethod {
        DataProtectionApi = 0,
        AES = 1
    }
}
