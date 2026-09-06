using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using FoodSurvivors.Data;

namespace FoodSurvivors.EditorTools
{
    public static class ContentAssetGenerator
    {
        [MenuItem("FoodSurvivors/Generate All Weapons and Passives Assets")]
        public static void GenerateAllContentAssets()
        {
            EnsureDirectories();
            GenerateWeapons();
            GeneratePassives();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[ContentAssetGenerator] All 9 Weapons and 9 Passives ScriptableObject assets generated successfully!");
        }

        private static void EnsureDirectories()
        {
            if (!AssetDatabase.IsValidFolder("Assets/ScriptableObjects/Weapons"))
            {
                AssetDatabase.CreateFolder("Assets/ScriptableObjects", "Weapons");
            }
            if (!AssetDatabase.IsValidFolder("Assets/ScriptableObjects/Passives"))
            {
                AssetDatabase.CreateFolder("Assets/ScriptableObjects", "Passives");
            }
        }

        private static void GenerateWeapons()
        {
            // 1. PastaLauncher
            CreateWeapon("PastaLauncher", "Паста-пушка", "Запускает пасту в ближайших туристов", Color.yellow,
                new (float dmg, float cd, float spd, float area, int count, float dur)[] {
                    (12f, 1.2f, 14f, 1.0f, 1, 3f),
                    (16f, 1.1f, 15f, 1.1f, 2, 3f),
                    (20f, 1.0f, 16f, 1.2f, 2, 3.5f),
                    (24f, 0.9f, 17f, 1.3f, 3, 3.5f),
                    (30f, 0.75f, 18f, 1.5f, 4, 4f)
                });

            // 2. SashimiSlice
            CreateWeapon("SashimiSlice", "Сашими-клинки", "Вращающиеся клинки вокруг повара", Color.cyan,
                new (float dmg, float cd, float spd, float area, int count, float dur)[] {
                    (8f, 1.0f, 120f, 1.0f, 2, 0f),
                    (12f, 1.0f, 150f, 1.1f, 3, 0f),
                    (16f, 1.0f, 180f, 1.2f, 4, 0f),
                    (20f, 1.0f, 200f, 1.3f, 5, 0f),
                    (26f, 1.0f, 240f, 1.5f, 6, 0f)
                });

            // 3. SalsaSplash
            CreateWeapon("SalsaSplash", "Острое Сальса-пятно", "AOE пятно острой сальсы под ногами", Color.red,
                new (float dmg, float cd, float spd, float area, int count, float dur)[] {
                    (10f, 3.5f, 0f, 1.0f, 1, 3.0f),
                    (14f, 3.2f, 0f, 1.2f, 1, 3.5f),
                    (18f, 2.8f, 0f, 1.4f, 1, 4.0f),
                    (22f, 2.5f, 0f, 1.6f, 1, 4.5f),
                    (28f, 2.0f, 0f, 2.0f, 2, 5.5f)
                });

            // 4. SoupBowl
            CreateWeapon("SoupBowl", "Горячий Суп", "Взрывной навесной снаряд с уроном по площади", new Color(1f, 0.5f, 0.1f),
                new (float dmg, float cd, float spd, float area, int count, float dur)[] {
                    (25f, 2.8f, 10f, 1.0f, 1, 1f),
                    (35f, 2.5f, 11f, 1.2f, 1, 1f),
                    (45f, 2.2f, 12f, 1.3f, 2, 1f),
                    (55f, 2.0f, 13f, 1.5f, 2, 1f),
                    (70f, 1.6f, 15f, 1.8f, 3, 1f)
                });

            // 5. SushiSet
            CreateWeapon("SushiSet", "Суши-сет", "Атака во все 4 стороны крестом", new Color(0.1f, 0.8f, 0.4f),
                new (float dmg, float cd, float spd, float area, int count, float dur)[] {
                    (14f, 2.0f, 12f, 1.0f, 1, 3f),
                    (18f, 1.8f, 13f, 1.1f, 1, 3f),
                    (22f, 1.6f, 14f, 1.2f, 2, 3f),
                    (28f, 1.4f, 15f, 1.3f, 2, 3f),
                    (35f, 1.1f, 17f, 1.5f, 2, 3.5f)
                });

            // 6. TacoThrow
            CreateWeapon("TacoThrow", "Тако-бросок", "Бросок тако с рикошетом по толпе", new Color(0.9f, 0.7f, 0.2f),
                new (float dmg, float cd, float spd, float area, int count, float dur)[] {
                    (15f, 2.2f, 14f, 1.0f, 1, 5f),
                    (20f, 2.0f, 15f, 1.1f, 2, 5f),
                    (25f, 1.8f, 16f, 1.2f, 2, 5f),
                    (30f, 1.6f, 17f, 1.3f, 3, 5f),
                    (40f, 1.3f, 19f, 1.5f, 4, 5f)
                });

            // 7. PizzaCutter
            CreateWeapon("PizzaCutter", "Нож для Пиццы", "Пробивающий всех врагов катящийся диск", new Color(0.85f, 0.85f, 0.95f),
                new (float dmg, float cd, float spd, float area, int count, float dur)[] {
                    (16f, 2.5f, 10f, 1.0f, 1, 4f),
                    (22f, 2.2f, 11f, 1.1f, 1, 4.5f),
                    (28f, 2.0f, 12f, 1.2f, 2, 5f),
                    (34f, 1.7f, 13f, 1.4f, 2, 5.5f),
                    (44f, 1.4f, 15f, 1.6f, 3, 6f)
                });

            // 8. TeaPot
            CreateWeapon("TeaPot", "Чайник с Кипятком", "Струя кипящей воды перед поваром", new Color(0.3f, 0.7f, 1f),
                new (float dmg, float cd, float spd, float area, int count, float dur)[] {
                    (10f, 3.0f, 0f, 1.0f, 1, 1.8f),
                    (14f, 2.7f, 0f, 1.1f, 1, 2.1f),
                    (18f, 2.4f, 0f, 1.3f, 1, 2.4f),
                    (24f, 2.1f, 0f, 1.4f, 1, 2.8f),
                    (32f, 1.7f, 0f, 1.7f, 1, 3.5f)
                });

            // 9. MegaBurger
            CreateWeapon("MegaBurger", "Мега-Бургер", "Падение колоссального бургера сверху", new Color(0.8f, 0.45f, 0.15f),
                new (float dmg, float cd, float spd, float area, int count, float dur)[] {
                    (45f, 4.5f, 22f, 1.0f, 1, 1f),
                    (60f, 4.0f, 24f, 1.2f, 1, 1f),
                    (80f, 3.5f, 26f, 1.4f, 2, 1f),
                    (100f, 3.0f, 28f, 1.7f, 2, 1f),
                    (130f, 2.4f, 32f, 2.1f, 3, 1f)
                });
        }

        private static void CreateWeapon(string fileName, string weaponName, string description, Color color, (float dmg, float cd, float spd, float area, int count, float dur)[] levelStats)
        {
            string path = $"Assets/ScriptableObjects/Weapons/{fileName}.asset";
            WeaponData asset = AssetDatabase.LoadAssetAtPath<WeaponData>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<WeaponData>();
                AssetDatabase.CreateAsset(asset, path);
            }

            asset.weaponName = weaponName;
            asset.description = description;
            asset.weaponColor = color;
            asset.levels.Clear();

            for (int i = 0; i < levelStats.Length; i++)
            {
                var s = levelStats[i];
                asset.levels.Add(new WeaponLevelData
                {
                    description = $"Уровень {i + 1}: Урон {s.dmg}, Перезарядка {s.cd:F1}с, Количество {s.count}",
                    damage = s.dmg,
                    cooldown = s.cd,
                    speed = s.spd,
                    areaScale = s.area,
                    projectileCount = s.count,
                    duration = s.dur
                });
            }

            EditorUtility.SetDirty(asset);
        }

        private static void GeneratePassives()
        {
            CreatePassive("CheeseArmor", "Сырная броня", "Уменьшает получаемый урон (+Броня)", Color.yellow, PassiveType.ArmorBonus, new float[] { 2f, 4f, 6f, 8f, 10f });
            CreatePassive("WasabiBoost", "Васаби-ускорение", "Острая приправа ускоряет передвижение (+Скорость)", new Color(0.4f, 0.9f, 0.3f), PassiveType.SpeedBonus, new float[] { 1.2f, 2.4f, 3.6f, 4.8f, 6.0f });
            CreatePassive("SpicyPepper", "Острый перчик", "Увеличивает область поражения и силу атак (+Радиус/Урон)", Color.red, PassiveType.AreaBonus, new float[] { 0.15f, 0.30f, 0.45f, 0.60f, 0.75f });
            CreatePassive("FastService", "Быстрая Подача", "Шеф быстрее отдает блюда (-Перезарядка)", new Color(0.9f, 0.5f, 0.9f), PassiveType.CooldownReduction, new float[] { 0.08f, 0.16f, 0.24f, 0.32f, 0.40f });
            CreatePassive("BigPortion", "Большая Порция", "Сытные порции наносят больше урона (+Базовый урон)", new Color(1f, 0.4f, 0.2f), PassiveType.DamageBonus, new float[] { 0.15f, 0.30f, 0.45f, 0.60f, 0.75f });
            CreatePassive("TipJar", "Чаевые", "Довольные гости оставляют больше чаевых (+Опыт)", Color.yellow, PassiveType.XpBonus, new float[] { 0.20f, 0.40f, 0.60f, 0.80f, 1.00f });
            CreatePassive("ComfortShoes", "Удобная Обувь", "Ортопедическая обувь для кухни (+Скорость бега)", Color.green, PassiveType.SpeedBonus, new float[] { 1.0f, 2.0f, 3.0f, 4.0f, 5.0f });
            CreatePassive("FreshIngredients", "Свежие Ингредиенты", "Органические продукты дают запас сил (+Макс. HP / Реген)", Color.magenta, PassiveType.HealthBonus, new float[] { 25f, 50f, 75f, 100f, 125f });
            CreatePassive("MagnetApron", "Магнитный Фартук", "Фартук с магнитами притягивает чаевые издалека (+Радиус сбора)", Color.cyan, PassiveType.MagnetBonus, new float[] { 1.5f, 3.0f, 4.5f, 6.0f, 7.5f });
        }

        private static void CreatePassive(string fileName, string passiveName, string description, Color color, PassiveType type, float[] values)
        {
            string path = $"Assets/ScriptableObjects/Passives/{fileName}.asset";
            PassiveData asset = AssetDatabase.LoadAssetAtPath<PassiveData>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<PassiveData>();
                AssetDatabase.CreateAsset(asset, path);
            }

            asset.passiveName = passiveName;
            asset.description = description;
            asset.passiveColor = color;
            asset.passiveType = type;
            asset.levels.Clear();

            for (int i = 0; i < values.Length; i++)
            {
                asset.levels.Add(new PassiveLevelData
                {
                    description = $"Уровень {i + 1}: +{values[i]} к бонусу",
                    value = values[i]
                });
            }

            EditorUtility.SetDirty(asset);
        }
    }
}
