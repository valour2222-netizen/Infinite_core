using UnityEngine;

namespace InfiniteAdventure
{
    public struct ProjectileRequest
    {
        public ProjectileDataSO Data;
        public Vector3 Position;
        public Vector3 Direction;
        public float Damage;
        public GameObject Source;
    }

    [CreateAssetMenu(menuName = "Game/Events/Projectile Request")]
    public class ProjectileRequestEventChannelSO : EventChannelSO<ProjectileRequest> { }
}
