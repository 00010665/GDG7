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
            GameObject sushiObj;
            Quaternion rot = Quaternion.LookRotation(direction);

            if (weaponData != null && weaponData.weaponPrefab != null)
            {
                sushiObj = Instantiate(weaponData.weaponPrefab, transform.position + Vector3.up * 0.5f, rot);
            }
            else
            {
                sushiObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                sushiObj.name = "SushiProjectile";
                sushiObj.transform.position = transform.position + Vector3.up * 0.5f;
                sushiObj.transform.rotation = rot;
            }

            Vector3 prefabScale = (weaponData != null && weaponData.weaponPrefab != null)
    ? weaponData.weaponPrefab.transform.localScale
    : new Vector3(0.8f, 0.4f, 1f);

            sushiObj.transform.localScale = prefabScale * GetArea(stats.areaScale);

            Collider col = sushiObj.GetComponent<Collider>();
            if (col == null) col = sushiObj.AddComponent<BoxCollider>();
            col.isTrigger = true;

            Rigidbody rb = sushiObj.GetComponent<Rigidbody>();
            if (rb == null) rb = sushiObj.AddComponent<Rigidbody>();
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
