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
            Debug.Log($"[PassiveManager] Applied/Upgraded {existing.passiveName} to Level {currentLvl}!");
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

            float val = 0f;
            if (data.levels != null && data.levels.Count >= level)
            {
                val = data.levels[level - 1].value;
            }

            string pName = data.passiveName != null ? data.passiveName.ToLower() : "";

            switch (data.passiveType)
            {
                case PassiveType.ArmorBonus:
                    float armorAdd = val > 0 ? val : 2f;
                    playerController.currentArmor += armorAdd;
                    break;

                case PassiveType.SpeedBonus:
                    float speedAdd = val > 0 ? val : 1.2f;
                    playerController.currentMoveSpeed += speedAdd;
                    break;

                case PassiveType.DamageBonus:
                    float dmgAdd = val > 0 ? val : 0.15f;
                    playerController.damageMultiplier += dmgAdd;
                    break;

                case PassiveType.AreaBonus:
                    float areaAdd = val > 0 ? val : 0.15f;
                    playerController.areaMultiplier += areaAdd;
                    playerController.damageMultiplier += 0.05f;
                    break;

                case PassiveType.CooldownReduction:
                    float cdAdd = val > 0 ? val : 0.08f;
                    playerController.cooldownReduction = Mathf.Min(0.70f, playerController.cooldownReduction + cdAdd);
                    break;

                case PassiveType.XpBonus:
                    float xpAdd = val > 0 ? val : 0.20f;
                    if (ExperienceManager.Instance != null)
                    {
                        ExperienceManager.Instance.xpMultiplier += xpAdd;
                    }
                    break;

                case PassiveType.HealthBonus:
                    float hpAdd = val > 0 ? val : 25f;
                    playerController.maxHealth += hpAdd;
                    playerController.Heal(hpAdd);
                    playerController.hpRegenPerSec += 1.5f;
                    break;

                case PassiveType.MagnetBonus:
                    float magAdd = val > 0 ? val : 1.5f;
                    playerController.magnetRadius += magAdd;
                    break;

                default:
                    if (pName.Contains("сыр") || pName.Contains("armor")) playerController.currentArmor += 2f;
                    else if (pName.Contains("васаби") || pName.Contains("обувь") || pName.Contains("speed")) playerController.currentMoveSpeed += 1.2f;
                    else if (pName.Contains("порция") || pName.Contains("damage")) playerController.damageMultiplier += 0.15f;
                    else if (pName.Contains("перец") || pName.Contains("area")) { playerController.areaMultiplier += 0.15f; playerController.damageMultiplier += 0.05f; }
                    else if (pName.Contains("подача") || pName.Contains("cooldown")) playerController.cooldownReduction = Mathf.Min(0.70f, playerController.cooldownReduction + 0.08f);
                    else if (pName.Contains("чаевые") || pName.Contains("xp")) { if (ExperienceManager.Instance != null) ExperienceManager.Instance.xpMultiplier += 0.20f; }
                    else if (pName.Contains("ингредиенты") || pName.Contains("health")) { playerController.maxHealth += 25f; playerController.Heal(25f); playerController.hpRegenPerSec += 1.5f; }
                    else if (pName.Contains("фартук") || pName.Contains("magnet")) playerController.magnetRadius += 1.5f;
                    break;
            }
        }
    }
}
