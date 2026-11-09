using UnityEngine;

namespace InfiniteAdventure
{
    public abstract class EnemyState : IState
    {
        protected readonly EnemyController e;
        protected EnemyState(EnemyController enemy) { e = enemy; }
        public virtual void Enter() { }
        public virtual void Tick() { }
        public virtual void FixedTick() { }
        public virtual void Exit() { }
    }

    public sealed class EnemyIdleState : EnemyState
    {
        public EnemyIdleState(EnemyController e) : base(e) { }

        public override void Tick()
        {
            if (e.DistanceToTarget() <= e.Data.aggroRange) e.Fsm.ChangeState(e.Chase);
        }

        public override void FixedTick() => e.Stop();
    }

    public sealed class EnemyChaseState : EnemyState
    {
        public EnemyChaseState(EnemyController e) : base(e) { }

        public override void Tick()
        {
            float d = e.DistanceToTarget();
            if (d <= e.Data.attackRange) e.Fsm.ChangeState(e.Attack);
            else if (d > e.Data.aggroRange * 1.5f) e.Fsm.ChangeState(e.Idle);
        }

        public override void FixedTick() => e.MoveInDirection(e.DirectionToTarget(), e.MoveSpeed);
    }

    public sealed class EnemyAttackState : EnemyState
    {
        float startTime;
        bool struck;

        public EnemyAttackState(EnemyController e) : base(e) { }

        public override void Enter()
        {
            startTime = Time.time;
            struck = false;
            e.Anim.Attack(false);
        }

        public override void FixedTick()
        {
            e.Stop();
            float t = Time.time - startTime;
            var d = e.Data;

            if (!struck)
            {
                e.Face(e.DirectionToTarget()); // tracks the player during wind-up only
                if (t >= d.windup)
                {
                    struck = true;
                    Strike();
                }
            }
            else if (t >= d.windup + d.recovery)
            {
                e.Fsm.ChangeState(e.Chase);
            }
        }

        void Strike()
        {
            var attack = e.Data.attack;
            if (attack != null) attack.Execute(e);
        }
    }

    public sealed class EnemyStunnedState : EnemyState
    {
        float timer;
        public EnemyStunnedState(EnemyController e) : base(e) { }
        public override void Enter() => timer = e.PendingStun;

        public override void FixedTick()
        {
            timer -= Time.fixedDeltaTime;
            if (timer <= 0f) e.Fsm.ChangeState(e.Chase);
        }
    }
}
