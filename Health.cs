using System;
using UnityEngine;

namespace InfiniteAdventure
{
    public class Health : MonoBehaviour, IDamageable
    {
        [SerializeField] DamageEventChannelSO damageChannel;

        public float Max { get; private set; } = 100f;
        public float Current { get; private set; }
        public bool Dead { get; private set; }
        public bool IsInvincible { get; set; }

        public event Action<DamageInfo> Damaged;
        public event Action Died;

        public void Init(float maxHealth)
        {
            Max = maxHealth;
            Current = maxHealth;
            Dead = false;
            IsInvincible = false;
        }

        public bool TakeDamage(in DamageInfo info)
        {
            if (Dead || IsInvincible) return false;

            Current -= info.Amount;
            if (damageChannel != null)
                damageChannel.Raise(new DamageReport { Target = gameObject, Amount = info.Amount });

            if (Current <= 0f)
            {
                Current = 0f;
                Dead = true;
                Died?.Invoke();
            }
            else
            {
                Damaged?.Invoke(info);
            }
            return true;
        }
    }
}
