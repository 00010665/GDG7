using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using TMPro;
using FoodSurvivors.Core;
using FoodSurvivors.Player;
using FoodSurvivors.Weapons;
using FoodSurvivors.UI;

namespace FoodSurvivors.EditorTools
{
    public static class GameSceneBuilder
    {
        [MenuItem("FoodSurvivors/Build Game Scene")]
        public static void BuildGameScene()
        {
            var newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 1. Directional Light
            GameObject lightObj = new GameObject("Directional Light");
            Light lightComponent = lightObj.AddComponent<Light>();
            lightComponent.type = LightType.Directional;
            lightComponent.intensity = 1.2f;
            lightObj.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            // 2. Ground Plane Arena
            GameObject planeObj = GameObject.CreatePrimitive(PrimitiveType.Plane);
            planeObj.name = "ArenaGround";
            planeObj.transform.position = Vector3.zero;
            planeObj.transform.localScale = new Vector3(5f, 1f, 5f); // 50x50 units

            Renderer planeRenderer = planeObj.GetComponent<Renderer>();
            if (planeRenderer != null)
            {
                Material groundMat = new Material(Shader.Find("Standard"));
                groundMat.color = new Color(0.25f, 0.3f, 0.25f);
                planeRenderer.material = groundMat;
            }

            // 3. Player GameObject (3D Capsule)
            GameObject playerObj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            playerObj.name = "Player";
            playerObj.tag = "Player";
            playerObj.transform.position = new Vector3(0f, 1f, 0f);

            playerObj.AddComponent<PlayerController>();
            playerObj.AddComponent<WeaponManager>();

            // 4. WaveManager & ExperienceManager GameObjects
            GameObject waveManagerObj = new GameObject("[WaveManager]");
            waveManagerObj.AddComponent<WaveManager>();

            GameObject expManagerObj = new GameObject("[ExperienceManager]");
            expManagerObj.AddComponent<ExperienceManager>();

            // 5. Main Camera
            GameObject camObj = new GameObject("Main Camera");
            camObj.tag = "MainCamera";
            Camera cam = camObj.AddComponent<Camera>();
            camObj.AddComponent<AudioListener>();

            camObj.transform.position = new Vector3(0f, 10f, -5f);
            camObj.transform.rotation = Quaternion.Euler(60f, 0f, 0f);

            CameraFollow camFollow = camObj.AddComponent<CameraFollow>();
            camFollow.target = playerObj.transform;
            camFollow.offset = new Vector3(0f, 12f, -8f);
            camFollow.smoothSpeed = 8f;

            // 6. In-Game Canvas & EventSystem
            GameObject canvasObj = new GameObject("Canvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();

            GameObject esObj = new GameObject("EventSystem");
            esObj.AddComponent<EventSystem>();
            esObj.AddComponent<InputSystemUIInputModule>();

            // --- GameHUD Setup ---
            GameObject hudObj = new GameObject("GameHUDManager");
            hudObj.transform.SetParent(canvasObj.transform, false);
            GameHUD gameHUD = hudObj.AddComponent<GameHUD>();

            // XP Bar (Top of Screen)
            GameObject xpBarObj = CreateSlider("XPBar", canvasObj.transform, new Vector2(0.5f, 0.96f), new Vector2(0.5f, 0.96f), Vector2.zero, new Vector2(800, 20), new Color(0.1f, 0.1f, 0.1f, 0.8f), new Color(0f, 0.8f, 1f, 1f));
            gameHUD.xpSlider = xpBarObj.GetComponent<Slider>();

            // Level Text (Top Right)
            gameHUD.levelText = CreateText("LevelText", "УР. 1", canvasObj.transform, new Vector2(0.92f, 0.96f), new Vector2(0.92f, 0.96f), Vector2.zero, new Vector2(150, 40), 22f, TextAlignmentOptions.Right);

            // Timer Text (Top Center)
            gameHUD.timerText = CreateText("TimerText", "00:00", canvasObj.transform, new Vector2(0.5f, 0.91f), new Vector2(0.5f, 0.91f), Vector2.zero, new Vector2(200, 40), 28f, TextAlignmentOptions.Center);

            // HP Bar (Bottom Left)
            GameObject hpBarObj = CreateSlider("HPBar", canvasObj.transform, new Vector2(0.18f, 0.05f), new Vector2(0.18f, 0.05f), Vector2.zero, new Vector2(250, 25), new Color(0.2f, 0.05f, 0.05f, 0.8f), new Color(0.9f, 0.1f, 0.1f, 1f));
            gameHUD.hpSlider = hpBarObj.GetComponent<Slider>();
            gameHUD.hpText = CreateText("HPText", "HP: 100 / 100", hpBarObj.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, 16f, TextAlignmentOptions.Center);

            // --- LevelUp UI Setup ---
            GameObject levelUpManagerObj = new GameObject("LevelUpUIManager");
            levelUpManagerObj.transform.SetParent(canvasObj.transform, false);
            LevelUpUI levelUpUI = levelUpManagerObj.AddComponent<LevelUpUI>();

            GameObject levelUpPanel = new GameObject("LevelUpPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            levelUpPanel.transform.SetParent(canvasObj.transform, false);
            RectTransform panelRt = levelUpPanel.GetComponent<RectTransform>();
            panelRt.anchorMin = Vector2.zero;
            panelRt.anchorMax = Vector2.one;
            panelRt.sizeDelta = Vector2.zero;
            levelUpPanel.GetComponent<Image>().color = new Color(0.05f, 0.05f, 0.1f, 0.85f);
            levelUpUI.levelUpPanel = levelUpPanel;

            CreateText("LevelUpTitle", "⚡ НОВЫЙ УРОВЕНЬ! ⚡", levelUpPanel.transform, new Vector2(0.5f, 0.85f), new Vector2(0.5f, 0.85f), Vector2.zero, new Vector2(600, 80), 38f, TextAlignmentOptions.Center);

            levelUpUI.cardButtons = new Button[3];
            levelUpUI.cardTitleTexts = new TextMeshProUGUI[3];
            levelUpUI.cardDescTexts = new TextMeshProUGUI[3];
            levelUpUI.cardTypeTexts = new TextMeshProUGUI[3];
            levelUpUI.cardColorPreviews = new Image[3];

            float[] xOffsets = new float[] { -260f, 0f, 260f };

            for (int i = 0; i < 3; i++)
            {
                GameObject cardObj = new GameObject($"Card_{i + 1}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
                cardObj.transform.SetParent(levelUpPanel.transform, false);
                RectTransform cardRt = cardObj.GetComponent<RectTransform>();
                cardRt.anchorMin = new Vector2(0.5f, 0.45f);
                cardRt.anchorMax = new Vector2(0.5f, 0.45f);
                cardRt.anchoredPosition = new Vector2(xOffsets[i], 0f);
                cardRt.sizeDelta = new Vector2(230, 320);

                Image cardImg = cardObj.GetComponent<Image>();
                cardImg.color = new Color(0.2f, 0.2f, 0.25f, 0.95f);
                levelUpUI.cardButtons[i] = cardObj.GetComponent<Button>();

                GameObject cardPrv = new GameObject("Preview", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                cardPrv.transform.SetParent(cardObj.transform, false);
                RectTransform prvRt = cardPrv.GetComponent<RectTransform>();
                prvRt.anchorMin = new Vector2(0.5f, 0.75f);
                prvRt.anchorMax = new Vector2(0.5f, 0.75f);
                prvRt.sizeDelta = new Vector2(60, 60);
                levelUpUI.cardColorPreviews[i] = cardPrv.GetComponent<Image>();

                levelUpUI.cardTypeTexts[i] = CreateText("Type", "[Блюдо]", cardObj.transform, new Vector2(0.5f, 0.58f), new Vector2(0.5f, 0.58f), Vector2.zero, new Vector2(200, 30), 16f, TextAlignmentOptions.Center);
                levelUpUI.cardTitleTexts[i] = CreateText("Title", "Название", cardObj.transform, new Vector2(0.5f, 0.45f), new Vector2(0.5f, 0.45f), Vector2.zero, new Vector2(210, 40), 20f, TextAlignmentOptions.Center);
                levelUpUI.cardDescTexts[i] = CreateText("Desc", "Описание улучшения...", cardObj.transform, new Vector2(0.5f, 0.22f), new Vector2(0.5f, 0.22f), Vector2.zero, new Vector2(200, 90), 15f, TextAlignmentOptions.Center);
            }

            levelUpPanel.SetActive(false);

            // --- GameOver UI Setup ---
            GameObject gameOverManagerObj = new GameObject("GameOverUIManager");
            gameOverManagerObj.transform.SetParent(canvasObj.transform, false);
            GameOverUI gameOverUI = gameOverManagerObj.AddComponent<GameOverUI>();

            GameObject gameOverPanel = new GameObject("GameOverPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            gameOverPanel.transform.SetParent(canvasObj.transform, false);
            RectTransform goPanelRt = gameOverPanel.GetComponent<RectTransform>();
            goPanelRt.anchorMin = Vector2.zero;
            goPanelRt.anchorMax = Vector2.one;
            goPanelRt.sizeDelta = Vector2.zero;
            gameOverPanel.GetComponent<Image>().color = new Color(0.05f, 0.05f, 0.05f, 0.92f);
            gameOverUI.gameOverPanel = gameOverPanel;

            gameOverUI.titleText = CreateText("GameOverTitle", "💀 ПОРАЖЕНИЕ 💀", gameOverPanel.transform, new Vector2(0.5f, 0.75f), new Vector2(0.5f, 0.75f), Vector2.zero, new Vector2(600, 80), 44f, TextAlignmentOptions.Center);
            gameOverUI.statsText = CreateText("GameOverStats", "Время в бою: 00:00\nДостигнутый уровень: 1", gameOverPanel.transform, new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.55f), Vector2.zero, new Vector2(500, 100), 24f, TextAlignmentOptions.Center);

            CreateButton("BtnRestart", "🔄 Играть Заново", gameOverPanel.transform, new Vector2(0, -20), new Vector2(280, 55), () => gameOverUI.OnClickRestart());
            CreateButton("BtnMainMenu", "🏠 Главное Меню", gameOverPanel.transform, new Vector2(0, -90), new Vector2(280, 50), () => gameOverUI.OnClickMainMenu());
            CreateButton("BtnQuit", "❌ Выход", gameOverPanel.transform, new Vector2(0, -155), new Vector2(280, 45), () => gameOverUI.OnClickQuit());

            gameOverPanel.SetActive(false);

            // Save Scene
            string scenePath = "Assets/Scenes/GameScene.unity";
            EditorSceneManager.SaveScene(newScene, scenePath);

            // Ensure Build Settings include GameScene
            var scenes = new EditorBuildSettingsScene[] {
                new EditorBuildSettingsScene("Assets/Scenes/MainMenuScene.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/GameScene.unity", true)
            };
            EditorBuildSettings.scenes = scenes;

            Debug.Log($"[GameSceneBuilder] Full GameScene with HUD, LevelUpUI, and GameOverUI saved successfully to {scenePath}");
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

        private static GameObject CreateSlider(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 sizeDelta, Color bgColor, Color fillColor)
        {
            GameObject sliderObj = new GameObject(name, typeof(RectTransform), typeof(Slider));
            sliderObj.transform.SetParent(parent, false);
            RectTransform rt = sliderObj.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = sizeDelta;

            Slider slider = sliderObj.GetComponent<Slider>();

            GameObject bgObj = new GameObject("Background", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            bgObj.transform.SetParent(sliderObj.transform, false);
            RectTransform bgRt = bgObj.GetComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.sizeDelta = Vector2.zero;
            bgObj.GetComponent<Image>().color = bgColor;

            GameObject fillArea = new GameObject("Fill Area", typeof(RectTransform));
            fillArea.transform.SetParent(sliderObj.transform, false);
            RectTransform faRt = fillArea.GetComponent<RectTransform>();
            faRt.anchorMin = Vector2.zero;
            faRt.anchorMax = Vector2.one;
            faRt.sizeDelta = Vector2.zero;

            GameObject fillObj = new GameObject("Fill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            fillObj.transform.SetParent(fillArea.transform, false);
            RectTransform fillRt = fillObj.GetComponent<RectTransform>();
            fillRt.anchorMin = Vector2.zero;
            fillRt.anchorMax = Vector2.one;
            fillRt.sizeDelta = Vector2.zero;
            fillObj.GetComponent<Image>().color = fillColor;

            slider.fillRect = fillRt;
            slider.targetGraphic = bgObj.GetComponent<Image>();

            return sliderObj;
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
