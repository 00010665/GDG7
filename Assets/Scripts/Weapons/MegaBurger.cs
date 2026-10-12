using UnityEngine;
using FoodSurvivors.Data;
using FoodSurvivors.Enemies;
using FoodSurvivors.Core;

namespace FoodSurvivors.Weapons
{
    public class MegaBurger : WeaponBase
    {
        protected override void PerformAttack()
        {
            WeaponLevelData stats = GetCurrentLevelData();
            Vector3 targetPos = FindDropTarget(14f * GetArea(stats.areaScale));

            // Падает СТРОГО ОДИН бургер
            SpawnSingleMegaBurger(targetPos, stats);
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

        private void SpawnSingleMegaBurger(Vector3 targetPos, WeaponLevelData stats)
        {
            Vector3 dropSpawnPos = new Vector3(targetPos.x, transform.position.y + 14f, targetPos.z);
            GameObject burgerObj = null;

            // Авто-поиск префаба бургера, если ссылка случайно слетела
            if (weaponData != null && weaponData.weaponPrefab == null)
            {
#if UNITY_EDITOR
                string[] guids = UnityEditor.AssetDatabase.FindAssets("Burger t:GameObject", new[] { "Assets/Prefabs/Weapons", "Assets/PolygonShops/Prefabs/Food" });
                if (guids.Length > 0)
                {
                    weaponData.weaponPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]));
                    UnityEditor.EditorUtility.SetDirty(weaponData);
                }
#endif
            }

            // Создаем настоящую 3D-модель бургера
            if (weaponData != null && weaponData.weaponPrefab != null)
            {
                burgerObj = Instantiate(weaponData.weaponPrefab, dropSpawnPos, Quaternion.identity);
            }
            else
            {
                // Резерв с аппетитным цветом бургера, а не белым!
                burgerObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                burgerObj.transform.position = dropSpawnPos;
                Renderer rend = burgerObj.GetComponent<Renderer>();
                if (rend != null) rend.sharedMaterial = DefaultMaterialsGenerator.CreateSolidColorMaterial("M_Burger", new Color(0.85f, 0.5f, 0.2f));
            }

            // Считываем точный масштаб из префаба
            Vector3 prefabScale = (weaponData != null && weaponData.weaponPrefab != null)
                ? weaponData.weaponPrefab.transform.localScale
                : Vector3.one * 3f;

            burgerObj.transform.localScale = prefabScale * GetArea(stats.areaScale);

            // Радиус взрыва равен размеру самого бургера
            float burgerRadius = Mathf.Max(burgerObj.transform.localScale.x, burgerObj.transform.localScale.z) / 2f;
            float impactRadius = Mathf.Max(2.5f, burgerRadius * 1.3f);

            MegaBurgerDrop drop = burgerObj.AddComponent<MegaBurgerDrop>();
            drop.Setup(targetPos, stats.speed > 0 ? stats.speed * 2.5f : 24f, GetDamage(stats.damage * 2.5f), impactRadius, isBossWeapon);
        }
    }

    public class MegaBurgerDrop : MonoBehaviour
    {
        private Vector3 groundPos;
        private float fallSpeed;
        private float damage;
        private float impactRadius;
        private bool isBoss;
        private bool hasLanded = false;

        public void Setup(Vector3 target, float speed, float dmg, float radius, bool bossSource)
        {
            groundPos = new Vector3(target.x, target.y + 0.1f, target.z);
            fallSpeed = speed;
            damage = dmg;
            impactRadius = radius;
            isBoss = bossSource;
        }

        private void Update()
        {
            if (hasLanded) return;

            transform.position = Vector3.MoveTowards(transform.position, groundPos, fallSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, groundPos) <= 0.3f)
            {
                TriggerImpact();
            }
        }

        private void TriggerImpact()
        {
            hasLanded = true;
            transform.position = groundPos;

            // НИКАКИХ БЕЛЫХ ЦИЛИНДРОВ!
            // Наносим сокрушительный сплеш-урон всем мобам прямо под бургером
            Collider[] hits = Physics.OverlapSphere(groundPos, impactRadius);
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

            // Бургер сочно впечатывается в асфальт и исчезает через долю секунды
            Destroy(gameObject, 0.25f);
        }
    }
}