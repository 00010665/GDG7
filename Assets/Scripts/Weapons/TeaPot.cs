using System.Collections.Generic;
using UnityEngine;
using FoodSurvivors.Data;

namespace FoodSurvivors.Weapons
{
    public class TeaPot : WeaponBase
    {
        private GameObject currentStream;

        protected override void PerformAttack()
        {
            WeaponLevelData stats = GetCurrentLevelData();

            if (currentStream != null)
            {
                Destroy(currentStream);
            }

            currentStream = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            currentStream.name = "BoilingWaterJet";
            currentStream.transform.SetParent(transform, false);

            float length = 4.5f * GetArea(stats.areaScale);
            float width = 1.2f * GetArea(stats.areaScale);

            currentStream.transform.localPosition = new Vector3(0f, 0.4f, length / 2f + 0.5f);
            currentStream.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            currentStream.transform.localScale = new Vector3(width, length / 2f, width);

            Renderer rend = currentStream.GetComponent<Renderer>();
            if (rend != null)
            {
                Color c = weaponData != null ? weaponData.weaponColor : new Color(0.3f, 0.7f, 1f, 0.6f);
                c.a = 0.6f;
                rend.material.color = c;
            }

            Collider col = currentStream.GetComponent<Collider>();
            if (col != null) col.isTrigger = true;

            Rigidbody rb = currentStream.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;

            BoilingWaterStream streamScript = currentStream.AddComponent<BoilingWaterStream>();
            streamScript.Setup(GetDamage(stats.damage), stats.duration > 0 ? stats.duration : 1.8f);
        }

        private void OnDestroy()
        {
            if (currentStream != null) Destroy(currentStream);
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
                if (enemy == null)
                {
                    toRemove.Add(enemy);
                    continue;
                }
                enemy.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
            }
            foreach (var r in toRemove) enemiesInRange.Remove(r);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Enemy"))
            {
                enemiesInRange.Add(other.gameObject);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Enemy"))
            {
                enemiesInRange.Remove(other.gameObject);
            }
        }
    }
}
