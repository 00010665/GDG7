using System.Collections.Generic;
using UnityEngine;
using FoodSurvivors.Data;

namespace FoodSurvivors.Weapons
{
    public class PizzaCutter : WeaponBase
    {
        protected override void PerformAttack()
        {
            WeaponLevelData stats = GetCurrentLevelData();
            Vector3 direction = transform.forward;
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.01f) direction = Vector3.forward;

            int count = Mathf.Max(1, stats.projectileCount);
            float spread = count > 1 ? 20f : 0f;
            float startAngle = -spread * (count - 1) / 2f;

            for (int i = 0; i < count; i++)
            {
                Quaternion rot = Quaternion.Euler(0, startAngle + i * spread, 0);
                Vector3 shootDir = rot * direction;
                SpawnCutter(shootDir, stats);
            }
        }

        private void SpawnCutter(Vector3 direction, WeaponLevelData stats)
        {
            GameObject cutterObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cutterObj.name = "PizzaCutterBlade";
            cutterObj.transform.position = transform.position + Vector3.up * 0.5f;
            cutterObj.transform.rotation = Quaternion.LookRotation(direction) * Quaternion.Euler(90f, 0f, 0f);
            cutterObj.transform.localScale = new Vector3(1.2f, 0.08f, 1.2f) * GetArea(stats.areaScale);

            Renderer rend = cutterObj.GetComponent<Renderer>();
            if (rend != null)
            {
                rend.material.color = weaponData != null ? weaponData.weaponColor : new Color(0.85f, 0.85f, 0.9f);
            }

            Collider col = cutterObj.GetComponent<Collider>();
            if (col != null) col.isTrigger = true;

            Rigidbody rb = cutterObj.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;

            PizzaCutterProjectile proj = cutterObj.AddComponent<PizzaCutterProjectile>();
            proj.Setup(direction, stats.speed > 0 ? stats.speed : 10f, GetDamage(stats.damage), stats.duration > 0 ? stats.duration : 4f);
        }
    }

    public class PizzaCutterProjectile : MonoBehaviour
    {
        private Vector3 direction;
        private float speed;
        private float damage;
        private float lifetime;
        private Dictionary<GameObject, float> hitCooldowns = new Dictionary<GameObject, float>();
        private const float HIT_INTERVAL = 0.35f;

        public void Setup(Vector3 dir, float moveSpeed, float dmg, float dur)
        {
            direction = dir.normalized;
            speed = moveSpeed;
            damage = dmg;
            lifetime = dur;
        }

        private void Update()
        {
            transform.position += direction * (speed * Time.deltaTime);
            transform.Rotate(500f * Time.deltaTime, 0f, 0f, Space.Self);

            lifetime -= Time.deltaTime;
            if (lifetime <= 0f)
            {
                Destroy(gameObject);
                return;
            }

            List<GameObject> toRemove = new List<GameObject>();
            List<GameObject> keys = new List<GameObject>(hitCooldowns.Keys);
            foreach (var k in keys)
            {
                hitCooldowns[k] -= Time.deltaTime;
                if (hitCooldowns[k] <= 0f) toRemove.Add(k);
            }
            foreach (var r in toRemove) hitCooldowns.Remove(r);
        }

        private void OnTriggerStay(Collider other)
        {
            if (other.CompareTag("Enemy"))
            {
                GameObject enemy = other.gameObject;
                if (!hitCooldowns.ContainsKey(enemy))
                {
                    hitCooldowns[enemy] = HIT_INTERVAL;
                    enemy.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
                }
            }
        }
    }
}
