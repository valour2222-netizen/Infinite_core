using System;
using UnityEngine;

namespace InfiniteAdventure
{
    /// <summary>Root component of a pooled effect prefab. Plays all child particle systems, then returns itself to the pool.</summary>
    public class PooledVfx : MonoBehaviour
    {
        ParticleSystem[] systems;
        Action<PooledVfx> release;
        float timer;

        public VfxDataSO Data { get; private set; }

        void Awake() => systems = GetComponentsInChildren<ParticleSystem>(true);

        public void Play(VfxDataSO data, Vector3 position, Quaternion rotation, Action<PooledVfx> onFinished)
        {
            Data = data;
            release = onFinished;
            timer = data.lifetime;
            transform.SetPositionAndRotation(position, rotation);
            for (int i = 0; i < systems.Length; i++)
            {
                systems[i].Clear(false);
                systems[i].Play(false);
            }
        }

        void Update()
        {
            timer -= Time.deltaTime;
            if (timer <= 0f) release?.Invoke(this);
        }
    }
}
