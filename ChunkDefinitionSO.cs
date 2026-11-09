using UnityEngine;

namespace InfiniteAdventure
{
    public enum SocketType { Open, Corridor, Wall }

    /// <summary>Flyweight data for a chunk type. Edge sockets: 0=N(+Z) 1=E(+X) 2=S(-Z) 3=W(-X).</summary>
    [CreateAssetMenu(menuName = "Game/Chunk Definition")]
    public class ChunkDefinitionSO : ScriptableObject
    {
        public Chunk prefab;
        public SocketType north, east, south, west;
        [Min(0.01f)] public float weight = 1f;

        public SocketType GetSocket(int dir)
        {
            switch (dir)
            {
                case 0: return north;
                case 1: return east;
                case 2: return south;
                default: return west;
            }
        }
    }
}
