using UnityEngine;

namespace InfiniteAdventure
{
    public struct EnemyDeathInfo
    {
        public int Score;
        public Vector3 Position;
    }

    [CreateAssetMenu(menuName = "Game/Events/Enemy Death")]
    public class EnemyDeathEventChannelSO : EventChannelSO<EnemyDeathInfo> { }
}
