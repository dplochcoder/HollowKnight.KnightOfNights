using UnityEngine;

namespace KnightOfNights.Scripts.Proxy
{
    public class HealthManagerProxy : MonoBehaviour
    {
        public int hp;

        // If false, don't give soul for this enemy.
        public bool SoulGain = true;

        public bool hasSpecialDeath;
        public bool deathReset;
        public bool damageOverride;
        public bool isDead;

        [Header("Geo Drops")]
        public int TotalGeo;
        [Tooltip("1 geo")]
        public int smallGeoDrops;
        [Tooltip("5 geo")]
        public int mediumGeoDrops;
        [Tooltip("25 geo")]
        public int largeGeoDrops;
    }
}
