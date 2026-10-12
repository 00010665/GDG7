using UnityEngine;
using FoodSurvivors.Core;
using FoodSurvivors.Enemies;
using FoodSurvivors.Player;
using FoodSurvivors.Weapons;
using FoodSurvivors.Data;

namespace FoodSurvivors.Boss
{
    public enum BossArchetype
    {
        FoodCart,       // Тележка
        FoodTruck,      // Фургончик
        TripleDelivery  // Тройная доставка
    }

    [RequireComponent(typeof(Rigidbody))]
    public class RivalBossController : MonoBehaviour
    {
        [Header("Boss Profile")]
        public BossArchetype archetype;
        public float moveSpeed = 6f;
        public float damageMultiplier = 1f;

        [Header("Food Cart Pull Aura")]
        public float pullRadius = 18f;
        public float pullSpeed = 5f;

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

            try { gameObject.tag = "Boss"; }
            catch { }

            rb = GetComponent<Rigidbody>();
            if (rb == null) rb = gameObject.AddComponent<Rigidbody>();

            rb.useGravity = false;
            rb.isKinematic = false;
            rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
            rb.interpolation = RigidbodyInterpolation.Interpolate;

            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.GetComponent<PlayerController>();

                Collider bossCol = GetComponent<Collider>();
                Collider playerCol = playerObj.GetComponent<Collider>();
                if (bossCol != null && playerCol != null)
                {
                    Physics.IgnoreCollision(bossCol, playerCol, true);
                }
            }

            weaponManager = gameObject.AddComponent<WeaponManager>();
            passiveManager = gameObject.AddComponent<PassiveManager>();

            switch (archetype)
            {
                case BossArchetype.FoodCart:
                    moveSpeed = 0f; // Тележка стоит на месте
                    break;
                case BossArchetype.FoodTruck:
                    moveSpeed = (player != null ? player.currentMoveSpeed : 6f) * 1.5f;
                    break;
                case BossArchetype.TripleDelivery:
                    moveSpeed = (player != null ? player.currentMoveSpeed : 6f) * 1.1f;
                    break;
            }

            SetupExactBossLoadout(levelOffset);
        }

        private void SetupExactBossLoadout(int levelOffset)
        {
            // Проверяем: есть ли сохраненный билд игрока из прошлого рана
            string savedWeapons = PlayerPrefs.GetString("PrevRun_Weapons", "");
            string savedPassives = PlayerPrefs.GetString("PrevRun_Passives", "");

            if (!string.IsNullOrEmpty(savedWeapons))
            {
                // Копируем билд игрока из прошлого раунда с (Уровень - 1)
                ApplySavedBuild(savedWeapons, savedPassives, levelOffset);
            }
            else
            {
                // ПЕРВЫЙ ЗАПУСК: Точные фиксированные пресеты!
                ApplyFirstRunDefaultBuild();
            }

            // Помечаем все оружия босса, чтобы они не роняли гемы
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
                    EquipPassiveByName("ComfortShoes", 3);
                    break;
            }
        }

        private void ApplySavedBuild(string wStr, string pStr, int offset)
        {
            string[] wArr = wStr.Split(';');
            foreach (var wData in wArr)
            {
                string[] parts = wData.Split(':');
                if (parts.Length == 2)
                {
                    int lvl = Mathf.Max(1, int.Parse(parts[1]) - offset);
                    EquipWeaponByName(parts[0], lvl);
                }
            }

            string[] pArr = pStr.Split(';');
            foreach (var pData in pArr)
            {
                string[] parts = pData.Split(':');
                if (parts.Length == 2)
                {
                    int lvl = Mathf.Max(1, int.Parse(parts[1]) - offset);
                    EquipPassiveByName(parts[0], lvl);
                }
            }
        }

        private void EquipWeaponByName(string assetName, int level)
        {
            WeaponData w = Resources.Load<WeaponData>($"Weapons/{assetName}");
#if UNITY_EDITOR
            if (w == null)
            {
                string[] guids = UnityEditor.AssetDatabase.FindAssets($"{assetName} t:WeaponData");
                if (guids.Length > 0)
                    w = UnityEditor.AssetDatabase.LoadAssetAtPath<WeaponData>(UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]));
            }
#endif
            if (w != null && weaponManager != null)
            {
                weaponManager.AddWeapon(w);
                var active = weaponManager.activeWeapons.Find(x => x.weaponData == w);
                if (active != null) active.currentLevel = level;
            }
        }

        private void EquipPassiveByName(string assetName, int level)
        {
            PassiveData p = Resources.Load<PassiveData>($"Passives/{assetName}");
#if UNITY_EDITOR
            if (p == null)
            {
                string[] guids = UnityEditor.AssetDatabase.FindAssets($"{assetName} t:PassiveData");
                if (guids.Length > 0)
                    p = UnityEditor.AssetDatabase.LoadAssetAtPath<PassiveData>(UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]));
            }
#endif
            if (p != null && passiveManager != null)
            {
                for (int i = 0; i < level; i++) passiveManager.AddOrUpgradePassive(p);
            }
        }

        private void Update()
        {
            HandleArchetypeBehavior();
            FindNearestTourist();
            RotateTowardsTarget(); // ПОВОРОТ К ЦЕЛИ ДАЖЕ ЕСЛИ БОСС СТОИТ НА МЕСТЕ!
            VacuumNearbyExperience(); // Поглощение опыта боссом!
        }

        private void FixedUpdate()
        {
            if (archetype == BossArchetype.FoodCart || isTruckStopped || currentMoveDirection.sqrMagnitude < 0.01f)
            {
                return;
            }

            Vector3 targetPos = rb.position + currentMoveDirection * (moveSpeed * Time.fixedDeltaTime);
            rb.MovePosition(targetPos);
        }

        private void VacuumNearbyExperience()
        {
            // Босс испепеляет любые гемы опыта рядом с собой
            float eatRadius = (archetype == BossArchetype.FoodCart) ? pullRadius : 8f;
            Collider[] colliders = Physics.OverlapSphere(transform.position, eatRadius);
            foreach (var c in colliders)
            {
                ExperienceGem gem = c.GetComponent<ExperienceGem>();
                if (gem != null)
                {
                    Destroy(gem.gameObject); // Мгновенно съедает гем!
                }
            }
        }

        private void RotateTowardsTarget()
        {
            // Тележка поворачивается лицом к туристу, чтобы направленные атаки стреляли в цель!
            if (targetTourist != null)
            {
                Vector3 lookDir = (targetTourist.position - transform.position);
                lookDir.y = 0f;
                if (lookDir.sqrMagnitude > 0.05f)
                {
                    Quaternion rot = Quaternion.LookRotation(lookDir.normalized, Vector3.up);
                    transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * 12f);
                }
            }
        }

        private void HandleArchetypeBehavior()
        {
            if (archetype == BossArchetype.FoodCart)
            {
                // Притягивает туристов к себе
                Collider[] colliders = Physics.OverlapSphere(transform.position, pullRadius);
                foreach (var c in colliders)
                {
                    if (c.CompareTag("Enemy"))
                    {
                        c.transform.position = Vector3.MoveTowards(c.transform.position, transform.position, pullSpeed * Time.deltaTime);
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

        private void FindNearestTourist()
        {
            if (targetTourist == null)
            {
                targetTourist = FindNearestEnemyTransform();
            }

            if (targetTourist != null)
            {
                Vector3 dir = (targetTourist.position - transform.position);
                dir.y = 0f;

                if (dir.sqrMagnitude > 0.05f)
                {
                    currentMoveDirection = dir.normalized;
                }
                else
                {
                    currentMoveDirection = Vector3.zero;
                }
            }
            else
            {
                currentMoveDirection = Vector3.zero;
            }
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
                if (d < minDist)
                {
                    minDist = d;
                    nearest = e.transform;
                }
            }
            return nearest;
        }

        private void OnTriggerStay(Collider other)
        {
            if (other.CompareTag("Enemy"))
            {
                EnemyController ec = other.GetComponent<EnemyController>();
                if (ec != null)
                {
                    ec.TakeDamageFromBoss(18f * damageMultiplier * Time.deltaTime);
                }
            }
        }

        private void OnCollisionStay(Collision collision)
        {
            if (collision.gameObject.CompareTag("Enemy"))
            {
                EnemyController ec = collision.gameObject.GetComponent<EnemyController>();
                if (ec != null)
                {
                    ec.TakeDamageFromBoss(18f * damageMultiplier * Time.deltaTime);
                }
            }
        }
    }
}