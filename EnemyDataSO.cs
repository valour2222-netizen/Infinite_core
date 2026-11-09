using UnityEngine;

namespace InfiniteAdventure
{
    /// <summary>Flyweight: all shared enemy tuning lives here, not on per-instance scripts.</summary>
    [CreateAssetMenu(menuName = "Game/Enemy Data")]
    public class EnemyDataSO : ScriptableObject
    {
        public EnemyController prefab;
        public float maxHealth = 30f;
        public float moveSpeed = 3.5f, acceleration = 30f, turnSpeed = 540f;
        public float aggroRange = 14f, attackRange = 1.8f;

        [Header("Attack (seconds)")]
        public float windup = 0.45f, recovery = 0.7f;
        public float damage = 10f, knockback = 5f, playerStun = 0.25f;
        public float hitRadius = 1.2f, hitOffset = 1.1f;
        public LayerMask playerMask;

        [Tooltip("Strategy asset: MeleeAttackSO or RangedAttackSO")]
        public EnemyAttackSO attack;

        [Header("Rewards")]
        public int scoreValue = 10;

        [Header("Spawning")]
        public float spawnWeight = 1f;
        [Tooltip("Not eligible until the difficulty multiplier reaches this value")]
        public float minDifficulty = 1f;
    }
}
