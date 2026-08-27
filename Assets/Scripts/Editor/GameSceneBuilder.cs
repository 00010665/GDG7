using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using FoodSurvivors.Core;
using FoodSurvivors.Player;
using FoodSurvivors.Weapons;

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

            // 4. WaveManager GameObject
            GameObject waveManagerObj = new GameObject("[WaveManager]");
            waveManagerObj.AddComponent<WaveManager>();

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

            // Save Scene
            string scenePath = "Assets/Scenes/GameScene.unity";
            EditorSceneManager.SaveScene(newScene, scenePath);

            // Ensure Build Settings include GameScene
            var scenes = new EditorBuildSettingsScene[] {
                new EditorBuildSettingsScene("Assets/Scenes/MainMenuScene.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/GameScene.unity", true)
            };
            EditorBuildSettings.scenes = scenes;

            Debug.Log($"[GameSceneBuilder] GameScene saved successfully to {scenePath}");
        }
    }
}
