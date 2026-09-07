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
        public bool spawnerPaused = false;

        private Rect windowRect = new Rect(15, 15, 450, 680);
        private Vector2 scrollPos = Vector2.zero;

        // Кэш данных для выдачи
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

        private void LoadDataCatalog()
        {
#if UNITY_EDITOR
            allWeapons.Clear();
            string[] wGuids = UnityEditor.AssetDatabase.FindAssets("t:WeaponData");
            foreach (var g in wGuids)
            {
                var w = UnityEditor.AssetDatabase.LoadAssetAtPath<WeaponData>(UnityEditor.AssetDatabase.GUIDToAssetPath(g));
                if (w != null && !allWeapons.Contains(w)) allWeapons.Add(w);
            }

            allPassives.Clear();
            string[] pGuids = UnityEditor.AssetDatabase.FindAssets("t:PassiveData");
            foreach (var g in pGuids)
            {
                var p = UnityEditor.AssetDatabase.LoadAssetAtPath<PassiveData>(UnityEditor.AssetDatabase.GUIDToAssetPath(g));
                if (p != null && !allPassives.Contains(p)) allPassives.Add(p);
            }

            allEnemies.Clear();
            string[] eGuids = UnityEditor.AssetDatabase.FindAssets("t:EnemyData");
            foreach (var g in eGuids)
            {
                var e = UnityEditor.AssetDatabase.LoadAssetAtPath<EnemyData>(UnityEditor.AssetDatabase.GUIDToAssetPath(g));
                if (e != null && !allEnemies.Contains(e)) allEnemies.Add(e);
            }
#endif
        }

        private void Update()
        {
            // Горячая клавиша: Numpad 0, F1 или Backquote (~)
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
            if (showMenu)
            {
                Time.timeScale = 0f;
                Debug.Log("[CheatMenu] 🛠️ Чит-меню открыто. Игра поставлена на паузу.");
            }
            else
            {
                Time.timeScale = 1f;
                Debug.Log("[CheatMenu] 🛠️ Чит-меню закрыто. Игра снята с паузы.");
            }
        }

        private void OnGUI()
        {
            // Отрисовка имен и ХП над головами врагов
            if (showEnemyLabels)
            {
                DrawEnemyWorldLabels();
            }

            if (!showMenu) return;

            // Полупрозрачный стиль окна
            GUI.backgroundColor = new Color(0.1f, 0.1f, 0.15f, 0.92f);
            windowRect = GUI.Window(999, windowRect, DrawCheatWindow, "🛠️ ЧИТ-МЕНЮ ТЕСТИРОВАНИЯ (Numpad 0 / F1)");
        }

        private void DrawCheatWindow(int windowID)
        {
            GUI.color = Color.white;
            scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.Width(430), GUILayout.Height(640));

            // --- 1. УПРАВЛЕНИЕ ВРЕМЕНЕМ ---
            GUILayout.Label("<b>⏱️ ВРЕМЯ И СОСТОЯНИЕ ИГРЫ</b>", GetHeaderStyle());
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("▶️ Продолжить", GUILayout.Height(30))) { ToggleMenu(); }
            if (GUILayout.Button("⏸️ Пауза", GUILayout.Height(30))) { Time.timeScale = 0f; }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("0.5x Скорость")) { Time.timeScale = 0.5f; }
            if (GUILayout.Button("1.0x Норма")) { Time.timeScale = 1.0f; }
            if (GUILayout.Button("2.0x Ускорение")) { Time.timeScale = 2.0f; }
            if (GUILayout.Button("5.0x Турбо")) { Time.timeScale = 5.0f; }
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            // --- 2. ПРОКАЧКА ИГРОКА ---
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            PlayerController player = playerObj != null ? playerObj.GetComponent<PlayerController>() : null;

            GUILayout.Label("<b>👨‍🍳 ХАРАКТЕРИСТИКИ ПОВАРА</b>", GetHeaderStyle());
            if (player != null)
            {
                GUILayout.Label($"ХП: {Mathf.CeilToInt(player.currentHealth)} / {Mathf.CeilToInt(player.maxHealth)} | Скорость: {player.currentMoveSpeed:F1} | Броня: {player.currentArmor} | Урон: +{Mathf.RoundToInt((player.damageMultiplier - 1f) * 100)}%");

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("❤️ Исцелить (100% HP)")) { player.Heal(player.maxHealth); }
                if (GUILayout.Button("➕ +50 Макс. HP")) { player.maxHealth += 50f; player.Heal(50f); }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("⚡ +2.0 к Скорости")) { player.currentMoveSpeed += 2.0f; }
                if (GUILayout.Button("🛡️ +5 к Броне")) { player.currentArmor += 5f; }
                if (GUILayout.Button("⚔️ +50% к Урону")) { player.damageMultiplier += 0.5f; }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("⭐ +100 XP")) { if (ExperienceManager.Instance != null) ExperienceManager.Instance.AddXP(100); }
                if (GUILayout.Button("🌟 +1000 XP (Level-Up)")) { if (ExperienceManager.Instance != null) ExperienceManager.Instance.AddXP(1000); }
                GUILayout.EndHorizontal();
            }
            else
            {
                GUILayout.Label("⚠️ Игрок не найден на сцене.");
            }

            GUILayout.Space(10);

            // --- 3. ВЫДАЧА И ПРОКАЧКА БЛЮД ---
            WeaponManager wm = playerObj != null ? playerObj.GetComponent<WeaponManager>() : null;
            GUILayout.Label("<b>🍲 ВЫДАТЬ / ПРОКАЧАТЬ БЛЮДО</b>", GetHeaderStyle());
            if (wm != null && allWeapons.Count > 0)
            {
                for (int i = 0; i < allWeapons.Count; i++)
                {
                    WeaponData w = allWeapons[i];
                    if (w == null) continue;

                    int curLvl = wm.GetWeaponLevel(w);
                    string btnText = curLvl > 0 ? $"LevelUp: {w.weaponName} (Счас: Ур. {curLvl})" : $"Выдать: {w.weaponName}";

                    if (GUILayout.Button(btnText))
                    {
                        wm.AddWeapon(w);
                        Debug.Log($"[CheatMenu] Выдано/Прокачано блюдо: {w.weaponName}");
                    }
                }
            }

            GUILayout.Space(10);

            // --- 4. ВЫДАЧА И ПРОКАЧКА ПАССИВОК ---
            PassiveManager pm = playerObj != null ? playerObj.GetComponent<PassiveManager>() : null;
            GUILayout.Label("<b>🧂 ВЫДАТЬ / ПРОКАЧАТЬ ПАССИВКУ</b>", GetHeaderStyle());
            if (pm != null && allPassives.Count > 0)
            {
                for (int i = 0; i < allPassives.Count; i++)
                {
                    PassiveData p = allPassives[i];
                    if (p == null) continue;

                    int curLvl = pm.GetPassiveLevel(p);
                    string btnText = curLvl > 0 ? $"LevelUp: {p.passiveName} (Счас: Ур. {curLvl})" : $"Выдать: {p.passiveName}";

                    if (GUILayout.Button(btnText))
                    {
                        pm.AddOrUpgradePassive(p);
                        Debug.Log($"[CheatMenu] Выдана/Прокачана пассивка: {p.passiveName}");
                    }
                }
            }

            GUILayout.Space(10);

            // --- 5. УПРАВЛЕНИЕ ВРАГАМИ И СПАВНОМ ---
            WaveManager waveManager = Object.FindFirstObjectByType<WaveManager>();
            GUILayout.Label("<b>👾 УПРАВЛЕНИЕ ВРАГАМИ И ВОЛНАМИ</b>", GetHeaderStyle());

            GUILayout.BeginHorizontal();
            string labelBtnText = showEnemyLabels ? "👁️ Скрыть Имена и ХП" : "👁️ Показать Имена и ХП";
            if (GUILayout.Button(labelBtnText)) { showEnemyLabels = !showEnemyLabels; }

            if (waveManager != null)
            {
                string spawnBtnText = waveManager.enabled ? "⏸️ Остановить Спавн" : "▶️ Включить Спавн";
                if (GUILayout.Button(spawnBtnText))
                {
                    waveManager.enabled = !waveManager.enabled;
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("🧹 Убрать всех врагов"))
            {
                EnemyController[] enemies = Object.FindObjectsByType<EnemyController>(FindObjectsSortMode.None);
                foreach (var e in enemies) { Destroy(e.gameObject); }
                Debug.Log($"[CheatMenu] Удалено врагов: {enemies.Length}");
            }

            if (GUILayout.Button("💣 УБИТЬ ВСЕХ ВРАГОВ"))
            {
                EnemyController[] enemies = Object.FindObjectsByType<EnemyController>(FindObjectsSortMode.None);
                foreach (var e in enemies) { e.TakeDamage(99999f); }
                Debug.Log($"[CheatMenu] Уничтожено врагов: {enemies.Length}");
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("👹 ЗАСПАВНИТЬ БОССА"))
            {
                if (waveManager != null && waveManager.currentLevelData != null && waveManager.currentLevelData.bossData != null)
                {
                    waveManager.SpawnEnemy(waveManager.currentLevelData.bossData, true);
                    Debug.Log("[CheatMenu] Заспавнен Босс локации!");
                }
            }

            if (GUILayout.Button("☠️ УБИТЬ БОССА (ПОБЕДА)"))
            {
                EnemyController[] enemies = Object.FindObjectsByType<EnemyController>(FindObjectsSortMode.None);
                foreach (var e in enemies)
                {
                    if (e.isBoss) { e.Die(); }
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(5);
            GUILayout.Label("Заспавнить конкретного врага:");
            if (waveManager != null && allEnemies.Count > 0)
            {
                for (int i = 0; i < allEnemies.Count; i++)
                {
                    EnemyData e = allEnemies[i];
                    if (e == null) continue;

                    if (GUILayout.Button($"Заспавнить: {e.enemyName} {(e.isBoss ? "[БОСС]" : "")}"))
                    {
                        waveManager.SpawnEnemy(e, e.isBoss);
                    }
                }
            }

            GUILayout.EndScrollView();
            GUI.DragWindow();
        }

        private void DrawEnemyWorldLabels()
        {
            Camera mainCam = Camera.main;
            if (mainCam == null) return;

            EnemyController[] enemies = Object.FindObjectsByType<EnemyController>(FindObjectsSortMode.None);
            GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.alignment = TextAnchor.MiddleCenter;
            labelStyle.fontStyle = FontStyle.Bold;
            labelStyle.fontSize = 12;

            foreach (var enemy in enemies)
            {
                if (enemy == null) continue;

                Vector3 screenPos = mainCam.WorldToScreenPoint(enemy.transform.position + Vector3.up * 1.8f);
                if (screenPos.z > 0)
                {
                    float guiY = Screen.height - screenPos.y;
                    Rect rect = new Rect(screenPos.x - 75, guiY - 15, 150, 25);

                    labelStyle.normal.textColor = enemy.isBoss ? Color.magenta : Color.yellow;
                    string txt = $"{enemy.enemyData?.enemyName ?? "Враг"}\nHP: {Mathf.CeilToInt(enemy.currentHealth)}/{Mathf.CeilToInt(enemy.maxHealth)}";

                    GUI.Box(rect, "", GUI.skin.box);
                    GUI.Label(rect, txt, labelStyle);
                }
            }
        }

        private GUIStyle GetHeaderStyle()
        {
            GUIStyle header = new GUIStyle(GUI.skin.label);
            header.fontSize = 14;
            header.fontStyle = FontStyle.Bold;
            header.normal.textColor = new Color(0.2f, 0.8f, 1f);
            return header;
        }
    }
}