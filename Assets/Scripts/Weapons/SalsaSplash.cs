using System.Collections.Generic;
using UnityEngine;
using FoodSurvivors.Data;

namespace FoodSurvivors.Weapons
{
    public class SalsaSplash : WeaponBase
    {
        protected override void Attack()
        {
            WeaponLevelData stats = GetCurrentLevelData();
            Vector3 spawnPos = new Vector3(transform.position.x, 0.02f, transform.position.z);

            GameObject puddleObj;
            if (weaponData != null && weaponData.weaponPrefab != null)
            {
                puddleObj = Instantiate(weaponData.weaponPrefab, spawnPos, Quaternion.identity);
            }
            else
            {
                puddleObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                puddleObj.name = "SalsaPuddle";
                puddleObj.transform.position = spawnPos;

                float diameter = 3f * stats.areaScale;
                puddleObj.transform.localScale = new Vector3(diameter, 0.02f, diameter);

                Renderer rend = puddleObj.GetComponent<Renderer>();
                if (rend != null && weaponData != null)
                {
                    Color c = weaponData.weaponColor;
                    c.a = 0.6f;
                    rend.material.color = c;
                }
            }

            Collider col = puddleObj.GetComponent<Collider>();
            if (col != null) col.isTrigger = true;

            Rigidbody rb = puddleObj.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = puddleObj.AddComponent<Rigidbody>();
                rb.isKinematic = true;
                rb.useGravity = false;
            }

            SalsaPuddleZone zone = puddleObj.AddComponent<SalsaPuddleZone>();
            zone.Setup(stats.damage, stats.duration, 0.5f);
        }
    }

    public class SalsaPuddleZone : MonoBehaviour
    {
        private float damage;
        private float duration;
        private float tickInterval;

        private HashSet<GameObject> enemiesInZone = new HashSet<GameObject>();
        private float tickTimer;

        public void Setup(float dmg, float dur, float interval)
        {
            damage = dmg;
            duration = dur > 0 ? dur : 3f;
            tickInterval = interval > 0 ? interval : 0.5f;
            tickTimer = 0f;
        }

        private void Update()
        {
            duration -= Time.deltaTime;
            tickTimer -= Time.deltaTime;

            if (tickTimer <= 0f)
            {
                tickTimer = tickInterval;
                ApplyDamageToAllInZone();
            }

            if (duration <= 0f)
            {
                Destroy(gameObject);
            }
        }

        private void ApplyDamageToAllInZone()
        {
            List<GameObject> toRemove = new List<GameObject>();
            foreach (var enemy in enemiesInZone)
            {
                if (enemy == null)
                {
                    toRemove.Add(enemy);
                    continue;
                }
                enemy.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
            }

            foreach (var r in toRemove)
            {
                enemiesInZone.Remove(r);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Enemy"))
            {
                enemiesInZone.Add(other.gameObject);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Enemy"))
            {
                enemiesInZone.Remove(other.gameObject);
            }
        }
    }
}
