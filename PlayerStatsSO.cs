using System;
using UnityEngine;

namespace InfiniteAdventure
{
    // Timings are in FixedUpdate ticks so hitboxes are deterministic (50 ticks/s at the default 0.02 fixed step).
    [Serializable]
    public struct AttackData
    {
        public float damage, staminaCost;
        public int startupTicks, activeTicks, recoveryTicks;
        public float radius, forwardOffset, knockback, stunDuration;
        [Range(0f, 1f)] public float moveSpeedMultiplier;
    }

    [Serializable]
    public struct DashData
    {
        public float speed, staminaCost, cooldown;
        public int totalTicks, iFrameStartTick, iFrameEndTick;
        [Range(0f, 1f)] public float endSpeedMultiplier;
    }

    [CreateAssetMenu(menuName = "Game/Player Stats")]
    public class PlayerStatsSO : ScriptableObject
    {
        public float maxHealth = 100f;
        public float moveSpeed = 6f, acceleration = 60f, turnSpeed = 720f;
        public float maxStamina = 100f, staminaRegenPerSecond = 25f, staminaRegenDelay = 0.6f;
        public float inputBufferSeconds = 0.15f;
        public AttackData lightAttack, heavyAttack;
        public DashData dash;
        public LayerMask enemyMask;

        void Reset()
        {
            lightAttack = new AttackData { damage = 10, staminaCost = 10, startupTicks = 6, activeTicks = 4, recoveryTicks = 8,
                radius = 1.1f, forwardOffset = 1.2f, knockback = 3, stunDuration = 0.15f, moveSpeedMultiplier = 0.3f };
            heavyAttack = new AttackData { damage = 28, staminaCost = 25, startupTicks = 16, activeTicks = 5, recoveryTicks = 18,
                radius = 1.5f, forwardOffset = 1.4f, knockback = 8, stunDuration = 0.4f, moveSpeedMultiplier = 0.1f };
            dash = new DashData { speed = 16, staminaCost = 20, cooldown = 0.5f, totalTicks = 12,
                iFrameStartTick = 1, iFrameEndTick = 8, endSpeedMultiplier = 0.3f };
        }
    }
}
