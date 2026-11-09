using UnityEngine;

namespace InfiniteAdventure
{
    [CreateAssetMenu(menuName = "Game/Vfx Data")]
    public class VfxDataSO : ScriptableObject
    {
        public PooledVfx prefab;
        [Min(0.05f)] public float lifetime = 1.5f;
    }
}
