using UnityEngine;
using FoodSurvivors.Data;

namespace FoodSurvivors.Weapons
{
    public class MegaBurger : WeaponBase
    {
        protected override void Attack()
        {
            WeaponLevelData stats = GetCurrentLevelData();
            Vector3 targetPos = FindDropTarget(10f * GetArea(stats.areaScale));

            int count = Mathf.Max(1, stats.projectileCount);
            for (int i = 0; i < count; i++)
            {
                Vector3 offset = (i == 0) ? Vector3.zero : new Vector3(Random.Range(-3f, 3f), 0f, Random.Range(-3f, 3f));
                SpawnBurger(targetPos + offset, stats);
            }
        }

        private Vector3 FindDropTarget(float maxRadius)
        {
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            if (enemies != null && enemies.Length > 0)
            {
                int r = Random.Range(0, enemies.Length);
                if (enemies[r] != null) return enemies[r].transform.position;
            }

            Vector2 circle = Random.insideUnitCircle * maxRadius;
            return transform.position + new Vector3(circle.x, 0f, circle.y);
        }

        private void SpawnBurger(Vector3 targetPos, WeaponLevelData stats)
        {
            GameObject burgerObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            burgerObj.name = "MegaBurgerSlam";
            burgerObj.transform.position = new Vector3(targetPos.x, 14f, targetPos.z);

            float burgerScale = 2.5f * GetArea(stats.areaScale);
            burgerObj.transform.localScale = new Vector3(burgerScale, 0.8f, burgerScale);

            Renderer rend = burgerObj.GetComponent<Renderer>();
            if (rend != null)
            {
                rend.material.color = weaponData != null ? weaponData.weaponColor : new Color(0.8f, 0.5f, 0.2f);
            }

            Collider col = burgerObj.GetComponent<Collider>();
            if (col != null) col.isTrigger = true;

            MegaBurgerDrop drop = burgerObj.AddComponent<MegaBurgerDrop>();
            drop.Setup(targetPos, stats.speed > 0 ? stats.speed * 2.5f : 24f, GetDamage(stats.damage * 2.2f), 3.5f * GetArea(stats.areaScale));
        }
    }

    public class MegaBurgerDrop : MonoBehaviour
    {
        private Vector3 groundPos;
        private float fallSpeed;
        private float damage;
        private float impactRadius;

        public void Setup(Vector3 target, float speed, float dmg, float radius)
        {
            groundPos = new Vector3(target.x, 0.4f, target.z);
            fallSpeed = speed;
            damage = dmg;
            impactRadius = radius;
        }

        private void Update()
        {
            transform.position = Vector3.MoveTowards(transform.position, groundPos, fallSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, groundPos) <= 0.2f)
            {
                Impact();
            }
        }

        private void Impact()
        {
            GameObject shockwave = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            shockwave.name = "BurgerShockwave";
            shockwave.transform.position = groundPos;
            shockwave.transform.localScale = new Vector3(impactRadius * 2f, 0.05f, impactRadius * 2f);

            Renderer r = shockwave.GetComponent<Renderer>();
            if (r != null)
            {
                Color c = new Color(1f, 0.6f, 0.1f, 0.7f);
                r.material.color = c;
            }

            Collider cCol = shockwave.GetComponent<Collider>();
            if (cCol != null) cCol.isTrigger = true;

            Collider[] hits = Physics.OverlapSphere(groundPos, impactRadius);
            foreach (var h in hits)
            {
                if (h.CompareTag("Enemy"))
                {
                    h.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
                }
            }

            Destroy(shockwave, 0.5f);
            Destroy(gameObject, 0.4f);
        }
    }
}
