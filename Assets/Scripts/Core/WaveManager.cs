using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WaveManager : MonoBehaviour
{
    [Header("Level Data")]
    public LevelData levelData;

    [Header("Wave Settings")]
    public int currentWaveIndex = 0;
    public bool isBossWave = false;
    public bool isBossActive = false;
    public float bossSpawnDelay = 5f;

    private List<EnemyData> enemiesInWave = new List<EnemyData>();
    private List<Enemy> activeEnemies = new List<Enemy>();
    private float bossSpawnTimer = 0f;
    private Enemy bossEnemy;

    public void SetLevelData(LevelData data)
    {
        levelData = data;
        currentWaveIndex = 0;
        isBossWave = false;
        bossSpawnTimer = 0f;
        bossEnemy = null;
    }

    public void StartWave(EnemyData enemyData)
    {
        if (isBossWave)
        {
            return;
        }

        currentWaveIndex++;
        enemiesInWave.Clear();
        enemiesInWave.Add(enemyData);

        SpawnEnemies(enemyData);
    }

    public void SetBossWave(LevelData levelData)
    {
        isBossWave = true;
        currentWaveIndex++;
        enemiesInWave.Clear();
        bossSpawnTimer = bossSpawnDelay;
        bossEnemy = null;
    }

    public void StartBossWave()
    {
        if (isBossWave && bossEnemy == null)
        {
            bossSpawnTimer = bossSpawnDelay;
        }
    }

    public void Update()
    {
        if (isBossWave && bossEnemy == null)
        {
            bossSpawnTimer -= Time.deltaTime;

            if (bossSpawnTimer <= 0)
            {
                SpawnBoss();
            }
        }

        UpdateEnemies();
    }

    private void SpawnEnemies(EnemyData enemyData)
    {
        for (int i = 0; i < enemyData.count; i++)
        {
            SpawnEnemy(enemyData);
        }
    }

    private void SpawnEnemy(EnemyData enemyData)
    {
        Enemy enemy = new Enemy();
        enemy.SetData(enemyData);
        enemy.Activate();
        activeEnemies.Add(enemy);
    }

    private void SpawnBoss()
    {
        bossEnemy = new Enemy();
        bossEnemy.SetData(levelData.bossData);
        bossEnemy.Activate();
        bossEnemy.isBoss = true;
        bossEnemy.isBossActive = true;
        activeEnemies.Add(bossEnemy);
        isBossActive = true;
    }

    private void UpdateEnemies()
    {
        // Update all active enemies
        foreach (var enemy in activeEnemies)
        {
            enemy.Update();
        }

        // Remove dead enemies
        activeEnemies.RemoveAll(e => e.isDead);

        // Check if wave is complete
        if (isBossWave && bossEnemy != null && bossEnemy.isDead)
        {
            EndBossWave();
        }
        else if (!isBossWave && enemiesInWave.Count == 0)
        {
            EndWave();
        }
    }

    private void EndWave()
    {
        enemiesInWave.Clear();
        activeEnemies.Clear();
        isBossWave = false;
        bossEnemy = null;
        isBossActive = false;
    }

    private void EndBossWave()
    {
        isBossWave = false;
        bossEnemy = null;
        isBossActive = false;
        activeEnemies.Clear();
    }

    public void EndWave()
    {
        isBossWave = false;
        bossEnemy = null;
        isBossActive = false;
        activeEnemies.Clear();
    }

    public Enemy GetBoss()
    {
        return bossEnemy;
    }

    public List<Enemy> GetActiveEnemies()
    {
        return activeEnemies;
    }

    public int GetCurrentWaveIndex()
    {
        return currentWaveIndex;
    }

    public bool IsBossWave()
    {
        return isBossWave;
    }

    public bool IsBossActive()
    {
        return isBossActive;
    }

    public bool IsWaveComplete()
    {
        return enemiesInWave.Count == 0 && !isBossWave;
    }
}