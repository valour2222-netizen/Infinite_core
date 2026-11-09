using UnityEngine;

namespace InfiniteAdventure
{
    /// <summary>
    /// Animator facade shared by player and enemies. Safe to use with no Animator present.
    /// Required Animator parameters: Speed (float), LightAttack, HeavyAttack, Dash, Hurt (triggers), IsDead (bool).
    /// Generate a matching controller via the menu: Game > Create Actor Animator Controller.
    /// </summary>
    public class ActorAnimator : MonoBehaviour
    {
        static readonly int SpeedHash = Animator.StringToHash("Speed");
        static readonly int LightHash = Animator.StringToHash("LightAttack");
        static readonly int HeavyHash = Animator.StringToHash("HeavyAttack");
        static readonly int DashHash = Animator.StringToHash("Dash");
        static readonly int HurtHash = Animator.StringToHash("Hurt");
        static readonly int DeadHash = Animator.StringToHash("IsDead");

        Animator animator;
        bool active;

        void Awake()
        {
            animator = GetComponentInChildren<Animator>();
            active = animator != null && animator.runtimeAnimatorController != null;
        }

        /// <summary>Finds or adds an ActorAnimator. Called once from Awake (pre-warm time), never during gameplay.</summary>
        public static ActorAnimator Resolve(GameObject go, ActorAnimator assigned)
        {
            if (assigned != null) return assigned;
            var found = go.GetComponentInChildren<ActorAnimator>();
            return found != null ? found : go.AddComponent<ActorAnimator>();
        }

        public void SetSpeed(float normalized)
        {
            if (active) animator.SetFloat(SpeedHash, normalized, 0.1f, Time.deltaTime);
        }

        public void Attack(bool heavy) { if (active) animator.SetTrigger(heavy ? HeavyHash : LightHash); }
        public void Dash() { if (active) animator.SetTrigger(DashHash); }
        public void Hurt() { if (active) animator.SetTrigger(HurtHash); }
        public void Die() { if (active) animator.SetBool(DeadHash, true); }

        /// <summary>Called when a pooled actor is reused.</summary>
        public void ResetState()
        {
            if (!active) return;
            animator.ResetTrigger(LightHash);
            animator.ResetTrigger(HeavyHash);
            animator.ResetTrigger(DashHash);
            animator.ResetTrigger(HurtHash);
            animator.SetBool(DeadHash, false);
            animator.SetFloat(SpeedHash, 0f);
        }
    }
}
