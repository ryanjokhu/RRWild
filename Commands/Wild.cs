using Rocket.API;
using Rocket.Unturned.Player;
using RRWild.Models;
using System.Collections.Generic;
using UnityEngine;

namespace RRWild.Commands
{
    public class CommandWild : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;
        public string Name => "wild";
        public string Help => "/wild";
        public string Syntax => "/wild";
        public List<string> Aliases => new List<string>();
        public List<string> Permissions => new List<string> { "rrwild.wild" };

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

            if (p.Player.life.isDead)
            {
                plugin.T(p, "CantWhileDead");
                return;
            }

            if (p.Player.movement.getVehicle() != null)
            {
                plugin.T(p, "CantWhileInVehicle");
                return;
            }

            if (plugin.IsOnCooldown(p, out int left))
            {
                plugin.T(p, "TeleportCooldown", left);
                return;
            }

            if (!plugin.WildPoints.TryGetRandom(out WildPoint wp) || wp == null)
            {
                plugin.T(p, "NoWildPoints");
                return;
            }

            plugin.StartCoroutine(plugin.DelayedWildTeleport(p, wp));
        }
    }
}
