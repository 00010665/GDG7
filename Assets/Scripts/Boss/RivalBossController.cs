using UnityEngine;
using System.Collections.Generic;
using FoodSurvivors.Core;
using FoodSurvivors.Enemies;
using FoodSurvivors.Player;
using FoodSurvivors.Weapons;
using FoodSurvivors.Data;

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

            if (!string.IsNullOrEmpty(savedWeapons))
            {
                ApplySavedBuild(savedWeapons, savedPassives, levelOffset);
            }
            else
            {
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

        private void ApplySavedBuild(string wStr, string pStr, int offset)
        {
            foreach (var wData in wStr.Split(';'))
            {
                string[] parts = wData.Split(':');
                if (parts.Length == 2) EquipWeaponByName(parts[0], Mathf.Max(1, int.Parse(parts[1]) - offset));
            }
            foreach (var pData in pStr.Split(';'))
            {
                string[] parts = pData.Split(':');
                if (parts.Length == 2) EquipPassiveByName(parts[0], Mathf.Max(1, int.Parse(parts[1]) - offset));
            }
        }

        private void EquipWeaponByName(string name, int lvl)
        {
            WeaponData w = Resources.Load<WeaponData>($"Weapons/{name}");
#if UNITY_EDITOR
            if (w == null)
            {
                string[] g = UnityEditor.AssetDatabase.FindAssets($"{name} t:WeaponData");
                if (g.Length > 0) w = UnityEditor.AssetDatabase.LoadAssetAtPath<WeaponData>(UnityEditor.AssetDatabase.GUIDToAssetPath(g[0]));
            }
#endif
            if (w != null && weaponManager != null)
            {
                weaponManager.AddWeapon(w);
                var a = weaponManager.activeWeapons.Find(x => x.weaponData == w);
                if (a != null) a.currentLevel = lvl;
            }
        }

        private void EquipPassiveByName(string name, int lvl)
        {
            PassiveData p = Resources.Load<PassiveData>($"Passives/{name}");
#if UNITY_EDITOR
            if (p == null)
            {
                string[] g = UnityEditor.AssetDatabase.FindAssets($"{name} t:PassiveData");
                if (g.Length > 0) p = UnityEditor.AssetDatabase.LoadAssetAtPath<PassiveData>(UnityEditor.AssetDatabase.GUIDToAssetPath(g[0]));
            }
#endif
            if (p != null && passiveManager != null)
            {
                for (int i = 0; i < lvl; i++) passiveManager.AddOrUpgradePassive(p);
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
