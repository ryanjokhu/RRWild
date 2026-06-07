using Rocket.API;
using Rocket.Unturned.Player;
using System.Collections.Generic;

namespace RRWild.Commands
{
    public class CommandRemoveWild : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;
        public string Name => "removewild";
        public string Help => "/removewild <index>";
        public string Syntax => "/removewild <index>";
        public List<string> Aliases => new List<string> { "delwild" };
        public List<string> Permissions => new List<string> { "rrwild.setwild" };

        public void Execute(IRocketPlayer caller, string[] command)
        {
            var p = (UnturnedPlayer)caller;
            var plugin = RRWildPlugin.Instance;

            if (plugin == null) return;

            if (command.Length != 1 || !int.TryParse(command[0], out int index) || index <= 0)
            {
                plugin.T(p, "InvalidIndex");
                return;
            }

            if (!plugin.WildPoints.Remove(index))
            {
                plugin.T(p, "WildNotFound", index);
                return;
            }

            plugin.WildPoints.Save();
            plugin.T(p, "RemoveWildSuccess", index);
        }
    }
}
