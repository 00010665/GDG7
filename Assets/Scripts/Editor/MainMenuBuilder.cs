using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using TMPro;
using FoodSurvivors.Core;
using FoodSurvivors.UI;
using FoodSurvivors.Data;

namespace FoodSurvivors.EditorTools
{
    public static class MainMenuBuilder
    {
        [MenuItem("FoodSurvivors/Build Main Menu Scene")]
        public static void BuildMainMenuScene()
        {
            // Create new empty scene
            var newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 0. Main Camera (orthographic, MainCamera tag, fixed transform)
            GameObject camObj = new GameObject("Main Camera");
            camObj.tag = "MainCamera";
            Camera cam = camObj.AddComponent<Camera>();
            camObj.AddComponent<AudioListener>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.1f, 0.1f, 0.15f, 1f);
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            camObj.transform.position = new Vector3(0f, 0f, -10f);
            camObj.transform.rotation = Quaternion.identity;
            camObj.transform.localScale = Vector3.one;

            // 1. GameManager GameObject
            GameObject gmObj = new GameObject("[GameManager]");
            var gmComponent = gmObj.AddComponent<GameManager>();

            GameObject audioObj = new GameObject("[AudioManager]");
            audioObj.AddComponent<AudioManager>();

            // Load all Chefs and Levels into GameManager
            string[] chefGuids = AssetDatabase.FindAssets("t:ChefData", new[] { "Assets/ScriptableObjects/Chefs" });
            foreach (var g in chefGuids)
            {
                var c = AssetDatabase.LoadAssetAtPath<ChefData>(AssetDatabase.GUIDToAssetPath(g));
                if (c != null && !gmComponent.availableChefs.Contains(c)) gmComponent.availableChefs.Add(c);
            }

            string[] levelGuids = AssetDatabase.FindAssets("t:LevelData", new[] { "Assets/ScriptableObjects/Levels" });
            foreach (var g in levelGuids)
            {
                var l = AssetDatabase.LoadAssetAtPath<LevelData>(AssetDatabase.GUIDToAssetPath(g));
                if (l != null && !gmComponent.availableLevels.Contains(l)) gmComponent.availableLevels.Add(l);
            }

            if (gmComponent.availableChefs.Count > 0) gmComponent.selectedChef = gmComponent.availableChefs[0];
            if (gmComponent.availableLevels.Count > 0) gmComponent.selectedLevel = gmComponent.availableLevels[0];

            // 2. Canvas & EventSystem
            GameObject canvasObj = new GameObject("Canvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();

            GameObject esObj = Object.FindFirstObjectByType<EventSystem>()?.gameObject;
            if (esObj == null)
            {
                esObj = new GameObject("EventSystem");
                esObj.AddComponent<EventSystem>();
                esObj.AddComponent<InputSystemUIInputModule>();
            }
            else
            {
                if (esObj.GetComponent<InputSystemUIInputModule>() == null)
                {
                    esObj.AddComponent<InputSystemUIInputModule>();
                }
            }

            // 3. MainMenuUI Manager Component
            GameObject uiManagerObj = new GameObject("MainMenuManager");
            uiManagerObj.transform.SetParent(canvasObj.transform, false);
            MainMenuUI menuUI = uiManagerObj.AddComponent<MainMenuUI>();

            // Create Main Menu Panel
            GameObject mainMenuPanel = CreatePanel("MainMenuPanel", canvasObj.transform, new Color(0.1f, 0.1f, 0.15f, 1f));
            CreateText("Title", "FOOD SURVIVORS", mainMenuPanel.transform, new Vector2(0.5f, 0.8f), new Vector2(0.5f, 0.8f), Vector2.zero, new Vector2(600, 100), 42f);

            CreateButton("BtnChef", "Выбор Шеф-повара", mainMenuPanel.transform, new Vector2(0, 80), new Vector2(300, 50), () => menuUI.ShowChefSelection());
            CreateButton("BtnLevel", "Выбор Уровня", mainMenuPanel.transform, new Vector2(0, 10), new Vector2(300, 50), () => menuUI.ShowLevelSelection());
            CreateButton("BtnStart", "В БОЙ!", mainMenuPanel.transform, new Vector2(0, -60), new Vector2(300, 60), () => menuUI.OnStartGameButtonClicked());
            CreateButton("BtnQuit", "Выход", mainMenuPanel.transform, new Vector2(0, -140), new Vector2(300, 40), () => menuUI.OnClickQuitGame());

            // Create Chef Select Panel
            GameObject chefPanel = CreatePanel("ChefSelectPanel", canvasObj.transform, new Color(0.15f, 0.1f, 0.1f, 1f));
            CreateText("ChefHeader", "Выбор Шеф-повара", chefPanel.transform, new Vector2(0.5f, 0.9f), new Vector2(0.5f, 0.9f), Vector2.zero, new Vector2(500, 60), 32f);

            menuUI.chefNameText = CreateText("ChefName", "Имя Повара", chefPanel.transform, new Vector2(0.5f, 0.8f), new Vector2(0.5f, 0.8f), Vector2.zero, new Vector2(400, 40), 28f);
            menuUI.chefDescriptionText = CreateText("ChefDesc", "Описание повара...", chefPanel.transform, new Vector2(0.5f, 0.72f), new Vector2(0.5f, 0.72f), Vector2.zero, new Vector2(500, 40), 20f);

            GameObject previewObj = new GameObject("ChefPreview", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            previewObj.transform.SetParent(chefPanel.transform, false);
            RectTransform prvRt = previewObj.GetComponent<RectTransform>();
            prvRt.anchoredPosition = new Vector2(0, 80);
            prvRt.sizeDelta = new Vector2(100, 100);
            menuUI.chefColorPreview = previewObj.GetComponent<Image>();
            menuUI.chefColorPreview.color = Color.yellow;

            menuUI.chefHpText = CreateText("ChefHp", "ХП: 100", chefPanel.transform, new Vector2(0.5f, 0.45f), new Vector2(0.5f, 0.45f), Vector2.zero, new Vector2(300, 30), 22f);
            menuUI.chefSpeedText = CreateText("ChefSpeed", "Скорость: 5", chefPanel.transform, new Vector2(0.5f, 0.40f), new Vector2(0.5f, 0.40f), Vector2.zero, new Vector2(300, 30), 22f);
            menuUI.chefArmorText = CreateText("ChefArmor", "Броня: 0", chefPanel.transform, new Vector2(0.5f, 0.35f), new Vector2(0.5f, 0.35f), Vector2.zero, new Vector2(300, 30), 22f);
            menuUI.chefWeaponText = CreateText("ChefWeapon", "Оружие: Паста-пушка", chefPanel.transform, new Vector2(0.5f, 0.30f), new Vector2(0.5f, 0.30f), Vector2.zero, new Vector2(400, 30), 22f);
            menuUI.chefPassiveText = CreateText("ChefPassive", "Пассивка: Сырная броня", chefPanel.transform, new Vector2(0.5f, 0.25f), new Vector2(0.5f, 0.25f), Vector2.zero, new Vector2(400, 30), 22f);

            CreateButton("BtnPrevChef", "< Пред.", chefPanel.transform, new Vector2(-200, 80), new Vector2(100, 50), () => menuUI.PreviousChef());
            CreateButton("BtnNextChef", "След. >", chefPanel.transform, new Vector2(200, 80), new Vector2(100, 50), () => menuUI.NextChef());
            CreateButton("BtnBackChef", "Назад в Меню", chefPanel.transform, new Vector2(0, -200), new Vector2(250, 50), () => menuUI.ShowMainMenu());

            // Create Level Select Panel
            GameObject levelPanel = CreatePanel("LevelSelectPanel", canvasObj.transform, new Color(0.1f, 0.15f, 0.1f, 1f));
            CreateText("LevelHeader", "Выбор Уровня", levelPanel.transform, new Vector2(0.5f, 0.9f), new Vector2(0.5f, 0.9f), Vector2.zero, new Vector2(500, 60), 32f);

            menuUI.levelNameText = CreateText("LevelName", "Название Уровня", levelPanel.transform, new Vector2(0.5f, 0.75f), new Vector2(0.5f, 0.75f), Vector2.zero, new Vector2(400, 50), 28f);
            menuUI.levelDescriptionText = CreateText("LevelDesc", "Описание уровня...", levelPanel.transform, new Vector2(0.5f, 0.60f), new Vector2(0.5f, 0.60f), Vector2.zero, new Vector2(500, 60), 20f);
            menuUI.levelDurationText = CreateText("LevelDuration", "Длительность: 5 мин", levelPanel.transform, new Vector2(0.5f, 0.45f), new Vector2(0.5f, 0.45f), Vector2.zero, new Vector2(400, 40), 22f);

            CreateButton("BtnPrevLevel", "< Пред.", levelPanel.transform, new Vector2(-200, 80), new Vector2(100, 50), () => menuUI.PreviousLevel());
            CreateButton("BtnNextLevel", "След. >", levelPanel.transform, new Vector2(200, 80), new Vector2(100, 50), () => menuUI.NextLevel());
            CreateButton("BtnBackLevel", "Назад в Меню", levelPanel.transform, new Vector2(0, -200), new Vector2(250, 50), () => menuUI.ShowMainMenu());

            // Link Panels to Menu UI
            menuUI.mainMenuPanel = mainMenuPanel;
            menuUI.chefSelectPanel = chefPanel;
            menuUI.levelSelectPanel = levelPanel;

            // Ensure Scenes directory exists
            if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
            {
                AssetDatabase.CreateFolder("Assets", "Scenes");
            }

            // Save scene
            string scenePath = "Assets/Scenes/MainMenuScene.unity";
            EditorSceneManager.SaveScene(newScene, scenePath);

            // Configure Build Settings
            var scenes = new EditorBuildSettingsScene[] {
                new EditorBuildSettingsScene("Assets/Scenes/MainMenuScene.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/GameScene.unity", true)
            };
            EditorBuildSettings.scenes = scenes;

            Debug.Log($"[MainMenuBuilder] MainMenuScene with populated Chefs and Levels saved to {scenePath}");
        }

        private static GameObject CreatePanel(string name, Transform parent, Color bgColor)
        {
            GameObject panel = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            panel.transform.SetParent(parent, false);
            RectTransform rt = panel.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;
            panel.GetComponent<Image>().color = bgColor;
            return panel;
        }

        private static TextMeshProUGUI CreateText(string name, string textStr, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 sizeDelta, float fontSize, TextAlignmentOptions align = TextAlignmentOptions.Center)
        {
            GameObject txtObj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            txtObj.transform.SetParent(parent, false);
            RectTransform rt = txtObj.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = sizeDelta;

            TextMeshProUGUI tmp = txtObj.GetComponent<TextMeshProUGUI>();
            tmp.text = textStr;
            tmp.fontSize = fontSize;
            tmp.alignment = align;
            tmp.color = Color.white;
            return tmp;
        }

        private static Button CreateButton(string name, string label, Transform parent, Vector2 anchoredPos, Vector2 sizeDelta, UnityEngine.Events.UnityAction onClick)
        {
            GameObject btnObj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            btnObj.transform.SetParent(parent, false);
            RectTransform rt = btnObj.GetComponent<RectTransform>();
            rt.sizeDelta = sizeDelta;
            rt.anchoredPosition = anchoredPos;

            Image img = btnObj.GetComponent<Image>();
            img.color = new Color(0.25f, 0.25f, 0.3f, 1f);

            Button btn = btnObj.GetComponent<Button>();
            if (onClick != null) btn.onClick.AddListener(onClick);

            CreateText("Text", label, btnObj.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, 18f);
            return btn;
        }
    }
}
