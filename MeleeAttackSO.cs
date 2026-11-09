using UnityEngine;

namespace InfiniteAdventure
{
    [CreateAssetMenu(menuName = "Game/Enemy Attacks/Melee")]
    public class MeleeAttackSO : EnemyAttackSO
    {
        static readonly Collider[] buffer = new Collider[8];

        public override void Execute(EnemyController e)
        {
            var d = e.Data;
            Transform tr = e.transform;
            Vector3 origin = tr.position + tr.forward * d.hitOffset + Vector3.up * 0.5f;
            int n = Physics.OverlapSphereNonAlloc(origin, d.hitRadius, buffer, d.playerMask, QueryTriggerInteraction.Collide);

            for (int i = 0; i < n; i++)
            {
                var target = buffer[i].GetComponentInParent<IDamageable>();
                if (target == null) continue;
                var info = new DamageInfo
                {
                    Amount = e.Damage,
                    Direction = tr.forward,
                    Knockback = d.knockback,
                    StunDuration = d.playerStun,
                    Source = e.gameObject
                };
                if (target.TakeDamage(in info)) break; // one hit per swing
            }
        }
    }
}
