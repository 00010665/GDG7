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
            PlayerController pc = playerObj != null ? playerObj.GetComponent<PlayerController>() : null;

            List<UpgradeOption> pool = new List<UpgradeOption>();

            pool.Add(new UpgradeOption
            {
                title = "🍝 Паста-пушка",
                description = "Запускает пасту в ближайших туристов",
                typeLabel = "[Блюдо]",
                optionColor = Color.yellow,
                onSelect = () => {
                    if (wm != null)
                    {
                        WeaponData data = ScriptableObject.CreateInstance<WeaponData>();
                        data.weaponName = "Паста-пушка";
                        data.weaponColor = Color.yellow;
                        wm.AddWeapon(data);
                    }
                }
            });

            pool.Add(new UpgradeOption
            {
                title = "🍣 Сашими-клинки",
                description = "Клинки вращаются вокруг повара",
                typeLabel = "[Блюдо]",
                optionColor = Color.cyan,
                onSelect = () => {
                    if (wm != null)
                    {
                        WeaponData data = ScriptableObject.CreateInstance<WeaponData>();
                        data.weaponName = "Сашими-клинки";
                        data.weaponColor = Color.cyan;
                        wm.AddWeapon(data);
                    }
                }
            });

            pool.Add(new UpgradeOption
            {
                title = "🌶️ Острое Сальса-пятно",
                description = "AOE сальса под ногами",
                typeLabel = "[Блюдо]",
                optionColor = Color.red,
                onSelect = () => {
                    if (wm != null)
                    {
                        WeaponData data = ScriptableObject.CreateInstance<WeaponData>();
                        data.weaponName = "Острое Сальса-пятно";
                        data.weaponColor = Color.red;
                        wm.AddWeapon(data);
                    }
                }
            });

            pool.Add(new UpgradeOption
            {
                title = "👟 Удобная Обувь",
                description = "+1.5 к Скорости перемещения",
                typeLabel = "[Пассивка]",
                optionColor = Color.green,
                onSelect = () => {
                    if (pc != null) pc.currentMoveSpeed += 1.5f;
                }
            });

            pool.Add(new UpgradeOption
            {
                title = "🧀 Сырная броня",
                description = "+2 к Броне",
                typeLabel = "[Пассивка]",
                optionColor = Color.yellow,
                onSelect = () => {
                    if (pc != null) pc.currentArmor += 2f;
                }
            });

            pool.Add(new UpgradeOption
            {
                title = "🥗 Свежие Ингредиенты",
                description = "+25 к Макс ХП и полное исцеление",
                typeLabel = "[Пассивка]",
                optionColor = Color.magenta,
                onSelect = () => {
                    if (pc != null)
                    {
                        pc.maxHealth += 25f;
                        pc.Heal(1000f);
                    }
                }
            });

            for (int i = 0; i < 3 && pool.Count > 0; i++)
            {
                int randomIndex = Random.Range(0, pool.Count);
                currentOptions.Add(pool[randomIndex]);
                pool.RemoveAt(randomIndex);
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
