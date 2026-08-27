using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    public Player player;
    public WaveManager waveManager;
    public LevelManager levelManager;
    public UIManager uiManager;

    [Header("Game State")]
    public int currentLevelIndex = 0;
    public int currentWaveIndex = 0;
    public bool gameRunning = false;
    public bool gamePaused = false;
    public bool gameOver = false;

    [Header("Level Data")]
    public LevelData currentLevelData;

    private List<FoodData> availableFoods = new List<FoodData>();
    private List<PassiveAbilityData> unlockedPassives = new List<PassiveAbilityData>();

    void Awake()
    {
        LoadGameData();
    }

    void Start()
    {
        InitializeGame();
    }

    void InitializeGame()
    {
        if (currentLevelIndex >= currentLevelData.levels.Length)
        {
            GameOver();
            return;
        }

        currentLevelData = currentLevelData.levels[currentLevelIndex];
        waveManager.SetLevelData(currentLevelData);
        gameRunning = true;
        gamePaused = false;
        gameOver = false;
        currentWaveIndex = 0;
    }

    public void StartNextWave()
    {
        if (currentWaveIndex >= currentLevelData.enemies.Length)
        {
            waveManager.SetBossWave(currentLevelData);
            return;
        }

        currentWaveIndex++;
        waveManager.StartWave(currentLevelData.enemies[currentWaveIndex - 1]);
    }

    public void EndLevel()
    {
        gameRunning = false;
        waveManager.EndWave();

        if (currentLevelIndex < currentLevelData.levels.Length - 1)
        {
            currentLevelIndex++;
            InitializeGame();
        }
        else
        {
            GameOver();
        }
    }

    public void GameOver()
    {
        gameRunning = false;
        gameOver = true;
    }

    public void PauseGame()
    {
        gamePaused = true;
    }

    public void UnpauseGame()
    {
        gamePaused = false;
    }

    public void AddFoodToInventory(FoodData food)
    {
        if (!availableFoods.Contains(food))
        {
            availableFoods.Add(food);
        }
    }

    public void UnlockPassive(PassiveAbilityData passive)
    {
        if (!unlockedPassives.Contains(passive))
        {
            unlockedPassives.Add(passive);
        }
    }

    public List<FoodData> GetAvailableFoods()
    {
        return availableFoods;
    }

    public List<PassiveAbilityData> GetUnlockedPassives()
    {
        return unlockedPassives;
    }

    public LevelData GetCurrentLevelData()
    {
        return currentLevelData;
    }

    public void SaveGame()
    {
        // Save game state
    }

    public void LoadGame()
    {
        // Load game state
    }

    private void LoadGameData()
    {
        // Load game data from JSON
    }

    private void SaveGameData()
    {
        // Save game data to JSON
    }
}