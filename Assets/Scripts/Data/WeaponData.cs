using System;
using System.Collections.Generic;
using UnityEngine;

namespace FoodSurvivors.Data
{
    public enum WeaponType
    {
        PastaLauncher,  // Снаряд в ближайшего
        SashimiSlice,   // Вращение по орбите
        SalsaSplash,    // Лужа / пятно под ногами
        SoupBowl,       // Взрывной навесной снаряд
        SushiSet,       // Выстрел во все 4 стороны
        TacoThrow,      // Рикошет по толпе
        PizzaCutter,    // Пробивающий катящийся диск
        TeaPot,         // Огромный хот-дог / струя перед поваром
        MegaBurger      // Падение сверху с ударом
    }

    [Serializable]
    public struct WeaponLevelData
    {
        [TextArea] public string description;
        public float damage;
        public float cooldown;
        public float speed;
        public float areaScale;
        public int projectileCount;
        public float duration;
    }

    [CreateAssetMenu(fileName = "NewWeaponData", menuName = "FoodSurvivors/Weapon Data")]
    public class WeaponData : ScriptableObject
    {
        public string weaponName;
        [TextArea] public string description;
        public Color weaponColor = Color.red;
        public GameObject weaponPrefab;

        [Header("Behavior Mechanics")]
        public WeaponType weaponType; // Механика оружия (не зависит от названия!)

        [Header("Roster Settings")]
        public bool isEnabledInRoster = true;

        public List<WeaponLevelData> levels = new List<WeaponLevelData>();

        [Header("Media Assets")]
        public Sprite icon;
        public AudioClip attackSound;
    }
}