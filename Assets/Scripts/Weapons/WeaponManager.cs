using System.Collections.Generic;
using UnityEngine;
using FoodSurvivors.Core;
using FoodSurvivors.Data;
using FoodSurvivors.Player;

namespace FoodSurvivors.Weapons
{
    public class WeaponManager : MonoBehaviour
    {
        [Header("Active Weapons")]
        public List<WeaponBase> activeWeapons = new List<WeaponBase>();

        private PlayerController playerController;

        private void Start()
        {
            playerController = GetComponent<PlayerController>();
            InitializeStartingWeapon();
            Debug.Log($"[WeaponManager] Active weapons after init: {(activeWeapons != null ? activeWeapons.Count : 0)}");
        }

        private void InitializeStartingWeapon()
        {
            WeaponData startingWeapon = null;

            if (playerController != null && playerController.chefData != null)
            {
                startingWeapon = playerController.chefData.startingWeapon;
            }
            else if (GameManager.Instance != null && GameManager.Instance.selectedChef != null)
            {
                startingWeapon = GameManager.Instance.selectedChef.startingWeapon;
            }

            if (startingWeapon != null)
            {
                AddWeapon(startingWeapon);
            }
            else
            {
                Debug.LogWarning("[WeaponManager] No starting weapon found on chef profile.");
            }
        }

        public void AddWeapon(WeaponData data)
        {
            if (data == null) return;

            WeaponBase existingWeapon = activeWeapons.Find(w => w.weaponData == data || (w.weaponData != null && w.weaponData.weaponName == data.weaponName));
            if (existingWeapon != null)
            {
                existingWeapon.LevelUp();
                return;
            }

            System.Type weaponType = GetWeaponType(data.weaponName);
            WeaponBase newWeapon = (WeaponBase)gameObject.AddComponent(weaponType);
            newWeapon.Initialize(data);

            activeWeapons.Add(newWeapon);
            Debug.Log($"[WeaponManager] Added new weapon: {data.weaponName} (Type: {weaponType.Name})");
        }

        public int GetWeaponLevel(WeaponData data)
        {
            if (data == null) return 0;
            WeaponBase existing = activeWeapons.Find(w => w.weaponData == data || (w.weaponData != null && w.weaponData.weaponName == data.weaponName));
            return existing != null ? existing.currentLevel : 0;
        }

        public System.Type GetWeaponType(string weaponName)
        {
            if (string.IsNullOrEmpty(weaponName)) return typeof(PastaLauncher);

            string lower = weaponName.ToLower();

            if (lower.Contains("сашими") || lower.Contains("sashimi") || lower.Contains("клинок") || lower.Contains("blade"))
            {
                return typeof(SashimiSlice);
            }
            else if (lower.Contains("сальса") || lower.Contains("salsa") || lower.Contains("пятно") || lower.Contains("splash"))
            {
                return typeof(SalsaSplash);
            }
            else if (lower.Contains("суп") || lower.Contains("soup"))
            {
                return typeof(SoupBowl);
            }
            else if (lower.Contains("суши") || lower.Contains("sushi"))
            {
                return typeof(SushiSet);
            }
            else if (lower.Contains("тако") || lower.Contains("taco"))
            {
                return typeof(TacoThrow);
            }
            else if (lower.Contains("пицц") || lower.Contains("pizza") || lower.Contains("cutter"))
            {
                return typeof(PizzaCutter);
            }
            else if (lower.Contains("чайник") || lower.Contains("кипяток") || lower.Contains("tea"))
            {
                return typeof(TeaPot);
            }
            else if (lower.Contains("бургер") || lower.Contains("burger"))
            {
                return typeof(MegaBurger);
            }
            else
            {
                return typeof(PastaLauncher);
            }
        }
    }
}
