using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace InfiniteAdventure
{
    /// <summary>Thin wrapper over UnityEngine.Pool.ObjectPool for prefab components.</summary>
    public sealed class PrefabPool<T> where T : Component
    {
        readonly ObjectPool<T> pool;
        readonly T prefab;
        readonly Transform parent;
        readonly List<T> warmBuffer = new List<T>();
        bool prewarmed;

        public PrefabPool(T prefab, Transform parent, int defaultCapacity, int maxSize)
        {
            this.prefab = prefab;
            this.parent = parent;
            pool = new ObjectPool<T>(Create, OnGet, OnRelease, OnDestroyItem,
                collectionCheck: false, defaultCapacity: defaultCapacity, maxSize: maxSize);
        }

        T Create()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (prewarmed)
                Debug.LogWarning($"Pool for '{prefab.name}' grew at runtime. Increase its prewarm count.");
#endif
            var item = Object.Instantiate(prefab, parent);
            item.gameObject.SetActive(false);
            return item;
        }

        static void OnGet(T item) => item.gameObject.SetActive(true);
        static void OnRelease(T item) => item.gameObject.SetActive(false);
        static void OnDestroyItem(T item) { if (item != null) Object.Destroy(item.gameObject); }

        public void Prewarm(int count)
        {
            warmBuffer.Clear();
            for (int i = 0; i < count; i++) warmBuffer.Add(pool.Get());
            for (int i = 0; i < warmBuffer.Count; i++) pool.Release(warmBuffer[i]);
            warmBuffer.Clear();
            prewarmed = true;
        }

        public T Get() => pool.Get();
        public void Release(T item) => pool.Release(item);
    }
}
