using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using FoodSurvivors.Core;
using FoodSurvivors.Data;

namespace FoodSurvivors.UI
{
    public class MainMenuUI : MonoBehaviour
    {
        [Header("Panels")]
        public GameObject mainMenuPanel;
        public GameObject chefSelectPanel;
        public GameObject levelSelectPanel;

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
            Debug.Log("[MainMenuUI] 🔗 Автоматическое привязывание событий к кнопкам...");

            BindButton("BtnChef", ShowChefSelection);
            BindButton("BtnLevel", ShowLevelSelection);
            BindButton("BtnStart", OnClickStartGame);
            BindButton("BtnQuit", OnClickQuitGame);

            BindButton("BtnBackChef", ShowMainMenu);
            BindButton("BtnBackLevel", ShowMainMenu);

            BindButton("BtnNextChef", NextChef);
            BindButton("BtnPrevChef", PreviousChef);

            BindButton("BtnNextLevel", NextLevel);
            BindButton("BtnPrevLevel", PreviousLevel);
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
                    Debug.Log($"[MainMenuUI] ✅ Успешно привязано действие к кнопке '{buttonGameObjectName}'");
                }
            }
            else
            {
                Debug.LogWarning($"[MainMenuUI] ⚠️ Кнопка '{buttonGameObjectName}' не найдена в иерархии сцены!");
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

            if (GameManager.Instance.availableLevels != null && GameManager.Instance.availableLevels.Count > 0)
            {
                currentLevelIndex = 0;
                GameManager.Instance.SelectLevel(GameManager.Instance.availableLevels[currentLevelIndex]);
            }

            UpdateChefDisplay();
            UpdateLevelDisplay();
        }

        public void ShowMainMenu()
        {
            Debug.Log("[MainMenuUI] 📱 Открытие Главного Меню");
            PlayButtonSfx();
            if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
            if (chefSelectPanel != null) chefSelectPanel.SetActive(false);
            if (levelSelectPanel != null) levelSelectPanel.SetActive(false);
        }

        public void ShowChefSelection()
        {
            Debug.Log("[MainMenuUI] 👨‍🍳 Открытие Панели Выбора Повара");
            PlayButtonSfx();
            if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
            if (chefSelectPanel != null) chefSelectPanel.SetActive(true);
            if (levelSelectPanel != null) levelSelectPanel.SetActive(false);
            UpdateChefDisplay();
        }

        public void ShowLevelSelection()
        {
            Debug.Log("[MainMenuUI] 🗺️ Открытие Панели Выбора Уровня");
            PlayButtonSfx();
            if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
            if (chefSelectPanel != null) chefSelectPanel.SetActive(false);
            if (levelSelectPanel != null) levelSelectPanel.SetActive(true);
            UpdateLevelDisplay();
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

        public void NextLevel()
        {
            PlayButtonSfx();
            if (GameManager.Instance == null || GameManager.Instance.availableLevels == null || GameManager.Instance.availableLevels.Count == 0) return;
            currentLevelIndex = (currentLevelIndex + 1) % GameManager.Instance.availableLevels.Count;
            GameManager.Instance.SelectLevel(GameManager.Instance.availableLevels[currentLevelIndex]);
            UpdateLevelDisplay();
        }

        public void PreviousLevel()
        {
            PlayButtonSfx();
            if (GameManager.Instance == null || GameManager.Instance.availableLevels == null || GameManager.Instance.availableLevels.Count == 0) return;
            currentLevelIndex--;
            if (currentLevelIndex < 0) currentLevelIndex = GameManager.Instance.availableLevels.Count - 1;
            GameManager.Instance.SelectLevel(GameManager.Instance.availableLevels[currentLevelIndex]);
            UpdateLevelDisplay();
        }

        public void UpdateChefDisplay()
        {
            ChefData chef = GameManager.Instance != null ? GameManager.Instance.selectedChef : null;
            if (chef == null) return;

            if (chefNameText != null) chefNameText.text = chef.chefName;
            if (chefDescriptionText != null) chefDescriptionText.text = chef.description;
            if (chefHpText != null) chefHpText.text = $"ХП: {chef.maxHealth}";
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
            if (levelDurationText != null) levelDurationText.text = $"Длительность: {level.levelDuration / 60f:F1} мин ({level.levelDuration} сек)";
        }

        public void OnClickStartGame()
        {
            PlayButtonSfx();
            Debug.Log("[MainMenuUI] ⚔️ НАЖАТА КНОПКА 'В БОЙ!' -> Переход в GameScene...");

            if (GameManager.Instance != null)
            {
                GameManager.Instance.StartGame();
            }
            else
            {
                Debug.LogWarning("[MainMenuUI] GameManager.Instance равен NULL! Загрузка сцены 'GameScene' напрямую");
                SceneManager.LoadScene("GameScene");
            }
        }

        // Алиас для вызова из MainMenuBuilder
        public void OnStartGameButtonClicked()
        {
            OnClickStartGame();
        }

        public void OnClickQuitGame()
        {
            PlayButtonSfx();
            Debug.Log("[MainMenuUI] 🚪 НАЖАТА КНОПКА 'ВЫХОД'");
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

        private void PlayButtonSfx()
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(uiClickSound);
            }
        }
    }
}