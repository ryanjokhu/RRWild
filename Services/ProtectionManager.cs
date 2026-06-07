using Rocket.Unturned.Player;
using RRWild.Models;
using SDG.Unturned;
using System.Collections.Generic;
using UnityEngine;

namespace RRWild.Services
{
    public sealed class ProtectionManager
    {
        private readonly RRWildPlugin _plugin;
        private readonly Dictionary<ulong, ProtectionState> _protected = new Dictionary<ulong, ProtectionState>(128);

        private float _cancelMoveDistSqr;
        private float _pollInterval;
        private float _protectionSeconds;

        public ProtectionManager(RRWildPlugin plugin)
        {
            _plugin = plugin;
            RefreshFromConfig();
        }

        public void RefreshFromConfig()
        {
            var cfg = _plugin?.Configuration?.Instance;
            if (cfg == null)
            {
                _protectionSeconds = 0f;
                _cancelMoveDistSqr = 0f;
                _pollInterval = 0.2f;
                return;
            }

            _protectionSeconds = Mathf.Max(0f, cfg.ProtectionSeconds);

            float d = Mathf.Max(0f, cfg.CancelMoveDistanceMeters);
            _cancelMoveDistSqr = d * d;

            _pollInterval = Mathf.Clamp(cfg.PollIntervalSeconds, 0.1f, 1.0f);
        }

        public bool IsProtected(ulong steamId) => _protected.ContainsKey(steamId);

        public void StartProtection(UnturnedPlayer player, Vector3 anchorPos, float anchorYaw)
        {
            if (player == null) return;
            if (_protectionSeconds <= 0f) return;

            ulong id = player.CSteamID.m_SteamID;

            _protected[id] = new ProtectionState
            {
                AnchorPosition = anchorPos,
                AnchorYaw = anchorYaw,
                EndTime = Time.realtimeSinceStartup + _protectionSeconds
            };
        }

        public void EndProtection(UnturnedPlayer player, string reasonTranslationKeyOrNull)
        {
            if (player == null) return;

            ulong id = player.CSteamID.m_SteamID;
            if (!_protected.Remove(id)) return;

            if (!string.IsNullOrEmpty(reasonTranslationKeyOrNull))
                _plugin.T(player, reasonTranslationKeyOrNull);
        }

        public void EndProtectionById(ulong steamId)
        {
            _protected.Remove(steamId);
        }

        public float PollInterval => _pollInterval;

        public void Tick()
        {
            if (_protected.Count == 0) return;

            if (_plugin == null) return;

            var cfg = _plugin.Configuration?.Instance;
            if (cfg == null) return;

            if (_plugin.TempRemoveIds == null) return;

            float now = Time.realtimeSinceStartup;

            _plugin.TempRemoveIds.Clear();

            foreach (var kv in _protected)
            {
                ulong steamId = kv.Key;
                ProtectionState st = kv.Value;

                Player p = _plugin.FindPlayer(steamId);
                if (p == null || p.transform == null)
                {
                    _plugin.TempRemoveIds.Add(steamId);
                    continue;
                }

                if (p.life != null && p.life.isDead)
                {
                    _plugin.TempRemoveIds.Add(steamId);
                    continue;
                }

                if (now >= st.EndTime)
                {
                    _plugin.TempRemoveIds.Add(steamId);

                    if (_plugin.TryGetPlayer(steamId, out var up))
                        _plugin.T(up, "ProtectionExpired");

                    continue;
                }

                if (_cancelMoveDistSqr > 0.0001f)
                {
                    Vector3 cur = p.transform.position;
                    Vector3 delta = cur - st.AnchorPosition;

                    if (delta.sqrMagnitude > _cancelMoveDistSqr)
                    {
                        _plugin.TempRemoveIds.Add(steamId);

                        if (_plugin.TryGetPlayer(steamId, out var up))
                            _plugin.T(up, "ProtectionEndedMove");

                        continue;
                    }
                }

                if (cfg.CancelOnAnyItemEquip || cfg.CancelOnGunEquip || cfg.CancelOnMeleeEquip)
                {
                    ItemAsset asset = p.equipment != null ? p.equipment.asset : null;

                    if (asset != null && ShouldCancelOnEquip(cfg, asset))
                    {
                        _plugin.TempRemoveIds.Add(steamId);

                        if (_plugin.TryGetPlayer(steamId, out var up))
                            _plugin.T(up, "ProtectionEndedEquip");

                        continue;
                    }
                }
            }

            for (int i = 0; i < _plugin.TempRemoveIds.Count; i++)
                _protected.Remove(_plugin.TempRemoveIds[i]);
        }

        private static bool ShouldCancelOnEquip(RRWildConfiguration cfg, ItemAsset asset)
        {
            if (asset == null) return false;

            if (cfg.CancelOnAnyItemEquip) return true;

            if (cfg.CancelOnGunEquip && asset is ItemGunAsset) return true;
            if (cfg.CancelOnMeleeEquip && asset is ItemMeleeAsset) return true;

            return false;
        }
    }
}
