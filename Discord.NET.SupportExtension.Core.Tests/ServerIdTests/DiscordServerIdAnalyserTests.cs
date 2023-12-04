using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace Discord.NET.SupportExtension.Core.Tests.AnalyserTests {
    [TestClass]
    public class DiscordServerIdAnalyserTests {
        private readonly ServerIdTestEngine serverIdTests = new ServerIdTestEngine();

        public DiscordServerIdAnalyserTests() {
            serverIdTests.Initialize(@"../../../Discord.NET.SupportExtension.sln", "Discord.NET.SupportExtension.TestCases");
        }

        [TestCleanup]
        public void Cleanup() {
            serverIdTests.Reset();
        }

        [TestMethod]
        public async Task Channel_PositiveTests() {
            serverIdTests.Add("ChannelServerId.cs")
                .Add("0000", new ulong[] { 0000 })
                .Add("0001", new ulong[] { 0001 })
                .Add("0002", new ulong[] { 0002 })
                .Add("0003", new ulong[] { 0000 })
                .Add("9999", new ulong[] { 9999 })
                .Add("9998", new ulong[] { 9998 });

            await serverIdTests.RunEngineAsync();
        }

        [TestMethod]
        public async Task User_PositiveTests() {
            serverIdTests.Add("UserServerId.cs")
                .Add("0000", new ulong[] { 0000 })
                .Add("0001", new ulong[] { 0001 })
                .Add("0002", new ulong[] { 0002 })
                .Add("0003", new ulong[] { 0000 })
                .Add("9999", new ulong[] { 9999 })
                .Add("9998", new ulong[] { 9998 });

            await serverIdTests.RunEngineAsync();
        }

        [TestMethod]
        public async Task Role_PositiveTests() {
            serverIdTests.Add("RoleServerId.cs")
                .Add("0000", new ulong[] { 0000 })
                .Add("0001", new ulong[] { 0001 })
                .Add("0002", new ulong[] { 0002 })
                .Add("0003", new ulong[] { 0000 })
                .Add("9999", new ulong[] { 9999 })
                .Add("9998", new ulong[] { 9998 })
                .Add("9997", new ulong[] { 99997 });

            await serverIdTests.RunEngineAsync();
        }
    }
}
