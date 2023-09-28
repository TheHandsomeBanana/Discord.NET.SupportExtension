using HB.NETF.Discord.NET.Toolkit.EntityService.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord.NET.SupportExtension.Models.VMModels {
    public class TokenEntryModel {
        public ObservableCollection<string> Tokens { get; set; } = new ObservableCollection<string>();
        public bool IsCanceled { get; set; } = false;
    }
}
