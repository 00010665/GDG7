using UnityEngine;
using FoodSurvivors.Data;

namespace FoodSurvivors.Weapons
{
    public class SoupBowl : WeaponBase
    {
        protected override void Attack()
        {
            WeaponLevelData stats = GetCurrentLevelData();
            int count = Mathf.Max(1, stats.projectileCount);

            for (int i = 0; i < count; i++)
            {
                Vector3 targetPos = GetRandomTargetPosition(8f * GetArea(stats.areaScale));
                SpawnSoupProjectile(targetPos, stats);
            }
        }

        private Vector3 GetRandomTargetPosition(float radius)
        {
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            if (enemies != null && enemies.Length > 0)
            {
                int r = Random.Range(0, enemies.Length);
                if (enemies[r] != null) return enemies[r].transform.position;
            }

            Vector2 circle = Random.insideUnitCircle * radius;
            return transform.position + new Vector3(circle.x, 0f, circle.y);
        }

        private void SpawnSoupProjectile(Vector3 targetPos, WeaponLevelData stats)
        {
            GameObject soupObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            soupObj.name = "SoupProjectile";
            soupObj.transform.position = transform.position + Vector3.up * 1f;
            soupObj.transform.localScale = Vector3.one * (0.6f * GetArea(stats.areaScale));

            Renderer rend = soupObj.GetComponent<Renderer>();
            if (rend != null)
            {
                rend.material.color = weaponData != null ? weaponData.weaponColor : new Color(1f, 0.5f, 0f);
            }

            Collider col = soupObj.GetComponent<Collider>();
            if (col != null) col.isTrigger = true;

            SoupLobProjectile lob = soupObj.AddComponent<SoupLobProjectile>();
            lob.Setup(transform.position, targetPos, stats.speed > 0 ? stats.speed : 10f, GetDamage(stats.damage), 2.5f * GetArea(stats.areaScale), weaponData != null ? weaponData.weaponColor : Color.yellow);
        }
    }

    public class SoupLobProjectile : MonoBehaviour
    {
        private Vector3 startPos;
        private Vector3 endPos;
        private float speed;
        private float damage;
        private float aoeRadius;
        private Color splashColor;

        private float progress = 0f;
        private float totalDistance;
        private float duration;

        public void Setup(Vector3 start, Vector3 end, float moveSpeed, float dmg, float radius, Color color)
        {
            startPos = start;
            endPos = end;
            endPos.y = 0.1f;
            speed = moveSpeed;
            damage = dmg;
            aoeRadius = radius;
            splashColor = color;

            totalDistance = Vector3.Distance(startPos, endPos);
            duration = Mathf.Max(0.4f, totalDistance / speed);
        }

        private void Update()
        {
            progress += Time.deltaTime / duration;

            if (progress >= 1f)
            {
                Explode();
                return;
            }

            Vector3 currentGround = Vector3.Lerp(startPos, endPos, progress);
            float height = Mathf.Sin(progress * Mathf.PI) * 4f;
            transform.position = new Vector3(currentGround.x, currentGround.y + height, currentGround.z);
        }

        private void Explode()
        {
            GameObject splash = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            splash.name = "SoupSplash";
            splash.transform.position = endPos;
            splash.transform.localScale = new Vector3(aoeRadius * 2f, 0.05f, aoeRadius * 2f);

            Renderer r = splash.GetComponent<Renderer>();
            if (r != null)
            {
                Color c = splashColor;
                c.a = 0.6f;
                r.material.color = c;
            }

            Collider cCol = splash.GetComponent<Collider>();
            if (cCol != null) cCol.isTrigger = true;

            Collider[] hits = Physics.OverlapSphere(endPos, aoeRadius);
            foreach (var h in hits)
            {
                if (h.CompareTag("Enemy"))
                {
                    h.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
                }
            }

            Destroy(splash, 0.6f);
            Destroy(gameObject);
        }
    }
}
