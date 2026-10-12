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

        public bool showMenu = false;
        public float dummyMaxHP = 1000f;
        private TargetDummy activeDummy;

        private int selectedTab = 0; // 0 = Стамина, 1 = Мобы/Волны, 2 = Экипировка, 3 = Баланс
        private int rebalanceCategory = 0; // 0 = Шефы, 1 = Враги, 2 = Блюда, 3 = Пассивки
        private int chefIndex = 0, enemyIndex = 0, weaponIndex = 0, passiveIndex = 0;

        private Rect windowRect = new Rect(15, 15, 520, 780);
        private Vector2 scrollPos = Vector2.zero;

        private List<ChefData> allChefs = new List<ChefData>();
        private List<WeaponData> allWeapons = new List<WeaponData>();
        private List<PassiveData> allPassives = new List<PassiveData>();
        private List<EnemyData> allEnemies = new List<EnemyData>();

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start() => LoadDataCatalog();

        public void LoadDataCatalog()
        {
            allChefs.Clear(); allWeapons.Clear(); allPassives.Clear(); allEnemies.Clear();
#if UNITY_EDITOR
            string[] c = UnityEditor.AssetDatabase.FindAssets("t:ChefData");
            foreach (var g in c) allChefs.Add(UnityEditor.AssetDatabase.LoadAssetAtPath<ChefData>(UnityEditor.AssetDatabase.GUIDToAssetPath(g)));
            string[] w = UnityEditor.AssetDatabase.FindAssets("t:WeaponData");
            foreach (var g in w) allWeapons.Add(UnityEditor.AssetDatabase.LoadAssetAtPath<WeaponData>(UnityEditor.AssetDatabase.GUIDToAssetPath(g)));
            string[] p = UnityEditor.AssetDatabase.FindAssets("t:PassiveData");
            foreach (var g in p) allPassives.Add(UnityEditor.AssetDatabase.LoadAssetAtPath<PassiveData>(UnityEditor.AssetDatabase.GUIDToAssetPath(g)));
            string[] e = UnityEditor.AssetDatabase.FindAssets("t:EnemyData");
            foreach (var g in e) allEnemies.Add(UnityEditor.AssetDatabase.LoadAssetAtPath<EnemyData>(UnityEditor.AssetDatabase.GUIDToAssetPath(g)));
#endif
        }

        private void Update()
        {
            if (Keyboard.current != null && (Keyboard.current.numpad0Key.wasPressedThisFrame || Keyboard.current.f1Key.wasPressedThisFrame || Keyboard.current.backquoteKey.wasPressedThisFrame))
                ToggleMenu();
        }

        public void ToggleMenu()
        {
            showMenu = !showMenu;
            if (showMenu) LoadDataCatalog();
            Time.timeScale = showMenu ? 0f : 1f;
        }

        private void OnGUI()
        {
            if (!showMenu) return;
            GUI.backgroundColor = new Color(0.1f, 0.1f, 0.15f, 0.95f);
            windowRect = GUI.Window(999, windowRect, DrawCheatWindow, "🛠️ ЧИТ-МЕНЮ ТЕСТИРОВАНИЯ И БАЛАНСА");
        }

        private void DrawCheatWindow(int id)
        {
            GUI.color = Color.white;
            GUILayout.BeginHorizontal();
            if (GUILayout.Toggle(selectedTab == 0, "⚡ Стамина", "Button", GUILayout.Height(30))) selectedTab = 0;
            if (GUILayout.Toggle(selectedTab == 1, "👾 Мобы/Волны", "Button", GUILayout.Height(30))) selectedTab = 1;
            if (GUILayout.Toggle(selectedTab == 2, "🍲 Ростер/Блюда", "Button", GUILayout.Height(30))) selectedTab = 2;
            if (GUILayout.Toggle(selectedTab == 3, "⚖️ БАЛАНС", "Button", GUILayout.Height(30))) selectedTab = 3;
            GUILayout.EndHorizontal();

            GUILayout.Space(10);
            scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.Width(500), GUILayout.Height(680));

            switch (selectedTab)
            {
                case 0: DrawStaminaTab(); break;
                case 1: DrawEnemiesAndWavesTab(); break;
                case 2: DrawEquipmentAndRosterTab(); break;
                case 3: DrawFullBalanceTab(); break;
            }

            GUILayout.EndScrollView();
            GUI.DragWindow();
        }

        // --- 0. СТАМИНА ---
        private void DrawStaminaTab()
        {
            PlayerController pc = Object.FindFirstObjectByType<PlayerController>();
            StaminaSystem st = pc != null ? pc.Stamina : null;

            GUILayout.Label("<b>⚡ СТАМИНА И РЕЖИМЫ</b>", GetHeaderStyle());
            if (st != null)
            {
                GUILayout.Label($"Режим: <color=yellow>{st.currentMode}</color> | Стамина: {st.currentStamina:F0}/{st.maxStamina:F0}");
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("🚫 Без стамины")) st.currentMode = StaminaMode.Disabled;
                if (GUILayout.Button("⚔️ Стамина-Урон")) st.currentMode = StaminaMode.DamageBoostOnly;
                if (GUILayout.Button("⚡ Стамина-Рывок")) st.currentMode = StaminaMode.DashOnly;
                GUILayout.EndHorizontal();

                st.staminaRegenPerSec = DrawFloatRow("Реген (сек):", st.staminaRegenPerSec, 2f);
                st.dashCost = DrawFloatRow("Цена рывка:", st.dashCost, 2f);
                st.damageDivisor = DrawFloatRow("Делитель урона:", st.damageDivisor, 1f);
            }
        }

        // --- 1. МОБЫ, ВОЛНЫ И ЧАНКИ ---
        private void DrawEnemiesAndWavesTab()
        {
            WaveManager wm = Object.FindFirstObjectByType<WaveManager>();
            CompetitionManager cm = CompetitionManager.Instance;
            MapBoundsManager mbm = MapBoundsManager.Instance;

            GUILayout.Label("<b>🌊 СПАВНЕР И ВОЛНЫ</b>", GetHeaderStyle());
            GUILayout.BeginHorizontal();
            if (wm != null)
            {
                string s = wm.enabled ? "⏸️ Выключить волны" : "▶️ Включить волны";
                if (GUILayout.Button(s)) wm.enabled = !wm.enabled;
            }
            if (GUILayout.Button("🧹 Очистить мобов"))
            {
                foreach (var e in Object.FindObjectsByType<EnemyController>(FindObjectsSortMode.None))
                    if (!(e is TargetDummy)) Destroy(e.gameObject);
            }
            if (GUILayout.Button("💣 Убить мобов"))
            {
                foreach (var e in Object.FindObjectsByType<EnemyController>(FindObjectsSortMode.None))
                    if (!(e is TargetDummy)) e.TakeDamage(99999f);
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(10);
            GUILayout.Label("<b>Спавн толпы мобов:</b>");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Спавн +5 мобов") && wm != null && allEnemies.Count > 0)
                for (int i = 0; i < 5; i++) wm.SpawnEnemyAtPosition(allEnemies[Random.Range(0, allEnemies.Count)], GetSpawnPos(), EnemyBehaviorState.Hungry);

            if (GUILayout.Button("Спавн +15 мобов (Толпа)") && wm != null && allEnemies.Count > 0)
                for (int i = 0; i < 15; i++) wm.SpawnEnemyAtPosition(allEnemies[Random.Range(0, allEnemies.Count)], GetSpawnPos(), EnemyBehaviorState.Hungry);
            GUILayout.EndHorizontal();

            // Спавн каждого моба отдельно
            GUILayout.Space(10);
            GUILayout.Label("<b>Спавн конкретного моба:</b>");
            foreach (var e in allEnemies)
            {
                if (e == null) continue;
                if (GUILayout.Button($"Заспавнить: {e.enemyName} {(e.isBoss ? "[БОСС]" : "")}"))
                {
                    if (wm != null) wm.SpawnEnemyAtPosition(e, GetSpawnPos(), EnemyBehaviorState.Hungry, e.isBoss);
                }
            }

            // Управление чанками
            if (mbm != null)
            {
                GUILayout.Space(15);
                GameObject p = GameObject.FindGameObjectWithTag("Player");
                Vector2Int curChunk = p != null ? mbm.GetChunkCoords(p.transform.position) : Vector2Int.zero;
                GUILayout.Label($"<b>🗺️ Чанк игрока: [{curChunk.x}, {curChunk.y}]</b>");

                bool isBlocked = mbm.blacklistedChunks.Contains(curChunk);
                string bBtn = isBlocked ? "✅ Разблокировать этот чанк для спавна" : "⛔ Заблокировать этот чанк от спавна";
                if (GUILayout.Button(bBtn))
                {
                    if (isBlocked) mbm.blacklistedChunks.Remove(curChunk);
                    else mbm.blacklistedChunks.Add(curChunk);
                }
            }

            // Манекен
            GUILayout.Space(15);
            GUILayout.Label("<b>🎯 МАНЕКЕН</b>", GetHeaderStyle());
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("🎯 Заспавнить")) SpawnTargetDummy();
            if (GUILayout.Button("🔄 Сброс ХП")) { if (activeDummy != null) activeDummy.ResetHP(); }
            if (GUILayout.Button("❌ Убрать")) { if (activeDummy != null) Destroy(activeDummy.gameObject); }
            GUILayout.EndHorizontal();
        }

        private Vector3 GetSpawnPos()
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            Vector3 pos = p != null ? p.transform.position : Vector3.zero;
            Vector2 r = Random.insideUnitCircle.normalized * 12f;
            return pos + new Vector3(r.x, 0.5f, r.y);
        }

        private void SpawnTargetDummy()
        {
            if (activeDummy != null) Destroy(activeDummy.gameObject);
            GameObject d = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            d.transform.position = GetSpawnPos();
            activeDummy = d.AddComponent<TargetDummy>();
            activeDummy.SetupDummy(dummyMaxHP);
        }

        // --- 2. ЭКИПИРОВКА И РОСТЕР ---
        private void DrawEquipmentAndRosterTab()
        {
            PlayerController pc = Object.FindFirstObjectByType<PlayerController>();
            WeaponManager wm = pc != null ? pc.GetComponent<WeaponManager>() : null;
            PassiveManager pm = pc != null ? pc.GetComponent<PassiveManager>() : null;

            GUILayout.Label("<b>🍲 БЛЮДА (Экипировка и Ростер)</b>", GetHeaderStyle());
            foreach (var w in allWeapons)
            {
                if (w == null) continue;
                int lvl = wm != null ? wm.GetWeaponLevel(w) : 0;
                GUILayout.BeginVertical("box");
                GUILayout.BeginHorizontal();
                w.isEnabledInRoster = GUILayout.Toggle(w.isEnabledInRoster, " В ростере", GUILayout.Width(100));
                GUILayout.Label($"<b>{w.weaponName}</b> (Ур. {lvl})", GUILayout.Width(180));
                if (wm != null && GUILayout.Button("+1 Ур")) wm.AddWeapon(w);
                if (wm != null && lvl > 0 && GUILayout.Button("❌"))
                {
                    var wb = wm.activeWeapons.Find(x => x.weaponData == w);
                    if (wb != null) { wm.activeWeapons.Remove(wb); Destroy(wb); }
                }
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
            }

            GUILayout.Space(15);
            GUILayout.Label("<b>🧂 ПАССИВКИ (Экипировка и Ростер)</b>", GetHeaderStyle());
            foreach (var p in allPassives)
            {
                if (p == null) continue;
                int lvl = pm != null ? pm.GetPassiveLevel(p) : 0;
                GUILayout.BeginVertical("box");
                GUILayout.BeginHorizontal();
                p.isEnabledInRoster = GUILayout.Toggle(p.isEnabledInRoster, " В ростере", GUILayout.Width(100));
                GUILayout.Label($"<b>{p.passiveName}</b> (Ур. {lvl})", GUILayout.Width(180));
                if (pm != null && GUILayout.Button("+1 Ур")) pm.AddOrUpgradePassive(p);
                if (pm != null && lvl > 0 && GUILayout.Button("❌"))
                {
                    pm.activePassives.Remove(p);
                    pm.passiveLevels.Remove(p);
                }
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
            }
        }

        // --- 3. ПОЛНЫЙ БАЛАНС (ВКЛЮЧАЯ ОРУЖИЕ!) ---
        private void DrawFullBalanceTab()
        {
            GUILayout.Label("<b>⚖️ РЕДАКТОР БАЛАНСА И СОХРАНЕНИЕ</b>", GetHeaderStyle());
            GUILayout.BeginHorizontal();
            if (GUILayout.Toggle(rebalanceCategory == 0, $"Повара ({allChefs.Count})", "Button")) rebalanceCategory = 0;
            if (GUILayout.Toggle(rebalanceCategory == 1, $"Враги ({allEnemies.Count})", "Button")) rebalanceCategory = 1;
            if (GUILayout.Toggle(rebalanceCategory == 2, $"БЛЮДА ({allWeapons.Count})", "Button")) rebalanceCategory = 2;
            if (GUILayout.Toggle(rebalanceCategory == 3, $"Пассивки ({allPassives.Count})", "Button")) rebalanceCategory = 3;
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            // Редактор Поваров
            if (rebalanceCategory == 0 && allChefs.Count > 0)
            {
                chefIndex = Mathf.Clamp(chefIndex, 0, allChefs.Count - 1);
                ChefData c = allChefs[chefIndex];
                DrawSelector(ref chefIndex, allChefs.Count, c.chefName);
                c.moveSpeed = DrawFloatRow("Скорость бега:", c.moveSpeed, 0.5f);
                c.armor = DrawFloatRow("Броня:", c.armor, 1f);
            }
            // Редактор Врагов
            else if (rebalanceCategory == 1 && allEnemies.Count > 0)
            {
                enemyIndex = Mathf.Clamp(enemyIndex, 0, allEnemies.Count - 1);
                EnemyData e = allEnemies[enemyIndex];
                DrawSelector(ref enemyIndex, allEnemies.Count, e.enemyName);
                e.moveSpeed = DrawFloatRow("Скорость:", e.moveSpeed, 0.5f);
                e.damage = DrawFloatRow("Урон:", e.damage, 1f);
                e.maxHealth = DrawFloatRow("Макс. ХП:", e.maxHealth, 5f);
            }
            // Редактор БЛЮД (ОРУЖИЯ!)
            else if (rebalanceCategory == 2 && allWeapons.Count > 0)
            {
                weaponIndex = Mathf.Clamp(weaponIndex, 0, allWeapons.Count - 1);
                WeaponData w = allWeapons[weaponIndex];
                DrawSelector(ref weaponIndex, allWeapons.Count, w.weaponName);
                if (w.levels != null && w.levels.Count > 0)
                {
                    var lvl = w.levels[0];
                    lvl.damage = DrawFloatRow("Урон (Ур. 1):", lvl.damage, 2f);
                    lvl.cooldown = DrawFloatRow("Перезарядка (сек):", lvl.cooldown, 0.2f);
                    lvl.speed = DrawFloatRow("Скорость снаряда:", lvl.speed, 1f);
                    lvl.areaScale = DrawFloatRow("Радиус (Scale):", lvl.areaScale, 0.2f);
                    w.levels[0] = lvl;
                }
            }
            // Редактор Пассивок
            else if (rebalanceCategory == 3 && allPassives.Count > 0)
            {
                passiveIndex = Mathf.Clamp(passiveIndex, 0, allPassives.Count - 1);
                PassiveData p = allPassives[passiveIndex];
                DrawSelector(ref passiveIndex, allPassives.Count, p.passiveName);
                if (p.levels != null && p.levels.Count > 0)
                {
                    var lvl = p.levels[0];
                    lvl.value = DrawFloatRow("Сила бонуса (Ур. 1):", lvl.value, 0.5f);
                    p.levels[0] = lvl;
                }
            }

            GUILayout.Space(15);
            if (GUILayout.Button("💾 СОХРАНИТЬ БАЛАНС НА ДИСК", GUILayout.Height(35)))
            {
#if UNITY_EDITOR
                foreach (var c in allChefs) UnityEditor.EditorUtility.SetDirty(c);
                foreach (var e in allEnemies) UnityEditor.EditorUtility.SetDirty(e);
                foreach (var w in allWeapons) UnityEditor.EditorUtility.SetDirty(w);
                foreach (var p in allPassives) UnityEditor.EditorUtility.SetDirty(p);
                UnityEditor.AssetDatabase.SaveAssets();
                Debug.Log("[CheatMenu] 💾 Изменения баланса успешно сохранены!");
#endif
            }
        }

        private void DrawSelector(ref int idx, int count, string label)
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("◄", GUILayout.Width(40))) idx = (idx - 1 + count) % count;
            GUILayout.Label($"<b>{label}</b> ({idx + 1}/{count})", GetHeaderStyle(), GUILayout.Width(240));
            if (GUILayout.Button("►", GUILayout.Width(40))) idx = (idx + 1) % count;
            GUILayout.EndHorizontal();
        }

        private float DrawFloatRow(string label, float val, float step)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, GUILayout.Width(180));
            float.TryParse(GUILayout.TextField(val.ToString("F1"), GUILayout.Width(70)), out val);
            if (GUILayout.Button($"-{step}")) val -= step;
            if (GUILayout.Button($"+{step}")) val += step;
            GUILayout.EndHorizontal();
            return val;
        }

        private GUIStyle GetHeaderStyle() => new GUIStyle(GUI.skin.label) { fontSize = 13, fontStyle = FontStyle.Bold, normal = { textColor = new Color(0.2f, 0.85f, 1f) } };
    }
}