using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FoodSurvivors.Core;
using FoodSurvivors.Player;
using FoodSurvivors.Weapons;

namespace FoodSurvivors.UI
{
    public class GameHUD : MonoBehaviour
    {
        [Header("XP & Level HUD")]
        public Slider xpSlider;
        public TextMeshProUGUI levelText;

        [Header("HP HUD")]
        public Slider hpSlider;
        public TextMeshProUGUI hpText;

        [Header("Timer HUD")]
        public TextMeshProUGUI timerText;

        [Header("Item Slots Containers")]
        public Transform weaponIconsContainer;
        public Transform passiveIconsContainer;

        private PlayerController playerController;
        private WaveManager waveManager;

        private void Start()
        {
            FindReferences();
        }

        private void FindReferences()
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerController = playerObj.GetComponent<PlayerController>();
            }

            waveManager = Object.FindFirstObjectByType<WaveManager>();
        }

        private void Update()
        {
            if (playerController == null || waveManager == null)
            {
                FindReferences();
            }

            UpdateXPHUD();
            UpdateHPHUD();
            UpdateTimerHUD();
        }

        private void UpdateXPHUD()
        {
            if (ExperienceManager.Instance == null) return;

            float curXP = ExperienceManager.Instance.currentXP;
            float maxXP = ExperienceManager.Instance.xpToNextLevel;
            int level = ExperienceManager.Instance.currentLevel;

            if (xpSlider != null)
            {
                xpSlider.maxValue = maxXP;
                xpSlider.value = curXP;
            }

            if (levelText != null)
            {
                levelText.text = $"УР. {level}";
            }
        }

        private void UpdateHPHUD()
        {
            if (playerController == null) return;

            float curHP = playerController.currentHealth;
            float maxHP = playerController.maxHealth;

            if (hpSlider != null)
            {
                hpSlider.maxValue = maxHP;
                hpSlider.value = curHP;
            }

            if (hpText != null)
            {
                hpText.text = $"HP: {Mathf.CeilToInt(curHP)} / {Mathf.CeilToInt(maxHP)}";
            }
        }

        private void UpdateTimerHUD()
        {
            float elapsedSeconds = waveManager != null ? waveManager.gameTimer : Time.timeSinceLevelLoad;
            int minutes = Mathf.FloorToInt(elapsedSeconds / 60f);
            int seconds = Mathf.FloorToInt(elapsedSeconds % 60f);

            if (timerText != null)
            {
                timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
            }
        }
    }
}
