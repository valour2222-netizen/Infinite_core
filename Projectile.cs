using System;
using UnityEngine;

namespace InfiniteAdventure
{
    /// <summary>Swept-sphere projectile (no tunnelling at high speed). Pooled; never destroyed.</summary>
    public class Projectile : MonoBehaviour
    {
        ProjectileDataSO data;
        VfxRequestEventChannelSO vfxChannel;
        Action<Projectile> release;
        GameObject source;
        Vector3 direction;
        float damage, life;

        public ProjectileDataSO Data => data;

        public void Launch(ProjectileDataSO d, Vector3 position, Vector3 dir, float dmg, GameObject src,
                           Action<Projectile> onFinished, VfxRequestEventChannelSO vfx)
        {
            data = d;
            direction = dir.normalized;
            damage = dmg;
            source = src;
            release = onFinished;
            vfxChannel = vfx;
            life = d.lifetime;
            transform.SetPositionAndRotation(position, Quaternion.LookRotation(direction));
        }

        void FixedUpdate()
        {
            float step = data.speed * Time.fixedDeltaTime;
            int mask = data.targetMask.value | data.obstacleMask.value;

            if (Physics.SphereCast(transform.position, data.radius, direction, out RaycastHit hit, step, mask, QueryTriggerInteraction.Ignore))
            {
                OnHit(hit);
                return;
            }

            transform.position += direction * step;
            life -= Time.fixedDeltaTime;
            if (life <= 0f) release(this);
        }

        void OnHit(RaycastHit hit)
        {
            var target = hit.collider.GetComponentInParent<IDamageable>();
            if (target != null)
            {
                var info = new DamageInfo
                {
                    Amount = damage,
                    Direction = direction,
                    Knockback = data.knockback,
                    StunDuration = data.stunDuration,
                    Source = source
                };
                target.TakeDamage(in info);
            }

            if (data.impactVfx != null && vfxChannel != null)
            {
                vfxChannel.Raise(new VfxRequest
                {
                    Data = data.impactVfx,
                    Position = hit.point,
                    Rotation = Quaternion.LookRotation(hit.normal)
                });
            }
            release(this);
        }
    }
}
