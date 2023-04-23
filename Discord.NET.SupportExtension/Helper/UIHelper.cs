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

            IntPtr hwnd;
            uiShell.GetDialogOwnerHwnd(out hwnd);
            uiShell.EnableModeless(0);
            WindowHelper.ShowModal(window, hwnd);
            uiShell.EnableModeless(1);
        }
    }
}
