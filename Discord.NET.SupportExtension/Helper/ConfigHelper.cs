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
            if (!string.IsNullOrWhiteSpace(project.FullName))
                return Path.GetDirectoryName(project.FullName) + "\\" + DiscordServerImageConfig;
            else if (!string.IsNullOrWhiteSpace(project.Name))
                return Path.GetDirectoryName(project.Name) + "\\" + DiscordServerImageConfig;
            else
                return "undefined";

        }
    }
}
