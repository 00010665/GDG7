using System.Collections.Generic;
using UnityEngine;
using FoodSurvivors.Data;

namespace FoodSurvivors.Weapons
{
    public class SashimiSlice : WeaponBase
    {
        private List<GameObject> activeBlades = new List<GameObject>();
        private float currentAngle = 0f;

        protected override void Attack()
        {
            RefreshBlades();
        }

        public override void LevelUp()
        {
            base.LevelUp();
            RefreshBlades();
        }

        private void Start()
        {
            RefreshBlades();
        }

        protected override void Update()
        {
            base.Update();

            WeaponLevelData stats = GetCurrentLevelData();
            float rotSpeed = (stats.speed > 0 ? stats.speed : 120f);
            currentAngle += rotSpeed * Time.deltaTime;
            if (currentAngle >= 360f) currentAngle -= 360f;

            UpdateBladePositions(stats);
        }

        private void RefreshBlades()
        {
            foreach (var blade in activeBlades)
            {
                if (blade != null) Destroy(blade);
            }
            activeBlades.Clear();

            WeaponLevelData stats = GetCurrentLevelData();
            int count = Mathf.Max(1, stats.projectileCount);

            for (int i = 0; i < count; i++)
            {
                GameObject bladeObj;
                if (weaponData != null && weaponData.weaponPrefab != null)
                {
                    bladeObj = Instantiate(weaponData.weaponPrefab, transform);
                }
                else
                {
                    bladeObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    bladeObj.name = "SashimiBlade";
                    bladeObj.transform.SetParent(transform, false);

                    Renderer rend = bladeObj.GetComponent<Renderer>();
                    if (rend != null && weaponData != null)
                    {
                        rend.material.color = weaponData.weaponColor;
                    }
                }

                bladeObj.transform.localScale = new Vector3(0.3f, 0.1f, 0.8f) * stats.areaScale;

                Collider col = bladeObj.GetComponent<Collider>();
                if (col != null) col.isTrigger = true;

                Rigidbody rb = bladeObj.GetComponent<Rigidbody>();
                if (rb == null)
                {
                    rb = bladeObj.AddComponent<Rigidbody>();
                    rb.isKinematic = true;
                    rb.useGravity = false;
                }

                SashimiBlade bladeScript = bladeObj.AddComponent<SashimiBlade>();
                bladeScript.Setup(this);

                activeBlades.Add(bladeObj);
            }

            UpdateBladePositions(stats);
        }

        private void UpdateBladePositions(WeaponLevelData stats)
        {
            int count = activeBlades.Count;
            if (count == 0) return;

            float radius = 2.5f * stats.areaScale;
            float angleStep = 360f / count;

            for (int i = 0; i < count; i++)
            {
                if (activeBlades[i] == null) continue;

                float angle = currentAngle + i * angleStep;
                float rad = angle * Mathf.Deg2Rad;

                Vector3 localPos = new Vector3(Mathf.Cos(rad) * radius, 0.5f, Mathf.Sin(rad) * radius);
                activeBlades[i].transform.localPosition = localPos;
                activeBlades[i].transform.localRotation = Quaternion.Euler(0, -angle, 0);
            }
        }

        private void OnDestroy()
        {
            foreach (var blade in activeBlades)
            {
                if (blade != null) Destroy(blade);
            }
        }
    }

    public class SashimiBlade : MonoBehaviour
    {
        private SashimiSlice parentWeapon;
        private Dictionary<GameObject, float> hitTimerMap = new Dictionary<GameObject, float>();
        private const float HIT_COOLDOWN = 0.4f;

        public void Setup(SashimiSlice weapon)
        {
            parentWeapon = weapon;
        }

        private void Update()
        {
            List<GameObject> keys = new List<GameObject>(hitTimerMap.Keys);
            foreach (var enemy in keys)
            {
                hitTimerMap[enemy] -= Time.deltaTime;
                if (hitTimerMap[enemy] <= 0f)
                {
                    hitTimerMap.Remove(enemy);
                }
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (other.CompareTag("Enemy") && parentWeapon != null)
            {
                GameObject enemy = other.gameObject;
                if (!hitTimerMap.ContainsKey(enemy))
                {
                    hitTimerMap[enemy] = HIT_COOLDOWN;
                    WeaponLevelData stats = parentWeapon.GetCurrentLevelData();
                    other.SendMessage("TakeDamage", stats.damage, SendMessageOptions.DontRequireReceiver);
                }
            }
        }
    }
}
