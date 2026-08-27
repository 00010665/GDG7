using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    [Header("Level Data")]
    public LevelData levelData;

    [Header("Level Settings")]
    public int currentLevelIndex = 0;
    public bool isLevelComplete = false;
    public bool isLevelFailed = false;

    private List<LevelData> levels = new List<LevelData>();

    public void LoadLevels(List<LevelData> newLevels)
    {
        levels = newLevels;
        currentLevelIndex = 0;
        isLevelComplete = false;
        isLevelFailed = false;
    }

    public void StartNextLevel()
    {
        if (currentLevelIndex >= levels.Count)
        {
            GameOver();
            return;
        }

        currentLevelIndex++;
        levelData = levels[currentLevelIndex - 1];
        isLevelComplete = false;
        isLevelFailed = false;
    }

    public void EndLevel()
    {
        isLevelComplete = true;
        Debug.Log($"Level {currentLevelIndex} completed!");
    }

    public void FailLevel()
    {
        isLevelFailed = true;
        Debug.Log($"Level {currentLevelIndex} failed!");
    }

    public void GameOver()
    {
        Debug.Log("Game Over!");
    }

    public LevelData GetLevelData()
    {
        return levelData;
    }

    public int GetCurrentLevelIndex()
    {
        return currentLevelIndex;
    }

    public bool IsLevelComplete()
    {
        return isLevelComplete;
    }

    public bool IsLevelFailed()
    {
        return isLevelFailed;
    }

    public bool IsGameOver()
    {
        return currentLevelIndex >= levels.Count;
    }

    public void Update()
    {
        // Update level state
    }
}