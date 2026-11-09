using System;
using UnityEngine;

namespace InfiniteAdventure
{
    [RequireComponent(typeof(Rigidbody), typeof(Health))]
    public class EnemyController : MonoBehaviour
    {
        [SerializeField] EnemyDeathEventChannelSO deathChannel;
        [SerializeField] ActorAnimator animator;

        public EnemyDataSO Data { get; private set; }
        public TransformAnchorSO Target { get; private set; }
        public Rigidbody Rb { get; private set; }
        public Health Health { get; private set; }
        public ActorAnimator Anim { get; private set; }
        public StateMachine Fsm { get; } = new StateMachine();
        public Vector2Int ChunkCoord { get; private set; }
        public float MoveSpeed { get; private set; }
        public float Damage { get; private set; }
        public float PendingStun { get; private set; }
        public Action<EnemyController> DespawnRequested;

        public EnemyIdleState Idle { get; private set; }
        public EnemyChaseState Chase { get; private set; }
        public EnemyAttackState Attack { get; private set; }
        public EnemyStunnedState Stunned { get; private set; }

        void Awake()
        {
            Rb = GetComponent<Rigidbody>();
            Health = GetComponent<Health>();
            Anim = ActorAnimator.Resolve(gameObject, animator);
            Rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            Idle = new EnemyIdleState(this);
            Chase = new EnemyChaseState(this);
            Attack = new EnemyAttackState(this);
            Stunned = new EnemyStunnedState(this);
        }

        void OnEnable()
        {
            Health.Damaged += OnDamaged;
            Health.Died += OnDied;
        }

        void OnDisable()
        {
            Health.Damaged -= OnDamaged;
            Health.Died -= OnDied;
        }

        public void Init(EnemyDataSO data, TransformAnchorSO target, float speedScale, float damageScale,
                         Vector2Int chunkCoord, Vector3 position, Quaternion rotation)
        {
            Data = data;
            Target = target;
            ChunkCoord = chunkCoord;
            MoveSpeed = data.moveSpeed * speedScale;
            Damage = data.damage * damageScale;

            transform.SetPositionAndRotation(position, rotation);
            Rb.position = position;
            Rb.rotation = rotation;
            Rb.velocity = Vector3.zero;
            Health.Init(data.maxHealth);
            Anim.ResetState();
            Fsm.ChangeState(Idle);
        }

        void Update()
        {
            Fsm.Tick();
            Vector3 v = Rb.velocity;
            Anim.SetSpeed(new Vector2(v.x, v.z).magnitude / Mathf.Max(0.01f, MoveSpeed));
        }
        void FixedUpdate() => Fsm.FixedTick();

        public float DistanceToTarget()
        {
            if (Target == null || Target.Value == null) return float.MaxValue;
            Vector3 d = Target.Value.position - transform.position;
            d.y = 0f;
            return d.magnitude;
        }

        public Vector3 DirectionToTarget()
        {
            if (Target == null || Target.Value == null) return transform.forward;
            Vector3 d = Target.Value.position - transform.position;
            d.y = 0f;
            return d.sqrMagnitude > 0.001f ? d.normalized : transform.forward;
        }

        public void MoveInDirection(Vector3 dir, float speed)
        {
            Vector3 v = Rb.velocity;
            Vector3 planar = Vector3.MoveTowards(new Vector3(v.x, 0f, v.z), dir * speed, Data.acceleration * Time.fixedDeltaTime);
            Rb.velocity = new Vector3(planar.x, v.y, planar.z);
            Face(dir);
        }

        public void Stop() => MoveInDirection(Vector3.zero, 0f);

        public void Face(Vector3 dir)
        {
            if (dir.sqrMagnitude < 0.001f) return;
            Rb.MoveRotation(Quaternion.RotateTowards(Rb.rotation, Quaternion.LookRotation(dir, Vector3.up), Data.turnSpeed * Time.fixedDeltaTime));
        }

        void OnDamaged(DamageInfo info)
        {
            Rb.AddForce(info.Direction * info.Knockback, ForceMode.VelocityChange);
            Anim.Hurt();
            if (info.StunDuration > 0f)
            {
                PendingStun = info.StunDuration;
                Fsm.ChangeState(Stunned);
            }
        }

        void OnDied()
        {
            Anim.Die();
            if (deathChannel != null)
                deathChannel.Raise(new EnemyDeathInfo { Score = Data.scoreValue, Position = transform.position });
            DespawnRequested?.Invoke(this);
        }
    }
}
