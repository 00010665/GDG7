using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using FoodSurvivors.Core;
using FoodSurvivors.Data;
using FoodSurvivors.Player;

namespace FoodSurvivors.UI
{
    public class MainMenuUI : MonoBehaviour
    {
        [Header("Panels")]
        public GameObject mainMenuPanel;
        public GameObject chefSelectPanel;
        public GameObject levelSelectPanel;
        public GameObject settingsPanel;

        [Header("Settings Display")]
        public TextMeshProUGUI staminaModeLabelText;

        [Header("Chef UI Display")]
        public TextMeshProUGUI chefNameText;
        public TextMeshProUGUI chefDescriptionText;
        public TextMeshProUGUI chefHpText;
        public TextMeshProUGUI chefSpeedText;
        public TextMeshProUGUI chefArmorText;
        public TextMeshProUGUI chefWeaponText;
        public TextMeshProUGUI chefPassiveText;
        public Image chefColorPreview;

        [Header("Level UI Display")]
        public TextMeshProUGUI levelNameText;
        public TextMeshProUGUI levelDescriptionText;
        public TextMeshProUGUI levelDurationText;

        [Header("UI Click Sound")]
        public AudioClip uiClickSound;

        private int currentChefIndex = 0;
        private int currentLevelIndex = 0;

        private void Awake()
        {
            BindAllButtonsProgrammatically();
        }

        private void Start()
        {
            ShowMainMenu();
            InitSelections();
        }

        private void BindAllButtonsProgrammatically()
        {
            BindButton("BtnChef", ShowChefSelection);
            BindButton("BtnLevel", ShowLevelSelection);
            BindButton("BtnStart", OnClickStartGame);
            BindButton("BtnQuit", OnClickQuitGame);
            BindButton("BtnSettings", ShowSettings);

            // Кнопки Назад
            BindButton("BtnBackChef", ShowMainMenu);
            BindButton("BtnBackLevel", ShowMainMenu);
            BindButton("BtnBackSettings", ShowMainMenu);

            // Кнопки переключения режима стамины
            BindButton("BtnModeNoStamina", () => SetStaminaMode(StaminaMode.Disabled));
            BindButton("BtnModeDamage", () => SetStaminaMode(StaminaMode.DamageBoostOnly));
            BindButton("BtnModeDash", () => SetStaminaMode(StaminaMode.DashOnly));

            BindButton("BtnNextChef", NextChef);
            BindButton("BtnPrevChef", PreviousChef);
        }

        private void BindButton(string buttonGameObjectName, UnityEngine.Events.UnityAction action)
        {
            GameObject btnObj = GameObject.Find(buttonGameObjectName);
            if (btnObj != null)
            {
                Button btn = btnObj.GetComponent<Button>();
                if (btn != null)
                {
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(action);
                }
            }
        }

        private void InitSelections()
        {
            if (GameManager.Instance == null) return;

            if (GameManager.Instance.availableChefs != null && GameManager.Instance.availableChefs.Count > 0)
            {
                currentChefIndex = 0;
                GameManager.Instance.SelectChef(GameManager.Instance.availableChefs[currentChefIndex]);
            }

            // Фиксируем карту Бульвар
            if (GameManager.Instance.availableLevels != null && GameManager.Instance.availableLevels.Count > 0)
            {
                int boulevardIdx = GameManager.Instance.availableLevels.FindIndex(l => l != null && l.levelName.Contains("Бульвар"));
                currentLevelIndex = boulevardIdx >= 0 ? boulevardIdx : 0;
                GameManager.Instance.SelectLevel(GameManager.Instance.availableLevels[currentLevelIndex]);
            }

            UpdateChefDisplay();
            UpdateLevelDisplay();
        }

        public void ShowMainMenu()
        {
            PlayButtonSfx();
            if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
            if (chefSelectPanel != null) chefSelectPanel.SetActive(false);
            if (levelSelectPanel != null) levelSelectPanel.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(false);
        }

        public void ShowChefSelection()
        {
            PlayButtonSfx();
            if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
            if (chefSelectPanel != null) chefSelectPanel.SetActive(true);
            if (levelSelectPanel != null) levelSelectPanel.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(false);
            UpdateChefDisplay();
        }

        public void ShowLevelSelection()
        {
            PlayButtonSfx();
            // Показываем зафиксированный Бульвар
            InitSelections();
        }

        public void ShowSettings()
        {
            PlayButtonSfx();
            if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
            if (chefSelectPanel != null) chefSelectPanel.SetActive(false);
            if (levelSelectPanel != null) levelSelectPanel.SetActive(false);

            if (settingsPanel != null)
            {
                settingsPanel.SetActive(true);
            }
            UpdateSettingsDisplay();
        }

        public void SetStaminaMode(StaminaMode mode)
        {
            PlayButtonSfx();
            if (GameManager.Instance != null)
            {
                GameManager.Instance.selectedStaminaMode = mode;
                Debug.Log($"[MainMenuUI] ⚙️ Выбран режим игры: {mode}");
            }
            UpdateSettingsDisplay();
        }

        private void UpdateSettingsDisplay()
        {
            if (staminaModeLabelText == null || GameManager.Instance == null) return;

            switch (GameManager.Instance.selectedStaminaMode)
            {
                case StaminaMode.Disabled:
                    staminaModeLabelText.text = "Текущий режим: <b><color=red>БЕЗ СТАМИНЫ</color></b>\n<i>Классика, бесконечный бесплатный рывок</i>";
                    break;
                case StaminaMode.DamageBoostOnly:
                    staminaModeLabelText.text = "Текущий режим: <b><color=yellow>СТАМИНА - УРОН</color></b>\n<i>Тратится от ударов, дает бонус урона атак</i>";
                    break;
                case StaminaMode.DashOnly:
                    staminaModeLabelText.text = "Текущий режим: <b><color=cyan>СТАМИНА - РЫВОК</color></b>\n<i>Тратится на рывок с неуязвимостью, регенерирует</i>";
                    break;
            }
        }

        // Встроенный OnGUI интерфейс настроек (работает сразу без ручной сборки в Canvas!)
        private void OnGUI()
        {
            if (settingsPanel != null && settingsPanel.activeSelf) return;

            // Если панель настроек открыта, но не собрана в Canvas
            if (mainMenuPanel != null && !mainMenuPanel.activeSelf && (chefSelectPanel == null || !chefSelectPanel.activeSelf) && (levelSelectPanel == null || !levelSelectPanel.activeSelf))
            {
                GUI.backgroundColor = new Color(0.1f, 0.1f, 0.15f, 0.95f);
                Rect r = new Rect(Screen.width / 2f - 200, Screen.height / 2f - 180, 400, 360);
                GUI.Window(888, r, DrawSettingsWindow, "⚙️ НАСТРОЙКИ РЕЖИМА ИГРЫ");
            }
        }

        private void DrawSettingsWindow(int id)
        {
            GUILayout.Space(15);
            StaminaMode current = GameManager.Instance != null ? GameManager.Instance.selectedStaminaMode : StaminaMode.DashOnly;

            GUILayout.Label($"<b>Выбранный режим:</b> <color=yellow>{current}</color>");
            GUILayout.Space(15);

            if (GUILayout.Button("1. 🚫 БЕЗ СТАМИНЫ\n(Классика, свободный рывок)", GUILayout.Height(45)))
            {
                SetStaminaMode(StaminaMode.Disabled);
            }

            if (GUILayout.Button("2. ⚔️ СТАМИНА - УРОН\n(Тратится от ударов, усиливает блюда)", GUILayout.Height(45)))
            {
                SetStaminaMode(StaminaMode.DamageBoostOnly);
            }

            if (GUILayout.Button("3. ⚡ СТАМИНА - РЫВОК\n(Тратится на рывки с неуязвимостью)", GUILayout.Height(45)))
            {
                SetStaminaMode(StaminaMode.DashOnly);
            }

            GUILayout.Space(20);
            if (GUILayout.Button("◀ НАЗАД В МЕНЮ", GUILayout.Height(35)))
            {
                ShowMainMenu();
            }
        }

        public void NextChef()
        {
            PlayButtonSfx();
            if (GameManager.Instance == null || GameManager.Instance.availableChefs == null || GameManager.Instance.availableChefs.Count == 0) return;
            currentChefIndex = (currentChefIndex + 1) % GameManager.Instance.availableChefs.Count;
            GameManager.Instance.SelectChef(GameManager.Instance.availableChefs[currentChefIndex]);
            UpdateChefDisplay();
        }

        public void PreviousChef()
        {
            PlayButtonSfx();
            if (GameManager.Instance == null || GameManager.Instance.availableChefs == null || GameManager.Instance.availableChefs.Count == 0) return;
            currentChefIndex--;
            if (currentChefIndex < 0) currentChefIndex = GameManager.Instance.availableChefs.Count - 1;
            GameManager.Instance.SelectChef(GameManager.Instance.availableChefs[currentChefIndex]);
            UpdateChefDisplay();
        }

        public void UpdateChefDisplay()
        {
            ChefData chef = GameManager.Instance != null ? GameManager.Instance.selectedChef : null;
            if (chef == null) return;

            if (chefNameText != null) chefNameText.text = chef.chefName;
            if (chefDescriptionText != null) chefDescriptionText.text = chef.description;
            if (chefHpText != null) chefHpText.text = "Бессмертен";
            if (chefSpeedText != null) chefSpeedText.text = $"Скорость: {chef.moveSpeed}";
            if (chefArmorText != null) chefArmorText.text = $"Броня: {chef.armor}";
            if (chefWeaponText != null) chefWeaponText.text = $"Блюдо: {(chef.startingWeapon != null ? chef.startingWeapon.weaponName : "Нет")}";
            if (chefPassiveText != null) chefPassiveText.text = $"Пассивка: {(chef.startingPassive != null ? chef.startingPassive.passiveName : "Нет")}";
            if (chefColorPreview != null) chefColorPreview.color = chef.chefColor;
        }

        public void UpdateLevelDisplay()
        {
            LevelData level = GameManager.Instance != null ? GameManager.Instance.selectedLevel : null;
            if (level == null) return;
            if (levelNameText != null) levelNameText.text = level.levelName;
            if (levelDescriptionText != null) levelDescriptionText.text = level.description;
            if (levelDurationText != null) levelDurationText.text = "2:00 (1:30 фарм + 0:30 батл)";
        }

        public void OnClickStartGame()
        {
            PlayButtonSfx();
            Debug.Log("[MainMenuUI] ⚔️ Запуск игры на Бульваре...");
            if (GameManager.Instance != null) GameManager.Instance.StartGame();
            else SceneManager.LoadScene("GameScene");
        }

        public void OnStartGameButtonClicked() => OnClickStartGame();

        public void OnClickQuitGame()
        {
            PlayButtonSfx();
            if (GameManager.Instance != null) GameManager.Instance.QuitGame();
            else Application.Quit();
        }

        private void PlayButtonSfx()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(uiClickSound);
        }

        public void NextLevel()
        {
            PlayButtonSfx();
            InitSelections();
        }

        public void PreviousLevel()
        {
            PlayButtonSfx();
            InitSelections();
        }
    }

}