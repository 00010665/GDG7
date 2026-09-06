using System;
using System.Collections.Generic;
using UnityEngine;

namespace FoodSurvivors.Data
{
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
        public List<WeaponLevelData> levels = new List<WeaponLevelData>();

        [Header("Media Assets")]
        public Sprite icon;
        public AudioClip attackSound;
    }
}
