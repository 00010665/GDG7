using System.Collections.Generic;
using UnityEngine;
using FoodSurvivors.Data;
using FoodSurvivors.Enemies;

namespace FoodSurvivors.Core
{
    public class TargetDummy : EnemyController
    {
        [Header("Dummy DPS Stats")]
        public float dps = 0f;
        public string lastDamageSource = "-";
        public float lastDamageValue = 0f;

        private struct DamageEntry
        {
            public float time;
            public float amount;
            public string source;
        }

        private List<DamageEntry> damageHistory = new List<DamageEntry>();

        public void SetupDummy(float maxHp)
        {
            maxHealth = maxHp;
            currentHealth = maxHp;
            moveSpeed = 0f;
            damage = 0f;
            armor = 0f;
            xpValue = 0;
            isBoss = false;
            gameObject.tag = "Enemy";
            gameObject.name = "TargetDummy_Манекен";

            EnemyData dummyData = ScriptableObject.CreateInstance<EnemyData>();
            dummyData.enemyName = "Манекен";
            dummyData.maxHealth = maxHp;
            dummyData.moveSpeed = 0f;
            dummyData.enemyColor = new Color(0.95f, 0.9f, 0.2f);
            dummyData.scale = Vector3.one;

            Initialize(dummyData);
            moveSpeed = 0f; // Гарантированно не двигается
        }

        public void ResetHP()
        {
            currentHealth = maxHealth;
            damageHistory.Clear();
            dps = 0f;
            lastDamageSource = "-";
            lastDamageValue = 0f;
            Debug.Log($"[TargetDummy] ХП Манекена восстановлено: {currentHealth}/{maxHealth}");
        }

        public void TakeDamageCustom(float dmg, string source)
        {
            currentHealth -= dmg;
            if (currentHealth < 0) currentHealth = 0;

            lastDamageSource = source;
            lastDamageValue = dmg;

            damageHistory.Add(new DamageEntry { time = Time.time, amount = dmg, source = source });

            // Рассчитываем DPS
            RecalculateDPS();

            // Лог тиков урона в консоль
            Debug.Log($"🎯 [МАНЕКЕН] Тик урона: {dmg:F1} | Источник: '{source}' | Осталось ХП: {currentHealth:F1}/{maxHealth} | DPS: {dps:F1}");
        }

        private void Update()
        {
            // Враг не двигается
            RecalculateDPS();
        }

        private void RecalculateDPS()
        {
            float now = Time.time;
            damageHistory.RemoveAll(e => now - e.time > 1.0f);

            float totalDmgInWindow = 0f;
            foreach (var entry in damageHistory)
            {
                totalDmgInWindow += entry.amount;
            }
            dps = totalDmgInWindow;
        }
    }
}