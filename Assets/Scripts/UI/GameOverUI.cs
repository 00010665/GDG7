using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using FoodSurvivors.Core;
using FoodSurvivors.Player;
using FoodSurvivors.Weapons;

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

            BindButtonInPanel("BtnRestart", OnClickRestart);
            BindButtonInPanel("BtnMainMenu", OnClickMainMenu);
            BindButtonInPanel("BtnQuit", OnClickQuit);
        }

        private void BindButtonInPanel(string btnName, UnityEngine.Events.UnityAction action)
        {
            if (gameOverPanel == null) return;

            Transform btnTr = gameOverPanel.transform.Find(btnName);
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
                Button[] buttons = gameOverPanel.GetComponentsInChildren<Button>(true);
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

        public void ShowGameOver(bool isVictory)
        {
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);
            }

            BindButtonsProgrammatically();
            Time.timeScale = 0f;

            if (titleText != null)
            {
                titleText.text = isVictory ? "🏆 ПОБЕДА! 🏆" : "💀 ПОРАЖЕНИЕ 💀";
                titleText.color = isVictory ? new Color(1f, 0.85f, 0.2f) : new Color(1f, 0.25f, 0.25f);
            }

            if (statsText != null)
            {
                WaveManager wm = Object.FindFirstObjectByType<WaveManager>();
                float elapsed = wm != null ? wm.gameTimer : Time.timeSinceLevelLoad;
                int mins = Mathf.FloorToInt(elapsed / 60f);
                int secs = Mathf.FloorToInt(elapsed % 60f);

                int level = ExperienceManager.Instance != null ? ExperienceManager.Instance.currentLevel : 1;
                int kills = ExperienceManager.Instance != null ? ExperienceManager.Instance.enemiesKilledCount : 0;
                int totalXp = ExperienceManager.Instance != null ? Mathf.RoundToInt(ExperienceManager.Instance.totalXpCollected) : 0;

                // Сбор списка активных Блюд
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                WeaponManager wmPlayer = playerObj != null ? playerObj.GetComponent<WeaponManager>() : null;
                PassiveManager pmPlayer = playerObj != null ? playerObj.GetComponent<PassiveManager>() : null;

                string weaponsList = "Нет";
                if (wmPlayer != null && wmPlayer.activeWeapons != null && wmPlayer.activeWeapons.Count > 0)
                {
                    List<string> wNames = new List<string>();
                    foreach (var w in wmPlayer.activeWeapons)
                    {
                        if (w != null && w.weaponData != null)
                            wNames.Add($"{w.weaponData.weaponName} (Ур. {w.currentLevel})");
                    }
                    if (wNames.Count > 0) weaponsList = string.Join(", ", wNames);
                }

                // Сбор списка активных Пассивок
                string passivesList = "Нет";
                if (pmPlayer != null && pmPlayer.activePassives != null && pmPlayer.activePassives.Count > 0)
                {
                    List<string> pNames = new List<string>();
                    foreach (var p in pmPlayer.activePassives)
                    {
                        if (p != null)
                        {
                            int lvl = pmPlayer.GetPassiveLevel(p);
                            pNames.Add($"{p.passiveName} (Ур. {lvl})");
                        }
                    }
                    if (pNames.Count > 0) passivesList = string.Join(", ", pNames);
                }

                // Формируем детальный финансово-кулинарный отчет
                statsText.text = $"<b>⏱️ Время в бою:</b> {mins:00}:{secs:00}\n" +
                                 $"<b>🏆 Уровень повара:</b> {level}\n" +
                                 $"<b>👾 Накормлено туристов:</b> {kills}\n" +
                                 $"<b>💰 Собранные чаевые (XP):</b> {totalXp}\n\n" +
                                 $"<b>🍲 Экипированные блюда:</b>\n<color=#FFFF88>{weaponsList}</color>\n\n" +
                                 $"<b>🧂 Активные таланты:</b>\n<color=#88FFFF>{passivesList}</color>";
            }

            Debug.Log($"[GameOverUI] Матч окончен! Итоговый отчет сгенерирован.");
        }

        public void OnClickRestart()
        {
            Time.timeScale = 1f;
            if (ExperienceManager.Instance != null) ExperienceManager.Instance.ResetStats();
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void OnClickMainMenu()
        {
            Time.timeScale = 1f;
            if (ExperienceManager.Instance != null) ExperienceManager.Instance.ResetStats();

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
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#endif
            }
        }
    }
}