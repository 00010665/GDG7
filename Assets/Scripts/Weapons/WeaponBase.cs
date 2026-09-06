using UnityEngine;
using FoodSurvivors.Core;
using FoodSurvivors.Data;
using FoodSurvivors.Player;

namespace FoodSurvivors.Weapons
{
    public abstract class WeaponBase : MonoBehaviour
    {
        [Header("Weapon Configuration")]
        public WeaponData weaponData;
        public int currentLevel = 1;

        protected float cooldownTimer;
        protected PlayerController player;

        protected virtual void Awake()
        {
            player = GetComponentInParent<PlayerController>();
        }

        public virtual void Initialize(WeaponData data)
        {
            weaponData = data;
            currentLevel = 1;
            cooldownTimer = 0f;
            if (player == null) player = GetComponentInParent<PlayerController>();
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

        public float GetDamage(float baseDmg)
        {
            float mult = player != null ? player.damageMultiplier : 1f;
            return baseDmg * mult;
        }

        public float GetCooldown(float baseCooldown)
        {
            float reduction = player != null ? player.cooldownReduction : 0f;
            reduction = Mathf.Clamp(reduction, 0f, 0.75f);
            return baseCooldown * (1f - reduction);
        }

        public float GetArea(float baseArea)
        {
            float mult = player != null ? player.areaMultiplier : 1f;
            return baseArea * mult;
        }

        protected virtual void Update()
        {
            if (weaponData == null) return;

            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0f)
            {
                Attack();

                WeaponLevelData stats = GetCurrentLevelData();
                cooldownTimer = Mathf.Max(0.1f, GetCooldown(stats.cooldown));
            }
        }

        protected void Attack()
        {
            if (AudioManager.Instance != null && weaponData != null)
            {
                AudioManager.Instance.PlaySFX(weaponData.attackSound);
            }

            PerformAttack();
        }

        protected abstract void PerformAttack();
    }
}
