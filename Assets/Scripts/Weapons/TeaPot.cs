using System.Collections.Generic;
using UnityEngine;
using FoodSurvivors.Data;

namespace FoodSurvivors.Weapons
{
    public class TeaPot : WeaponBase
    {
        private GameObject activeBeamObj;

        protected override void PerformAttack()
        {
            WeaponLevelData stats = GetCurrentLevelData();

            if (activeBeamObj != null) Destroy(activeBeamObj);

            Vector3 spawnOffset = transform.forward * (2.0f * GetArea(stats.areaScale)) + Vector3.up * 0.5f;

            if (weaponData != null && weaponData.weaponPrefab != null)
            {
                activeBeamObj = Instantiate(weaponData.weaponPrefab, transform.position + spawnOffset, transform.rotation);
            }
            else
            {
                activeBeamObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                activeBeamObj.transform.position = transform.position + spawnOffset;
                activeBeamObj.transform.rotation = transform.rotation * Quaternion.Euler(90f, 0f, 0f);
            }

            activeBeamObj.transform.SetParent(transform, true);
            float scale = 2.2f * GetArea(stats.areaScale); //???
            Vector3 prefabScale = (weaponData != null && weaponData.weaponPrefab != null)
     ? weaponData.weaponPrefab.transform.localScale
     : new Vector3(2f, 2f, 3f);

            // Применяем масштаб префаба!
            activeBeamObj.transform.localScale = prefabScale * GetArea(stats.areaScale);

            Collider col = activeBeamObj.GetComponent<Collider>();
            if (col == null) col = activeBeamObj.AddComponent<BoxCollider>();
            col.isTrigger = true;

            Rigidbody rb = activeBeamObj.GetComponent<Rigidbody>();
            if (rb == null) rb = activeBeamObj.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;

            BoilingWaterStream streamScript = activeBeamObj.AddComponent<BoilingWaterStream>();
            streamScript.Setup(GetDamage(stats.damage), stats.duration > 0 ? stats.duration : 2.0f);
        }

        private void OnDestroy()
        {
            if (activeBeamObj != null) Destroy(activeBeamObj);
        }
    }

    public class BoilingWaterStream : MonoBehaviour
    {
        private float damage;
        private float duration;
        private float tickTimer = 0f;
        private const float TICK_INTERVAL = 0.35f;
        private HashSet<GameObject> enemiesInRange = new HashSet<GameObject>();

        public void Setup(float dmg, float dur)
        {
            damage = dmg;
            duration = dur;
            tickTimer = 0f;
        }

        private void Update()
        {
            duration -= Time.deltaTime;
            tickTimer -= Time.deltaTime;

            if (tickTimer <= 0f)
            {
                tickTimer = TICK_INTERVAL;
                DamageEnemies();
            }

            if (duration <= 0f)
            {
                Destroy(gameObject);
            }
        }

        private void DamageEnemies()
        {
            List<GameObject> toRemove = new List<GameObject>();
            foreach (var enemy in enemiesInRange)
            {
                if (enemy == null) { toRemove.Add(enemy); continue; }
                enemy.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
            }
            foreach (var r in toRemove) enemiesInRange.Remove(r);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Enemy")) enemiesInRange.Add(other.gameObject);
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Enemy")) enemiesInRange.Remove(other.gameObject);
        }
    }
}