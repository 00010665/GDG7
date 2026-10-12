using UnityEngine;
using FoodSurvivors.Core;
using FoodSurvivors.Enemies;
using FoodSurvivors.Player;
using FoodSurvivors.Weapons;

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

            // Безопасное назначение тэга
            try { gameObject.tag = "Boss"; }
            catch { Debug.LogWarning("[RivalBoss] Тэг 'Boss' еще не добавлен в TagManager"); }

            rb = GetComponent<Rigidbody>();
            if (rb == null) rb = gameObject.AddComponent<Rigidbody>();

            // Настройка физики (упирается в стены зданий и машин!)
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

            switch (archetype)
            {
                case BossArchetype.FoodCart:
                    moveSpeed = 0f;
                    break;
                case BossArchetype.FoodTruck:
                    moveSpeed = (player != null ? player.currentMoveSpeed : 6f) * 1.5f;
                    break;
                case BossArchetype.TripleDelivery:
                    moveSpeed = (player != null ? player.currentMoveSpeed : 6f) * 1.1f;
                    break;
            }

            EquipRivalWeapons(levelOffset);
        }

        private void EquipRivalWeapons(int levelOffset)
        {
            if (player != null)
            {
                WeaponManager pw = player.GetComponent<WeaponManager>();
                if (pw != null && pw.activeWeapons != null)
                {
                    foreach (var w in pw.activeWeapons)
                    {
                        if (w != null && w.weaponData != null)
                        {
                            weaponManager.AddWeapon(w.weaponData);
                            WeaponBase bw = weaponManager.activeWeapons.Find(x => x.weaponData == w.weaponData);
                            if (bw != null) bw.currentLevel = Mathf.Max(1, w.currentLevel - levelOffset);
                        }
                    }
                }
            }
        }

        private void Update()
        {
            HandleArchetypeBehavior();
            FindNearestTourist();
        }

        private void FixedUpdate()
        {
            // Физическое перемещение без прохождения сквозь стены
            if (archetype == BossArchetype.FoodCart || isTruckStopped || currentMoveDirection.sqrMagnitude < 0.01f)
            {
                return;
            }

            Vector3 targetPos = rb.position + currentMoveDirection * (moveSpeed * Time.fixedDeltaTime);
            rb.MovePosition(targetPos);

            Quaternion targetRotation = Quaternion.LookRotation(currentMoveDirection, Vector3.up);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, Time.fixedDeltaTime * 10f));
        }

        private void HandleArchetypeBehavior()
        {
            if (archetype == BossArchetype.FoodCart)
            {
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
            if (archetype == BossArchetype.FoodCart || isTruckStopped)
            {
                currentMoveDirection = Vector3.zero;
                return;
            }

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
                    ec.TakeDamageFromBoss(15f * damageMultiplier * Time.deltaTime);
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
                    ec.TakeDamageFromBoss(15f * damageMultiplier * Time.deltaTime);
                }
            }
        }
    }
}