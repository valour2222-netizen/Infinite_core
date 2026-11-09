using UnityEngine;

namespace InfiniteAdventure
{
    [CreateAssetMenu(menuName = "Game/Enemy Attacks/Ranged")]
    public class RangedAttackSO : EnemyAttackSO
    {
        public ProjectileDataSO projectile;
        public ProjectileRequestEventChannelSO channel;
        [Min(1)] public int projectileCount = 1;
        [Tooltip("Total fan angle in degrees when firing several projectiles")]
        public float spreadAngle = 20f;
        public float muzzleHeight = 1f, muzzleForward = 0.8f;

        public override void Execute(EnemyController e)
        {
            if (projectile == null || channel == null) return;

            Vector3 aim = e.DirectionToTarget();
            Vector3 muzzle = e.transform.position + Vector3.up * muzzleHeight + aim * muzzleForward;

            for (int i = 0; i < projectileCount; i++)
            {
                float t = projectileCount == 1 ? 0f : (i / (float)(projectileCount - 1)) - 0.5f;
                Vector3 dir = Quaternion.AngleAxis(t * spreadAngle, Vector3.up) * aim;
                channel.Raise(new ProjectileRequest
                {
                    Data = projectile,
                    Position = muzzle,
                    Direction = dir,
                    Damage = e.Damage,
                    Source = e.gameObject
                });
            }
        }
    }
}
