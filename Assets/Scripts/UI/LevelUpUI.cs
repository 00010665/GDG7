using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using FoodSurvivors.Core;
using FoodSurvivors.Data;
using FoodSurvivors.Player;
using FoodSurvivors.Weapons;

namespace FoodSurvivors.UI
{
    public struct UpgradeOption
    {
        public string title;
        public string description;
        public string typeLabel;
        public Color optionColor;
        public Sprite icon;
        public System.Action onSelect;
    }

    public class LevelUpUI : MonoBehaviour
    {
        public static LevelUpUI Instance { get; private set; }
        public static bool IsRewardChoiceOpen { get; private set; }

        private static int dashInputBlockedThroughFrame = -1;

        public static bool BlocksPlayerDashInput =>
            IsRewardChoiceOpen || Time.frameCount <= dashInputBlockedThroughFrame;

        [Header("UI Panels")]
        public GameObject levelUpPanel;

        [Header("Cards UI")]
        public Button[] cardButtons;
        public TextMeshProUGUI[] cardTitleTexts;
        public TextMeshProUGUI[] cardDescTexts;
        public TextMeshProUGUI[] cardTypeTexts;
        public Image[] cardColorPreviews;

        [Header("Real Assets Pool")]
        public List<WeaponData> weaponPool = new List<WeaponData>();
        public List<PassiveData> passivePool = new List<PassiveData>();

        private List<UpgradeOption> currentOptions = new List<UpgradeOption>();
        private int selectedOptionIndex;
        private int hoveredOptionIndex = -1;
        private bool keyboardFocusActive;
        private Color[] defaultTitleColors;
        private Color[] defaultDescColors;
        private Color[] defaultTypeColors;
        private static readonly Color FocusTextColor = new Color(1f, 0.78f, 0.24f, 1f);

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance != this) return;

            Instance = null;
            IsRewardChoiceOpen = false;
            dashInputBlockedThroughFrame = Time.frameCount + 1;
        }

        private void Start()
        {
            if (levelUpPanel != null) levelUpPanel.SetActive(false);
            CacheDefaultTextColors();
            LoadRealAssets();
        }

        private void Update()
        {
            if (levelUpPanel == null || !levelUpPanel.activeInHierarchy || currentOptions.Count == 0 || Keyboard.current == null)
                return;

            if (Keyboard.current.aKey.wasPressedThisFrame)
                MoveSelection(-1);
            else if (Keyboard.current.dKey.wasPressedThisFrame)
                MoveSelection(1);

            if (Keyboard.current.spaceKey.wasPressedThisFrame)
                SelectOption(selectedOptionIndex);
        }

        private void CacheDefaultTextColors()
        {
            defaultTitleColors = CaptureColors(cardTitleTexts);
            defaultDescColors = CaptureColors(cardDescTexts);
            defaultTypeColors = CaptureColors(cardTypeTexts);
        }

        private static Color[] CaptureColors(TextMeshProUGUI[] texts)
        {
            if (texts == null) return System.Array.Empty<Color>();
            Color[] colors = new Color[texts.Length];
            for (int i = 0; i < texts.Length; i++)
                colors[i] = texts[i] != null ? texts[i].color : Color.white;
            return colors;
        }

        private void MoveSelection(int direction)
        {
            int optionCount = Mathf.Min(currentOptions.Count, cardButtons != null ? cardButtons.Length : 0);
            if (optionCount == 0) return;

            selectedOptionIndex = (selectedOptionIndex + direction + optionCount) % optionCount;
            hoveredOptionIndex = -1;
            keyboardFocusActive = true;
            RefreshCardTextColors();
        }

        private void SetHoveredOption(int index, bool isHovered)
        {
            if (isHovered)
            {
                hoveredOptionIndex = index;
                selectedOptionIndex = index;
                keyboardFocusActive = false;
            }
            else if (hoveredOptionIndex == index)
            {
                hoveredOptionIndex = -1;
            }

            RefreshCardTextColors();
        }

        private void RefreshCardTextColors()
        {
            for (int i = 0; i < currentOptions.Count; i++)
            {
                bool highlighted = i == hoveredOptionIndex || (keyboardFocusActive && i == selectedOptionIndex);
                SetTextColor(cardTitleTexts, defaultTitleColors, i, highlighted);
                SetTextColor(cardDescTexts, defaultDescColors, i, highlighted);
                SetTextColor(cardTypeTexts, defaultTypeColors, i, highlighted);
            }
        }

        private static void SetTextColor(TextMeshProUGUI[] texts, Color[] defaults, int index, bool highlighted)
        {
            if (texts == null || index >= texts.Length || texts[index] == null) return;
            Color normalColor = defaults != null && index < defaults.Length ? defaults[index] : Color.white;
            texts[index].color = highlighted ? FocusTextColor : normalColor;
        }

        private void ConfigurePointerHighlight(Button button, int index)
        {
            EventTrigger trigger = button.GetComponent<EventTrigger>();
            if (trigger == null) trigger = button.gameObject.AddComponent<EventTrigger>();
            if (trigger.triggers == null) trigger.triggers = new List<EventTrigger.Entry>();
            trigger.triggers.Clear();

            EventTrigger.Entry enter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
            enter.callback.AddListener(_ => SetHoveredOption(index, true));
            trigger.triggers.Add(enter);

            EventTrigger.Entry exit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
            exit.callback.AddListener(_ => SetHoveredOption(index, false));
            trigger.triggers.Add(exit);
        }

        private void LoadRealAssets()
        {
            weaponPool.Clear();
            passivePool.Clear();

            LevelUpContentCatalog catalog = Resources.Load<LevelUpContentCatalog>("LevelUpContentCatalog");
            if (catalog != null)
            {
                AddUniqueAssets(weaponPool, catalog.weapons);
                AddUniqueAssets(passivePool, catalog.passives);
            }

#if UNITY_EDITOR
            string[] wGuids = UnityEditor.AssetDatabase.FindAssets("t:WeaponData", new[] { "Assets/ScriptableObjects/Weapons" });
            foreach (var g in wGuids)
            {
                var w = UnityEditor.AssetDatabase.LoadAssetAtPath<WeaponData>(UnityEditor.AssetDatabase.GUIDToAssetPath(g));
                if (w != null && !weaponPool.Contains(w)) weaponPool.Add(w);
            }

            string[] pGuids = UnityEditor.AssetDatabase.FindAssets("t:PassiveData", new[] { "Assets/ScriptableObjects/Passives" });
            foreach (var g in pGuids)
            {
                var p = UnityEditor.AssetDatabase.LoadAssetAtPath<PassiveData>(UnityEditor.AssetDatabase.GUIDToAssetPath(g));
                if (p != null && !passivePool.Contains(p)) passivePool.Add(p);
            }
#endif

            // Also include any assets placed under Resources for projects that extend the catalog at runtime.
            AddUniqueAssets(weaponPool, Resources.LoadAll<WeaponData>("Weapons"));
            AddUniqueAssets(passivePool, Resources.LoadAll<PassiveData>("Passives"));

            if (weaponPool.Count == 0 || passivePool.Count == 0)
                Debug.LogError($"[LevelUpUI] Reward assets unavailable. Weapons: {weaponPool.Count}, passives: {passivePool.Count}. Ensure LevelUpContentCatalog is present under Assets/Resources.");
        }

        private static void AddUniqueAssets<T>(List<T> destination, IEnumerable<T> assets) where T : Object
        {
            if (destination == null || assets == null) return;
            foreach (T asset in assets)
            {
                if (asset != null && !destination.Contains(asset))
                    destination.Add(asset);
            }
        }

        public void ShowLevelUpWindow()
        {
            if (weaponPool.Count == 0 || passivePool.Count == 0) LoadRealAssets();

            GenerateUpgradeOptions();
            if (currentOptions.Count == 0)
            {
                Debug.LogError("[LevelUpUI] No available reward options; keeping gameplay running instead of freezing on an empty reward screen.");
                return;
            }

            IsRewardChoiceOpen = true;
            if (levelUpPanel != null) levelUpPanel.SetActive(true);
            Time.timeScale = 0f;

            selectedOptionIndex = 0;
            hoveredOptionIndex = -1;
            keyboardFocusActive = true;
            DisplayOptions();
            RefreshCardTextColors();
        }

        private void GenerateUpgradeOptions()
        {
            currentOptions.Clear();

            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            WeaponManager wm = playerObj != null ? playerObj.GetComponent<WeaponManager>() : null;
            PassiveManager pm = playerObj != null ? playerObj.GetComponent<PassiveManager>() : null;

            List<UpgradeOption> pool = new List<UpgradeOption>();

            // --- ДОБАВЛЯЕМ ТОЛЬКО НАСТОЯЩИЕ БЛЮДА С ПРЕФАБАМИ! ---
            foreach (var wData in weaponPool)
            {
                if (wData == null || !wData.isEnabledInRoster) continue;

                int curLvl = wm != null ? wm.GetWeaponLevel(wData) : 0;
                int maxLvl = wData.levels != null ? wData.levels.Count : 5;
                if (curLvl >= maxLvl) continue;

                WeaponData capturedWeapon = wData;
                pool.Add(new UpgradeOption
                {
                    title = $"{capturedWeapon.weaponName} (Ур. {curLvl + 1}/{maxLvl})",
                    description = capturedWeapon.description,
                    typeLabel = "[Блюдо]",
                    optionColor = capturedWeapon.weaponColor,
                    icon = FoodIconLibrary.Get(capturedWeapon.name),
                    onSelect = () =>
                    {
                        if (wm != null)
                        {
                            // ПЕРЕДАЕМ НАСТОЯЩИЙ АССЕТ С ПРЕФАБОМ!
                            wm.AddWeapon(capturedWeapon);
                        }
                    }
                });
            }

            // --- ДОБАВЛЯЕМ ТОЛЬКО НАСТОЯЩИЕ ПАССИВКИ ---
            foreach (var pData in passivePool)
            {
                if (pData == null || !pData.isEnabledInRoster) continue;

                int curLvl = pm != null ? pm.GetPassiveLevel(pData) : 0;
                int maxLvl = pData.levels != null ? pData.levels.Count : 5;
                if (curLvl >= maxLvl) continue;

                PassiveData capturedPassive = pData;
                pool.Add(new UpgradeOption
                {
                    title = $"{capturedPassive.passiveName} (Ур. {curLvl + 1}/{maxLvl})",
                    description = capturedPassive.description,
                    typeLabel = "[Талант]",
                    optionColor = capturedPassive.passiveColor,
                    icon = FoodIconLibrary.Get(capturedPassive.name),
                    onSelect = () =>
                    {
                        if (pm != null)
                        {
                            pm.AddOrUpgradePassive(capturedPassive);
                        }
                    }
                });
            }

            // Выбираем 3 уникальные случайные карточки
            for (int i = 0; i < 3 && pool.Count > 0; i++)
            {
                int rIdx = Random.Range(0, pool.Count);
                currentOptions.Add(pool[rIdx]);
                pool.RemoveAt(rIdx);
            }
        }

        private void DisplayOptions()
        {
            if (cardButtons == null) return;

            for (int i = 0; i < cardButtons.Length; i++)
            {
                if (cardButtons[i] == null) continue;

                if (i < currentOptions.Count)
                {
                    cardButtons[i].gameObject.SetActive(true);
                    UpgradeOption opt = currentOptions[i];

                    if (cardTitleTexts != null && i < cardTitleTexts.Length && cardTitleTexts[i] != null)
                        cardTitleTexts[i].text = opt.title;

                    if (cardDescTexts != null && i < cardDescTexts.Length && cardDescTexts[i] != null)
                        cardDescTexts[i].text = opt.description;

                    if (cardTypeTexts != null && i < cardTypeTexts.Length && cardTypeTexts[i] != null)
                        cardTypeTexts[i].text = opt.typeLabel;

                    if (cardColorPreviews != null && i < cardColorPreviews.Length && cardColorPreviews[i] != null)
                    {
                        cardColorPreviews[i].sprite = opt.icon;
                        cardColorPreviews[i].preserveAspect = true;
                        cardColorPreviews[i].color = opt.icon != null ? Color.white : opt.optionColor;
                    }

                    int idx = i;
                    cardButtons[i].onClick.RemoveAllListeners();
                    cardButtons[i].onClick.AddListener(() => SelectOption(idx));
                    ConfigurePointerHighlight(cardButtons[i], idx);
                }
                else
                {
                    cardButtons[i].gameObject.SetActive(false);
                }
            }
        }

        public void SelectOption(int index)
        {
            if (index >= 0 && index < currentOptions.Count)
            {
                currentOptions[index].onSelect?.Invoke();
            }
            CloseLevelUpWindow();
        }

        public void CloseLevelUpWindow()
        {
            // Keep the confirmation key from also triggering a dash later in this frame.
            IsRewardChoiceOpen = false;
            dashInputBlockedThroughFrame = Time.frameCount + 1;
            if (levelUpPanel != null) levelUpPanel.SetActive(false);
            Time.timeScale = 1f;
        }
    }
}
