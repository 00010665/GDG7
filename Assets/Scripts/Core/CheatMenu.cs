using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using FoodSurvivors.Data;
using FoodSurvivors.Enemies;
using FoodSurvivors.Player;
using FoodSurvivors.Weapons;

namespace FoodSurvivors.Core
{
    public class CheatMenu : MonoBehaviour
    {
        public static CheatMenu Instance { get; private set; }

        [Header("Menu Settings")]
        public bool showMenu = false;
        public bool showEnemyLabels = false;

        [Header("Dummy Settings")]
        public float dummyMaxHP = 1000f;
        private TargetDummy activeDummy;

        [Header("Navigation Tabs")]
        private int selectedTab = 0; // 0 = Общее, 1 = Манекен и Враги, 2 = Экипировка, 3 = БАЛАНС
        private int rebalanceCategory = 0; // 0 = Повара, 1 = Враги, 2 = Блюда, 3 = Пассивки

        // Отдельные индексы выбора в Балансе
        private int chefIndex = 0;
        private int enemyIndex = 0;
        private int weaponIndex = 0;
        private int passiveIndex = 0;

        private Rect windowRect = new Rect(15, 15, 500, 750);
        private Vector2 scrollPos = Vector2.zero;

        // Кэш каталога
        private List<ChefData> allChefs = new List<ChefData>();
        private List<WeaponData> allWeapons = new List<WeaponData>();
        private List<PassiveData> allPassives = new List<PassiveData>();
        private List<EnemyData> allEnemies = new List<EnemyData>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            LoadDataCatalog();
        }

        public void LoadDataCatalog()
        {
            allChefs.Clear();
            allWeapons.Clear();
            allPassives.Clear();
            allEnemies.Clear();

#if UNITY_EDITOR
            string[] cGuids = UnityEditor.AssetDatabase.FindAssets("t:ChefData");
            foreach (var g in cGuids)
            {
                var c = UnityEditor.AssetDatabase.LoadAssetAtPath<ChefData>(UnityEditor.AssetDatabase.GUIDToAssetPath(g));
                if (c != null && !allChefs.Contains(c)) allChefs.Add(c);
            }

            string[] wGuids = UnityEditor.AssetDatabase.FindAssets("t:WeaponData");
            foreach (var g in wGuids)
            {
                var w = UnityEditor.AssetDatabase.LoadAssetAtPath<WeaponData>(UnityEditor.AssetDatabase.GUIDToAssetPath(g));
                if (w != null && !allWeapons.Contains(w)) allWeapons.Add(w);
            }

            string[] pGuids = UnityEditor.AssetDatabase.FindAssets("t:PassiveData");
            foreach (var g in pGuids)
            {
                var p = UnityEditor.AssetDatabase.LoadAssetAtPath<PassiveData>(UnityEditor.AssetDatabase.GUIDToAssetPath(g));
                if (p != null && !allPassives.Contains(p)) allPassives.Add(p);
            }

            string[] eGuids = UnityEditor.AssetDatabase.FindAssets("t:EnemyData");
            foreach (var g in eGuids)
            {
                var e = UnityEditor.AssetDatabase.LoadAssetAtPath<EnemyData>(UnityEditor.AssetDatabase.GUIDToAssetPath(g));
                if (e != null && !allEnemies.Contains(e)) allEnemies.Add(e);
            }
#endif

            // Резервная подгрузка из GameManager
            if (GameManager.Instance != null)
            {
                if (GameManager.Instance.availableChefs != null)
                {
                    foreach (var c in GameManager.Instance.availableChefs)
                        if (c != null && !allChefs.Contains(c)) allChefs.Add(c);
                }
                if (GameManager.Instance.availableLevels != null)
                {
                    foreach (var l in GameManager.Instance.availableLevels)
                    {
                        if (l == null) continue;
                        if (l.bossData != null && !allEnemies.Contains(l.bossData)) allEnemies.Add(l.bossData);
                        if (l.waves != null)
                        {
                            foreach (var w in l.waves)
                                if (w.enemy != null && !allEnemies.Contains(w.enemy)) allEnemies.Add(w.enemy);
                        }
                    }
                }
            }
        }

        private void Update()
        {
            if (Keyboard.current != null)
            {
                if (Keyboard.current.numpad0Key.wasPressedThisFrame ||
                    Keyboard.current.f1Key.wasPressedThisFrame ||
                    Keyboard.current.backquoteKey.wasPressedThisFrame)
                {
                    ToggleMenu();
                }
            }
        }

        public void ToggleMenu()
        {
            showMenu = !showMenu;
            if (showMenu) LoadDataCatalog();
            Time.timeScale = showMenu ? 0f : 1f;
        }

        private void OnGUI()
        {
            if (showEnemyLabels) DrawWorldLabels();

            if (!showMenu) return;

            GUI.backgroundColor = new Color(0.1f, 0.1f, 0.15f, 0.95f);
            windowRect = GUI.Window(999, windowRect, DrawCheatWindow, "🛠️ ЧИТ-МЕНЮ ТЕСТИРОВАНИЯ И БАЛАНСА");
        }

        private void DrawCheatWindow(int windowID)
        {
            GUI.color = Color.white;

            GUILayout.BeginHorizontal();
            if (GUILayout.Toggle(selectedTab == 0, "⏱️ Общее", "Button", GUILayout.Height(30))) selectedTab = 0;
            if (GUILayout.Toggle(selectedTab == 1, "🎯 Враги / Волна", "Button", GUILayout.Height(30))) selectedTab = 1;
            if (GUILayout.Toggle(selectedTab == 2, "🍲 Экипировка", "Button", GUILayout.Height(30))) selectedTab = 2;
            if (GUILayout.Toggle(selectedTab == 3, "⚖️ БАЛАНС", "Button", GUILayout.Height(30))) selectedTab = 3;
            GUILayout.EndHorizontal();

            GUILayout.Space(10);
            scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.Width(480), GUILayout.Height(650));

            switch (selectedTab)
            {
                case 0: DrawGeneralTab(); break;
                case 1: DrawEnemiesAndWavesTab(); break;
                case 2: DrawEquipmentTab(); break;
                case 3: DrawBalanceEditorTab(); break;
            }

            GUILayout.EndScrollView();
            GUI.DragWindow();
        }

        // --- 0. ОБЩЕЕ ---
        private void DrawGeneralTab()
        {
            GUILayout.Label("<b>⏱️ ВРЕМЯ И СОСТОЯНИЕ ИГРЫ</b>", GetHeaderStyle());
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("▶️ Продолжить", GUILayout.Height(30))) { ToggleMenu(); }
            if (GUILayout.Button("0.5x")) Time.timeScale = 0.5f;
            if (GUILayout.Button("1.0x")) Time.timeScale = 1.0f;
            if (GUILayout.Button("2.0x")) Time.timeScale = 2.0f;
            if (GUILayout.Button("5.0x")) Time.timeScale = 5.0f;
            GUILayout.EndHorizontal();

            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            PlayerController player = playerObj != null ? playerObj.GetComponent<PlayerController>() : null;

            if (player != null)
            {
                GUILayout.Space(10);
                GUILayout.Label($"<b>Повар:</b> {player.chefData?.chefName ?? "Герой"} | <b>ХП:</b> {Mathf.CeilToInt(player.currentHealth)}/{Mathf.CeilToInt(player.maxHealth)} | <b>Скорость:</b> {player.currentMoveSpeed:F1} | <b>Броня:</b> {player.currentArmor}");

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("❤️ Исцелить")) player.Heal(player.maxHealth);
                if (GUILayout.Button("⚡ +2.0 Скорость")) player.currentMoveSpeed += 2f;
                if (GUILayout.Button("🛡️ +5 Броня")) player.currentArmor += 5f;
                if (GUILayout.Button("⚔️ +50% Урон")) player.damageMultiplier += 0.5f;
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("⭐ +100 XP")) { if (ExperienceManager.Instance != null) ExperienceManager.Instance.AddXP(100); }
                if (GUILayout.Button("🌟 +1000 XP (Level-Up)")) { if (ExperienceManager.Instance != null) ExperienceManager.Instance.AddXP(1000); }
                GUILayout.EndHorizontal();
            }
        }

        // --- 1. МАНЕКЕН, ВРАГИ И ВОЛНЫ ---
        private void DrawEnemiesAndWavesTab()
        {
            WaveManager wm = Object.FindFirstObjectByType<WaveManager>();

            // --- Манекен ---
            GUILayout.Label("<b>🎯 НАСТРОЙКА И СПАВН МАНЕКЕНА</b>", GetHeaderStyle());
            GUILayout.BeginHorizontal();
            GUILayout.Label("Макс. ХП Манекена:", GUILayout.Width(150));
            float.TryParse(GUILayout.TextField(dummyMaxHP.ToString(), GUILayout.Width(100)), out dummyMaxHP);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("🎯 Заспавнить Манекен")) SpawnTargetDummy();
            if (GUILayout.Button("🔄 Сбросить ХП")) { if (activeDummy != null) activeDummy.ResetHP(); }
            if (GUILayout.Button("❌ Убрать Манекен")) { if (activeDummy != null) Destroy(activeDummy.gameObject); }
            GUILayout.EndHorizontal();

            if (activeDummy != null)
            {
                GUILayout.Box($"<b>СТАТИСТИКА МАНЕКЕНА:</b>\n" +
                              $"ХП: {Mathf.CeilToInt(activeDummy.currentHealth)} / {activeDummy.maxHealth}\n" +
                              $"DPS (Урон/сек): <b><color=yellow>{activeDummy.dps:F1}</color></b>\n" +
                              $"Последний удар: {activeDummy.lastDamageValue:F1} ({activeDummy.lastDamageSource})");
            }

            GUILayout.Space(15);

            // --- Управление спавном и волнами ---
            GUILayout.Label("<b>👾 УПРАВЛЕНИЕ СПАВНОМ И ВОЛНАМИ</b>", GetHeaderStyle());

            GUILayout.BeginHorizontal();
            showEnemyLabels = GUILayout.Toggle(showEnemyLabels, " 👁️ Имена и ХП над врагами");
            if (wm != null)
            {
                string spawnState = wm.enabled ? "⏸️ Остановить Спавн" : "▶️ Запустить Спавн";
                if (GUILayout.Button(spawnState)) { wm.enabled = !wm.enabled; }
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("🧹 Убрать всех врагов"))
            {
                EnemyController[] enemies = Object.FindObjectsByType<EnemyController>(FindObjectsSortMode.None);
                foreach (var e in enemies) Destroy(e.gameObject);
            }
            if (GUILayout.Button("💣 УБИТЬ ВСЕХ ВРАГОВ"))
            {
                EnemyController[] enemies = Object.FindObjectsByType<EnemyController>(FindObjectsSortMode.None);
                foreach (var e in enemies) e.TakeDamage(99999f);
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("👹 Заспавнить Босса"))
            {
                if (wm != null && wm.currentLevelData != null && wm.currentLevelData.bossData != null)
                    wm.SpawnEnemy(wm.currentLevelData.bossData, true);
            }
            if (GUILayout.Button("☠️ УБИТЬ БОССА (ПОБЕДА)"))
            {
                EnemyController[] enemies = Object.FindObjectsByType<EnemyController>(FindObjectsSortMode.None);
                foreach (var e in enemies) { if (e.isBoss) e.Die(); }
            }
            GUILayout.EndHorizontal();

            // Спавн волн
            if (wm != null && wm.currentLevelData != null && wm.currentLevelData.waves != null)
            {
                GUILayout.Space(10);
                GUILayout.Label("<b>Заспавнить волну уровня:</b>");
                GUILayout.BeginHorizontal();
                for (int i = 0; i < wm.currentLevelData.waves.Count; i++)
                {
                    var wave = wm.currentLevelData.waves[i];
                    if (GUILayout.Button($"Волна {i + 1} ({wave.enemy?.enemyName ?? "Враг"})"))
                    {
                        for (int c = 0; c < 5; c++) wm.SpawnEnemy(wave.enemy, false);
                    }
                }
                GUILayout.EndHorizontal();
            }

            // Спавн конкретного врага
            GUILayout.Space(10);
            GUILayout.Label("<b>Заспавнить конкретного врага:</b>");
            if (allEnemies.Count > 0)
            {
                for (int i = 0; i < allEnemies.Count; i++)
                {
                    EnemyData e = allEnemies[i];
                    if (e == null) continue;

                    if (GUILayout.Button($"Заспавнить: {e.enemyName} {(e.isBoss ? "[БОСС]" : "")}"))
                    {
                        if (wm != null) wm.SpawnEnemy(e, e.isBoss);
                    }
                }
            }
        }

        private void SpawnTargetDummy()
        {
            if (activeDummy != null) Destroy(activeDummy.gameObject);

            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            Vector3 spawnPos = playerObj != null ? playerObj.transform.position + playerObj.transform.forward * 3f : Vector3.zero;

            GameObject dummyObj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            dummyObj.transform.position = spawnPos;

            Renderer r = dummyObj.GetComponent<Renderer>();
            if (r != null) r.material.color = new Color(0.95f, 0.9f, 0.2f);

            activeDummy = dummyObj.AddComponent<TargetDummy>();
            activeDummy.SetupDummy(dummyMaxHP);
        }

        // --- 2. ЭКИПИРОВКА С ОПИСАНИЯМИ ---
        private void DrawEquipmentTab()
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            WeaponManager wm = playerObj != null ? playerObj.GetComponent<WeaponManager>() : null;
            PassiveManager pm = playerObj != null ? playerObj.GetComponent<PassiveManager>() : null;

            GUILayout.Label("<b>🍲 БЛЮДА (Управление)</b>", GetHeaderStyle());
            if (wm != null && allWeapons.Count > 0)
            {
                foreach (var w in allWeapons)
                {
                    if (w == null) continue;
                    int lvl = wm.GetWeaponLevel(w);

                    GUILayout.BeginVertical("box");
                    GUILayout.BeginHorizontal();
                    GUILayout.Label($"<b>{w.weaponName}</b> (Ур. {lvl})", GUILayout.Width(200));

                    if (GUILayout.Button(" +1 Ур ")) { wm.AddWeapon(w); }

                    if (lvl > 0 && GUILayout.Button(" -1 Ур "))
                    {
                        WeaponBase wb = wm.activeWeapons.Find(x => x.weaponData == w || x.weaponData.weaponName == w.weaponName);
                        if (wb != null)
                        {
                            if (wb.currentLevel > 1) wb.currentLevel--;
                            else { wm.activeWeapons.Remove(wb); Destroy(wb); }
                        }
                    }

                    if (lvl > 0 && GUILayout.Button("❌ Убрать"))
                    {
                        WeaponBase wb = wm.activeWeapons.Find(x => x.weaponData == w || x.weaponData.weaponName == w.weaponName);
                        if (wb != null) { wm.activeWeapons.Remove(wb); Destroy(wb); }
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.Label($"<color=silver><i>{w.description}</i></color>");
                    GUILayout.EndVertical();
                }
            }

            GUILayout.Space(15);
            GUILayout.Label("<b>🧂 ПАССИВКИ (Управление и Тултипы)</b>", GetHeaderStyle());
            if (pm != null && allPassives.Count > 0)
            {
                foreach (var p in allPassives)
                {
                    if (p == null) continue;
                    int lvl = pm.GetPassiveLevel(p);

                    GUILayout.BeginVertical("box");
                    GUILayout.BeginHorizontal();
                    GUILayout.Label($"<b>{p.passiveName}</b> (Ур. {lvl})", GUILayout.Width(200));

                    if (GUILayout.Button(" +1 Ур ")) { pm.AddOrUpgradePassive(p); }

                    if (lvl > 0 && GUILayout.Button("❌ Убрать"))
                    {
                        pm.activePassives.Remove(p);
                        pm.passiveLevels.Remove(p);
                    }
                    GUILayout.EndHorizontal();

                    // Полный тултип / описание эффекта
                    GUILayout.Label($"<b>Эффект:</b> <color=cyan>[{p.passiveType}]</color> {p.description}");
                    GUILayout.EndVertical();
                }
            }
        }

        // --- 3. РЕДАКТОР БАЛАНСА ---
        private void DrawBalanceEditorTab()
        {
            if (allChefs.Count == 0 && allEnemies.Count == 0 && allWeapons.Count == 0) LoadDataCatalog();

            GUILayout.Label("<b>⚖️ БЫСТРЫЙ БАЛАНС И СОХРАНЕНИЕ</b>", GetHeaderStyle());

            GUILayout.BeginHorizontal();
            if (GUILayout.Toggle(rebalanceCategory == 0, $"Повара ({allChefs.Count})", "Button", GUILayout.Height(30))) rebalanceCategory = 0;
            if (GUILayout.Toggle(rebalanceCategory == 1, $"Враги ({allEnemies.Count})", "Button", GUILayout.Height(30))) rebalanceCategory = 1;
            if (GUILayout.Toggle(rebalanceCategory == 2, $"Блюда ({allWeapons.Count})", "Button", GUILayout.Height(30))) rebalanceCategory = 2;
            if (GUILayout.Toggle(rebalanceCategory == 3, $"Пассивки ({allPassives.Count})", "Button", GUILayout.Height(30))) rebalanceCategory = 3;
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            switch (rebalanceCategory)
            {
                case 0: DrawChefBalance(); break;
                case 1: DrawEnemyBalance(); break;
                case 2: DrawWeaponBalance(); break;
                case 3: DrawPassiveBalance(); break;
            }

            GUILayout.Space(15);
            if (GUILayout.Button("💾 СОХРАНИТЬ БАЛАНС НА ДИСК", GUILayout.Height(40)))
            {
                SaveBalanceToAssets();
            }
        }

        private void DrawChefBalance()
        {
            if (allChefs.Count == 0) { GUILayout.Label("Нет загруженных Поваров."); return; }
            chefIndex = Mathf.Clamp(chefIndex, 0, allChefs.Count - 1);
            ChefData chef = allChefs[chefIndex];

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("◄ Пред.", GUILayout.Width(80), GUILayout.Height(30)))
                chefIndex = (chefIndex - 1 + allChefs.Count) % allChefs.Count;

            GUILayout.Label($"<b>{chef.chefName}</b> ({chefIndex + 1}/{allChefs.Count})", GetHeaderStyle(), GUILayout.Width(220));

            if (GUILayout.Button("След. ►", GUILayout.Width(80), GUILayout.Height(30)))
                chefIndex = (chefIndex + 1) % allChefs.Count;
            GUILayout.EndHorizontal();

            GUILayout.Label($"<i>{chef.description}</i>");
            chef.maxHealth = DrawFloatField("Макс. ХП:", chef.maxHealth);
            chef.moveSpeed = DrawFloatField("Скорость бега:", chef.moveSpeed);
            chef.armor = DrawFloatField("Броня:", chef.armor);
        }

        private void DrawEnemyBalance()
        {
            if (allEnemies.Count == 0) { GUILayout.Label("Нет загруженных Врагов."); return; }
            enemyIndex = Mathf.Clamp(enemyIndex, 0, allEnemies.Count - 1);
            EnemyData enemy = allEnemies[enemyIndex];

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("◄ Пред.", GUILayout.Width(80), GUILayout.Height(30)))
                enemyIndex = (enemyIndex - 1 + allEnemies.Count) % allEnemies.Count;

            GUILayout.Label($"<b>{enemy.enemyName} {(enemy.isBoss ? "[БОСС]" : "")}</b> ({enemyIndex + 1}/{allEnemies.Count})", GetHeaderStyle(), GUILayout.Width(220));

            if (GUILayout.Button("След. ►", GUILayout.Width(80), GUILayout.Height(30)))
                enemyIndex = (enemyIndex + 1) % allEnemies.Count;
            GUILayout.EndHorizontal();

            enemy.maxHealth = DrawFloatField("Макс. ХП:", enemy.maxHealth);
            enemy.moveSpeed = DrawFloatField("Скорость:", enemy.moveSpeed);
            enemy.damage = DrawFloatField("Урон:", enemy.damage);
            enemy.armor = DrawFloatField("Броня:", enemy.armor);
            enemy.xpValue = (int)DrawFloatField("XP за гибель:", enemy.xpValue);
        }

        private void DrawWeaponBalance()
        {
            if (allWeapons.Count == 0) { GUILayout.Label("Нет загруженных Блюд."); return; }
            weaponIndex = Mathf.Clamp(weaponIndex, 0, allWeapons.Count - 1);
            WeaponData w = allWeapons[weaponIndex];

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("◄ Пред.", GUILayout.Width(80), GUILayout.Height(30)))
                weaponIndex = (weaponIndex - 1 + allWeapons.Count) % allWeapons.Count;

            GUILayout.Label($"<b>{w.weaponName}</b> ({weaponIndex + 1}/{allWeapons.Count})", GetHeaderStyle(), GUILayout.Width(220));

            if (GUILayout.Button("След. ►", GUILayout.Width(80), GUILayout.Height(30)))
                weaponIndex = (weaponIndex + 1) % allWeapons.Count;
            GUILayout.EndHorizontal();

            GUILayout.Label($"<i>{w.description}</i>");

            if (w.levels != null && w.levels.Count > 0)
            {
                var lvl1 = w.levels[0];
                lvl1.damage = DrawFloatField("Урон (Ур. 1):", lvl1.damage);
                lvl1.cooldown = DrawFloatField("Перезарядка (сек):", lvl1.cooldown);
                lvl1.speed = DrawFloatField("Скорость снаряда:", lvl1.speed);
                lvl1.areaScale = DrawFloatField("Радиус (Area Scale):", lvl1.areaScale);
                w.levels[0] = lvl1;
            }
        }

        private void DrawPassiveBalance()
        {
            if (allPassives.Count == 0) { GUILayout.Label("Нет загруженных Пассивок."); return; }
            passiveIndex = Mathf.Clamp(passiveIndex, 0, allPassives.Count - 1);
            PassiveData p = allPassives[passiveIndex];

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("◄ Пред.", GUILayout.Width(80), GUILayout.Height(30)))
                passiveIndex = (passiveIndex - 1 + allPassives.Count) % allPassives.Count;

            GUILayout.Label($"<b>{p.passiveName}</b> ({passiveIndex + 1}/{allPassives.Count})", GetHeaderStyle(), GUILayout.Width(220));

            if (GUILayout.Button("След. ►", GUILayout.Width(80), GUILayout.Height(30)))
                passiveIndex = (passiveIndex + 1) % allPassives.Count;
            GUILayout.EndHorizontal();

            // ПОЛНОЕ ОПИСАНИЕ И ТИП ЭФФЕКТА
            GUILayout.Box($"<b>Тип бонуса:</b> <color=cyan>{p.passiveType}</color>\n<b>Что делает:</b> {p.description}");

            if (p.levels != null && p.levels.Count > 0)
            {
                var lvl1 = p.levels[0];
                lvl1.value = DrawFloatField("Значение бонуса (Ур. 1):", lvl1.value);
                p.levels[0] = lvl1;
            }
        }

        private float DrawFloatField(string label, float val)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, GUILayout.Width(180));
            float.TryParse(GUILayout.TextField(val.ToString("F1"), GUILayout.Width(80)), out val);
            if (GUILayout.Button("-0.5")) val -= 0.5f;
            if (GUILayout.Button("+0.5")) val += 0.5f;
            GUILayout.EndHorizontal();
            return val;
        }

        private void SaveBalanceToAssets()
        {
#if UNITY_EDITOR
            foreach (var c in allChefs) if (c != null) UnityEditor.EditorUtility.SetDirty(c);
            foreach (var e in allEnemies) if (e != null) UnityEditor.EditorUtility.SetDirty(e);
            foreach (var w in allWeapons) if (w != null) UnityEditor.EditorUtility.SetDirty(w);
            foreach (var p in allPassives) if (p != null) UnityEditor.EditorUtility.SetDirty(p);

            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
            Debug.Log("[CheatMenu] 💾 Изменения баланса успешно сохранены в ScriptableObjects (.asset)!");
#endif
        }

        private void DrawWorldLabels()
        {
            Camera mainCam = Camera.main;
            if (mainCam == null) return;

            EnemyController[] enemies = Object.FindObjectsByType<EnemyController>(FindObjectsSortMode.None);
            GUIStyle style = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };

            foreach (var e in enemies)
            {
                if (e == null) continue;
                Vector3 screenPos = mainCam.WorldToScreenPoint(e.transform.position + Vector3.up * 1.8f);
                if (screenPos.z > 0)
                {
                    float guiY = Screen.height - screenPos.y;
                    Rect r = new Rect(screenPos.x - 80, guiY - 15, 160, 28);
                    style.normal.textColor = e is TargetDummy ? Color.yellow : (e.isBoss ? Color.magenta : Color.red);
                    string label = $"{e.enemyData?.enemyName ?? e.gameObject.name}\nHP: {Mathf.CeilToInt(e.currentHealth)}/{Mathf.CeilToInt(e.maxHealth)}";
                    GUI.Box(r, "", GUI.skin.box);
                    GUI.Label(r, label, style);
                }
            }
        }

        private GUIStyle GetHeaderStyle()
        {
            return new GUIStyle(GUI.skin.label) { fontSize = 14, fontStyle = FontStyle.Bold, normal = { textColor = new Color(0.2f, 0.8f, 1f) } };
        }
    }
}