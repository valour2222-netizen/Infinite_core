using UnityEngine;

namespace InfiniteAdventure
{
    /// <summary>Strategy pattern: how an enemy resolves its attack. Swap behaviour by swapping the asset.</summary>
    public abstract class EnemyAttackSO : ScriptableObject
    {
        public abstract void Execute(EnemyController enemy);
    }
}
