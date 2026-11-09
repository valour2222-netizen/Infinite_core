using System.Collections.Generic;
using UnityEngine;

namespace InfiniteAdventure
{
    public class ProjectileSpawner : MonoBehaviour, IPrewarmable
    {
        [SerializeField] ProjectileDataSO[] projectileTypes;
        [SerializeField] int prewarmPerType = 32;
        [SerializeField] ProjectileRequestEventChannelSO requests;
        [SerializeField] VfxRequestEventChannelSO vfxChannel;

        readonly Dictionary<ProjectileDataSO, PrefabPool<Projectile>> pools = new Dictionary<ProjectileDataSO, PrefabPool<Projectile>>();
        System.Action<Projectile> onFinished;
        bool ready;

        void Awake() => onFinished = Release;
        void OnEnable() => requests.Raised += OnRequest;
        void OnDisable() => requests.Raised -= OnRequest;

        public void Prewarm()
        {
            foreach (var d in projectileTypes)
            {
                if (d == null || d.prefab == null || pools.ContainsKey(d)) continue;
                var pool = new PrefabPool<Projectile>(d.prefab, transform, prewarmPerType, 1024);
                pool.Prewarm(prewarmPerType);
                pools.Add(d, pool);
            }
            ready = true;
        }

        void OnRequest(ProjectileRequest r)
        {
            if (!ready || r.Data == null) return;
            if (!pools.TryGetValue(r.Data, out var pool))
            {
                Debug.LogWarning($"Projectile '{r.Data.name}' is not in ProjectileSpawner.projectileTypes, so it has no pool.", r.Data);
                return;
            }
            pool.Get().Launch(r.Data, r.Position, r.Direction, r.Damage, r.Source, onFinished, vfxChannel);
        }

        void Release(Projectile p) => pools[p.Data].Release(p);
    }
}
