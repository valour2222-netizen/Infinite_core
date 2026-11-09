using System.Collections.Generic;
using UnityEngine;

namespace InfiniteAdventure
{
    public class EnemySpawner : MonoBehaviour, IPrewarmable
    {
        [SerializeField] EnemyDataSO[] spawnTable;
        [SerializeField] int prewarmPerType = 24;
        [SerializeField] float baseEnemiesPerChunk = 1.2f;
        [SerializeField] int maxEnemiesPerChunk = 6;
        [SerializeField] float minSpawnDistanceFromPlayer = 10f;

        [Header("Wiring")]
        [SerializeField] TransformAnchorSO player;
        [SerializeField] DifficultyConfigSO difficultyConfig;
        [SerializeField] FloatVariableSO difficulty;
        [SerializeField] ChunkEventChannelSO chunkSpawned;
        [SerializeField] ChunkEventChannelSO chunkDespawned;

        readonly Dictionary<EnemyDataSO, PrefabPool<EnemyController>> pools = new Dictionary<EnemyDataSO, PrefabPool<EnemyController>>();
        readonly Dictionary<Vector2Int, List<EnemyController>> perChunk = new Dictionary<Vector2Int, List<EnemyController>>(128);
        readonly Stack<List<EnemyController>> listPool = new Stack<List<EnemyController>>();
        System.Action<EnemyController> onEnemyDespawn;
        bool prewarmed;

        void Awake() => onEnemyDespawn = ReleaseEnemy; // cached once: no per-spawn delegate allocation

        void OnEnable()
        {
            chunkSpawned.Raised += OnChunkSpawned;
            chunkDespawned.Raised += OnChunkDespawned;
        }

        void OnDisable()
        {
            chunkSpawned.Raised -= OnChunkSpawned;
            chunkDespawned.Raised -= OnChunkDespawned;
        }

        public void Prewarm()
        {
            foreach (var data in spawnTable)
            {
                if (data == null || data.prefab == null || pools.ContainsKey(data)) continue;
                var pool = new PrefabPool<EnemyController>(data.prefab, transform, prewarmPerType, 1024);
                pool.Prewarm(prewarmPerType);
                pools.Add(data, pool);
            }
            for (int i = 0; i < 64; i++) listPool.Push(new List<EnemyController>(maxEnemiesPerChunk));
            prewarmed = true;
        }

        void OnChunkSpawned(Chunk chunk)
        {
            if (!prewarmed) return;
            var points = chunk.EnemySpawnPoints;
            if (points == null || points.Length == 0) return;

            float m = difficulty.Value;
            float expected = baseEnemiesPerChunk * difficultyConfig.SpawnScale(m);
            int count = Mathf.FloorToInt(expected);
            if (Random.value < expected - count) count++;
            count = Mathf.Min(count, Mathf.Min(points.Length, maxEnemiesPerChunk));
            if (count <= 0) return;

            float speedScale = difficultyConfig.SpeedScale(m);
            float damageScale = difficultyConfig.DamageScale(m);
            int start = Random.Range(0, points.Length);
            List<EnemyController> list = null;

            for (int i = 0; i < count; i++)
            {
                Transform p = points[(start + i) % points.Length];

                if (player.Value != null)
                {
                    Vector3 d = player.Value.position - p.position;
                    d.y = 0f;
                    if (d.sqrMagnitude < minSpawnDistanceFromPlayer * minSpawnDistanceFromPlayer) continue;
                }

                EnemyDataSO data = PickData(m);
                if (data == null) return;

                EnemyController enemy = pools[data].Get();
                enemy.DespawnRequested = onEnemyDespawn;
                enemy.Init(data, player, speedScale, damageScale, chunk.Coord, p.position, p.rotation);

                if (list == null)
                {
                    list = listPool.Count > 0 ? listPool.Pop() : new List<EnemyController>(maxEnemiesPerChunk);
                    perChunk[chunk.Coord] = list;
                }
                list.Add(enemy);
            }
        }

        void OnChunkDespawned(Chunk chunk)
        {
            if (!perChunk.TryGetValue(chunk.Coord, out var list)) return;
            for (int i = list.Count - 1; i >= 0; i--)
            {
                var e = list[i];
                pools[e.Data].Release(e);
            }
            list.Clear();
            perChunk.Remove(chunk.Coord);
            listPool.Push(list);
        }

        void ReleaseEnemy(EnemyController e)
        {
            if (perChunk.TryGetValue(e.ChunkCoord, out var list)) list.Remove(e);
            pools[e.Data].Release(e);
        }

        EnemyDataSO PickData(float m)
        {
            float total = 0f;
            for (int i = 0; i < spawnTable.Length; i++)
                if (Eligible(spawnTable[i], m)) total += spawnTable[i].spawnWeight;
            if (total <= 0f) return null;

            float roll = Random.value * total;
            for (int i = 0; i < spawnTable.Length; i++)
            {
                if (!Eligible(spawnTable[i], m)) continue;
                roll -= spawnTable[i].spawnWeight;
                if (roll <= 0f) return spawnTable[i];
            }
            return null;
        }

        bool Eligible(EnemyDataSO d, float m) => d != null && pools.ContainsKey(d) && m >= d.minDifficulty;
    }
}
