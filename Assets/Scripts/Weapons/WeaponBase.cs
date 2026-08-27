using UnityEngine;
using FoodSurvivors.Data;

namespace FoodSurvivors.Weapons
{
    public abstract class WeaponBase : MonoBehaviour
    {
        [Header("Weapon Configuration")]
        public WeaponData weaponData;
        public int currentLevel = 1;

        protected float cooldownTimer;

        public virtual void Initialize(WeaponData data)
        {
            weaponData = data;
            currentLevel = 1;
            cooldownTimer = 0f;
        }

        public virtual void LevelUp()
        {
            if (weaponData == null || weaponData.levels == null || weaponData.levels.Count == 0) return;

            if (currentLevel < weaponData.levels.Count)
            {
                currentLevel++;
                Debug.Log($"[Weapon] {weaponData.weaponName} leveled up to Level {currentLevel}!");
            }
        }

        public WeaponLevelData GetCurrentLevelData()
        {
            if (weaponData == null || weaponData.levels == null || weaponData.levels.Count == 0)
            {
                return new WeaponLevelData
                {
                    description = "Default Level",
                    damage = 10f,
                    cooldown = 1f,
                    speed = 10f,
                    areaScale = 1f,
                    projectileCount = 1,
                    duration = 3f
                };
            }

            int index = Mathf.Clamp(currentLevel - 1, 0, weaponData.levels.Count - 1);
            return weaponData.levels[index];
        }

        protected virtual void Update()
        {
            if (weaponData == null) return;

            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0f)
            {
                Attack();
                WeaponLevelData stats = GetCurrentLevelData();
                cooldownTimer = Mathf.Max(0.1f, stats.cooldown);
            }
        }

        protected abstract void Attack();
    }
}
