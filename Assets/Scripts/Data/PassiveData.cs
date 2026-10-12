using System;
using System.Collections.Generic;
using UnityEngine;

namespace FoodSurvivors.Data
{
    public enum PassiveType
    {
        StaminaRegenBonus,  // Регенерация стамины
        StaminaDamageBonus, // Усиление бонуса урона
        DashCostBonus,      // Удешевление рывка
        SpeedBonus,         // Скорость
        ArmorBonus,         // Броня
        DamageBonus,        // Урон
        CooldownReduction,  // Перезарядка
        AreaBonus,          // Размер атак
        XpBonus,            // Опыт
        MagnetBonus,        // Магнит
        HealthBonus         // Сохранено для обратной совместимости
    }

    [Serializable]
    public struct PassiveLevelData
    {
        [TextArea] public string description;
        public float value;
    }

    [CreateAssetMenu(fileName = "NewPassiveData", menuName = "FoodSurvivors/Passive Data")]
    public class PassiveData : ScriptableObject
    {
        public string passiveName;
        [TextArea] public string description;
        public PassiveType passiveType;
        public Color passiveColor = Color.white;

        [Header("Roster Settings")]
        public bool isEnabledInRoster = true; // Если false — пассивка не выпадет в игре

        public List<PassiveLevelData> levels = new List<PassiveLevelData>();

        [Header("Media Assets")]
        public Sprite icon;
        public AudioClip attackSound;
    }
}