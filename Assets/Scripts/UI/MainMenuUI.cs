using UnityEngine;
using UnityEngine.UI;
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

        private void Start()
        {
            ShowMainMenu();
            InitSelections();
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
            PlayButtonSfx();
            if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
            if (chefSelectPanel != null) chefSelectPanel.SetActive(false);
            if (levelSelectPanel != null) levelSelectPanel.SetActive(false);
        }

        public void ShowChefSelection()
        {
            PlayButtonSfx();
            if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
            if (chefSelectPanel != null) chefSelectPanel.SetActive(true);
            if (levelSelectPanel != null) levelSelectPanel.SetActive(false);
            UpdateChefDisplay();
        }

        public void ShowLevelSelection()
        {
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
            if (chef == null)
            {
                if (chefNameText != null) chefNameText.text = "Шеф-повар не выбран";
                if (chefDescriptionText != null) chefDescriptionText.text = "-";
                if (chefHpText != null) chefHpText.text = "ХП: -";
                if (chefSpeedText != null) chefSpeedText.text = "Скорость: -";
                if (chefArmorText != null) chefArmorText.text = "Броня: -";
                if (chefWeaponText != null) chefWeaponText.text = "Оружие: -";
                if (chefPassiveText != null) chefPassiveText.text = "Пассивка: -";
                return;
            }

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
            if (level == null)
            {
                if (levelNameText != null) levelNameText.text = "Уровень не выбран";
                if (levelDescriptionText != null) levelDescriptionText.text = "-";
                if (levelDurationText != null) levelDurationText.text = "Длительность: -";
                return;
            }

            if (levelNameText != null) levelNameText.text = level.levelName;
            if (levelDescriptionText != null) levelDescriptionText.text = level.description;
            if (levelDurationText != null) levelDurationText.text = $"Длительность: {level.levelDuration / 60f:F1} мин ({level.levelDuration} сек)";
        }

        public void OnClickStartGame()
        {
            PlayButtonSfx();
            Debug.Log("[MainMenuUI] Clicked 'Start Game' button. Starting game...");
            if (GameManager.Instance != null)
            {
                GameManager.Instance.StartGame();
            }
            else
            {
                Debug.LogError("[MainMenuUI] GameManager.Instance is null! Cannot start game.");
            }
        }

        // Псевдоним для OnClick() в кнопке "В БОЙ!"
        public void OnStartGameButtonClicked()
        {
            Debug.Log("[MainMenuUI] Клик кнопки 'В БОЙ!'. Переход в игру...");
            OnClickStartGame();
        }

        public void OnClickQuitGame()
        {
            PlayButtonSfx();
            if (GameManager.Instance != null)
            {
                GameManager.Instance.QuitGame();
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
