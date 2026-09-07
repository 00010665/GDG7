using UnityEngine;
using UnityEngine.UI;
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
            BindButtonsProgrammatically();
        }

        private void BindButtonsProgrammatically()
        {
            if (gameOverPanel == null) return;

            Debug.Log("[GameOverUI] 🔗 Привязывание кнопок экрана Поражения/Победы...");

            // Ищем кнопки внутри панели даже если она скрыта (SetActive(false))
            BindButtonInPanel("BtnRestart", OnClickRestart);
            BindButtonInPanel("BtnMainMenu", OnClickMainMenu);
            BindButtonInPanel("BtnQuit", OnClickQuit);
        }

        private void BindButtonInPanel(string btnName, UnityEngine.Events.UnityAction action)
        {
            if (gameOverPanel == null) return;

            // transform.Find ищет скрытых детей
            Transform btnTr = gameOverPanel.transform.Find(btnName);
            if (btnTr != null)
            {
                Button btn = btnTr.GetComponent<Button>();
                if (btn != null)
                {
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(action);
                    Debug.Log($"[GameOverUI] ✅ Успешно привязано действие к кнопке '{btnName}'");
                }
            }
            else
            {
                // Резервный поиск по всем дочерним кнопкам
                Button[] buttons = gameOverPanel.GetComponentsInChildren<Button>(true);
                foreach (var b in buttons)
                {
                    if (b.gameObject.name == btnName)
                    {
                        b.onClick.RemoveAllListeners();
                        b.onClick.AddListener(action);
                        Debug.Log($"[GameOverUI] ✅ Найдена скрытая кнопка через GetComponentsInChildren: '{btnName}'");
                        break;
                    }
                }
            }
        }

        public void ShowGameOver(bool isVictory)
        {
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);
            }

            // Повторно привязываем события при открытии для надежности
            BindButtonsProgrammatically();

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

            Debug.Log($"[GameOverUI] Игра завершена! Победа: {isVictory}");
        }

        public void OnClickRestart()
        {
            Time.timeScale = 1f;
            Debug.Log("[GameOverUI] 🔄 Нажата кнопка 'Играть Заново'");
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void OnClickMainMenu()
        {
            Time.timeScale = 1f;
            Debug.Log("[GameOverUI] 🏠 Нажата кнопка 'Главное Меню'");
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
            Debug.Log("[GameOverUI] 🚪 Нажата кнопка 'Выход'");
            if (GameManager.Instance != null)
            {
                GameManager.Instance.QuitGame();
            }
            else
            {
                Application.Quit();
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#endif
            }
        }
    }
}