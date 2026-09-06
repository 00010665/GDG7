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

        public void AddXP(float amount)
        {
            if (amount <= 0f) return;

            float modifiedAmount = amount * xpMultiplier;
            currentXP += modifiedAmount;
            Debug.Log($"[ExperienceManager] +{modifiedAmount} XP (Base: {amount}, Multiplier: {xpMultiplier}, Progress: {currentXP}/{xpToNextLevel}, Level: {currentLevel})");

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

            Debug.Log($"[ExperienceManager] LEVEL UP! Reached Level {currentLevel}! Next XP: {xpToNextLevel}");

            OnXPChanged?.Invoke(currentXP, xpToNextLevel, currentLevel);

            Time.timeScale = 0f;

            if (LevelUpUI.Instance != null)
            {
                LevelUpUI.Instance.ShowLevelUpWindow();
            }
            else
            {
                Debug.LogWarning("[ExperienceManager] LevelUpUI Instance not found! Auto-unpausing.");
                Time.timeScale = 1f;
            }
        }
    }
}
