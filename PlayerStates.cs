using System.Collections.Generic;
using UnityEngine;

namespace InfiniteAdventure
{
    public abstract class PlayerState : IState
    {
        protected readonly PlayerController c;
        protected PlayerState(PlayerController controller) { c = controller; }

        public virtual void Enter() { }
        public virtual void Tick() { }
        public virtual void FixedTick() { }
        public virtual void Exit() { }

        /// <summary>Shared transitions out of locomotion states. Priority: dash, heavy, light.</summary>
        protected bool TryStartAction()
        {
            var s = c.Stats;
            if (c.DashBuffer > 0f && c.CanDash)
            {
                c.DashBuffer = 0f;
                c.Fsm.ChangeState(c.Dash);
                return true;
            }
            if (c.HeavyBuffer > 0f && c.Stamina >= s.heavyAttack.staminaCost)
            {
                c.HeavyBuffer = 0f;
                c.Fsm.ChangeState(c.HeavyAttack);
                return true;
            }
            if (c.LightBuffer > 0f && c.Stamina >= s.lightAttack.staminaCost)
            {
                c.LightBuffer = 0f;
                c.Fsm.ChangeState(c.LightAttack);
                return true;
            }
            return false;
        }
    }

    public sealed class PlayerIdleState : PlayerState
    {
        public PlayerIdleState(PlayerController c) : base(c) { }

        public override void Tick()
        {
            if (TryStartAction()) return;
            if (c.MoveInput.sqrMagnitude > 0.01f) c.Fsm.ChangeState(c.Move);
        }

        public override void FixedTick() => c.MoveWorld(Vector3.zero, 1f);
    }

    public sealed class PlayerMoveState : PlayerState
    {
        public PlayerMoveState(PlayerController c) : base(c) { }

        public override void Tick()
        {
            if (TryStartAction()) return;
            if (c.MoveInput.sqrMagnitude <= 0.01f) c.Fsm.ChangeState(c.Idle);
        }

        public override void FixedTick() => c.MoveWorld(c.WorldMoveDir(), 1f);
    }

    public sealed class PlayerAttackState : PlayerState
    {
        static readonly Collider[] buffer = new Collider[24];
        readonly AttackData data;
        readonly bool heavy;
        readonly List<IDamageable> alreadyHit = new List<IDamageable>(16);
        int tick;

        public PlayerAttackState(PlayerController c, AttackData data, bool heavy) : base(c) { this.data = data; this.heavy = heavy; }

        public override void Enter()
        {
            if (!c.TrySpendStamina(data.staminaCost))
            {
                c.Fsm.ChangeState(c.Idle);
                return;
            }
            tick = 0;
            alreadyHit.Clear();
            c.Anim.Attack(heavy);
        }

        public override void Tick()
        {
            // Dash-cancel during recovery only.
            bool inRecovery = tick > data.startupTicks + data.activeTicks;
            if (inRecovery && c.DashBuffer > 0f && c.CanDash)
            {
                c.DashBuffer = 0f;
                c.Fsm.ChangeState(c.Dash);
            }
        }

        public override void FixedTick()
        {
            tick++;
            c.MoveWorld(c.WorldMoveDir(), data.moveSpeedMultiplier);

            if (tick > data.startupTicks && tick <= data.startupTicks + data.activeTicks)
                CheckHitbox();

            if (tick >= data.startupTicks + data.activeTicks + data.recoveryTicks)
                c.Fsm.ChangeState(c.MoveInput.sqrMagnitude > 0.01f ? (IState)c.Move : c.Idle);
        }

        void CheckHitbox()
        {
            Transform t = c.transform;
            Vector3 origin = t.position + t.forward * data.forwardOffset + Vector3.up * 0.5f;
            int n = Physics.OverlapSphereNonAlloc(origin, data.radius, buffer, c.Stats.enemyMask, QueryTriggerInteraction.Collide);

            for (int i = 0; i < n; i++)
            {
                var target = buffer[i].GetComponentInParent<IDamageable>();
                if (target == null || alreadyHit.Contains(target)) continue;

                alreadyHit.Add(target);
                Vector3 dir = buffer[i].transform.position - t.position;
                dir.y = 0f;
                var info = new DamageInfo
                {
                    Amount = data.damage,
                    Direction = dir.sqrMagnitude > 0.001f ? dir.normalized : t.forward,
                    Knockback = data.knockback,
                    StunDuration = data.stunDuration,
                    Source = c.gameObject
                };
                target.TakeDamage(in info);
            }
        }
    }

    public sealed class PlayerDashState : PlayerState
    {
        Vector3 dir;
        int tick;

        public PlayerDashState(PlayerController c) : base(c) { }

        public override void Enter()
        {
            var d = c.Stats.dash;
            if (!c.TrySpendStamina(d.staminaCost))
            {
                c.Fsm.ChangeState(c.Idle);
                return;
            }
            c.DashCooldownRemaining = d.cooldown;
            dir = c.WorldMoveDir();
            if (dir.sqrMagnitude < 0.01f) dir = c.transform.forward;
            dir.Normalize();
            tick = 0;
            c.Anim.Dash();
        }

        public override void FixedTick()
        {
            var d = c.Stats.dash;
            tick++;
            c.Health.IsInvincible = tick >= d.iFrameStartTick && tick <= d.iFrameEndTick; // I-frames
            c.SetPlanarVelocity(dir * d.speed);
            c.Face(dir);

            if (tick >= d.totalTicks)
            {
                c.SetPlanarVelocity(dir * (d.speed * d.endSpeedMultiplier));
                c.Fsm.ChangeState(c.MoveInput.sqrMagnitude > 0.01f ? (IState)c.Move : c.Idle);
            }
        }

        public override void Exit() => c.Health.IsInvincible = false;
    }

    public sealed class PlayerStunnedState : PlayerState
    {
        float timer;
        public PlayerStunnedState(PlayerController c) : base(c) { }

        public override void Enter() => timer = c.PendingStun;

        public override void FixedTick()
        {
            timer -= Time.fixedDeltaTime;
            if (timer <= 0f) c.Fsm.ChangeState(c.Idle);
        }
    }

    public sealed class PlayerDeadState : PlayerState
    {
        public PlayerDeadState(PlayerController c) : base(c) { }
        public override void Enter() => c.SetPlanarVelocity(Vector3.zero);
        public override void FixedTick() => c.SetPlanarVelocity(Vector3.zero);
    }
}
