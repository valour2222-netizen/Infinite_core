using UnityEngine;

namespace InfiniteAdventure
{
    public struct DamageReport
    {
        public GameObject Target;
        public float Amount;
    }

    [CreateAssetMenu(menuName = "Game/Events/Damage")]
    public class DamageEventChannelSO : EventChannelSO<DamageReport> { }
}
