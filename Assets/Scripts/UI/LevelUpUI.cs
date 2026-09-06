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
        public WeaponData weaponData;
        public PassiveData passiveData;
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

        [Header("Available Data Pools (Optional)")]
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
            if (levelUpPanel != null)
            {
                levelUpPanel.SetActive(false);
            }
        }

        public void ShowLevelUpWindow()
        {
            if (levelUpPanel != null)
            {
                levelUpPanel.SetActive(true);
            }

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

            // --- 9 Weapons ---
            AddWeaponOption(pool, wm, "Паста-пушка", "Запускает пасту в ближайших туристов", Color.yellow);
            AddWeaponOption(pool, wm, "Сашими-клинки", "Клинки вращаются вокруг повара", Color.cyan);
            AddWeaponOption(pool, wm, "Острое Сальса-пятно", "AOE сальса под ногами", Color.red);
            AddWeaponOption(pool, wm, "Горячий Суп", "Взрывной навесной снаряд по площади", new Color(1f, 0.5f, 0.1f));
            AddWeaponOption(pool, wm, "Суши-сет", "Атака во все 4 стороны крестом", new Color(0.1f, 0.8f, 0.4f));
            AddWeaponOption(pool, wm, "Тако-бросок", "Рикошет по цепочке туристов", new Color(0.9f, 0.7f, 0.2f));
            AddWeaponOption(pool, wm, "Нож для Пиццы", "Пробивающий всех насквозь вращающийся диск", new Color(0.85f, 0.85f, 0.95f));
            AddWeaponOption(pool, wm, "Чайник с Кипятком", "Непрерывная струя кипятка перед собой", new Color(0.3f, 0.7f, 1f));
            AddWeaponOption(pool, wm, "Мега-Бургер", "Падение гигантского бургера сверху", new Color(0.8f, 0.45f, 0.15f));

            // --- 9 Passives ---
            AddPassiveOption(pool, pm, "Сырная броня", "+2 к Броне", Color.yellow, PassiveType.ArmorBonus, 2f);
            AddPassiveOption(pool, pm, "Васаби-ускорение", "+1.2 к Скорости перемещения", new Color(0.4f, 0.9f, 0.3f), PassiveType.SpeedBonus, 1.2f);
            AddPassiveOption(pool, pm, "Острый перчик", "+15% к Зоне поражения и +5% к Урону", Color.red, PassiveType.AreaBonus, 0.15f);
            AddPassiveOption(pool, pm, "Быстрая Подача", "-8% к Перезарядке атак", new Color(0.9f, 0.5f, 0.9f), PassiveType.CooldownReduction, 0.08f);
            AddPassiveOption(pool, pm, "Большая Порция", "+15% к Базовому урону блюд", new Color(1f, 0.4f, 0.2f), PassiveType.DamageBonus, 0.15f);
            AddPassiveOption(pool, pm, "Чаевые", "+20% к Получаемому опыту", Color.yellow, PassiveType.XpBonus, 0.20f);
            AddPassiveOption(pool, pm, "Удобная Обувь", "+1.0 к Скорости бега", Color.green, PassiveType.SpeedBonus, 1.0f);
            AddPassiveOption(pool, pm, "Свежие Ингредиенты", "+25 к Макс. HP и +1.5 HP/сек регенерация", Color.magenta, PassiveType.HealthBonus, 25f);
            AddPassiveOption(pool, pm, "Магнитный Фартук", "+1.5 к Радиусу сбора чаевых", Color.cyan, PassiveType.MagnetBonus, 1.5f);

            // Select 3 unique random options
            for (int i = 0; i < 3 && pool.Count > 0; i++)
            {
                int randomIndex = Random.Range(0, pool.Count);
                currentOptions.Add(pool[randomIndex]);
                pool.RemoveAt(randomIndex);
            }
        }

        private void AddWeaponOption(List<UpgradeOption> pool, WeaponManager wm, string weaponName, string description, Color color)
        {
            int lvl = 1;
            if (wm != null)
            {
                var existing = wm.activeWeapons.Find(w => w.weaponData != null && w.weaponData.weaponName == weaponName);
                if (existing != null) lvl = existing.currentLevel + 1;
            }

            pool.Add(new UpgradeOption
            {
                title = $"{weaponName} (Ур. {lvl})",
                description = description,
                typeLabel = "[Блюдо]",
                optionColor = color,
                onSelect = () =>
                {
                    if (wm != null)
                    {
                        WeaponData data = ScriptableObject.CreateInstance<WeaponData>();
                        data.weaponName = weaponName;
                        data.weaponColor = color;
                        wm.AddWeapon(data);
                    }
                }
            });
        }

        private void AddPassiveOption(List<UpgradeOption> pool, PassiveManager pm, string passiveName, string description, Color color, PassiveType type, float value)
        {
            int lvl = 1;
            if (pm != null)
            {
                var existing = pm.activePassives.Find(p => p != null && p.passiveName == passiveName);
                if (existing != null && pm.passiveLevels.ContainsKey(existing)) lvl = pm.passiveLevels[existing] + 1;
            }

            pool.Add(new UpgradeOption
            {
                title = $"{passiveName} (Ур. {lvl})",
                description = description,
                typeLabel = "[Пассивка]",
                optionColor = color,
                onSelect = () =>
                {
                    if (pm != null)
                    {
                        PassiveData data = ScriptableObject.CreateInstance<PassiveData>();
                        data.passiveName = passiveName;
                        data.passiveType = type;
                        data.passiveColor = color;
                        pm.AddOrUpgradePassive(data);
                    }
                }
            });
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

                    int optionIndex = i;
                    cardButtons[i].onClick.RemoveAllListeners();
                    cardButtons[i].onClick.AddListener(() => SelectOption(optionIndex));
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
                Debug.Log($"[LevelUpUI] Selected Option: {currentOptions[index].title}");
                currentOptions[index].onSelect?.Invoke();
            }

            CloseLevelUpWindow();
        }

        public void CloseLevelUpWindow()
        {
            if (levelUpPanel != null)
            {
                levelUpPanel.SetActive(false);
            }

            Time.timeScale = 1f;
        }
    }
}
