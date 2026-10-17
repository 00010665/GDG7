using System.Collections.Generic;
using System.Globalization;
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
        public bool showMenu;
        public float dummyMaxHP = 1000f;
        private TargetDummy activeDummy;
        private int selectedTab;
        private int rebalanceCategory;
        private int chefIndex, enemyIndex, weaponIndex, passiveIndex;
        private Rect windowRect = new Rect(15, 15, 560, 780);
        private Vector2 scrollPos;
        private bool pausedBeforeMenu;
        private float gameSpeed = 1f;
        private readonly List<ChefData> allChefs = new List<ChefData>();
        private readonly List<WeaponData> allWeapons = new List<WeaponData>();
        private readonly List<PassiveData> allPassives = new List<PassiveData>();
        private readonly List<EnemyData> allEnemies = new List<EnemyData>();

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        private void Start() => LoadDataCatalog();
        private void OnDestroy() { if (Instance == this) Instance = null; }

        public void LoadDataCatalog()
        {
            allChefs.Clear(); allWeapons.Clear(); allPassives.Clear(); allEnemies.Clear();
#if UNITY_EDITOR
            LoadAssets("t:ChefData", allChefs);
            LoadAssets("t:WeaponData", allWeapons);
            LoadAssets("t:PassiveData", allPassives);
            LoadAssets("t:EnemyData", allEnemies);
#else
            allWeapons.AddRange(Resources.LoadAll<WeaponData>("Weapons"));
            allPassives.AddRange(Resources.LoadAll<PassiveData>("Passives"));
            allEnemies.AddRange(Resources.LoadAll<EnemyData>("Enemies"));
            allChefs.AddRange(Resources.LoadAll<ChefData>("Chefs"));
#endif
        }
#if UNITY_EDITOR
        private static void LoadAssets<T>(string filter, List<T> target) where T : UnityEngine.Object
        {
            foreach (string guid in UnityEditor.AssetDatabase.FindAssets(filter))
            {
                T asset = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(UnityEditor.AssetDatabase.GUIDToAssetPath(guid));
                if (asset != null) target.Add(asset);
            }
        }
#endif
        private void Update()
        {
            if (Keyboard.current == null) return;
            if (Keyboard.current.numpad0Key.wasPressedThisFrame || Keyboard.current.f1Key.wasPressedThisFrame || Keyboard.current.backquoteKey.wasPressedThisFrame)
                ToggleMenu();
        }
        public void ToggleMenu()
        {
            showMenu = !showMenu;
            if (showMenu) { LoadDataCatalog(); pausedBeforeMenu = Time.timeScale == 0f; Time.timeScale = 0f; }
            else Time.timeScale = pausedBeforeMenu ? 0f : gameSpeed;
        }
        private void OnGUI()
        {
            if (!showMenu) return;
            Color old = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.1f, 0.1f, 0.15f, 0.95f);
            windowRect = GUI.Window(999, windowRect, DrawCheatWindow, "🛠️ ЧИТ-МЕНЮ ТЕСТИРОВАНИЯ И БАЛАНСА");
            GUI.backgroundColor = old;
        }
        private void DrawCheatWindow(int id)
        {
            GUILayout.BeginHorizontal();
            string[] tabs = { "⚡ Общее", "🏆 Батл", "👾 Мобы", "🍲 Ростер", "⚖️ Баланс" };
            for (int i = 0; i < tabs.Length; i++) if (GUILayout.Toggle(selectedTab == i, tabs[i], "Button", GUILayout.Height(30))) selectedTab = i;
            GUILayout.EndHorizontal();
            scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.Width(540), GUILayout.Height(680));
            switch (selectedTab)
            {
                case 0: DrawGeneralTab(); break;
                case 1: DrawCompetitionTab(); break;
                case 2: DrawEnemiesAndWavesTab(); break;
                case 3: DrawEquipmentAndRosterTab(); break;
                case 4: DrawFullBalanceTab(); break;
            }
            GUILayout.EndScrollView();
            if (GUILayout.Button("Закрыть чит-меню (Numpad 0 / F1)", GUILayout.Height(28))) ToggleMenu();
            GUI.DragWindow();
        }
        private void DrawGeneralTab()
        {
            GUILayout.Label("<b>⏱ ВРЕМЯ</b>", GetHeaderStyle());
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("▶ Продолжить")) { gameSpeed = gameSpeed <= 0f ? 1f : gameSpeed; Time.timeScale = gameSpeed; }
            if (GUILayout.Button("⏸ Пауза")) Time.timeScale = 0f;
            foreach (float speed in new[] { 0.5f, 1f, 2f, 5f }) if (GUILayout.Button(speed.ToString("0.0", CultureInfo.InvariantCulture) + "x")) { gameSpeed = speed; Time.timeScale = speed; }
            GUILayout.EndHorizontal();
            GUILayout.Label($"Текущий Time Scale: {Time.timeScale:F1}");
            PlayerController pc = Object.FindFirstObjectByType<PlayerController>();
            StaminaSystem st = pc != null ? pc.Stamina : null;
            GUILayout.Space(8); GUILayout.Label("<b>⚡ ПОВАР И СТАМИНА</b>", GetHeaderStyle());
            if (pc != null)
            {
                GUILayout.Label($"Стamina: {(st != null ? st.currentStamina : 0):F0}/{(st != null ? st.maxStamina : 0):F0}");
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("❤️ Исцелить / полный запас")) { if (st != null) st.currentStamina = st.maxStamina; }
                if (GUILayout.Button("💯 100% стамины")) { if (st != null) st.currentStamina = st.maxStamina; }
                GUILayout.EndHorizontal();
                if (st != null)
                {
                    st.staminaRegenPerSec = DrawFloatRow("Реген стамины/сек:", st.staminaRegenPerSec, 2f);
                    st.dashCost = DrawFloatRow("Стоимость рывка:", st.dashCost, 2f);
                    st.damageDivisor = DrawFloatRow("Делитель бонуса урона:", st.damageDivisor, 1f);
                }
            }
            GUILayout.Space(8); GUILayout.Label("<b>✨ ОПЫТ</b>", GetHeaderStyle());
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("+100 XP")) ExperienceManager.Instance?.AddXP(100f);
            if (GUILayout.Button("+1000 XP")) ExperienceManager.Instance?.AddXP(1000f);
            GUILayout.EndHorizontal();
        }
        private void DrawCompetitionTab()
        {
            CompetitionManager cm = CompetitionManager.Instance;
            GUILayout.Label("<b>🏆 СОСТОЯНИЕ МАТЧА</b>", GetHeaderStyle());
            if (cm == null) { GUILayout.Label("CompetitionManager не найден."); return; }
            GUILayout.Label($"Фаза: {(cm.isBossPhase ? "Босс" : "Подготовка")} | Осталось: {cm.RemainingTime:F1} сек.");
            GUILayout.Label($"Счет: Игрок {cm.playerScore} : {cm.bossScore} Босс");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("👹 Вызвать босса СЕЙЧАС")) cm.ForceBossPhase();
            if (GUILayout.Button("🏁 Завершить матч / Финал")) cm.ForceEndMatch();
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("+5 Очков Игроку")) cm.AddPlayerScore(5);
            if (GUILayout.Button("+5 Очков Боссу")) cm.AddBossScore(5);
            GUILayout.EndHorizontal();
            GUILayout.Space(12); GUILayout.Label("<b>🎯 ТЕСТОВЫЙ МАНЕКЕН</b>", GetHeaderStyle());
            dummyMaxHP = DrawFloatRow("Макс. ХП манекена:", dummyMaxHP, 100f);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Заспавнить Манекен")) SpawnTargetDummy();
            if (GUILayout.Button("Сбросить ХП")) activeDummy?.ResetHP();
            if (GUILayout.Button("Убрать Манекен")) { if (activeDummy != null) Destroy(activeDummy.gameObject); activeDummy = null; }
            GUILayout.EndHorizontal();
            if (activeDummy == null) activeDummy = Object.FindFirstObjectByType<TargetDummy>();
            if (activeDummy != null) GUILayout.Label($"ХП: {activeDummy.currentHealth:F0}/{activeDummy.maxHealth:F0} | DPS: {activeDummy.dps:F1} | Источник: {activeDummy.lastDamageSource} ({activeDummy.lastDamageValue:F1})");
        }
        private void DrawEnemiesAndWavesTab()
        {
            WaveManager wm = Object.FindFirstObjectByType<WaveManager>();
            MapBoundsManager mbm = MapBoundsManager.Instance;
            GUILayout.Label("<b>🌊 СПАВНЕР И ВОЛНЫ</b>", GetHeaderStyle());
            GUILayout.BeginHorizontal();
            if (wm != null && GUILayout.Button(wm.enabled ? "⏸ Выключить волны" : "▶ Включить волны")) wm.enabled = !wm.enabled;
            if (GUILayout.Button("🧹 Очистить мобов")) foreach (var e in Object.FindObjectsByType<EnemyController>(FindObjectsSortMode.None)) if (!(e is TargetDummy)) Destroy(e.gameObject);
            if (GUILayout.Button("💣 Убить мобов")) foreach (var e in Object.FindObjectsByType<EnemyController>(FindObjectsSortMode.None)) if (!(e is TargetDummy)) e.TakeDamage(99999f);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Спавн +5") && wm != null) for (int i = 0; i < 5; i++) SpawnRandomEnemy(wm);
            if (GUILayout.Button("Спавн +15 (Толпа)") && wm != null) for (int i = 0; i < 15; i++) SpawnRandomEnemy(wm);
            GUILayout.EndHorizontal();
            GUILayout.Label("<b>Точечный спавн:</b>");
            foreach (EnemyData e in allEnemies) if (e != null && GUILayout.Button($"Заспавнить: {e.enemyName} {(e.isBoss ? "[БОСС]" : "")}")) wm?.SpawnEnemyAtPosition(e, GetSpawnPos(), EnemyBehaviorState.Hungry, e.isBoss);
            if (mbm != null)
            {
                GameObject p = GameObject.FindGameObjectWithTag("Player");
                Vector2Int chunk = p != null ? mbm.GetChunkCoords(p.transform.position) : Vector2Int.zero;
                GUILayout.Label($"<b>🗺 Чанк игрока: [{chunk.x}, {chunk.y}]</b>");
                bool blocked = mbm.blacklistedChunks.Contains(chunk);
                if (GUILayout.Button(blocked ? "✅ Разблокировать чанк" : "⛔ Заблокировать чанк от спавна"))
                { if (blocked) mbm.blacklistedChunks.Remove(chunk); else mbm.blacklistedChunks.Add(chunk); }
            }
        }
        private void SpawnRandomEnemy(WaveManager wm)
        { if (allEnemies.Count == 0) return; EnemyData e = allEnemies[Random.Range(0, allEnemies.Count)]; wm.SpawnEnemyAtPosition(e, GetSpawnPos(), EnemyBehaviorState.Hungry, e.isBoss); }
        private void DrawEquipmentAndRosterTab()
        {
            PlayerController pc = Object.FindFirstObjectByType<PlayerController>();
            WeaponManager wm = pc != null ? pc.GetComponent<WeaponManager>() : null;
            PassiveManager pm = pc != null ? pc.GetComponent<PassiveManager>() : null;
            GUILayout.Label("<b>🍲 БЛЮДА (Экипировка и Ростер)</b>", GetHeaderStyle());
            foreach (WeaponData w in allWeapons) if (w != null)
            {
                GUILayout.BeginHorizontal("box"); w.isEnabledInRoster = GUILayout.Toggle(w.isEnabledInRoster, "В ростере", GUILayout.Width(90));
                int lvl = wm != null ? wm.GetWeaponLevel(w) : 0; GUILayout.Label($"{w.weaponName} (Ур. {lvl})", GUILayout.Width(180));
                if (wm != null && GUILayout.Button("+1")) wm.AddWeapon(w);
                if (wm != null && lvl > 0 && GUILayout.Button("-1")) ChangeWeaponLevel(wm, w, -1);
                if (wm != null && lvl > 0 && GUILayout.Button("Убрать")) RemoveWeapon(wm, w);
                GUILayout.EndHorizontal();
            }
            GUILayout.Space(10); GUILayout.Label("<b>🧂 ПАССИВКИ (Экипировка и Ростер)</b>", GetHeaderStyle());
            foreach (PassiveData p in allPassives) if (p != null)
            {
                GUILayout.BeginHorizontal("box"); p.isEnabledInRoster = GUILayout.Toggle(p.isEnabledInRoster, "В ростере", GUILayout.Width(90));
                int lvl = pm != null ? pm.GetPassiveLevel(p) : 0; GUILayout.Label($"{p.passiveName} (Ур. {lvl})", GUILayout.Width(180));
                if (pm != null && GUILayout.Button("+1")) pm.AddOrUpgradePassive(p);
                if (pm != null && lvl > 1 && GUILayout.Button("-1")) SetPassiveLevel(pm, p, lvl - 1);
                if (pm != null && lvl > 0 && GUILayout.Button("Убрать")) SetPassiveLevel(pm, p, 0);
                GUILayout.EndHorizontal();
            }
        }
        private static void ChangeWeaponLevel(WeaponManager manager, WeaponData data, int delta)
        { WeaponBase weapon = manager.activeWeapons.Find(x => x != null && x.weaponData == data); if (weapon == null) return; weapon.currentLevel = Mathf.Clamp(weapon.currentLevel + delta, 1, Mathf.Max(1, data.levels.Count)); }
        private static void RemoveWeapon(WeaponManager manager, WeaponData data)
        { WeaponBase weapon = manager.activeWeapons.Find(x => x != null && x.weaponData == data); if (weapon != null) { manager.activeWeapons.Remove(weapon); Destroy(weapon); } }
        private static void SetPassiveLevel(PassiveManager manager, PassiveData data, int level)
        { manager.activePassives.Remove(data); manager.passiveLevels.Remove(data); for (int i = 0; i < level; i++) manager.AddOrUpgradePassive(data); }
        private void DrawFullBalanceTab()
        {
            GUILayout.Label("<b>⚖️ РЕДАКТОР БАЛАНСА И СОХРАНЕНИЕ</b>", GetHeaderStyle());
            GUILayout.BeginHorizontal();
            string[] categories = { $"Повара ({allChefs.Count})", $"Враги ({allEnemies.Count})", $"Блюда ({allWeapons.Count})", $"Пассивки ({allPassives.Count})" };
            for (int i = 0; i < categories.Length; i++) if (GUILayout.Toggle(rebalanceCategory == i, categories[i], "Button")) rebalanceCategory = i;
            GUILayout.EndHorizontal();
            if (rebalanceCategory == 0 && allChefs.Count > 0) { chefIndex = Mathf.Clamp(chefIndex, 0, allChefs.Count - 1); ChefData c = allChefs[chefIndex]; DrawSelector(ref chefIndex, allChefs.Count, c.chefName); c.moveSpeed = DrawFloatRow("Скорость бега:", c.moveSpeed, .5f); c.armor = DrawFloatRow("Броня:", c.armor, 1f); }
            else if (rebalanceCategory == 1 && allEnemies.Count > 0) { enemyIndex = Mathf.Clamp(enemyIndex, 0, allEnemies.Count - 1); EnemyData e = allEnemies[enemyIndex]; DrawSelector(ref enemyIndex, allEnemies.Count, e.enemyName); e.moveSpeed = DrawFloatRow("Скорость:", e.moveSpeed, .5f); e.damage = DrawFloatRow("Урон:", e.damage, 1f); e.maxHealth = DrawFloatRow("Макс. ХП:", e.maxHealth, 5f); }
            else if (rebalanceCategory == 2 && allWeapons.Count > 0) { weaponIndex = Mathf.Clamp(weaponIndex, 0, allWeapons.Count - 1); WeaponData w = allWeapons[weaponIndex]; DrawSelector(ref weaponIndex, allWeapons.Count, w.weaponName); if (w.levels != null && w.levels.Count > 0) { var l = w.levels[0]; l.damage = DrawFloatRow("Урон (Ур. 1):", l.damage, 2f); l.cooldown = DrawFloatRow("Перезарядка:", l.cooldown, .2f); l.speed = DrawFloatRow("Скорость снаряда:", l.speed, 1f); l.areaScale = DrawFloatRow("Радиус (Scale):", l.areaScale, .2f); w.levels[0] = l; } }
            else if (rebalanceCategory == 3 && allPassives.Count > 0) { passiveIndex = Mathf.Clamp(passiveIndex, 0, allPassives.Count - 1); PassiveData p = allPassives[passiveIndex]; DrawSelector(ref passiveIndex, allPassives.Count, p.passiveName); if (p.levels != null && p.levels.Count > 0) { var l = p.levels[0]; l.value = DrawFloatRow("Сила бонуса (Ур. 1):", l.value, .5f); p.levels[0] = l; } }
            GUILayout.Space(15);
            if (GUILayout.Button("💾 СОХРАНИТЬ БАЛАНС НА ДИСК", GUILayout.Height(35)))
            {
#if UNITY_EDITOR
                foreach (var c in allChefs) UnityEditor.EditorUtility.SetDirty(c);
                foreach (var e in allEnemies) UnityEditor.EditorUtility.SetDirty(e);
                foreach (var w in allWeapons) UnityEditor.EditorUtility.SetDirty(w);
                foreach (var p in allPassives) UnityEditor.EditorUtility.SetDirty(p);
                UnityEditor.AssetDatabase.SaveAssets();
                Debug.Log("[CheatMenu] Баланс и ростер сохранены.");
#else
                Debug.LogWarning("Сохранение ScriptableObject доступно только в Unity Editor.");
#endif
            }
        }
        private void DrawSelector(ref int idx, int count, string label)
        { GUILayout.BeginHorizontal(); if (GUILayout.Button("◄", GUILayout.Width(40))) idx = (idx - 1 + count) % count; GUILayout.Label($"<b>{label}</b> ({idx + 1}/{count})", GetHeaderStyle(), GUILayout.Width(300)); if (GUILayout.Button("►", GUILayout.Width(40))) idx = (idx + 1) % count; GUILayout.EndHorizontal(); }
        private float DrawFloatRow(string label, float val, float step)
        { GUILayout.BeginHorizontal(); GUILayout.Label(label, GUILayout.Width(190)); string input = GUILayout.TextField(val.ToString("F1", CultureInfo.InvariantCulture), GUILayout.Width(70)); if (float.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out float parsed)) val = parsed; if (GUILayout.Button($"-{step}")) val -= step; if (GUILayout.Button($"+{step}")) val += step; GUILayout.EndHorizontal(); return val; }
        private Vector3 GetSpawnPos()
        { GameObject p = GameObject.FindGameObjectWithTag("Player"); Vector3 pos = p != null ? p.transform.position : Vector3.zero; Vector2 r = Random.insideUnitCircle.normalized * 12f; return pos + new Vector3(r.x, .5f, r.y); }
        private void SpawnTargetDummy()
        { if (activeDummy != null) Destroy(activeDummy.gameObject); GameObject d = GameObject.CreatePrimitive(PrimitiveType.Capsule); d.transform.position = GetSpawnPos(); activeDummy = d.AddComponent<TargetDummy>(); activeDummy.SetupDummy(dummyMaxHP); }
        private GUIStyle GetHeaderStyle() => new GUIStyle(GUI.skin.label) { richText = true, fontSize = 13, fontStyle = FontStyle.Bold, normal = { textColor = new Color(.2f, .85f, 1f) } };
    }
}
