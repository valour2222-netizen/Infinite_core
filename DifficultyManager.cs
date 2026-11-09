using UnityEngine;

namespace InfiniteAdventure
{
    public class DifficultyManager : MonoBehaviour
    {
        [SerializeField] DifficultyConfigSO config;
        [SerializeField] TransformAnchorSO player;
        [SerializeField] FloatVariableSO multiplier;
        [SerializeField] VoidEventChannelSO gameReady;
        [SerializeField, Min(0.05f)] float publishInterval = 0.25f;

        float elapsed, distance, nextPublish;
        Vector3 origin;
        bool running;

        void OnEnable() => gameReady.Raised += Begin;
        void OnDisable() => gameReady.Raised -= Begin;

        void Begin()
        {
            origin = player.Value != null ? player.Value.position : Vector3.zero;
            elapsed = distance = 0f;
            running = true;
            multiplier.Set(config.Evaluate(0f, 0f));
        }

        void Update()
        {
            if (!running) return;
            elapsed += Time.deltaTime;

            if (player.Value != null)
            {
                Vector3 delta = player.Value.position - origin;
                delta.y = 0f;
                // Furthest distance reached, so shuffling back and forth can't farm progress.
                distance = Mathf.Max(distance, delta.magnitude);
            }

            if (Time.time >= nextPublish)
            {
                nextPublish = Time.time + publishInterval;
                multiplier.Set(config.Evaluate(elapsed, distance));
            }
        }
    }
}
