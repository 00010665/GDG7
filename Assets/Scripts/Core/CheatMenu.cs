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

        [Header("Tabs")]
        private int selectedTab = 0; // 0 = Время/Стамина, 1 = Соревнование/Босс, 2 = Экипировка, 3 = Баланс
        private int rebalanceCategory = 0;
        private int chefIndex = 0, enemyIndex = 0, weaponIndex = 0, passiveIndex = 0;

        private Rect windowRect = new Rect(15, 15, 500, 750);
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

        private void DrawCheatWindow(int windowID)
        {
            GUI.color = Color.white;
            GUILayout.BeginHorizontal();
            if (GUILayout.Toggle(selectedTab == 0, "⚡ Стамина", "Button", GUILayout.Height(30))) selectedTab = 0;
            if (GUILayout.Toggle(selectedTab == 1, "🏆 Батл / Босс", "Button", GUILayout.Height(30))) selectedTab = 1;
            if (GUILayout.Toggle(selectedTab == 2, "🍲 Блюда", "Button", GUILayout.Height(30))) selectedTab = 2;
            if (GUILayout.Toggle(selectedTab == 3, "⚖️ Баланс", "Button", GUILayout.Height(30))) selectedTab = 3;
            GUILayout.EndHorizontal();

            GUILayout.Space(10);
            scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.Width(480), GUILayout.Height(650));

            switch (selectedTab)
            {
                case 0: DrawStaminaTab(); break;
                case 1: DrawCompetitionTab(); break;
                case 2: DrawEquipmentTab(); break;
                case 3: DrawBalanceEditorTab(); break;
            }

            GUILayout.EndScrollView();
            GUI.DragWindow();
        }

        // --- 0. СТАМИНА И ИГРОК ---
        private void DrawStaminaTab()
        {
            GameObject pObj = GameObject.FindGameObjectWithTag("Player");
            PlayerController pc = pObj != null ? pObj.GetComponent<PlayerController>() : null;
            StaminaSystem st = pc != null ? pc.Stamina : null;

            GUILayout.Label("<b>⚡ РЕЖИМ И ПАРАМЕТРЫ СТАМИНЫ</b>", GetHeaderStyle());
            if (st != null)
            {
                GUILayout.Label($"Текущий режим: <b><color=yellow>{st.currentMode}</color></b>");
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("🚫 Без стамины")) st.currentMode = StaminaMode.Disabled;
                if (GUILayout.Button("⚔️ Стамина-Урон")) st.currentMode = StaminaMode.DamageBoostOnly;
                if (GUILayout.Button("⚡ Стамина-Рывок")) st.currentMode = StaminaMode.DashOnly;
                GUILayout.EndHorizontal();

                GUILayout.Space(10);
                GUILayout.Label($"Стамина: {st.currentStamina:F0} / {st.maxStamina:F0} (Бонус урона: +{st.GetFlatDamageBonus()})");
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("+50 Стамины")) st.currentStamina = Mathf.Min(st.maxStamina, st.currentStamina + 50f);
                if (GUILayout.Button("-50 Стамины")) st.currentStamina = Mathf.Max(0f, st.currentStamina - 50f);
                if (GUILayout.Button("Восстановить (100%)")) st.currentStamina = st.maxStamina;
                GUILayout.EndHorizontal();

                GUILayout.Space(5);
                st.staminaRegenPerSec = DrawFloatRow("Реген в секунду:", st.staminaRegenPerSec, 2f);
                st.dashCost = DrawFloatRow("Стоимость рывка:", st.dashCost, 2f);
                st.damageDivisor = DrawFloatRow("Делитель бонуса урона:", st.damageDivisor, 1f);
            }

            GUILayout.Space(15);
            GUILayout.Label("<b>⏱️ СКОРОСТЬ ВРЕМЕНИ</b>", GetHeaderStyle());
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("0.5x")) Time.timeScale = 0.5f;
            if (GUILayout.Button("1.0x (Норма)")) Time.timeScale = 1.0f;
            if (GUILayout.Button("2.0x (Ускорение)")) Time.timeScale = 2.0f;
            if (GUILayout.Button("5.0x (Турбо)")) Time.timeScale = 5.0f;
            GUILayout.EndHorizontal();
        }

        // --- 1. СОРЕВНОВАНИЕ И БОСС ---
        private void DrawCompetitionTab()
        {
            CompetitionManager cm = CompetitionManager.Instance;
            GUILayout.Label("<b>🏆 УПРАВЛЕНИЕ КУЛИНАРНЫМ БАТЛОМ</b>", GetHeaderStyle());

            if (cm != null)
            {
                GUILayout.Box($"<b>Статус:</b> {(cm.isBossPhase ? "<color=red>ФАЗА БОССА</color>" : "ФАЗА ПОДГОТОВКИ")}\n" +
                              $"Осталось времени: {cm.RemainingTime:F1} сек (Таймер: {cm.matchTimer:F1}/{cm.TotalMatchDuration:F0})\n" +
                              $"Счет: <b><color=cyan>Игрок: {cm.playerScore}</color></b>   VS   <b><color=magenta>Босс: {cm.bossScore}</color></b>");

                GUILayout.BeginHorizontal();
                if (!cm.isBossPhase && GUILayout.Button("🚨 ВЫЗВАТЬ БОССА СЕЙЧАС (Катсцена)"))
                {
                    cm.matchTimer = cm.prepPhaseDuration;
                    ToggleMenu();
                }
                if (GUILayout.Button("🏁 ЗАВЕРШИТЬ МАТЧ (Финал)"))
                {
                    cm.matchTimer = cm.TotalMatchDuration;
                    ToggleMenu();
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("+5 Очков Игроку")) cm.AddPlayerScore(5);
                if (GUILayout.Button("+5 Очков Боссу")) cm.AddBossScore(5);
                GUILayout.EndHorizontal();
            }

            GUILayout.Space(15);
            GUILayout.Label("<b>🎯 МАНЕКЕН И ВРАГИ</b>", GetHeaderStyle());
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("🎯 Заспавнить Манекен")) SpawnTargetDummy();
            if (GUILayout.Button("🔄 Сброс ХП")) { if (activeDummy != null) activeDummy.ResetHP(); }
            if (GUILayout.Button("❌ Убрать")) { if (activeDummy != null) Destroy(activeDummy.gameObject); }
            GUILayout.EndHorizontal();

            if (activeDummy != null)
            {
                GUILayout.Label($"Манекен DPS: <color=yellow>{activeDummy.dps:F1}</color> | ХП: {activeDummy.currentHealth:F0}/{activeDummy.maxHealth:F0}");
            }
        }

        private void SpawnTargetDummy()
        {
            if (activeDummy != null) Destroy(activeDummy.gameObject);
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            Vector3 pos = p != null ? p.transform.position + p.transform.forward * 3f : Vector3.zero;
            GameObject dObj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            dObj.transform.position = pos;
            activeDummy = dObj.AddComponent<TargetDummy>();
            activeDummy.SetupDummy(dummyMaxHP);
        }

        // --- 2. ЭКИПИРОВКА ---
        private void DrawEquipmentTab()
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            WeaponManager wm = p != null ? p.GetComponent<WeaponManager>() : null;
            PassiveManager pm = p != null ? p.GetComponent<PassiveManager>() : null;

            GUILayout.Label("<b>🍲 БЛЮДА</b>", GetHeaderStyle());
            if (wm != null)
            {
                foreach (var w in allWeapons)
                {
                    if (w == null) continue;
                    int lvl = wm.GetWeaponLevel(w);
                    GUILayout.BeginHorizontal("box");
                    GUILayout.Label($"{w.weaponName} (Ур. {lvl})", GUILayout.Width(200));
                    if (GUILayout.Button("+1 Ур")) wm.AddWeapon(w);
                    if (lvl > 0 && GUILayout.Button("❌"))
                    {
                        var wb = wm.activeWeapons.Find(x => x.weaponData == w);
                        if (wb != null) { wm.activeWeapons.Remove(wb); Destroy(wb); }
                    }
                    GUILayout.EndHorizontal();
                }
            }

            GUILayout.Space(10);
            GUILayout.Label("<b>🧂 ПАССИВКИ (СТАМИНА И БОНУСЫ)</b>", GetHeaderStyle());
            if (pm != null)
            {
                foreach (var pass in allPassives)
                {
                    if (pass == null) continue;
                    int lvl = pm.GetPassiveLevel(pass);
                    GUILayout.BeginHorizontal("box");
                    GUILayout.Label($"{pass.passiveName} (Ур. {lvl})", GUILayout.Width(200));
                    if (GUILayout.Button("+1 Ур")) pm.AddOrUpgradePassive(pass);
                    if (lvl > 0 && GUILayout.Button("❌"))
                    {
                        pm.activePassives.Remove(pass);
                        pm.passiveLevels.Remove(pass);
                    }
                    GUILayout.EndHorizontal();
                }
            }
        }

        // --- 3. БАЛАНС ---
        private void DrawBalanceEditorTab()
        {
            GUILayout.Label("<b>⚖️ РЕДАКТОР БАЛАНСА И СОХРАНЕНИЕ</b>", GetHeaderStyle());
            GUILayout.BeginHorizontal();
            if (GUILayout.Toggle(rebalanceCategory == 0, $"Повара ({allChefs.Count})", "Button")) rebalanceCategory = 0;
            if (GUILayout.Toggle(rebalanceCategory == 1, $"Враги ({allEnemies.Count})", "Button")) rebalanceCategory = 1;
            if (GUILayout.Toggle(rebalanceCategory == 2, $"Блюда ({allWeapons.Count})", "Button")) rebalanceCategory = 2;
            GUILayout.EndHorizontal();

            GUILayout.Space(10);
            if (rebalanceCategory == 0 && allChefs.Count > 0)
            {
                chefIndex = Mathf.Clamp(chefIndex, 0, allChefs.Count - 1);
                ChefData c = allChefs[chefIndex];
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("◄")) chefIndex = (chefIndex - 1 + allChefs.Count) % allChefs.Count;
                GUILayout.Label($"<b>{c.chefName}</b> ({chefIndex + 1}/{allChefs.Count})", GetHeaderStyle(), GUILayout.Width(220));
                if (GUILayout.Button("►")) chefIndex = (chefIndex + 1) % allChefs.Count;
                GUILayout.EndHorizontal();
                c.moveSpeed = DrawFloatRow("Скорость бега:", c.moveSpeed, 0.5f);
                c.armor = DrawFloatRow("Броня:", c.armor, 1f);
            }
            else if (rebalanceCategory == 1 && allEnemies.Count > 0)
            {
                enemyIndex = Mathf.Clamp(enemyIndex, 0, allEnemies.Count - 1);
                EnemyData e = allEnemies[enemyIndex];
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("◄")) enemyIndex = (enemyIndex - 1 + allEnemies.Count) % allEnemies.Count;
                GUILayout.Label($"<b>{e.enemyName}</b> ({enemyIndex + 1}/{allEnemies.Count})", GetHeaderStyle(), GUILayout.Width(220));
                if (GUILayout.Button("►")) enemyIndex = (enemyIndex + 1) % allEnemies.Count;
                GUILayout.EndHorizontal();
                e.moveSpeed = DrawFloatRow("Скорость:", e.moveSpeed, 0.5f);
                e.damage = DrawFloatRow("Урон:", e.damage, 1f);
                e.maxHealth = DrawFloatRow("Макс. ХП:", e.maxHealth, 5f);
            }

            GUILayout.Space(15);
            if (GUILayout.Button("💾 СОХРАНИТЬ БАЛАНС НА ДИСК", GUILayout.Height(35)))
            {
#if UNITY_EDITOR
                foreach (var c in allChefs) UnityEditor.EditorUtility.SetDirty(c);
                foreach (var e in allEnemies) UnityEditor.EditorUtility.SetDirty(e);
                UnityEditor.AssetDatabase.SaveAssets();
                Debug.Log("[CheatMenu] Баланс сохранен!");
#endif
            }
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