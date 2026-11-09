using System.Collections.Generic;
using UnityEngine;

namespace InfiniteAdventure
{
    public class ChunkGenerator : MonoBehaviour, IPrewarmable
    {
        [Header("Grid")]
        [SerializeField] float cellSize = 20f;
        [SerializeField] int seed = 12345;

        [Header("Dynamic render distance (in cells)")]
        [SerializeField] int baseRadius = 3;
        [SerializeField] int maxRadius = 7;
        [SerializeField] float lookAheadSeconds = 2.5f;
        [SerializeField] int despawnMargin = 2;
        [SerializeField] int maxSpawnsPerFrame = 2;

        [Header("Content")]
        [SerializeField] ChunkDefinitionSO[] definitions;
        [SerializeField] int prewarmPerDefinition = 16;
        [Range(0f, 1f)] [SerializeField] float baseObstacleDensity = 0.35f;

        [Header("Wiring")]
        [SerializeField] TransformAnchorSO player;
        [SerializeField] DifficultyConfigSO difficultyConfig;
        [SerializeField] FloatVariableSO difficulty;
        [SerializeField] VoidEventChannelSO gameReady;
        [SerializeField] ChunkEventChannelSO chunkSpawned;
        [SerializeField] ChunkEventChannelSO chunkDespawned;

        static readonly Vector2Int[] Dirs =
            { new Vector2Int(0, 1), new Vector2Int(1, 0), new Vector2Int(0, -1), new Vector2Int(-1, 0) };

        readonly Dictionary<Vector2Int, Chunk> active = new Dictionary<Vector2Int, Chunk>(256);
        readonly Dictionary<ChunkDefinitionSO, PrefabPool<Chunk>> pools = new Dictionary<ChunkDefinitionSO, PrefabPool<Chunk>>();
        readonly List<Vector2Int> removeBuffer = new List<Vector2Int>(64);
        readonly int[] required = new int[4];

        bool ready, dirty;
        Vector2Int lastCell = new Vector2Int(int.MinValue, int.MinValue);
        int lastRadius = -1;
        Vector3 lastPos;
        float smoothedSpeed;

        void OnEnable() => gameReady.Raised += OnReady;
        void OnDisable() => gameReady.Raised -= OnReady;

        void OnReady()
        {
            ready = true;
            if (player.Value != null) lastPos = player.Value.position;
        }

        public void Prewarm()
        {
            foreach (var def in definitions)
            {
                if (def == null || def.prefab == null || pools.ContainsKey(def)) continue;
                var pool = new PrefabPool<Chunk>(def.prefab, transform, prewarmPerDefinition, 1024);
                pool.Prewarm(prewarmPerDefinition);
                pools.Add(def, pool);
#if UNITY_EDITOR
                ValidateFootprint(def);
#endif
            }
        }

#if UNITY_EDITOR
        // Best-effort seam check: warn if a prefab's renderers spill outside its grid cell.
        void ValidateFootprint(ChunkDefinitionSO def)
        {
            var rends = def.prefab.GetComponentsInChildren<Renderer>();
            if (rends.Length == 0) return;
            Bounds b = rends[0].bounds;
            for (int i = 1; i < rends.Length; i++) b.Encapsulate(rends[i].bounds);
            if (b.size.x > cellSize + 0.01f || b.size.z > cellSize + 0.01f)
                Debug.LogWarning($"Chunk '{def.name}' is {b.size.x:F1}x{b.size.z:F1}, larger than the {cellSize} cell. Geometry will overlap.", def);
        }
#endif

        void Update()
        {
            if (!ready || player.Value == null) return;

            Vector3 p = player.Value.position;
            float dt = Time.deltaTime;
            if (dt > 0f)
            {
                float inst = (p - lastPos).magnitude / dt;
                smoothedSpeed = Mathf.Lerp(smoothedSpeed, inst, 1f - Mathf.Exp(-3f * dt));
            }
            lastPos = p;

            // Render distance grows with player speed so fast movement never outruns generation.
            int radius = Mathf.Clamp(
                baseRadius + Mathf.CeilToInt(smoothedSpeed * lookAheadSeconds / cellSize), baseRadius, maxRadius);
            var cell = new Vector2Int(Mathf.FloorToInt(p.x / cellSize), Mathf.FloorToInt(p.z / cellSize));

            if (cell != lastCell || radius != lastRadius)
            {
                lastCell = cell;
                lastRadius = radius;
                dirty = true;
                Despawn(cell, radius + despawnMargin);
            }

            if (dirty) Spawn(cell, radius);
        }

        void Spawn(Vector2Int center, int radius)
        {
            int spawned = 0;
            for (int d = 0; d <= radius; d++)
            {
                for (int x = -d; x <= d; x++)
                {
                    int step = (Mathf.Abs(x) == d) ? 1 : 2 * d; // ring only: full column on the edges, two cells inside
                    for (int y = -d; y <= d; y += step)
                    {
                        var coord = center + new Vector2Int(x, y);
                        if (active.ContainsKey(coord)) continue;
                        if (spawned >= maxSpawnsPerFrame) return; // stay dirty, continue next frame
                        SpawnChunk(coord);
                        spawned++;
                    }
                }
            }
            dirty = false;
        }

        void Despawn(Vector2Int center, int limit)
        {
            removeBuffer.Clear();
            foreach (var kv in active)
            {
                int dist = Mathf.Max(Mathf.Abs(kv.Key.x - center.x), Mathf.Abs(kv.Key.y - center.y));
                if (dist > limit) removeBuffer.Add(kv.Key);
            }

            for (int i = 0; i < removeBuffer.Count; i++)
            {
                var chunk = active[removeBuffer[i]];
                chunkDespawned.Raise(chunk);           // listeners release their children first
                pools[chunk.Definition].Release(chunk);
                active.Remove(removeBuffer[i]);
            }
        }

        void SpawnChunk(Vector2Int coord)
        {
            ChunkDefinitionSO def = PickDefinition(coord);
            if (def == null) return;

            float m = difficulty.Value;
            float density = Mathf.Clamp01(baseObstacleDensity * difficultyConfig.DensityScale(m));
            var worldPos = new Vector3((coord.x + 0.5f) * cellSize, 0f, (coord.y + 0.5f) * cellSize);

            Chunk chunk = pools[def].Get();
            chunk.Setup(coord, def, worldPos, density);
            active.Add(coord, chunk);
            chunkSpawned.Raise(chunk);
        }

        // Socket validation: every edge shared with an existing neighbour must carry the same socket type.
        ChunkDefinitionSO PickDefinition(Vector2Int coord)
        {
            for (int i = 0; i < 4; i++)
            {
                required[i] = active.TryGetValue(coord + Dirs[i], out var n)
                    ? (int)n.Definition.GetSocket((i + 2) % 4)
                    : -1;
            }

            float total = 0f;
            for (int k = 0; k < definitions.Length; k++)
                if (Matches(definitions[k])) total += definitions[k].weight;

            if (total <= 0f)
            {
                Debug.LogWarning($"No chunk definition fits the sockets at {coord}. Add a definition for this socket combination.");
                return null;
            }

            float roll = Hash01(coord.x, coord.y, seed) * total;
            for (int k = 0; k < definitions.Length; k++)
            {
                if (!Matches(definitions[k])) continue;
                roll -= definitions[k].weight;
                if (roll <= 0f) return definitions[k];
            }
            return definitions[definitions.Length - 1];
        }

        bool Matches(ChunkDefinitionSO def)
        {
            if (def == null || !pools.ContainsKey(def)) return false;
            for (int i = 0; i < 4; i++)
                if (required[i] >= 0 && (int)def.GetSocket(i) != required[i]) return false;
            return true;
        }

        static float Hash01(int x, int y, int seed)
        {
            unchecked
            {
                uint h = (uint)(x * 73856093) ^ (uint)(y * 19349663) ^ (uint)(seed * 83492791);
                h ^= h >> 13; h *= 0x5bd1e995u; h ^= h >> 15;
                return (h & 0xFFFFFFu) / (float)0x1000000;
            }
        }
    }
}
