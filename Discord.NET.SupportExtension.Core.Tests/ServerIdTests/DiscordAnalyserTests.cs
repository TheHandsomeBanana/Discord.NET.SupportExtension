using HB.NETF.Code.Analysis.Tests.Engine;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord.NET.SupportExtension.Core.Tests.AnalyserTests {
    [TestClass]
    public class DiscordAnalyserTests {
        private readonly ServerIdTestEngine serverIdTests = new ServerIdTestEngine();

        public DiscordAnalyserTests() {
            serverIdTests.Initialize(@"Discord.NET.SupportExtension.sln", "Discord.NET.SupportExtension.TestCases");
        }

        [TestCleanup]
        public void Cleanup() {
            serverIdTests.Reset();
        }

        [TestMethod]
        public async Task ServerIdTests() {
            serverIdTests.Add("GuildFindings.cs")
                .Add("ServerId", new ulong[] { 0123456789 });

            await serverIdTests.RunEngineAsync();
        }
    }
}
