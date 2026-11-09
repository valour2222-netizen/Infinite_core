using UnityEngine;

namespace InfiniteAdventure
{
    public struct DamageInfo
    {
        public float Amount;
        public Vector3 Direction;   // world-space push direction (normalized)
        public float Knockback;     // velocity change applied on hit
        public float StunDuration;  // seconds, 0 = no stagger
        public GameObject Source;
    }

    public interface IDamageable
    {
        /// <returns>true if the hit landed (false while dead or invincible).</returns>
        bool TakeDamage(in DamageInfo info);
    }

    /// <summary>Implemented by systems that build pools; called by GameBootstrapper during the loading screen.</summary>
    public interface IPrewarmable
    {
        void Prewarm();
    }
}
