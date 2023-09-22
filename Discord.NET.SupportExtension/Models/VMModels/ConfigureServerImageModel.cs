using HB.NETF.Discord.NET.Toolkit.EntityService.Models;
using HB.NETF.Services.Security.Cryptography.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord.NET.SupportExtension.Models.VMModels {
    public class ConfigureServerImageModel {
        public bool SaveTokens { get; set; }
        public bool EncryptData { get; set; }
        public EncryptionMode? TokenEncryptionMode { get; set; }
        public EncryptionMode? DataEncryptionMode { get; set; }
        public Guid? TokenKeyIdentifier { get; set; }
        public Guid? DataKeyIdentifier { get; set; }
    }
}
