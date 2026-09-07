using UnityEngine;
using FoodSurvivors.UI;

namespace FoodSurvivors.Core
{
    public class ExperienceManager : MonoBehaviour
    {
        public static ExperienceManager Instance { get; private set; }

        [Header("XP & Level Progress")]
        public int currentLevel = 1;
        public float currentXP = 0f;
        public float xpToNextLevel = 100f;
        public float xpMultiplier = 1f;

        [Header("Match Statistics")]
        public float totalXpCollected = 0f;
        public int enemiesKilledCount = 0;

        public delegate void OnXPChangedHandler(float current, float max, int level);
        public event OnXPChangedHandler OnXPChanged;

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
            ResetStats();
        }

        public void ResetStats()
        {
            currentLevel = 1;
            currentXP = 0f;
            xpToNextLevel = 100f;
            xpMultiplier = 1f;
            totalXpCollected = 0f;
            enemiesKilledCount = 0;
        }

        public void AddXP(float amount)
        {
            if (amount <= 0f) return;

            float modifiedAmount = amount * xpMultiplier;
            currentXP += modifiedAmount;
            totalXpCollected += modifiedAmount;

            Debug.Log($"[ExperienceManager] +{modifiedAmount} XP (Всего опыта: {totalXpCollected:F0})");

            OnXPChanged?.Invoke(currentXP, xpToNextLevel, currentLevel);

            if (currentXP >= xpToNextLevel)
            {
                LevelUp();
            }
        }

        private void LevelUp()
        {
            currentXP -= xpToNextLevel;
            currentLevel++;
            xpToNextLevel = Mathf.Round(xpToNextLevel * 1.25f);

            Debug.Log($"[ExperienceManager] LEVEL UP! Достигнут уровень {currentLevel}!");

            OnXPChanged?.Invoke(currentXP, xpToNextLevel, currentLevel);

            Time.timeScale = 0f;

            if (LevelUpUI.Instance != null)
            {
                LevelUpUI.Instance.ShowLevelUpWindow();
            }
            else
            {
                Time.timeScale = 1f;
            }
        }
    }
}