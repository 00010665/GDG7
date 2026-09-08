using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FoodSurvivors.Core;
using FoodSurvivors.Player;
using FoodSurvivors.Weapons;
using FoodSurvivors.Data;

namespace FoodSurvivors.UI
{
    public class GameHUD : MonoBehaviour
    {
        [Header("XP & Level HUD")]
        public Slider xpSlider;
        public TextMeshProUGUI levelText;

        [Header("HP HUD")]
        public Slider hpSlider;
        public TextMeshProUGUI hpText;

        [Header("Timer HUD")]
        public TextMeshProUGUI timerText;

        [Header("Item Slots Containers")]
        public Transform weaponIconsContainer;
        public Transform passiveIconsContainer;

        private PlayerController playerController;
        private WaveManager waveManager;

        // Кэш созданных UI слотов
        private List<GameObject> weaponSlotUIList = new List<GameObject>();
        private List<GameObject> passiveSlotUIList = new List<GameObject>();

        private void Start()
        {
            EnsureContainersExist();
            FindReferences();
        }

        private void EnsureContainersExist()
        {
            Transform parentCanvas = transform.parent != null ? transform.parent : transform;

            // Контейнер для блюд (Верхний левый угол под XP)
            if (weaponIconsContainer == null)
            {
                GameObject wContObj = new GameObject("WeaponSlotsContainer", typeof(RectTransform));
                wContObj.transform.SetParent(parentCanvas, false);
                RectTransform rt = wContObj.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.02f, 0.88f);
                rt.anchorMax = new Vector2(0.02f, 0.88f);
                rt.pivot = new Vector2(0f, 1f);
                rt.anchoredPosition = Vector2.zero;
                rt.sizeDelta = new Vector2(500f, 32f);
                weaponIconsContainer = wContObj.transform;
            }

            // Контейнер для пассивок (Чуть ниже блюд)
            if (passiveIconsContainer == null)
            {
                GameObject pContObj = new GameObject("PassiveSlotsContainer", typeof(RectTransform));
                pContObj.transform.SetParent(parentCanvas, false);
                RectTransform rt = pContObj.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.02f, 0.82f);
                rt.anchorMax = new Vector2(0.02f, 0.82f);
                rt.pivot = new Vector2(0f, 1f);
                rt.anchoredPosition = Vector2.zero;
                rt.sizeDelta = new Vector2(500f, 32f);
                passiveIconsContainer = pContObj.transform;
            }
        }

        private void FindReferences()
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerController = playerObj.GetComponent<PlayerController>();
            }

            waveManager = Object.FindFirstObjectByType<WaveManager>();
        }

        private void Update()
        {
            if (playerController == null || waveManager == null)
            {
                FindReferences();
            }

            UpdateXPHUD();
            UpdateHPHUD();
            UpdateTimerHUD();
            UpdateEquippedItemsHUD();
        }

        private void UpdateXPHUD()
        {
            if (ExperienceManager.Instance == null) return;

            float curXP = ExperienceManager.Instance.currentXP;
            float maxXP = ExperienceManager.Instance.xpToNextLevel;
            int level = ExperienceManager.Instance.currentLevel;

            if (xpSlider != null)
            {
                xpSlider.maxValue = maxXP;
                xpSlider.value = curXP;
            }

            if (levelText != null)
            {
                levelText.text = $"УР. {level}";
            }
        }

        private void UpdateHPHUD()
        {
            if (playerController == null) return;

            float curHP = playerController.currentHealth;
            float maxHP = playerController.maxHealth;

            if (hpSlider != null)
            {
                hpSlider.maxValue = maxHP;
                hpSlider.value = curHP;
            }

            if (hpText != null)
            {
                hpText.text = $"HP: {Mathf.CeilToInt(curHP)} / {Mathf.CeilToInt(maxHP)}";
            }
        }

        private void UpdateTimerHUD()
        {
            float elapsedSeconds = waveManager != null ? waveManager.gameTimer : Time.timeSinceLevelLoad;
            int minutes = Mathf.FloorToInt(elapsedSeconds / 60f);
            int seconds = Mathf.FloorToInt(elapsedSeconds % 60f);

            if (timerText != null)
            {
                timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
            }
        }

        private void UpdateEquippedItemsHUD()
        {
            if (playerController == null) return;

            WeaponManager wm = playerController.GetComponent<WeaponManager>();
            PassiveManager pm = playerController.GetComponent<PassiveManager>();

            // Обновление иконок Блюд
            if (wm != null && weaponIconsContainer != null)
            {
                UpdateWeaponsSlots(wm.activeWeapons);
            }

            // Обновление иконок Пассивок
            if (pm != null && passiveIconsContainer != null)
            {
                UpdatePassivesSlots(pm);
            }
        }

        private void UpdateWeaponsSlots(List<WeaponBase> weapons)
        {
            if (weapons == null) return;

            // Очищаем лишние слоты при уменьшении количества
            while (weaponSlotUIList.Count > weapons.Count)
            {
                int last = weaponSlotUIList.Count - 1;
                Destroy(weaponSlotUIList[last]);
                weaponSlotUIList.RemoveAt(last);
            }

            // Создаем или обновляем слоты
            for (int i = 0; i < weapons.Count; i++)
            {
                WeaponBase w = weapons[i];
                if (w == null || w.weaponData == null) continue;

                if (i >= weaponSlotUIList.Count)
                {
                    GameObject slotObj = CreateItemSlotUI($"WeaponSlot_{i}", weaponIconsContainer, i);
                    weaponSlotUIList.Add(slotObj);
                }

                GameObject slot = weaponSlotUIList[i];
                int maxLvl = w.weaponData.levels != null ? w.weaponData.levels.Count : 5;
                SetSlotData(slot, w.weaponData.weaponColor, $"{w.weaponData.weaponName}\n<b>{w.currentLevel}/{maxLvl}</b>");
            }
        }

        private void UpdatePassivesSlots(PassiveManager pm)
        {
            if (pm == null || pm.activePassives == null) return;

            while (passiveSlotUIList.Count > pm.activePassives.Count)
            {
                int last = passiveSlotUIList.Count - 1;
                Destroy(passiveSlotUIList[last]);
                passiveSlotUIList.RemoveAt(last);
            }

            for (int i = 0; i < pm.activePassives.Count; i++)
            {
                PassiveData p = pm.activePassives[i];
                if (p == null) continue;

                if (i >= passiveSlotUIList.Count)
                {
                    GameObject slotObj = CreateItemSlotUI($"PassiveSlot_{i}", passiveIconsContainer, i);
                    passiveSlotUIList.Add(slotObj);
                }

                GameObject slot = passiveSlotUIList[i];
                int curLvl = pm.GetPassiveLevel(p);
                int maxLvl = p.levels != null ? p.levels.Count : 5;
                SetSlotData(slot, p.passiveColor, $"{p.passiveName}\n<b>{curLvl}/{maxLvl}</b>");
            }
        }

        private GameObject CreateItemSlotUI(string name, Transform parent, int index)
        {
            GameObject slotObj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            slotObj.transform.SetParent(parent, false);

            RectTransform rt = slotObj.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 0.5f);
            rt.anchorMax = new Vector2(0f, 0.5f);
            rt.pivot = new Vector2(0f, 0.5f);
            rt.anchoredPosition = new Vector2(index * 115f, 0f);
            rt.sizeDelta = new Vector2(110f, 28f);

            Image bg = slotObj.GetComponent<Image>();
            bg.color = new Color(0.12f, 0.12f, 0.18f, 0.85f);

            // Квадратный цветовой индикатор предмета
            GameObject colorBox = new GameObject("ColorPreview", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            colorBox.transform.SetParent(slotObj.transform, false);
            RectTransform boxRt = colorBox.GetComponent<RectTransform>();
            boxRt.anchorMin = new Vector2(0.05f, 0.5f);
            boxRt.anchorMax = new Vector2(0.05f, 0.5f);
            boxRt.pivot = new Vector2(0f, 0.5f);
            boxRt.anchoredPosition = new Vector2(3f, 0f);
            boxRt.sizeDelta = new Vector2(20f, 20f);

            // Текст названия и уровня
            GameObject txtObj = new GameObject("SlotText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            txtObj.transform.SetParent(slotObj.transform, false);
            RectTransform txtRt = txtObj.GetComponent<RectTransform>();
            txtRt.anchorMin = new Vector2(0.28f, 0f);
            txtRt.anchorMax = new Vector2(1f, 1f);
            txtRt.sizeDelta = Vector2.zero;

            TextMeshProUGUI tmp = txtObj.GetComponent<TextMeshProUGUI>();
            tmp.fontSize = 11f;
            tmp.alignment = TextAlignmentOptions.Left;
            tmp.color = Color.white;

            return slotObj;
        }

        private void SetSlotData(GameObject slotObj, Color itemColor, string labelText)
        {
            if (slotObj == null) return;

            Transform colorBox = slotObj.transform.Find("ColorPreview");
            if (colorBox != null)
            {
                Image img = colorBox.GetComponent<Image>();
                if (img != null) img.color = itemColor;
            }

            Transform txtObj = slotObj.transform.Find("SlotText");
            if (txtObj != null)
            {
                TextMeshProUGUI tmp = txtObj.GetComponent<TextMeshProUGUI>();
                if (tmp != null) tmp.text = labelText;
            }
        }
    }
}