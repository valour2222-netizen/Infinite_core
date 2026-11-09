using UnityEngine;

namespace InfiniteAdventure
{
    public class ScoreTracker : MonoBehaviour
    {
        [SerializeField] EnemyDeathEventChannelSO enemyDied;
        [SerializeField] IntEventChannelSO scoreChanged;

        public int Score { get; private set; }

        void OnEnable() => enemyDied.Raised += OnEnemyDied;
        void OnDisable() => enemyDied.Raised -= OnEnemyDied;

        void OnEnemyDied(EnemyDeathInfo info)
        {
            Score += info.Score;
            scoreChanged.Raise(Score);
        }
    }
}
