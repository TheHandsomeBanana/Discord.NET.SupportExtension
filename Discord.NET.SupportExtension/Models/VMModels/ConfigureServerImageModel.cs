using HB.NETF.Services.Security.Cryptography.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord.NET.SupportExtension.Models.VMModels {
    public class ConfigureServerImageModel {
        public bool SaveToken { get; set; } = false;
        public bool EncryptData { get; set; } = false;
        public EncryptionMode? TokenEncryptionMode { get; set; } = null;
        public EncryptionMode? DataEncryptionMode { get; set; } = null;
        public Guid? TokenKeyIdentifier { get; set; } = null;
        public Guid? DataKeyIdentifier { get; set; } = null;
        public string Token { get; set; } = null;
        public List<RunLogEntry> RunLog { get; set; } = new List<RunLogEntry>();
    }
}
