using Rocket.API;

namespace RRWild
{
    public class RRWildConfiguration : IRocketPluginConfiguration
    {
        public float ProtectionSeconds;
        public float CancelMoveDistanceMeters;

        public bool CancelOnGunEquip;
        public bool CancelOnMeleeEquip;
        public bool CancelOnAnyItemEquip;

        public bool BlockDamageTaken;
        public bool BlockDamageDealt;

        public float PollIntervalSeconds;
        public float TeleportDelay;
        public uint CooldownSeconds;

        public void LoadDefaults()
        {
            ProtectionSeconds = 10f;
            CancelMoveDistanceMeters = 12f;

            CancelOnGunEquip = true;
            CancelOnMeleeEquip = true;
            CancelOnAnyItemEquip = false;

            BlockDamageTaken = true;
            BlockDamageDealt = true;

            PollIntervalSeconds = 0.2f;
            TeleportDelay = 5f;
            CooldownSeconds = 0;
        }
    }
}
