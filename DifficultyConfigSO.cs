using UnityEngine;

namespace InfiniteAdventure
{
    /// <summary>
    /// M(t, d) = min( exp(timeRate * t + distanceRate * d), maxMultiplier )
    /// Each gameplay stat scales as M ^ exponent so they can ramp at different speeds.
    /// </summary>
    [CreateAssetMenu(menuName = "Game/Difficulty Config")]
    public class DifficultyConfigSO : ScriptableObject
    {
        [Min(0)] public float timeRate = 0.002f;      // per second
        [Min(0)] public float distanceRate = 0.001f;  // per world unit
        [Min(1)] public float maxMultiplier = 8f;

        [Header("Per-stat exponents (scale = M ^ exponent)")]
        public float spawnRateExponent = 1f;
        public float speedExponent = 0.25f;
        public float damageExponent = 0.5f;
        public float obstacleDensityExponent = 0.6f;
        [Min(1)] public float maxSpeedScale = 2f;

        public float Evaluate(float t, float d) =>
            Mathf.Min(Mathf.Exp(timeRate * t + distanceRate * d), maxMultiplier);

        public float SpawnScale(float m) => Mathf.Pow(m, spawnRateExponent);
        public float SpeedScale(float m) => Mathf.Min(Mathf.Pow(m, speedExponent), maxSpeedScale);
        public float DamageScale(float m) => Mathf.Pow(m, damageExponent);
        public float DensityScale(float m) => Mathf.Pow(m, obstacleDensityExponent);
    }
}
