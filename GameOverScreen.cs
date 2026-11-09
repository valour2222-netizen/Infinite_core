using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace InfiniteAdventure
{
    public class GameOverScreen : MonoBehaviour
    {
        [SerializeField] VoidEventChannelSO playerDied;
        [SerializeField] ScoreTracker score;
        [SerializeField] GameObject panel;
        [SerializeField] TMP_Text finalScoreText;
        [SerializeField] Button restartButton;
        [SerializeField] float showDelay = 1.2f;

        void OnEnable()
        {
            panel.SetActive(false);
            playerDied.Raised += OnPlayerDied;
            restartButton.onClick.AddListener(Restart);
        }

        void OnDisable()
        {
            playerDied.Raised -= OnPlayerDied;
            restartButton.onClick.RemoveListener(Restart);
        }

        void OnPlayerDied() => StartCoroutine(ShowRoutine());

        IEnumerator ShowRoutine()
        {
            yield return new WaitForSeconds(showDelay);
            finalScoreText.SetText("Final score {0}", score.Score);
            panel.SetActive(true);
            Time.timeScale = 0f;
        }

        void Restart()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
