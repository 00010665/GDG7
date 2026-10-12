using UnityEngine;
using FoodSurvivors.Data;
using FoodSurvivors.Enemies;

namespace FoodSurvivors.Weapons
{
    public class SalsaSplash : WeaponBase
    {
        protected override void PerformAttack()
        {
            WeaponLevelData stats = GetCurrentLevelData();
            Vector3 spawnPos = new Vector3(transform.position.x, transform.position.y + 0.05f, transform.position.z);

            // Считываем базовый размер из префаба!
            Vector3 prefabScale = (weaponData != null && weaponData.weaponPrefab != null)
                ? weaponData.weaponPrefab.transform.localScale
                : new Vector3(3.5f, 0.08f, 3.5f);

            GameObject puddleObj;
            if (weaponData != null && weaponData.weaponPrefab != null)
            {
                puddleObj = Instantiate(weaponData.weaponPrefab, spawnPos, Quaternion.identity);
            }
            else
            {
                puddleObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                puddleObj.transform.position = spawnPos;
                Renderer rend = puddleObj.GetComponent<Renderer>();
                if (rend != null) rend.material.color = new Color(0.9f, 0.1f, 0.1f, 0.6f);
            }

            // Сохраняем масштаб префаба!
            puddleObj.transform.localScale = prefabScale * GetArea(stats.areaScale);

            // Радиус урона СТРОГО соответствует ширине префаба!
            float maxHorizontalScale = Mathf.Max(prefabScale.x, prefabScale.z);
            float damageRadius = (maxHorizontalScale / 2f) * GetArea(stats.areaScale);

            SalsaPuddleZone zone = puddleObj.AddComponent<SalsaPuddleZone>();
            zone.Setup(GetDamage(stats.damage), stats.duration > 0 ? stats.duration : 3.5f, 0.4f, damageRadius, isBossWeapon);
        }
    }

    public class SalsaPuddleZone : MonoBehaviour
    {
        private float damage;
        private float duration;
        private float tickInterval;
        private float radius;
        private bool isBoss;
        private float tickTimer = 0f;

        public void Setup(float dmg, float dur, float interval, float rad, bool bossSource)
        {
            damage = dmg;
            duration = dur;
            tickInterval = interval;
            radius = rad;
            isBoss = bossSource;
            tickTimer = 0f;
        }

        private void Update()
        {
            duration -= Time.deltaTime;
            tickTimer -= Time.deltaTime;

            if (tickTimer <= 0f)
            {
                tickTimer = tickInterval;
                ApplyDamageInZone();
            }

            if (duration <= 0f)
            {
                Destroy(gameObject);
            }
        }

        private void ApplyDamageInZone()
        {
            // Наносим урон всем мобам строго в радиусе размера префаба!
            Collider[] hits = Physics.OverlapSphere(transform.position, radius);
            foreach (var h in hits)
            {
                if (h.CompareTag("Enemy"))
                {
                    EnemyController ec = h.GetComponent<EnemyController>();
                    if (ec != null)
                    {
                        if (isBoss) ec.TakeDamageFromBoss(damage);
                        else ec.TakeDamage(damage);
                    }
                }
            }
        }
    }
}