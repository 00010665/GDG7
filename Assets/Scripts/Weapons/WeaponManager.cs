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

        private System.Type GetWeaponType(string weaponName)
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
            else
            {
                return typeof(PastaLauncher);
            }
        }
    }
}
