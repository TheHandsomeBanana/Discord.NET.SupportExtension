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
        public bool SaveTokens { get; set; } = false;
        public bool EncryptData { get; set; } = false;
        public EncryptionMode? TokenEncryptionMode { get; set; } = null;
        public EncryptionMode? DataEncryptionMode { get; set; } = null;
        public Guid? TokenKeyIdentifier { get; set; } = null;
        public Guid? DataKeyIdentifier { get; set; } = null;
        public TokenModel[] Tokens { get; set; } = new TokenModel[0];
    }
}
