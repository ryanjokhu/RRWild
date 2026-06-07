using Rocket.API;
using Rocket.Unturned.Player;
using System.Collections.Generic;

namespace RRWild.Commands
{
    public class CommandClearWild : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;
        public string Name => "clearwild";
        public string Help => "/clearwild";
        public string Syntax => "/clearwild";
        public List<string> Aliases => new List<string>();
        public List<string> Permissions => new List<string> { "rrwild.setwild" };

        public void Execute(IRocketPlayer caller, string[] command)
        {
            var p = (UnturnedPlayer)caller;
            var plugin = RRWildPlugin.Instance;

            if (plugin == null) return;

            if (plugin.WildPoints.Count == 0)
            {
                plugin.T(p, "NoWildPoints");
                return;
            }

            plugin.WildPoints.Clear();
            plugin.WildPoints.Save();

            plugin.T(p, "ClearWildSuccess");
        }
    }
}
