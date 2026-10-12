using System.Collections.Generic;
using UnityEngine;
using FoodSurvivors.Core;
using FoodSurvivors.Data;

namespace FoodSurvivors.Player
{
    public class PassiveManager : MonoBehaviour
    {
        [Header("Equipped Passives")]
        public List<PassiveData> activePassives = new List<PassiveData>();
        public Dictionary<PassiveData, int> passiveLevels = new Dictionary<PassiveData, int>();

        private PlayerController playerController;

        private void Start()
        {
            playerController = GetComponent<PlayerController>();
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
            if (playerController == null) playerController = GetComponent<PlayerController>();
            StaminaSystem st = playerController.Stamina;

            float val = 0f;
            if (data.levels != null && data.levels.Count >= level)
            {
                val = data.levels[level - 1].value;
            }

            switch (data.passiveType)
            {
                // 1. Регенерация стамины
                case PassiveType.StaminaRegenBonus:
                    if (st != null) st.staminaRegenPerSec += (val > 0 ? val : 5f);
                    break;

                // 2. Усиление бонуса урона (снижение делителя)
                case PassiveType.StaminaDamageBonus:
                    if (st != null) st.damageDivisor = Mathf.Max(6f, st.damageDivisor - (val > 0 ? val : 3f));
                    break;

                // 3. Удешевление рывка
                case PassiveType.DashCostBonus:
                    if (st != null) st.dashCost = Mathf.Max(8f, st.dashCost - (val > 0 ? val : 4f));
                    break;

                case PassiveType.ArmorBonus:
                    playerController.currentArmor += (val > 0 ? val : 2f);
                    break;

                case PassiveType.SpeedBonus:
                    playerController.currentMoveSpeed += (val > 0 ? val : 1.2f);
                    break;

                case PassiveType.DamageBonus:
                    playerController.damageMultiplier += (val > 0 ? val : 0.15f);
                    break;

                case PassiveType.AreaBonus:
                    playerController.areaMultiplier += (val > 0 ? val : 0.15f);
                    playerController.damageMultiplier += 0.05f;
                    break;

                case PassiveType.CooldownReduction:
                    playerController.cooldownReduction = Mathf.Min(0.70f, playerController.cooldownReduction + (val > 0 ? val : 0.08f));
                    break;

                case PassiveType.XpBonus:
                    if (ExperienceManager.Instance != null) ExperienceManager.Instance.xpMultiplier += (val > 0 ? val : 0.20f);
                    break;

                case PassiveType.MagnetBonus:
                    playerController.magnetRadius += (val > 0 ? val : 1.5f);
                    break;
            }
        }
    }
}