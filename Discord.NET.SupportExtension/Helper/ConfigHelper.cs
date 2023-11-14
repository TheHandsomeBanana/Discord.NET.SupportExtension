using EnvDTE;
using HB.NETF.VisualStudio.Workspace;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord.NET.SupportExtension.Helper {
    public static class ConfigHelper {
        public const string DiscordServerImageConfig = "DiscordServerImageConfig.json";
        public static string GetConfigPath() => GetConfigPath(SolutionHelper.GetCurrentProject());
        public static string GetConfigPath(Project project) {
            string path = "";
            if (!string.IsNullOrWhiteSpace(project.FullName))
                path = Path.GetDirectoryName(project.FullName) + "\\" + DiscordServerImageConfig;

            return path;
        }
    }
}
