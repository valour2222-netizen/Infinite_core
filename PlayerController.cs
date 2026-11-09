using UnityEngine;
using UnityEngine.InputSystem;

namespace InfiniteAdventure
{
    [RequireComponent(typeof(Rigidbody), typeof(Health))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] PlayerStatsSO stats;
        [SerializeField] TransformAnchorSO anchor;
        [SerializeField] Transform cameraTransform;
        [SerializeField] ActorAnimator animator;
        [SerializeField] VoidEventChannelSO playerDied;

        [Header("Input actions (New Input System)")]
        [SerializeField] InputActionReference moveAction;
        [SerializeField] InputActionReference lightAttackAction;
        [SerializeField] InputActionReference heavyAttackAction;
        [SerializeField] InputActionReference dashAction;

        public PlayerStatsSO Stats => stats;
        public Rigidbody Rb { get; private set; }
        public Health Health { get; private set; }
        public ActorAnimator Anim { get; private set; }
        public StateMachine Fsm { get; } = new StateMachine();

        public PlayerIdleState Idle { get; private set; }
        public PlayerMoveState Move { get; private set; }
        public PlayerAttackState LightAttack { get; private set; }
        public PlayerAttackState HeavyAttack { get; private set; }
        public PlayerDashState Dash { get; private set; }
        public PlayerStunnedState Stunned { get; private set; }
        public PlayerDeadState Dead { get; private set; }

        public Vector2 MoveInput { get; private set; }
        public float LightBuffer, HeavyBuffer, DashBuffer;   // seconds remaining
        public float Stamina { get; private set; }
        public float DashCooldownRemaining { get; set; }
        public float PendingStun { get; private set; }

        float sinceStaminaSpent;

        public bool CanDash => DashCooldownRemaining <= 0f && Stamina >= stats.dash.staminaCost;

        void Awake()
        {
            Rb = GetComponent<Rigidbody>();
            Health = GetComponent<Health>();
            Anim = ActorAnimator.Resolve(gameObject, animator);
            Rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            Rb.interpolation = RigidbodyInterpolation.Interpolate;

            Health.Init(stats.maxHealth);
            Stamina = stats.maxStamina;

            Idle = new PlayerIdleState(this);
            Move = new PlayerMoveState(this);
            LightAttack = new PlayerAttackState(this, stats.lightAttack, false);
            HeavyAttack = new PlayerAttackState(this, stats.heavyAttack, true);
            Dash = new PlayerDashState(this);
            Stunned = new PlayerStunnedState(this);
            Dead = new PlayerDeadState(this);
            Fsm.ChangeState(Idle);
        }

        void OnEnable()
        {
            moveAction.action.Enable();
            lightAttackAction.action.Enable();
            heavyAttackAction.action.Enable();
            dashAction.action.Enable();
            anchor.Set(transform);
            Health.Damaged += OnDamaged;
            Health.Died += OnDied;
        }

        void OnDisable()
        {
            Health.Damaged -= OnDamaged;
            Health.Died -= OnDied;
            anchor.Clear(transform);
        }

        void Update()
        {
            float dt = Time.deltaTime;

            // Input buffering: presses stay valid for a short window so inputs during recovery aren't lost.
            LightBuffer = Mathf.Max(0f, LightBuffer - dt);
            HeavyBuffer = Mathf.Max(0f, HeavyBuffer - dt);
            DashBuffer = Mathf.Max(0f, DashBuffer - dt);
            DashCooldownRemaining = Mathf.Max(0f, DashCooldownRemaining - dt);

            MoveInput = moveAction.action.ReadValue<Vector2>();
            if (lightAttackAction.action.WasPressedThisFrame()) LightBuffer = stats.inputBufferSeconds;
            if (heavyAttackAction.action.WasPressedThisFrame()) HeavyBuffer = stats.inputBufferSeconds;
            if (dashAction.action.WasPressedThisFrame()) DashBuffer = stats.inputBufferSeconds;

            sinceStaminaSpent += dt;
            if (sinceStaminaSpent >= stats.staminaRegenDelay)
                Stamina = Mathf.Min(stats.maxStamina, Stamina + stats.staminaRegenPerSecond * dt);

            Fsm.Tick();

            Vector3 v = Rb.velocity;
            Anim.SetSpeed(new Vector2(v.x, v.z).magnitude / stats.moveSpeed);
        }

        void FixedUpdate() => Fsm.FixedTick();

        public bool TrySpendStamina(float cost)
        {
            if (Stamina < cost) return false;
            Stamina -= cost;
            sinceStaminaSpent = 0f;
            return true;
        }

        public Vector3 WorldMoveDir()
        {
            Vector3 f = Vector3.forward, r = Vector3.right;
            if (cameraTransform != null)
            {
                f = cameraTransform.forward; f.y = 0f; f.Normalize();
                r = cameraTransform.right;   r.y = 0f; r.Normalize();
            }
            return Vector3.ClampMagnitude(f * MoveInput.y + r * MoveInput.x, 1f);
        }

        // Velocity-driven movement (no transform translation).
        public void MoveWorld(Vector3 dir, float speedMultiplier)
        {
            Vector3 v = Rb.velocity;
            Vector3 planar = new Vector3(v.x, 0f, v.z);
            Vector3 target = dir * (stats.moveSpeed * speedMultiplier);
            planar = Vector3.MoveTowards(planar, target, stats.acceleration * Time.fixedDeltaTime);
            Rb.velocity = new Vector3(planar.x, v.y, planar.z);
            Face(dir);
        }

        public void SetPlanarVelocity(Vector3 planar)
        {
            Rb.velocity = new Vector3(planar.x, Rb.velocity.y, planar.z);
        }

        public void Face(Vector3 dir)
        {
            if (dir.sqrMagnitude < 0.001f) return;
            Quaternion target = Quaternion.LookRotation(dir, Vector3.up);
            Rb.MoveRotation(Quaternion.RotateTowards(Rb.rotation, target, stats.turnSpeed * Time.fixedDeltaTime));
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
            Fsm.ChangeState(Dead);
            Anim.Die();
            if (playerDied != null) playerDied.Raise();
        }
    }
}
