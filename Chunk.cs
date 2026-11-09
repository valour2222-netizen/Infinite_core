using UnityEngine;

namespace InfiniteAdventure
{
    /// <summary>Put on each chunk prefab. Pivot must be at the centre of the cell footprint.</summary>
    public class Chunk : MonoBehaviour
    {
        [SerializeField] Transform[] enemySpawnPoints;
        [Tooltip("Optional props toggled on/off by the obstacle-density roll")]
        [SerializeField] GameObject[] obstacles;

        public Transform[] EnemySpawnPoints => enemySpawnPoints;
        public Vector2Int Coord { get; private set; }
        public ChunkDefinitionSO Definition { get; private set; }

        public void Setup(Vector2Int coord, ChunkDefinitionSO definition, Vector3 worldPos, float obstacleDensity)
        {
            Coord = coord;
            Definition = definition;
            transform.SetPositionAndRotation(worldPos, Quaternion.identity);

            for (int i = 0; i < obstacles.Length; i++)
                obstacles[i].SetActive(Random.value < obstacleDensity);
        }
    }
}
