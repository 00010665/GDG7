using UnityEngine;
using FoodSurvivors.Data;

namespace FoodSurvivors.Weapons
{
    public class SushiSet : WeaponBase
    {
        protected override void PerformAttack()
        {
            WeaponLevelData stats = GetCurrentLevelData();

            Vector3[] directions = new Vector3[]
            {
                Vector3.forward,
                Vector3.back,
                Vector3.right,
                Vector3.left
            };

            for (int i = 0; i < directions.Length; i++)
            {
                SpawnSushiProjectile(directions[i], stats);
            }

            if (stats.projectileCount >= 2)
            {
                Vector3[] diagonals = new Vector3[]
                {
                    (Vector3.forward + Vector3.right).normalized,
                    (Vector3.forward + Vector3.left).normalized,
                    (Vector3.back + Vector3.right).normalized,
                    (Vector3.back + Vector3.left).normalized
                };

                for (int i = 0; i < diagonals.Length; i++)
                {
                    SpawnSushiProjectile(diagonals[i], stats);
                }
            }
        }

        private void SpawnSushiProjectile(Vector3 direction, WeaponLevelData stats)
        {
            GameObject sushiObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sushiObj.name = "SushiProjectile";
            sushiObj.transform.position = transform.position + Vector3.up * 0.5f;
            sushiObj.transform.rotation = Quaternion.LookRotation(direction);
            sushiObj.transform.localScale = new Vector3(0.5f, 0.25f, 0.7f) * GetArea(stats.areaScale);

            Renderer rend = sushiObj.GetComponent<Renderer>();
            if (rend != null)
            {
                rend.material.color = weaponData != null ? weaponData.weaponColor : new Color(0.1f, 0.8f, 0.4f);
            }

            Collider col = sushiObj.GetComponent<Collider>();
            if (col != null) col.isTrigger = true;

            Rigidbody rb = sushiObj.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;

            SushiProjectile proj = sushiObj.AddComponent<SushiProjectile>();
            proj.Setup(direction, stats.speed > 0 ? stats.speed : 12f, GetDamage(stats.damage), stats.duration > 0 ? stats.duration : 3f);
        }
    }

    public class SushiProjectile : MonoBehaviour
    {
        private Vector3 direction;
        private float speed;
        private float damage;
        private float lifetime;

        public void Setup(Vector3 dir, float moveSpeed, float dmg, float duration)
        {
            direction = dir.normalized;
            speed = moveSpeed;
            damage = dmg;
            lifetime = duration;
        }

        private void Update()
        {
            transform.position += direction * (speed * Time.deltaTime);
            lifetime -= Time.deltaTime;
            if (lifetime <= 0f)
            {
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Enemy"))
            {
                other.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
                Destroy(gameObject);
            }
        }
    }
}
