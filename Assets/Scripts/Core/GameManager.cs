using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using FoodSurvivors.Data;

namespace FoodSurvivors.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Selection Data")]
        public ChefData selectedChef;
        public LevelData selectedLevel;

        [Header("Available Data Catalog")]
        public List<ChefData> availableChefs = new List<ChefData>();
        public List<LevelData> availableLevels = new List<LevelData>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            EnsureDataLoaded();
        }

        private void EnsureDataLoaded()
        {
#if UNITY_EDITOR
            if (availableChefs == null || availableChefs.Count == 0)
            {
                availableChefs = new List<ChefData>();
                string[] chefGuids = UnityEditor.AssetDatabase.FindAssets("t:ChefData");
                foreach (var g in chefGuids)
                {
                    string path = UnityEditor.AssetDatabase.GUIDToAssetPath(g);
                    var c = UnityEditor.AssetDatabase.LoadAssetAtPath<ChefData>(path);
                    if (c != null && !availableChefs.Contains(c)) availableChefs.Add(c);
                }
            }

            if (availableLevels == null || availableLevels.Count == 0)
            {
                availableLevels = new List<LevelData>();
                string[] levelGuids = UnityEditor.AssetDatabase.FindAssets("t:LevelData");
                foreach (var g in levelGuids)
                {
                    string path = UnityEditor.AssetDatabase.GUIDToAssetPath(g);
                    var l = UnityEditor.AssetDatabase.LoadAssetAtPath<LevelData>(path);
                    if (l != null && !availableLevels.Contains(l)) availableLevels.Add(l);
                }
            }
#endif

            if (selectedChef == null && availableChefs != null && availableChefs.Count > 0)
            {
                selectedChef = availableChefs[0];
            }

            if (selectedLevel == null && availableLevels != null && availableLevels.Count > 0)
            {
                selectedLevel = availableLevels[0];
            }
        }

        public void SelectChef(ChefData chef)
        {
            selectedChef = chef;
            Debug.Log($"[GameManager] Selected Chef: {(chef != null ? chef.chefName : "None")}");
        }

        public void SelectLevel(LevelData level)
        {
            selectedLevel = level;
            Debug.Log($"[GameManager] Selected Level: {(level != null ? level.levelName : "None")}");
        }

        public void StartGame()
        {
            Time.timeScale = 1f;

            if (selectedChef == null && availableChefs != null && availableChefs.Count > 0)
            {
                selectedChef = availableChefs[0];
            }

            if (selectedLevel == null && availableLevels != null && availableLevels.Count > 0)
            {
                selectedLevel = availableLevels[0];
            }

            Debug.Log($"[GameManager] Starting game with Chef: {(selectedChef != null ? selectedChef.chefName : "Default")} and Level: {(selectedLevel != null ? selectedLevel.levelName : "Default")}");
            Debug.Log("[GameManager] Loading scene 'GameScene'...");
            SceneManager.LoadScene("GameScene");
        }

        public void ReturnToMenu()
        {
            Time.timeScale = 1f;
            Debug.Log("[GameManager] Returning to Main Menu");
            SceneManager.LoadScene("MainMenuScene");
        }

        public void QuitGame()
        {
            Debug.Log("[GameManager] Quitting Game...");
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}