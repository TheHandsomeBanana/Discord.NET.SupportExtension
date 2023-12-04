namespace Discord.NET.SupportExtension.TestCases.Guild {
    public static class GuildCustomMethodsExtra {
        public static void GetSocketServerNT1(this IDiscordClient client, ulong id, string s) {
        }

        public static IGuild GetSocketServer1(this IDiscordClient client, ulong id) {
            return client.GetGuildAsync(id).Result;
        }
    }
}
