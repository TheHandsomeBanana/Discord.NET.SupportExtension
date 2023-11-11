using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord.NET.SupportExtension.Models.VMModels {
    public class TokenEntryModel {
        public string Token { get; set; }
        public bool IsCanceled { get; set; } = false;
    }
}
