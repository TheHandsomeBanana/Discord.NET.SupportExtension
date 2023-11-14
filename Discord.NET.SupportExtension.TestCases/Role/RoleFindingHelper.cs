using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordISCaseTester.Role {
    internal class RoleFindingHelper {

        public void Test() {
            IDiscordClient client = null;

            RoleFindings findings = new RoleFindings(client.GetGuildAsync(948571888148443156).Result);
            RoleFindings findings1 = new RoleFindings(client.GetGuildAsync(990).Result);
            RoleFindings findings2 = new RoleFindings(client.GetGuildAsync(1234).Result);
        }
    }
}
