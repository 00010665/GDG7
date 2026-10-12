using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
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
        public System.Action onSelect;
    }

    public class LevelUpUI : MonoBehaviour
    {
        public static LevelUpUI Instance { get; private set; }

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

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            if (levelUpPanel != null) levelUpPanel.SetActive(false);
            LoadRealAssets();
        }

        private void LoadRealAssets()
        {
            weaponPool.Clear();
            passivePool.Clear();

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
            // Резерв через Resources
            if (weaponPool.Count == 0)
            {
                var loadedW = Resources.LoadAll<WeaponData>("Weapons");
                weaponPool.AddRange(loadedW);
            }
            if (passivePool.Count == 0)
            {
                var loadedP = Resources.LoadAll<PassiveData>("Passives");
                passivePool.AddRange(loadedP);
            }
        }

        public void ShowLevelUpWindow()
        {
            if (weaponPool.Count == 0 || passivePool.Count == 0) LoadRealAssets();

            if (levelUpPanel != null) levelUpPanel.SetActive(true);
            Time.timeScale = 0f;

            GenerateUpgradeOptions();
            DisplayOptions();
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
                        cardColorPreviews[i].color = opt.optionColor;

                    int idx = i;
                    cardButtons[i].onClick.RemoveAllListeners();
                    cardButtons[i].onClick.AddListener(() => SelectOption(idx));
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
            if (levelUpPanel != null) levelUpPanel.SetActive(false);
            Time.timeScale = 1f;
        }
    }
}