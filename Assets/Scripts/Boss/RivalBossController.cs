using UnityEngine;
using System.Collections.Generic;
using FoodSurvivors.Core;
using FoodSurvivors.Enemies;
using FoodSurvivors.Player;
using FoodSurvivors.Weapons;
using FoodSurvivors.Data;
using System;

namespace FoodSurvivors.Boss
{
    public enum BossArchetype
    {
        FoodCart,       // Тележка (стоит, притягивает, поворачивается)
        FoodTruck,      // Фургончик (ездит, прямоугольный коллайдер)
        TripleDelivery  // Тройная доставка (3 курьера)
    }

    [RequireComponent(typeof(Rigidbody))]
    public class RivalBossController : MonoBehaviour
    {
        [Header("Boss Profile")]
        public BossArchetype archetype;
        public float moveSpeed = 6f;
        public float damageMultiplier = 1f;
        [HideInInspector] public float weaponDamageMultiplier = 1f;
        [HideInInspector] public float weaponCooldownReduction;
        [HideInInspector] public float weaponAreaMultiplier = 1f;

        [Header("Food Cart Pull Aura")]
        public float pullRadius = 7.5f; // Уменьшенный радиус притяжения
        public float pullSpeed = 4f;

        [Header("Food Truck Timing")]
        private float truckTimer = 0f;
        private bool isTruckStopped = false;

        private Rigidbody rb;
        private Transform targetTourist;
        private WeaponManager weaponManager;
        private PassiveManager passiveManager;
        private PlayerController player;
        private Vector3 currentMoveDirection = Vector3.zero;

        public string DisplayName
        {
            get
            {
                switch (archetype)
                {
                    case BossArchetype.FoodCart: return "ТЕЛЕЖКА";
                    case BossArchetype.FoodTruck: return "ФУРГОНЧИК";
                    case BossArchetype.TripleDelivery: return "ДОСТАВКА";
                    default: return "СОПЕРНИК";
                }
            }
        }

        public void InitializeBoss(BossArchetype arch, float dmgMult, int levelOffset)
        {
            archetype = arch;
            damageMultiplier = dmgMult;
            weaponDamageMultiplier = 1f;
            weaponCooldownReduction = 0f;
            weaponAreaMultiplier = 1f;

            try { gameObject.tag = "Boss"; } catch { }

            rb = GetComponent<Rigidbody>();
            if (rb == null) rb = gameObject.AddComponent<Rigidbody>();

            rb.useGravity = false;
            rb.isKinematic = false;
            rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
            rb.interpolation = RigidbodyInterpolation.Interpolate;

            // Если это Фургончик — гарантируем прямоугольный BoxCollider для клинков Сашими!
            if (archetype == BossArchetype.FoodTruck)
            {
                BoxCollider box = GetComponent<BoxCollider>();
                if (box == null) box = gameObject.AddComponent<BoxCollider>();
                FitTruckColliderToRenderers(box);
            }

            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.GetComponent<PlayerController>();
                Collider bossCol = GetComponent<Collider>();
                Collider playerCol = playerObj.GetComponent<Collider>();
                if (bossCol != null && playerCol != null) Physics.IgnoreCollision(bossCol, playerCol, true);
            }

            weaponManager = gameObject.AddComponent<WeaponManager>();
            passiveManager = gameObject.AddComponent<PassiveManager>();

            switch (archetype)
            {
                case BossArchetype.FoodCart: moveSpeed = 0f; break;
                case BossArchetype.FoodTruck: moveSpeed = (player != null ? player.currentMoveSpeed : 6f) * 1.5f; break;
                case BossArchetype.TripleDelivery: moveSpeed = (player != null ? player.currentMoveSpeed : 6f) * 1.1f; break;
            }

            SetupExactBossLoadout(levelOffset);
        }

        private void SetupExactBossLoadout(int levelOffset)
        {
            string savedWeapons = PlayerPrefs.GetString("PrevRun_Weapons", "");
            string savedPassives = PlayerPrefs.GetString("PrevRun_Passives", "");

            bool loadedSavedWeapons = !string.IsNullOrWhiteSpace(savedWeapons) &&
                                      ApplySavedBuild(savedWeapons, savedPassives, levelOffset);
            if (!loadedSavedWeapons)
            {
                Debug.Log("[RivalBossController] No usable previous-run weapon build found; equipping the archetype fallback loadout.");
                ApplyFirstRunDefaultBuild();
            }

            foreach (var w in weaponManager.activeWeapons)
            {
                if (w != null) w.isBossWeapon = true;
            }
        }

        private void ApplyFirstRunDefaultBuild()
        {
            switch (archetype)
            {
                case BossArchetype.FoodCart:
                    EquipWeaponByName("SalsaSplash", 3);
                    EquipPassiveByName("SpicyPepper", 3);
                    break;
                case BossArchetype.FoodTruck:
                    EquipWeaponByName("TacoThrow", 3);
                    EquipPassiveByName("FastService", 3);
                    break;
                case BossArchetype.TripleDelivery:
                    EquipWeaponByName("PastaLauncher", 3);
                    EquipPassiveByName("FastService", 3);
                    break;
            }
        }

        private bool ApplySavedBuild(string wStr, string pStr, int offset)
        {
            int loadedWeaponCount = 0;
            foreach (var entry in wStr.Split(';'))
            {
                if (!TryParseBuildEntry(entry, out string assetName, out int savedLevel)) continue;
                if (EquipWeaponByName(assetName, Mathf.Max(1, savedLevel - offset)))
                    loadedWeaponCount++;
            }

            if (loadedWeaponCount == 0) return false;

            foreach (var entry in pStr.Split(';'))
            {
                if (!TryParseBuildEntry(entry, out string assetName, out int savedLevel)) continue;
                EquipPassiveByName(assetName, Mathf.Max(1, savedLevel - offset));
            }

            return loadedWeaponCount > 0;
        }

        private static bool TryParseBuildEntry(string entry, out string assetName, out int level)
        {
            assetName = null;
            level = 0;
            if (string.IsNullOrWhiteSpace(entry)) return false;

            int separator = entry.LastIndexOf(':');
            if (separator <= 0 || separator >= entry.Length - 1) return false;
            assetName = entry.Substring(0, separator).Trim();
            return assetName.Length > 0 && int.TryParse(entry.Substring(separator + 1), out level) && level > 0;
        }

        private bool EquipWeaponByName(string name, int lvl)
        {
            WeaponData weapon = FindWeaponData(name);
            if (weapon == null || weaponManager == null)
            {
                Debug.LogWarning($"[RivalBossController] Could not resolve saved weapon '{name}' from LevelUpContentCatalog.");
                return false;
            }

            weaponManager.AddWeapon(weapon);
            WeaponBase equipped = weaponManager.activeWeapons.Find(x => x != null && x.weaponData == weapon);
            if (equipped == null) return false;
            equipped.currentLevel = Mathf.Clamp(lvl, 1, Mathf.Max(1, weapon.levels != null ? weapon.levels.Count : 1));
            equipped.isBossWeapon = true;
            return true;
        }

        private bool EquipPassiveByName(string name, int lvl)
        {
            PassiveData passive = FindPassiveData(name);
            if (passive == null || passiveManager == null)
            {
                Debug.LogWarning($"[RivalBossController] Could not resolve saved passive '{name}' from LevelUpContentCatalog.");
                return false;
            }

            for (int i = 0; i < lvl; i++) passiveManager.AddOrUpgradePassive(passive);
            return true;
        }

        private static WeaponData FindWeaponData(string name)
        {
            WeaponData direct = Resources.Load<WeaponData>($"Weapons/{name}");
            if (direct != null) return direct;

            LevelUpContentCatalog catalog = Resources.Load<LevelUpContentCatalog>("LevelUpContentCatalog");
            if (catalog == null || catalog.weapons == null) return null;
            return catalog.weapons.Find(item => item != null &&
                (string.Equals(item.name, name, StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(item.weaponName, name, StringComparison.OrdinalIgnoreCase)));
        }

        private static PassiveData FindPassiveData(string name)
        {
            PassiveData direct = Resources.Load<PassiveData>($"Passives/{name}");
            if (direct != null) return direct;

            LevelUpContentCatalog catalog = Resources.Load<LevelUpContentCatalog>("LevelUpContentCatalog");
            if (catalog == null || catalog.passives == null) return null;
            return catalog.passives.Find(item => item != null &&
                (string.Equals(item.name, name, StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(item.passiveName, name, StringComparison.OrdinalIgnoreCase)));
        }

        public void ApplyBuildPassive(PassiveType passiveType, float value)
        {
            switch (passiveType)
            {
                case PassiveType.SpeedBonus:
                    moveSpeed += Mathf.Max(0f, value);
                    break;
                case PassiveType.DamageBonus:
                    weaponDamageMultiplier += Mathf.Max(0f, value);
                    break;
                case PassiveType.StaminaSynergy:
                    weaponDamageMultiplier += Mathf.Max(0f, value) * 0.05f;
                    break;
                case PassiveType.StaminaDamageBonus:
                    weaponDamageMultiplier += Mathf.Max(0f, value) * 0.01f;
                    break;
                case PassiveType.CooldownReduction:
                    weaponCooldownReduction = Mathf.Min(0.70f, weaponCooldownReduction + Mathf.Max(0f, value));
                    break;
                case PassiveType.AreaBonus:
                    weaponAreaMultiplier += Mathf.Max(0f, value);
                    break;
                case PassiveType.MagnetBonus:
                    if (archetype == BossArchetype.FoodCart)
                        pullRadius += Mathf.Max(0f, value);
                    break;
            }
        }

        private void Update()
        {
            HandleArchetypeBehavior();
            FindNearestTourist();
            RotateTowardsTarget();
        }

        private void FixedUpdate()
        {
            if (archetype == BossArchetype.FoodCart || isTruckStopped || currentMoveDirection.sqrMagnitude < 0.01f) return;
            Vector3 targetPos = rb.position + currentMoveDirection * (moveSpeed * Time.fixedDeltaTime);
            rb.MovePosition(targetPos);
        }

        private void RotateTowardsTarget()
        {
            Vector3 lookDir = Vector3.forward;
            if (archetype == BossArchetype.FoodCart && targetTourist != null)
            {
                lookDir = (targetTourist.position - transform.position);
            }
            else if (currentMoveDirection.sqrMagnitude > 0.01f)
            {
                lookDir = currentMoveDirection;
            }

            lookDir.y = 0f;
            if (lookDir.sqrMagnitude > 0.05f)
            {
                Quaternion rot = Quaternion.LookRotation(lookDir.normalized, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * 12f);
            }
        }

        private void HandleArchetypeBehavior()
        {
            if (archetype == BossArchetype.FoodCart)
            {
                // Притягиваем туристов и разворачиваем их лицом к тележке!
                Collider[] colliders = Physics.OverlapSphere(transform.position, pullRadius);
                HashSet<EnemyController> pulledEnemies = new HashSet<EnemyController>();
                foreach (var c in colliders)
                {
                    EnemyController enemy = c.GetComponentInParent<EnemyController>();
                    if (enemy != null && pulledEnemies.Add(enemy))
                    {
                        enemy.transform.position = Vector3.MoveTowards(enemy.transform.position, transform.position, pullSpeed * Time.deltaTime);
                        enemy.SetCartFacingTarget(transform);
                        Vector3 toCart = (transform.position - enemy.transform.position);
                        toCart.y = 0f;
                        if (toCart.sqrMagnitude > 0.01f)
                        {
                            enemy.transform.rotation = Quaternion.LookRotation(toCart.normalized, Vector3.up);
                        }
                    }
                }
            }
            else if (archetype == BossArchetype.FoodTruck)
            {
                truckTimer += Time.deltaTime;
                if (truckTimer >= 10f) truckTimer = 0f;
                isTruckStopped = (truckTimer >= 7f);
            }
        }

        private static void FitTruckColliderToRenderers(BoxCollider box)
        {
            Renderer[] renderers = box.GetComponentsInChildren<Renderer>(true);
            bool foundRenderer = false;
            Bounds worldBounds = new Bounds(box.transform.position, Vector3.zero);
            foreach (Renderer renderer in renderers)
            {
                if (renderer == null || !renderer.enabled || !renderer.gameObject.activeInHierarchy) continue;
                if (!foundRenderer)
                {
                    worldBounds = renderer.bounds;
                    foundRenderer = true;
                }
                else worldBounds.Encapsulate(renderer.bounds);
            }

            if (!foundRenderer) return;

            Vector3 min = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
            Vector3 max = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);
            for (int i = 0; i < 8; i++)
            {
                Vector3 corner = new Vector3(
                    (i & 1) == 0 ? worldBounds.min.x : worldBounds.max.x,
                    (i & 2) == 0 ? worldBounds.min.y : worldBounds.max.y,
                    (i & 4) == 0 ? worldBounds.min.z : worldBounds.max.z);
                Vector3 local = box.transform.InverseTransformPoint(corner);
                min = Vector3.Min(min, local);
                max = Vector3.Max(max, local);
            }

            box.center = (min + max) * 0.5f;
            box.size = max - min;
            box.isTrigger = false;
        }

        private void FindNearestTourist()
        {
            if (targetTourist == null) targetTourist = FindNearestEnemyTransform();
            if (targetTourist != null)
            {
                Vector3 dir = (targetTourist.position - transform.position);
                dir.y = 0f;
                currentMoveDirection = dir.sqrMagnitude > 0.05f ? dir.normalized : Vector3.zero;
            }
            else { currentMoveDirection = Vector3.zero; }
        }

        private Transform FindNearestEnemyTransform()
        {
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            Transform nearest = null;
            float minDist = float.MaxValue;
            foreach (var e in enemies)
            {
                if (e == null) continue;
                float d = Vector3.Distance(transform.position, e.transform.position);
                if (d < minDist) { minDist = d; nearest = e.transform; }
            }
            return nearest;
        }

        private void OnTriggerStay(Collider other)
        {
            if (other.CompareTag("Enemy"))
            {
                EnemyController ec = other.GetComponent<EnemyController>();
                if (ec != null) ec.TakeDamageFromBoss(18f * damageMultiplier * Time.deltaTime);
            }
        }
    }
}
