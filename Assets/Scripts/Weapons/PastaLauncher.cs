using UnityEngine;
using FoodSurvivors.Data;

namespace FoodSurvivors.Weapons
{
    public class PastaLauncher : WeaponBase
    {
        protected override void Attack()
        {
            WeaponLevelData stats = GetCurrentLevelData();
            Transform nearestEnemy = FindNearestEnemy();

            Vector3 baseDirection = transform.forward;
            if (nearestEnemy != null)
            {
                baseDirection = (nearestEnemy.position - transform.position).normalized;
                baseDirection.y = 0;
            }

            if (baseDirection.sqrMagnitude < 0.01f)
            {
                baseDirection = Vector3.forward;
            }

            int count = Mathf.Max(1, stats.projectileCount);
            float spreadAngle = count > 1 ? 15f : 0f;
            float startAngle = -spreadAngle * (count - 1) / 2f;

            for (int i = 0; i < count; i++)
            {
                float angle = startAngle + i * spreadAngle;
                Quaternion rotation = Quaternion.Euler(0, angle, 0);
                Vector3 shootDir = rotation * baseDirection;

                SpawnProjectile(shootDir, stats);
            }
        }

        private Transform FindNearestEnemy()
        {
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            Transform nearest = null;
            float minDistance = float.MaxValue;

            foreach (GameObject enemyObj in enemies)
            {
                if (enemyObj == null) continue;
                float dist = Vector3.Distance(transform.position, enemyObj.transform.position);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    nearest = enemyObj.transform;
                }
            }
            return nearest;
        }

        private void SpawnProjectile(Vector3 direction, WeaponLevelData stats)
        {
            GameObject projObj;
            if (weaponData != null && weaponData.weaponPrefab != null)
            {
                projObj = Instantiate(weaponData.weaponPrefab, transform.position + Vector3.up * 0.5f, Quaternion.LookRotation(direction));
            }
            else
            {
                projObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                projObj.name = "PastaProjectile";
                projObj.transform.position = transform.position + Vector3.up * 0.5f;
                projObj.transform.rotation = Quaternion.LookRotation(direction);
                projObj.transform.localScale = Vector3.one * (0.4f * stats.areaScale);

                Renderer rend = projObj.GetComponent<Renderer>();
                if (rend != null && weaponData != null)
                {
                    rend.material.color = weaponData.weaponColor;
                }
            }

            Collider col = projObj.GetComponent<Collider>();
            if (col != null) col.isTrigger = true;

            Rigidbody rb = projObj.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = projObj.AddComponent<Rigidbody>();
                rb.isKinematic = true;
                rb.useGravity = false;
            }

            PastaProjectile proj = projObj.AddComponent<PastaProjectile>();
            proj.Setup(direction, stats.speed, stats.damage, stats.duration);
        }
    }

    public class PastaProjectile : MonoBehaviour
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
            lifetime = duration > 0 ? duration : 4f;
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
