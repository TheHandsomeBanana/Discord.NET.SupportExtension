using HB.NETF.Common.DependencyInjection;
using HB.NETF.Services.Logging;
using HB.NETF.Services.Logging.Factory;
using Microsoft;
using Microsoft.Internal.VisualStudio.PlatformUI;
using Microsoft.Internal.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Discord.NET.SupportExtension.Helper {
    internal static class UIHelper {
        public static IServiceProvider Package { get; set; }

        public static void Show(System.Windows.Window window) {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();

            IVsUIShell uiShell = (IVsUIShell)Package.GetService(typeof(IVsUIShell));
            Assumes.Present(uiShell);
            
            uiShell.GetDialogOwnerHwnd(out IntPtr hwnd);
            WindowHelper.ShowModal(window, hwnd);
        }
    }
}
