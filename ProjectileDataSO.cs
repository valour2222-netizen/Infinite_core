using UnityEngine;

namespace InfiniteAdventure
{
    [CreateAssetMenu(menuName = "Game/Projectile Data")]
    public class ProjectileDataSO : ScriptableObject
    {
        public Projectile prefab;
        public float speed = 14f;
        [Min(0.1f)] public float lifetime = 3f;
        [Min(0.01f)] public float radius = 0.25f;
        public float knockback = 4f;
        public float stunDuration = 0.15f;

        [Tooltip("Layers whose IDamageable takes the hit")]
        public LayerMask targetMask;
        [Tooltip("Layers that simply stop the projectile (walls, props). Do not include the shooter's own layer.")]
        public LayerMask obstacleMask;

        public VfxDataSO impactVfx;
    }
}
