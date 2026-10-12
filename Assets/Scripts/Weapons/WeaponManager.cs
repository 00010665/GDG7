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

            System.Type weaponType = GetWeaponType(data);
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

        public System.Type GetWeaponType(WeaponData data)
        {
            if (data == null) return typeof(PastaLauncher);

            // 1. По надежному перечислению WeaponType
            switch (data.weaponType)
            {
                case WeaponType.SashimiSlice: return typeof(SashimiSlice);
                case WeaponType.SalsaSplash: return typeof(SalsaSplash);
                case WeaponType.SoupBowl: return typeof(SoupBowl);
                case WeaponType.SushiSet: return typeof(SushiSet);
                case WeaponType.TacoThrow: return typeof(TacoThrow);
                case WeaponType.PizzaCutter: return typeof(PizzaCutter);
                case WeaponType.TeaPot: return typeof(TeaPot);
                case WeaponType.MegaBurger: return typeof(MegaBurger);
            }

            // 2. По имени файла ассета (TeaPot.asset, SalsaSplash.asset и т.д.)
            string assetFileName = data.name.ToLower();
            if (assetFileName.Contains("sashimi")) return typeof(SashimiSlice);
            if (assetFileName.Contains("salsa")) return typeof(SalsaSplash);
            if (assetFileName.Contains("teapot") || assetFileName.Contains("hotdog")) return typeof(TeaPot);
            if (assetFileName.Contains("soup")) return typeof(SoupBowl);
            if (assetFileName.Contains("sushi")) return typeof(SushiSet);
            if (assetFileName.Contains("taco")) return typeof(TacoThrow);
            if (assetFileName.Contains("pizza")) return typeof(PizzaCutter);
            if (assetFileName.Contains("burger")) return typeof(MegaBurger);

            return typeof(PastaLauncher);
        }

        public System.Type GetWeaponType(string weaponName)
        {
            // Устаревший вызов для обратной совместимости
            return typeof(PastaLauncher);
        }
    }
}
