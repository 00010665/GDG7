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

            // 6. In-Game Canvas & LevelUp UI
            GameObject canvasObj = new GameObject("Canvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();

            GameObject esObj = new GameObject("EventSystem");
            esObj.AddComponent<EventSystem>();
            esObj.AddComponent<InputSystemUIInputModule>();

            GameObject levelUpManagerObj = new GameObject("LevelUpUIManager");
            levelUpManagerObj.transform.SetParent(canvasObj.transform, false);
            LevelUpUI levelUpUI = levelUpManagerObj.AddComponent<LevelUpUI>();

            // LevelUp Panel
            GameObject levelUpPanel = new GameObject("LevelUpPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            levelUpPanel.transform.SetParent(canvasObj.transform, false);
            RectTransform panelRt = levelUpPanel.GetComponent<RectTransform>();
            panelRt.anchorMin = Vector2.zero;
            panelRt.anchorMax = Vector2.one;
            panelRt.sizeDelta = Vector2.zero;
            levelUpPanel.GetComponent<Image>().color = new Color(0.05f, 0.05f, 0.1f, 0.85f);
            levelUpUI.levelUpPanel = levelUpPanel;

            // Title
            GameObject titleObj = new GameObject("LevelUpTitle", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            titleObj.transform.SetParent(levelUpPanel.transform, false);
            RectTransform titleRt = titleObj.GetComponent<RectTransform>();
            titleRt.anchorMin = new Vector2(0.5f, 0.85f);
            titleRt.anchorMax = new Vector2(0.5f, 0.85f);
            titleRt.sizeDelta = new Vector2(600, 80);
            TextMeshProUGUI titleTmp = titleObj.GetComponent<TextMeshProUGUI>();
            titleTmp.text = "⚡ НОВЫЙ УРОВЕНЬ! ⚡";
            titleTmp.fontSize = 38f;
            titleTmp.alignment = TextAlignmentOptions.Center;
            titleTmp.color = Color.yellow;

            // Cards setup
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

                // Card Preview Image
                GameObject cardPrv = new GameObject("Preview", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                cardPrv.transform.SetParent(cardObj.transform, false);
                RectTransform prvRt = cardPrv.GetComponent<RectTransform>();
                prvRt.anchorMin = new Vector2(0.5f, 0.75f);
                prvRt.anchorMax = new Vector2(0.5f, 0.75f);
                prvRt.sizeDelta = new Vector2(60, 60);
                levelUpUI.cardColorPreviews[i] = cardPrv.GetComponent<Image>();

                // Type Text
                GameObject typeObj = new GameObject("Type", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
                typeObj.transform.SetParent(cardObj.transform, false);
                RectTransform typeRt = typeObj.GetComponent<RectTransform>();
                typeRt.anchorMin = new Vector2(0.5f, 0.58f);
                typeRt.anchorMax = new Vector2(0.5f, 0.58f);
                typeRt.sizeDelta = new Vector2(200, 30);
                TextMeshProUGUI typeTmp = typeObj.GetComponent<TextMeshProUGUI>();
                typeTmp.fontSize = 16f;
                typeTmp.alignment = TextAlignmentOptions.Center;
                typeTmp.color = Color.cyan;
                levelUpUI.cardTypeTexts[i] = typeTmp;

                // Title Text
                GameObject cardTitleObj = new GameObject("Title", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
                cardTitleObj.transform.SetParent(cardObj.transform, false);
                RectTransform cardTitleRt = cardTitleObj.GetComponent<RectTransform>();
                cardTitleRt.anchorMin = new Vector2(0.5f, 0.45f);
                cardTitleRt.anchorMax = new Vector2(0.5f, 0.45f);
                cardTitleRt.sizeDelta = new Vector2(210, 40);
                TextMeshProUGUI cardTitleTmp = cardTitleObj.GetComponent<TextMeshProUGUI>();
                cardTitleTmp.fontSize = 20f;
                cardTitleTmp.alignment = TextAlignmentOptions.Center;
                cardTitleTmp.color = Color.white;
                levelUpUI.cardTitleTexts[i] = cardTitleTmp;

                // Desc Text
                GameObject descObj = new GameObject("Desc", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
                descObj.transform.SetParent(cardObj.transform, false);
                RectTransform descRt = descObj.GetComponent<RectTransform>();
                descRt.anchorMin = new Vector2(0.5f, 0.22f);
                descRt.anchorMax = new Vector2(0.5f, 0.22f);
                descRt.sizeDelta = new Vector2(200, 90);
                TextMeshProUGUI descTmp = descObj.GetComponent<TextMeshProUGUI>();
                descTmp.fontSize = 15f;
                descTmp.alignment = TextAlignmentOptions.Center;
                descTmp.color = new Color(0.85f, 0.85f, 0.85f);
                levelUpUI.cardDescTexts[i] = descTmp;
            }

            levelUpPanel.SetActive(false);

            // Save Scene
            string scenePath = "Assets/Scenes/GameScene.unity";
            EditorSceneManager.SaveScene(newScene, scenePath);

            // Ensure Build Settings include GameScene
            var scenes = new EditorBuildSettingsScene[] {
                new EditorBuildSettingsScene("Assets/Scenes/MainMenuScene.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/GameScene.unity", true)
            };
            EditorBuildSettings.scenes = scenes;

            Debug.Log($"[GameSceneBuilder] GameScene with LevelUpUI saved successfully to {scenePath}");
        }
    }
}
