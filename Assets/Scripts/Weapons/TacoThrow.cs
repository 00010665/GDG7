using System.Collections.Generic;
using UnityEngine;
using FoodSurvivors.Data;

namespace FoodSurvivors.Weapons
{
    public class TacoThrow : WeaponBase
    {
        protected override void Attack()
        {
            WeaponLevelData stats = GetCurrentLevelData();
            Transform target = FindNearestEnemy(transform.position, null);

            Vector3 direction = transform.forward;
            if (target != null)
            {
                direction = (target.position - transform.position).normalized;
                direction.y = 0f;
            }

            GameObject tacoObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            tacoObj.name = "TacoProjectile";
            tacoObj.transform.position = transform.position + Vector3.up * 0.5f;
            tacoObj.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            tacoObj.transform.localScale = new Vector3(0.5f, 0.1f, 0.5f) * GetArea(stats.areaScale);

            Renderer rend = tacoObj.GetComponent<Renderer>();
            if (rend != null)
            {
                rend.material.color = weaponData != null ? weaponData.weaponColor : new Color(0.9f, 0.7f, 0.1f);
            }

            Collider col = tacoObj.GetComponent<Collider>();
            if (col != null) col.isTrigger = true;

            Rigidbody rb = tacoObj.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;

            TacoBounceProjectile proj = tacoObj.AddComponent<TacoBounceProjectile>();
            int maxBounces = 2 + Mathf.Max(1, stats.projectileCount);
            proj.Setup(direction, stats.speed > 0 ? stats.speed : 14f, GetDamage(stats.damage), maxBounces, target);
        }

        public static Transform FindNearestEnemy(Vector3 fromPos, HashSet<GameObject> exclude)
        {
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            Transform nearest = null;
            float minDist = float.MaxValue;

            foreach (var e in enemies)
            {
                if (e == null || (exclude != null && exclude.Contains(e))) continue;
                float dist = Vector3.Distance(fromPos, e.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    nearest = e.transform;
                }
            }
            return nearest;
        }
    }

    public class TacoBounceProjectile : MonoBehaviour
    {
        private Vector3 moveDirection;
        private float speed;
        private float damage;
        private int remainingBounces;
        private Transform currentTarget;
        private HashSet<GameObject> hitEnemies = new HashSet<GameObject>();
        private float lifetime = 5f;

        public void Setup(Vector3 initialDir, float moveSpeed, float dmg, int bounces, Transform target)
        {
            moveDirection = initialDir.normalized;
            speed = moveSpeed;
            damage = dmg;
            remainingBounces = bounces;
            currentTarget = target;
        }

        private void Update()
        {
            lifetime -= Time.deltaTime;
            if (lifetime <= 0f)
            {
                Destroy(gameObject);
                return;
            }

            if (currentTarget != null)
            {
                Vector3 toTarget = (currentTarget.position - transform.position);
                toTarget.y = 0f;
                if (toTarget.sqrMagnitude > 0.01f)
                {
                    moveDirection = toTarget.normalized;
                }
            }

            transform.position += moveDirection * (speed * Time.deltaTime);
            transform.Rotate(0f, 0f, 360f * Time.deltaTime);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Enemy") && !hitEnemies.Contains(other.gameObject))
            {
                hitEnemies.Add(other.gameObject);
                other.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);

                remainingBounces--;
                if (remainingBounces <= 0)
                {
                    Destroy(gameObject);
                    return;
                }

                Transform nextTarget = TacoThrow.FindNearestEnemy(transform.position, hitEnemies);
                if (nextTarget != null)
                {
                    currentTarget = nextTarget;
                    Vector3 newDir = (currentTarget.position - transform.position);
                    newDir.y = 0f;
                    moveDirection = newDir.normalized;
                }
                else
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}
