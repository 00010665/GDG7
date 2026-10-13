using System.Collections.Generic;
using UnityEngine;
using FoodSurvivors.Core;
using FoodSurvivors.Data;
using FoodSurvivors.Boss;

namespace FoodSurvivors.Player
{
    public class PassiveManager : MonoBehaviour
    {
        [Header("Equipped Passives")]
        public List<PassiveData> activePassives = new List<PassiveData>();
        public Dictionary<PassiveData, int> passiveLevels = new Dictionary<PassiveData, int>();

        private PlayerController playerController;
        private RivalBossController bossController;

        private void Start()
        {
            playerController = GetComponent<PlayerController>();
            if (playerController != null)
                InitializeStartingPassive();
        }

        private void InitializeStartingPassive()
        {
            PassiveData startingPassive = null;
            if (playerController != null && playerController.chefData != null)
            {
                startingPassive = playerController.chefData.startingPassive;
            }
            else if (GameManager.Instance != null && GameManager.Instance.selectedChef != null)
            {
                startingPassive = GameManager.Instance.selectedChef.startingPassive;
            }

            if (startingPassive != null)
            {
                AddOrUpgradePassive(startingPassive);
            }
        }

        public void AddOrUpgradePassive(PassiveData data)
        {
            if (data == null) return;

            PassiveData existing = activePassives.Find(p => p == data || (p != null && p.passiveName == data.passiveName));
            int currentLvl = 1;

            if (existing != null)
            {
                if (passiveLevels.ContainsKey(existing))
                {
                    passiveLevels[existing]++;
                    currentLvl = passiveLevels[existing];
                }
                else
                {
                    passiveLevels[existing] = 2;
                    currentLvl = 2;
                }
            }
            else
            {
                activePassives.Add(data);
                passiveLevels[data] = 1;
                existing = data;
                currentLvl = 1;
            }

            ApplyPassiveEffect(existing, currentLvl);
            Debug.Log($"[PassiveManager] Применена пассивка: {existing.passiveName} (Ур. {currentLvl})");
        }

        public int GetPassiveLevel(PassiveData data)
        {
            if (data == null) return 0;
            PassiveData existing = activePassives.Find(p => p == data || (p != null && p.passiveName == data.passiveName));
            if (existing != null && passiveLevels.ContainsKey(existing))
            {
                return passiveLevels[existing];
            }
            return 0;
        }

        private void ApplyPassiveEffect(PassiveData data, int level)
        {
            if (data == null) return;
            if (playerController == null) playerController = GetComponent<PlayerController>();

            float val = 0f;
            if (data.levels != null && data.levels.Count >= level)
                val = data.levels[level - 1].value;

            if (playerController != null)
            {
                StaminaSystem st = playerController.Stamina;

                switch (data.passiveType)
                {
                    case PassiveType.StaminaRegenBonus: // Второе дыхание
                        if (st != null) st.staminaRegenPerSec += (val > 0 ? val : 5f);
                        break;

                    case PassiveType.StaminaCapacityAndRegen: // Свежие ингредиенты
                        if (st != null)
                        {
                            float staminaIncrease = val > 0 ? val : 25f;
                            st.maxStamina += staminaIncrease;
                            st.currentStamina = Mathf.Min(st.maxStamina, st.currentStamina + staminaIncrease);
                            st.staminaRegenPerSec += Mathf.Max(1f, staminaIncrease * 0.1f);
                        }
                        break;

                    case PassiveType.StaminaSynergy: // Васаби усиливает бонусы, связанные со стаминой
                        if (st != null) st.staminaBenefitMultiplier += (val > 0 ? val : 1f) * 0.05f;
                        break;

                    case PassiveType.StaminaDamageBonus: // Силовой напор
                        if (st != null) st.damageDivisor = Mathf.Max(6f, st.damageDivisor - (val > 0 ? val : 3f));
                        break;

                    case PassiveType.DashCostBonus: // Удешевление рывка
                        if (st != null) st.dashCost = Mathf.Max(6f, st.dashCost - (val > 0 ? val : 4f));
                        break;

                    case PassiveType.SpeedBonus: // Васаби-ускорение (скорость бега)
                        playerController.currentMoveSpeed += (val > 0 ? val : 1.2f);
                        break;

                    case PassiveType.ArmorBonus: // Сырная броня (снижает потерю стамины)
                        if (st != null)
                        {
                            float reduction = val > 0 ? val : 2f;
                            st.staminaLostOnHit = Mathf.Max(0f, st.staminaLostOnHit - reduction);
                            st.dashCost = Mathf.Max(0f, st.dashCost - reduction);
                        }
                        break;

                    case PassiveType.DamageBonus: // Большая порция
                        playerController.damageMultiplier += (val > 0 ? val : 0.15f);
                        break;

                    case PassiveType.AreaBonus: // Острый перчик (ТОЛЬКО радиус атак!)
                        playerController.areaMultiplier += (val > 0 ? val : 0.15f);
                        break;

                    case PassiveType.CooldownReduction: // Быстрая подача
                        playerController.cooldownReduction = Mathf.Min(0.70f, playerController.cooldownReduction + (val > 0 ? val : 0.08f));
                        break;

                    case PassiveType.XpBonus: // Чаевые
                        if (ExperienceManager.Instance != null) ExperienceManager.Instance.xpMultiplier += (val > 0 ? val : 0.20f);
                        break;

                    case PassiveType.MagnetBonus: // Магнитный фартук
                        playerController.magnetRadius += (val > 0 ? val : 1.5f);
                        break;
                }
            }
            else
            {
                if (bossController == null) bossController = GetComponent<RivalBossController>();
                if (bossController != null)
                    bossController.ApplyBuildPassive(data.passiveType, val);
            }
        }
    }
}
