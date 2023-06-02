using HB.NETF.Common.DependencyInjection;
using HB.NETF.Services.Logging;
using HB.NETF.Services.Logging.Factory;
using Microsoft;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Internal.VisualStudio.PlatformUI;
using Microsoft.Internal.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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

        public static void ShowInfo(string message, string title) {
            VsShellUtilities.ShowMessageBox(
                Package,
                message,
                title,
                OLEMSGICON.OLEMSGICON_INFO,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }

        public static void ShowWarning(string message, string title) {
            VsShellUtilities.ShowMessageBox(
                Package,
                message,
                title,
                OLEMSGICON.OLEMSGICON_WARNING,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }

        public static void ShowError(string message, string title) {
            VsShellUtilities.ShowMessageBox(
               Package,
               message,
               title,
               OLEMSGICON.OLEMSGICON_CRITICAL,
               OLEMSGBUTTON.OLEMSGBUTTON_OK,
               OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }

        private static IVsOutputWindowPane pane;
        public static void InitOutputLog() {
            ThreadHelper.ThrowIfNotOnUIThread();

            IVsOutputWindow logWindow = AsyncPackage.GetGlobalService(typeof(SVsOutputWindow)) as IVsOutputWindow;
            Guid guid = Guid.Parse("89b42236-72c2-49a3-aba9-db69deb4adce");
            logWindow.CreatePane(ref guid, "Discord Support Extension", 1, 1);
            logWindow.GetPane(ref guid, out pane);

        }

        public static void OutputWindowFunc(LogStatement obj) {
            Func<Task> logTask = new Func<Task>(async () => {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                pane.OutputStringThreadSafe(obj.ToString() + "\n");
            });

            ThreadHelper.JoinableTaskFactory.Run(logTask);
        }
    }
}
