using Rocket.Core.Plugins;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using RRWild.Services;
using SDG.Unturned;
using Steamworks;
using System.Collections.Generic;
using UnityEngine;
using Logger = Rocket.Core.Logging.Logger;
using Rocket.API.Collections;
using System.Collections;
using RRWild.Models;

namespace RRWild
{
    public class RRWildPlugin : RocketPlugin<RRWildConfiguration>
    {
        public static RRWildPlugin Instance;

        internal WildPointStore WildPoints { get; private set; }
        internal ProtectionManager Protection { get; private set; }

        private readonly Dictionary<ulong, float> _cooldowns = new Dictionary<ulong, float>(256);
        internal readonly List<ulong> TempRemoveIds = new List<ulong>(128);

        private float _nextPoll;

        public override TranslationList DefaultTranslations => new TranslationList
        {
            { "NoWildPoints", "No wild points are set yet." },
            { "SetWildSuccess", "Set wild point {0}." },
            { "RemoveWildSuccess", "Removed wild point {0}." },
            { "WildNotFound", "Wild point {0} doesn't exist." },
            { "ClearWildSuccess", "All wild points have been cleared." },

            { "InvalidIndex", "Invalid index." },
            { "TeleportCooldown", "You can use /wild again in {0}s." },
            { "CantWhileDead", "You can't use this right now." },
            { "CantWhileInVehicle", "Exit your vehicle first." },

            { "SpawnProtectionStart", "You have spawn protection for {0} seconds." },
            { "ProtectionEndedMove", "Spawn protection ended (moved too far)." },
            { "ProtectionEndedEquip", "Spawn protection ended (item equipped)." },
            { "ProtectionExpired", "Spawn protection expired." }
        };

        protected override void Load()
        {
            Instance = this;

            WildPoints = new WildPointStore(Directory);
            WildPoints.Load();

            Protection = new ProtectionManager(this);
            Protection.RefreshFromConfig();

            _nextPoll = Time.realtimeSinceStartup + Protection.PollInterval;

            DamageTool.damagePlayerRequested += OnDamagePlayerRequested;
            Provider.onServerDisconnected += OnPlayerDisconnected;

            Logger.Log($"RRWild loaded. Wild points: {WildPoints.Count}");
        }

        protected override void Unload()
        {
            DamageTool.damagePlayerRequested -= OnDamagePlayerRequested;
            Provider.onServerDisconnected -= OnPlayerDisconnected;

            TempRemoveIds.Clear();
            _cooldowns.Clear();

            Instance = null;
        }

        private void Update()
        {
            if (Protection == null) return;

            float now = Time.realtimeSinceStartup;
            if (now < _nextPoll) return;

            _nextPoll = now + Protection.PollInterval;

            try
            {
                Protection.Tick();
            }
            catch (System.Exception ex)
            {
                Logger.LogError($"[RRWild] Protection.Tick crashed: {ex}");
                _nextPoll = now + 5f;
            }
        }

        internal void T(UnturnedPlayer p, string key, params object[] args)
        {
            if (p == null) return;

            string msg = Translate(key, args);

            ChatManager.serverSendMessage(
                text: msg,
                color: Color.yellow,
                iconURL: null,
                toPlayer: p.SteamPlayer(),
                useRichTextFormatting: true
            );
        }

        internal IEnumerator DelayedWildTeleport(UnturnedPlayer p, WildPoint wp)
        {
            float delay = Mathf.Max(0f, Configuration.Instance.TeleportDelay);

            if (delay > 0f)
                T(p, "TeleportStarting", Mathf.RoundToInt(delay));

            yield return new WaitForSeconds(delay);

            if (p == null || p.Player == null)
                yield break;

            Vector3 target = new Vector3(wp.x, wp.y, wp.z);

            p.Teleport(target, wp.yaw);

            Protection.StartProtection(p, target, wp.yaw);
            T(p, "SpawnProtectionStart", Mathf.RoundToInt(Configuration.Instance.ProtectionSeconds));

            SetCooldown(p);
        }


        internal bool IsOnCooldown(UnturnedPlayer p, out int secondsLeft)
        {
            secondsLeft = 0;

            uint cd = Configuration.Instance.CooldownSeconds;
            if (cd == 0) return false;

            ulong id = p.CSteamID.m_SteamID;

            if (!_cooldowns.TryGetValue(id, out float until))
                return false;

            float now = Time.realtimeSinceStartup;
            if (now >= until)
                return false;

            secondsLeft = Mathf.CeilToInt(until - now);
            return true;
        }

        internal void SetCooldown(UnturnedPlayer p)
        {
            uint cd = Configuration.Instance.CooldownSeconds;
            if (cd == 0) return;

            _cooldowns[p.CSteamID.m_SteamID] = Time.realtimeSinceStartup + cd;
        }

        internal bool TryGetPlayer(ulong steamId, out UnturnedPlayer player)
        {
            player = null;

            var sp = Provider.clients.Find(c => c.playerID.steamID.m_SteamID == steamId);
            if (sp == null) return false;

            player = UnturnedPlayer.FromSteamPlayer(sp);
            return player != null;
        }

        internal Player FindPlayer(ulong steamId)
        {
            var sp = Provider.clients.Find(c => c.playerID.steamID.m_SteamID == steamId);
            return sp?.player;
        }

        private void OnPlayerDisconnected(CSteamID steamId)
        {
            if (Protection != null)
                Protection.EndProtectionById(steamId.m_SteamID);

            _cooldowns.Remove(steamId.m_SteamID);
        }

        private void OnDamagePlayerRequested(ref DamagePlayerParameters parameters, ref bool shouldAllow)
        {
            if (!shouldAllow) return;
            if (Protection == null) return;

            ulong victimId = parameters.player.channel.owner.playerID.steamID.m_SteamID;

            if (Configuration.Instance.BlockDamageTaken && Protection.IsProtected(victimId))
            {
                shouldAllow = false;
                return;
            }

            if (Configuration.Instance.BlockDamageDealt)
            {
                CSteamID killer = parameters.killer;
                if (killer != CSteamID.Nil && Protection.IsProtected(killer.m_SteamID))
                {
                    shouldAllow = false;
                    return;
                }
            }
        }
    }
}
