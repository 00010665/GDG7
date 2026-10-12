using UnityEngine;
using FoodSurvivors.Core;
using FoodSurvivors.Enemies;
using FoodSurvivors.Player;
using FoodSurvivors.Weapons;

namespace FoodSurvivors.Boss
{
    public enum BossArchetype
    {
        FoodCart,       // Тележка: стоит на месте, притягивает туристов
        FoodTruck,      // Фургончик: скорость 1.5x, цикл 7с бега / 3с остановки
        TripleDelivery  // Тройная доставка: 3 курьера по 30% урона
    }

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

        private Transform targetTourist;
        private WeaponManager weaponManager;
        private PlayerController player;

        public void InitializeBoss(BossArchetype arch, float dmgMult, int levelOffset)
        {
            archetype = arch;
            damageMultiplier = dmgMult;
            gameObject.tag = "Boss";

            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.GetComponent<PlayerController>();

            weaponManager = gameObject.AddComponent<WeaponManager>();

            // Настройка архетипа
            switch (archetype)
            {
                case BossArchetype.FoodCart:
                    moveSpeed = 0f; // Неподвижен
                    break;

                case BossArchetype.FoodTruck:
                    moveSpeed = (player != null ? player.currentMoveSpeed : 6f) * 1.5f; // 1.5x от игрока
                    break;

                case BossArchetype.TripleDelivery:
                    moveSpeed = (player != null ? player.currentMoveSpeed : 6f) * 1.1f;
                    break;
            }

            EquipRivalWeapons(levelOffset);
        }

        private void EquipRivalWeapons(int levelOffset)
        {
            // Берем оружие игрока со сдвигом на -1 уровень
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
            FindAndChaseNearestTourist();
        }

        private void HandleArchetypeBehavior()
        {
            // 1. «Тележка с едой» — притяжение туристов
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
            // 2. «Фургончик с едой» — 7с бежит / 3с стоит
            else if (archetype == BossArchetype.FoodTruck)
            {
                truckTimer += Time.deltaTime;
                if (truckTimer >= 10f) truckTimer = 0f;
                isTruckStopped = (truckTimer >= 7f);
            }
        }

        private void FindAndChaseNearestTourist()
        {
            if (archetype == BossArchetype.FoodCart || isTruckStopped) return;

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
                    transform.position += dir.normalized * (moveSpeed * Time.deltaTime);
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir.normalized), Time.deltaTime * 10f);
                }
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

        // Авто-урон по касанию туристов боссом
        private void OnTriggerStay(Collider other)
        {
            if (other.CompareTag("Enemy"))
            {
                EnemyController ec = other.GetComponent<EnemyController>();
                if (ec != null)
                {
                    ec.TakeDamage(15f * damageMultiplier * Time.deltaTime, true);
                }
            }
        }
    }
}