using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace InfiniteAdventure
{
    /// <summary>Health and stamina bars, score and threat readout. Text updates use alloc-free SetText.</summary>
    public class HudController : MonoBehaviour
    {
        [SerializeField] Health playerHealth;
        [SerializeField] PlayerController player;
        [SerializeField] Image healthFill;
        [SerializeField] Image staminaFill;
        [SerializeField] TMP_Text scoreText;
        [SerializeField] TMP_Text difficultyText;

        [SerializeField] IntEventChannelSO scoreChanged;
        [SerializeField] FloatVariableSO difficulty;

        void OnEnable()
        {
            scoreChanged.Raised += OnScore;
            difficulty.Changed += OnDifficulty;
            OnScore(0);
            OnDifficulty(difficulty.Value);
        }

        void OnDisable()
        {
            scoreChanged.Raised -= OnScore;
            difficulty.Changed -= OnDifficulty;
        }

        void Update()
        {
            healthFill.fillAmount = playerHealth.Max > 0f ? playerHealth.Current / playerHealth.Max : 0f;
            staminaFill.fillAmount = player.Stamina / player.Stats.maxStamina;
        }

        void OnScore(int score) => scoreText.SetText("Score {0}", score);
        void OnDifficulty(float m) => difficultyText.SetText("Threat x{0:2}", m);
    }
}
