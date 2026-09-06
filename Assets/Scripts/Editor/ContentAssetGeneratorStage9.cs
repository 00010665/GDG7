using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using FoodSurvivors.Data;

namespace FoodSurvivors.EditorTools
{
    public static class ContentAssetGeneratorStage9
    {
        [MenuItem("FoodSurvivors/Generate Stage 9 Chefs, Enemies, and Levels")]
        public static void GenerateAllStage9Assets()
        {
            EnsureDirectories();
            var weapons = LoadAllWeapons();
            var passives = LoadAllPassives();

            var chefs = GenerateChefs(weapons, passives);
            var enemies = GenerateEnemies();
            var levels = GenerateLevels(enemies);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[ContentAssetGeneratorStage9] Successfully generated {chefs.Count} Chefs, {enemies.Count} Enemies/Bosses, and {levels.Count} Levels!");
        }

        private static void EnsureDirectories()
        {
            if (!AssetDatabase.IsValidFolder("Assets/ScriptableObjects/Chefs"))
                AssetDatabase.CreateFolder("Assets/ScriptableObjects", "Chefs");

            if (!AssetDatabase.IsValidFolder("Assets/ScriptableObjects/Enemies"))
                AssetDatabase.CreateFolder("Assets/ScriptableObjects", "Enemies");

            if (!AssetDatabase.IsValidFolder("Assets/ScriptableObjects/Levels"))
                AssetDatabase.CreateFolder("Assets/ScriptableObjects", "Levels");
        }

        private static Dictionary<string, WeaponData> LoadAllWeapons()
        {
            var dict = new Dictionary<string, WeaponData>();
            string[] guids = AssetDatabase.FindAssets("t:WeaponData", new[] { "Assets/ScriptableObjects/Weapons" });
            foreach (var g in guids)
            {
                string p = AssetDatabase.GUIDToAssetPath(g);
                var w = AssetDatabase.LoadAssetAtPath<WeaponData>(p);
                if (w != null) dict[w.name] = w;
            }
            return dict;
        }

        private static Dictionary<string, PassiveData> LoadAllPassives()
        {
            var dict = new Dictionary<string, PassiveData>();
            string[] guids = AssetDatabase.FindAssets("t:PassiveData", new[] { "Assets/ScriptableObjects/Passives" });
            foreach (var g in guids)
            {
                string p = AssetDatabase.GUIDToAssetPath(g);
                var pass = AssetDatabase.LoadAssetAtPath<PassiveData>(p);
                if (pass != null) dict[pass.name] = pass;
            }
            return dict;
        }

        private static List<ChefData> GenerateChefs(Dictionary<string, WeaponData> weapons, Dictionary<string, PassiveData> passives)
        {
            var list = new List<ChefData>();

            // 1. Mario
            list.Add(CreateChef("Chef_Mario", "Марио",
                "Итальянский мастер пасты. Обладает крепким здоровьем и надежной защитой.",
                new Color(0.9f, 0.2f, 0.2f),
                150f, 4.8f, 2f,
                weapons.ContainsKey("PastaLauncher") ? weapons["PastaLauncher"] : null,
                passives.ContainsKey("CheeseArmor") ? passives["CheeseArmor"] : null));

            // 2. Kenji
            list.Add(CreateChef("Chef_Kenji", "Кэндзи",
                "Японский виртуоз сашими. Невероятно быстрый и стремительный шеф.",
                new Color(0.95f, 0.95f, 0.98f),
                90f, 6.5f, 0f,
                weapons.ContainsKey("SashimiSlice") ? weapons["SashimiSlice"] : null,
                passives.ContainsKey("WasabiBoost") ? passives["WasabiBoost"] : null));

            // 3. Diego
            list.Add(CreateChef("Chef_Diego", "Диего",
                "Мексиканский кулинар с огненным темпераментом. Наносит сокрушительный урон.",
                new Color(0.2f, 0.8f, 0.25f),
                110f, 5.2f, 1f,
                weapons.ContainsKey("SalsaSplash") ? weapons["SalsaSplash"] : null,
                passives.ContainsKey("SpicyPepper") ? passives["SpicyPepper"] : null));

            return list;
        }

        private static ChefData CreateChef(string fileName, string chefName, string description, Color color, float hp, float speed, float armor, WeaponData weapon, PassiveData passive)
        {
            string path = $"Assets/ScriptableObjects/Chefs/{fileName}.asset";
            ChefData asset = AssetDatabase.LoadAssetAtPath<ChefData>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<ChefData>();
                AssetDatabase.CreateAsset(asset, path);
            }

            asset.chefName = chefName;
            asset.description = description;
            asset.chefColor = color;
            asset.maxHealth = hp;
            asset.moveSpeed = speed;
            asset.armor = armor;
            asset.startingWeapon = weapon;
            asset.startingPassive = passive;

            EditorUtility.SetDirty(asset);
            return asset;
        }

        private static Dictionary<string, EnemyData> GenerateEnemies()
        {
            var dict = new Dictionary<string, EnemyData>();

            // Fairground
            dict["HungryKid"] = CreateEnemy("HungryKid", "Голодный Ребенок", new Color(1f, 0.85f, 0.2f), 15f, 4.5f, 8f, 5, 0f, new Vector3(0.7f, 0.7f, 0.7f), false);
            dict["RegularTourist"] = CreateEnemy("RegularTourist", "Обычный Турист", new Color(1f, 0.55f, 0.1f), 30f, 3.0f, 12f, 10, 0f, Vector3.one, false);
            dict["StrollerFamily"] = CreateEnemy("StrollerFamily", "Семья с коляской", new Color(0.8f, 0.2f, 0.8f), 75f, 2.2f, 16f, 20, 2f, new Vector3(1.4f, 1.2f, 1.4f), false);
            dict["GluttonGiant"] = CreateEnemy("GluttonGiant", "Гигант-Обжора", new Color(0.85f, 0.1f, 0.1f), 400f, 2.0f, 30f, 150, 4f, new Vector3(2.5f, 2.5f, 2.5f), true);

            // FoodCourt
            dict["TeensGroup"] = CreateEnemy("TeensGroup", "Толпа Подростков", new Color(0.2f, 0.8f, 0.9f), 20f, 5.2f, 10f, 8, 0f, new Vector3(0.85f, 0.85f, 0.85f), false);
            dict["Shopaholic"] = CreateEnemy("Shopaholic", "Шопоголик", new Color(0.95f, 0.4f, 0.7f), 45f, 3.2f, 14f, 12, 2f, new Vector3(1.1f, 1.1f, 1.1f), false);
            dict["MallGuard"] = CreateEnemy("MallGuard", "Охранник", new Color(0.2f, 0.35f, 0.85f), 90f, 2.8f, 22f, 25, 3f, new Vector3(1.3f, 1.3f, 1.3f), false);
            dict["BlackFridayLeader"] = CreateEnemy("BlackFridayLeader", "Лидер толпы Black Friday", new Color(0.35f, 0.1f, 0.45f), 550f, 2.6f, 35f, 200, 5f, new Vector3(2.5f, 2.5f, 2.5f), true);

            // Boulevard
            dict["Hipster"] = CreateEnemy("Hipster", "Хипстер", new Color(0.6f, 0.85f, 0.2f), 35f, 4.2f, 15f, 15, 1f, Vector3.one, false);
            dict["FoodBlogger"] = CreateEnemy("FoodBlogger", "Фуд-блогер", new Color(0.95f, 0.7f, 0.1f), 50f, 3.6f, 24f, 20, 2f, new Vector3(1.1f, 1.1f, 1.1f), false);
            dict["VIPGuest"] = CreateEnemy("VIPGuest", "VIP-Гость", new Color(0.5f, 0.15f, 0.65f), 120f, 2.5f, 20f, 35, 6f, new Vector3(1.4f, 1.4f, 1.4f), false);
            dict["MichelinInspector"] = CreateEnemy("MichelinInspector", "Критик Мишлен", new Color(0.92f, 0.92f, 0.96f), 700f, 3.0f, 45f, 300, 7f, new Vector3(2.5f, 2.5f, 2.5f), true);

            return dict;
        }

        private static EnemyData CreateEnemy(string fileName, string enemyName, Color color, float hp, float speed, float damage, int xp, float armor, Vector3 scale, bool isBoss)
        {
            string path = $"Assets/ScriptableObjects/Enemies/{fileName}.asset";
            EnemyData asset = AssetDatabase.LoadAssetAtPath<EnemyData>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<EnemyData>();
                AssetDatabase.CreateAsset(asset, path);
            }

            asset.enemyName = enemyName;
            asset.enemyColor = color;
            asset.maxHealth = hp;
            asset.moveSpeed = speed;
            asset.damage = damage;
            asset.xpValue = xp;
            asset.armor = armor;
            asset.scale = scale;
            asset.isBoss = isBoss;

            EditorUtility.SetDirty(asset);
            return asset;
        }

        private static List<LevelData> GenerateLevels(Dictionary<string, EnemyData> enemies)
        {
            var list = new List<LevelData>();

            // 1. Fairground
            list.Add(CreateLevel("Level_Fairground", "Ярмарка на площади",
                "Шумная городская ярмарка под открытым небом, наполненная голодными туристами и семьями.",
                300f,
                new WaveData[] {
                    new WaveData { enemy = enemies["HungryKid"], startTime = 0f, endTime = 110f, spawnInterval = 1.5f, spawnCountPerTick = 2 },
                    new WaveData { enemy = enemies["RegularTourist"], startTime = 60f, endTime = 220f, spawnInterval = 2.0f, spawnCountPerTick = 3 },
                    new WaveData { enemy = enemies["StrollerFamily"], startTime = 150f, endTime = 300f, spawnInterval = 3.5f, spawnCountPerTick = 2 }
                },
                enemies["GluttonGiant"]));

            // 2. FoodCourt
            list.Add(CreateLevel("Level_FoodCourt", "Фудкорт в молле",
                "Огромный торговый центр в день мега-распродаж! Толпы шопоголиков и суровые охранники.",
                300f,
                new WaveData[] {
                    new WaveData { enemy = enemies["TeensGroup"], startTime = 0f, endTime = 110f, spawnInterval = 1.2f, spawnCountPerTick = 3 },
                    new WaveData { enemy = enemies["Shopaholic"], startTime = 70f, endTime = 230f, spawnInterval = 2.2f, spawnCountPerTick = 3 },
                    new WaveData { enemy = enemies["MallGuard"], startTime = 160f, endTime = 300f, spawnInterval = 3.0f, spawnCountPerTick = 2 }
                },
                enemies["BlackFridayLeader"]));

            // 3. Boulevard
            list.Add(CreateLevel("Level_Boulevard", "Ресторанный Бульвар",
                "Изысканный бульвар высокой кухни, где строгие хипстеры, фуд-блогеры и VIP-гости ждут идеальных блюд.",
                300f,
                new WaveData[] {
                    new WaveData { enemy = enemies["Hipster"], startTime = 0f, endTime = 120f, spawnInterval = 1.4f, spawnCountPerTick = 3 },
                    new WaveData { enemy = enemies["FoodBlogger"], startTime = 80f, endTime = 240f, spawnInterval = 2.0f, spawnCountPerTick = 3 },
                    new WaveData { enemy = enemies["VIPGuest"], startTime = 170f, endTime = 300f, spawnInterval = 3.5f, spawnCountPerTick = 2 }
                },
                enemies["MichelinInspector"]));

            return list;
        }

        private static LevelData CreateLevel(string fileName, string levelName, string description, float duration, WaveData[] waves, EnemyData boss)
        {
            string path = $"Assets/ScriptableObjects/Levels/{fileName}.asset";
            LevelData asset = AssetDatabase.LoadAssetAtPath<LevelData>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<LevelData>();
                AssetDatabase.CreateAsset(asset, path);
            }

            asset.levelName = levelName;
            asset.description = description;
            asset.levelDuration = duration;
            asset.waves.Clear();
            asset.waves.AddRange(waves);
            asset.bossData = boss;

            EditorUtility.SetDirty(asset);
            return asset;
        }
    }
}
