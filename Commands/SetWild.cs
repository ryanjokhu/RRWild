using Rocket.API;
using Rocket.Unturned.Player;
using System.Collections.Generic;
using UnityEngine;

namespace RRWild.Commands
{
    public class CommandSetWild : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;
        public string Name => "setwild";
        public string Help => "/setwild <index>";
        public string Syntax => "/setwild <index>";
        public List<string> Aliases => new List<string>();
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

            Vector3 pos = p.Player.transform.position;
            float yaw = p.Player.transform.eulerAngles.y;

            plugin.WildPoints.TrySet(index, pos, yaw);
            plugin.WildPoints.Save();

            plugin.T(p, "SetWildSuccess", index);
        }
    }
}
