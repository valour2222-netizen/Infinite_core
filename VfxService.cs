using System.Collections.Generic;
using UnityEngine;

namespace InfiniteAdventure
{
    /// <summary>Pools and plays all visual effects. Driven entirely by event channels.</summary>
    public class VfxService : MonoBehaviour, IPrewarmable
    {
        [SerializeField] VfxDataSO[] vfxTypes;
        [SerializeField] int prewarmPerType = 16;

        [Header("Automatic effects (optional)")]
        [SerializeField] VfxDataSO hitVfx;
        [SerializeField] VfxDataSO deathVfx;
        [SerializeField] float hitHeight = 1f;

        [Header("Channels")]
        [SerializeField] VfxRequestEventChannelSO requests;
        [SerializeField] DamageEventChannelSO damageChannel;
        [SerializeField] EnemyDeathEventChannelSO enemyDeathChannel;

        readonly Dictionary<VfxDataSO, PrefabPool<PooledVfx>> pools = new Dictionary<VfxDataSO, PrefabPool<PooledVfx>>();
        System.Action<PooledVfx> onFinished;
        bool ready;

        void Awake() => onFinished = Release;

        void OnEnable()
        {
            if (requests != null) requests.Raised += OnRequest;
            if (damageChannel != null) damageChannel.Raised += OnDamage;
            if (enemyDeathChannel != null) enemyDeathChannel.Raised += OnEnemyDeath;
        }

        void OnDisable()
        {
            if (requests != null) requests.Raised -= OnRequest;
            if (damageChannel != null) damageChannel.Raised -= OnDamage;
            if (enemyDeathChannel != null) enemyDeathChannel.Raised -= OnEnemyDeath;
        }

        public void Prewarm()
        {
            foreach (var d in vfxTypes) EnsurePool(d);
            EnsurePool(hitVfx);
            EnsurePool(deathVfx);
            ready = true;
        }

        void EnsurePool(VfxDataSO data)
        {
            if (data == null || data.prefab == null || pools.ContainsKey(data)) return;
            var pool = new PrefabPool<PooledVfx>(data.prefab, transform, prewarmPerType, 512);
            pool.Prewarm(prewarmPerType);
            pools.Add(data, pool);
        }

        void OnRequest(VfxRequest r) => Play(r.Data, r.Position, r.Rotation);

        void OnDamage(DamageReport r)
        {
            if (r.Target == null) return;
            Play(hitVfx, r.Target.transform.position + Vector3.up * hitHeight, Quaternion.identity);
        }

        void OnEnemyDeath(EnemyDeathInfo info) => Play(deathVfx, info.Position, Quaternion.identity);

        void Play(VfxDataSO data, Vector3 position, Quaternion rotation)
        {
            if (!ready || data == null) return;
            if (!pools.TryGetValue(data, out var pool))
            {
                Debug.LogWarning($"VFX '{data.name}' is not in VfxService.vfxTypes, so it has no pool.", data);
                return;
            }
            pool.Get().Play(data, position, rotation, onFinished);
        }

        void Release(PooledVfx v) => pools[v.Data].Release(v);
    }
}
