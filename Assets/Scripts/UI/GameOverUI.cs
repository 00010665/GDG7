using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using FoodSurvivors.Core;

namespace FoodSurvivors.UI
{
    public class GameOverUI : MonoBehaviour
    {
        public static GameOverUI Instance { get; private set; }

        [Header("UI Panels")]
        public GameObject gameOverPanel;
        public TextMeshProUGUI titleText;
        public TextMeshProUGUI statsText;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(false);
            }
        }

        public void ShowGameOver(bool isVictory)
        {
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);
            }

            Time.timeScale = 0f;

            if (titleText != null)
            {
                titleText.text = isVictory ? "ПОБЕДА!" : "ПОРАЖЕНИЕ";
                titleText.color = isVictory ? new Color(1f, 0.85f, 0.2f) : new Color(1f, 0.2f, 0.2f);
            }

            if (statsText != null)
            {
                WaveManager wm = Object.FindFirstObjectByType<WaveManager>();
                float elapsed = wm != null ? wm.gameTimer : Time.timeSinceLevelLoad;
                int mins = Mathf.FloorToInt(elapsed / 60f);
                int secs = Mathf.FloorToInt(elapsed % 60f);

                int level = ExperienceManager.Instance != null ? ExperienceManager.Instance.currentLevel : 1;

                statsText.text = $"Время в бою: {mins:00}:{secs:00}\nДостигнутый уровень: {level}";
            }

            Debug.Log($"[GameOverUI] Game Ended! Victory: {isVictory}");
        }

        public void OnClickRestart()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void OnClickMainMenu()
        {
            Time.timeScale = 1f;
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ReturnToMenu();
            }
            else
            {
                SceneManager.LoadScene("MainMenuScene");
            }
        }

        public void OnClickQuit()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.QuitGame();
            }
            else
            {
                Application.Quit();
            }
        }
    }
}
