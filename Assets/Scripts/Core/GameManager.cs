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
            if (selectedChef == null && availableChefs.Count > 0)
            {
                selectedChef = availableChefs[0];
            }

            if (selectedLevel == null && availableLevels.Count > 0)
            {
                selectedLevel = availableLevels[0];
            }

            Debug.Log($"[GameManager] Starting game with Chef: {(selectedChef != null ? selectedChef.chefName : "Default")} and Level: {(selectedLevel != null ? selectedLevel.levelName : "Default")}");
            SceneManager.LoadScene("GameScene");
        }

        public void ReturnToMenu()
        {
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