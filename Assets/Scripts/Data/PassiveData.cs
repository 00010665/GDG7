using System;
using System.Collections.Generic;
using UnityEngine;

namespace FoodSurvivors.Data
{
    public enum PassiveType
    {
        HealthBonus,
        SpeedBonus,
        ArmorBonus,
        DamageBonus,
        CooldownReduction,
        AreaBonus,
        XpBonus,
        MagnetBonus
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
        public List<PassiveLevelData> levels = new List<PassiveLevelData>();
    }
}
