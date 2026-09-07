using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;
using FoodSurvivors.Core;
using FoodSurvivors.Player;

namespace FoodSurvivors.UI
{
    public class PauseMenuUI : MonoBehaviour
    {
        public static PauseMenuUI Instance { get; private set; }

        [Header("UI Panels")]
        public GameObject pausePanel;
        public TextMeshProUGUI statsText;

        public bool IsPaused { get; private set; } = false;

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
            if (pausePanel != null)
            {
                pausePanel.SetActive(false);
            }
            BindButtonsProgrammatically();
        }

        private void BindButtonsProgrammatically()
        {
            if (pausePanel == null) return;

            BindButtonInPanel("BtnResume", ResumeGame);
            BindButtonInPanel("BtnRestartPause", OnClickRestart);
            BindButtonInPanel("BtnMainMenuPause", OnClickMainMenu);
        }

        private void BindButtonInPanel(string btnName, UnityEngine.Events.UnityAction action)
        {
            if (pausePanel == null) return;

            Transform btnTr = pausePanel.transform.Find(btnName);
            if (btnTr != null)
            {
                Button btn = btnTr.GetComponent<Button>();
                if (btn != null)
                {
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(action);
                }
            }
            else
            {
                Button[] buttons = pausePanel.GetComponentsInChildren<Button>(true);
                foreach (var b in buttons)
                {
                    if (b.gameObject.name == btnName)
                    {
                        b.onClick.RemoveAllListeners();
                        b.onClick.AddListener(action);
                        break;
                    }
                }
            }
        }

        private void Update()
        {
            // Проверка нажатия Esc или Пробела через новый Input System
            if (Keyboard.current != null)
            {
                if (Keyboard.current.escapeKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame)
                {
                    // Если сейчас НЕ открыто окно Level-Up или GameOver
                    if (LevelUpUI.Instance != null && LevelUpUI.Instance.levelUpPanel != null && LevelUpUI.Instance.levelUpPanel.activeSelf) return;
                    if (GameOverUI.Instance != null && GameOverUI.Instance.gameOverPanel != null && GameOverUI.Instance.gameOverPanel.activeSelf) return;

                    TogglePause();
                }
            }
        }

        public void TogglePause()
        {
            if (IsPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }

        public void PauseGame()
        {
            IsPaused = true;
            Time.timeScale = 0f;

            if (pausePanel != null)
            {
                pausePanel.SetActive(true);
            }

            BindButtonsProgrammatically();
            UpdateStatsDisplay();

            Debug.Log("[PauseMenuUI] Игра поставлена на Паузу.");
        }

        public void ResumeGame()
        {
            IsPaused = false;
            Time.timeScale = 1f;

            if (pausePanel != null)
            {
                pausePanel.SetActive(false);
            }

            Debug.Log("[PauseMenuUI] Игра снята с Паузы.");
        }

        private void UpdateStatsDisplay()
        {
            if (statsText == null) return;

            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            PlayerController pc = playerObj != null ? playerObj.GetComponent<PlayerController>() : null;
            WaveManager wm = Object.FindFirstObjectByType<WaveManager>();

            float elapsed = wm != null ? wm.gameTimer : Time.timeSinceLevelLoad;
            int mins = Mathf.FloorToInt(elapsed / 60f);
            int secs = Mathf.FloorToInt(elapsed % 60f);

            int level = ExperienceManager.Instance != null ? ExperienceManager.Instance.currentLevel : 1;

            string stats = $"<b>Время в бою:</b> {mins:00}:{secs:00}\n" +
                           $"<b>Уровень повара:</b> {level}\n";

            if (pc != null)
            {
                stats += $"<b>ХП:</b> {Mathf.CeilToInt(pc.currentHealth)} / {Mathf.CeilToInt(pc.maxHealth)}\n" +
                         $"<b>Скорость:</b> {pc.currentMoveSpeed:F1}\n" +
                         $"<b>Броня:</b> {pc.currentArmor}\n" +
                         $"<b>Множитель урона:</b> +{Mathf.RoundToInt((pc.damageMultiplier - 1f) * 100)}%";
            }

            statsText.text = stats;
        }

        public void OnClickRestart()
        {
            Time.timeScale = 1f;
            Debug.Log("[PauseMenuUI] Перезапуск уровня...");
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void OnClickMainMenu()
        {
            Time.timeScale = 1f;
            Debug.Log("[PauseMenuUI] Возврат в Главное Меню...");
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ReturnToMenu();
            }
            else
            {
                SceneManager.LoadScene("MainMenuScene");
            }
        }
    }
}