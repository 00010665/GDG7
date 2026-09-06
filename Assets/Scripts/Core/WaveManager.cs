using System.Collections.Generic;
using UnityEngine;
using FoodSurvivors.Data;
using FoodSurvivors.Enemies;

namespace FoodSurvivors.Core
{
    public class WaveManager : MonoBehaviour
    {
        [Header("Level Settings")]
        public LevelData currentLevelData;

        [Header("Spawn Settings")]
        public float gameTimer = 0f;
        public float spawnRadius = 15f;
        public Transform playerTransform;

        private bool bossSpawned = false;
        private List<float> waveTimers = new List<float>();

        private void Start()
        {
            Time.timeScale = 1f;
            FindPlayer();

            if (currentLevelData == null && GameManager.Instance != null && GameManager.Instance.selectedLevel != null)
            {
                currentLevelData = GameManager.Instance.selectedLevel;
            }

#if UNITY_EDITOR
            if (currentLevelData == null)
            {
                string[] guids = UnityEditor.AssetDatabase.FindAssets("t:LevelData");
                if (guids.Length > 0)
                {
                    string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                    currentLevelData = UnityEditor.AssetDatabase.LoadAssetAtPath<LevelData>(path);
                }
            }
#endif

            InitWaveTimers();

            if (AudioManager.Instance != null && currentLevelData != null)
            {
                AudioManager.Instance.PlayBGM(currentLevelData.backgroundMusic);
            }
        }

        private void FindPlayer()
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerTransform = playerObj.transform;
            }
        }

        private void InitWaveTimers()
        {
            waveTimers.Clear();
            if (currentLevelData != null && currentLevelData.waves != null)
            {
                for (int i = 0; i < currentLevelData.waves.Count; i++)
                {
                    waveTimers.Add(0f);
                }
            }
        }

        private void Update()
        {
            if (playerTransform == null)
            {
                FindPlayer();
                return;
            }

            gameTimer += Time.deltaTime;

            if (currentLevelData != null)
            {
                ProcessWaves();
                CheckBossSpawn();
            }
        }

        private void ProcessWaves()
        {
            if (currentLevelData.waves == null) return;

            for (int i = 0; i < currentLevelData.waves.Count; i++)
            {
                WaveData wave = currentLevelData.waves[i];
                if (wave.enemy == null) continue;

                if (gameTimer >= wave.startTime && gameTimer <= wave.endTime)
                {
                    if (i >= waveTimers.Count) waveTimers.Add(0f);

                    waveTimers[i] -= Time.deltaTime;
                    if (waveTimers[i] <= 0f)
                    {
                        int count = Mathf.Max(1, wave.spawnCountPerTick);
                        for (int c = 0; c < count; c++)
                        {
                            SpawnEnemy(wave.enemy);
                        }

                        float interval = wave.spawnInterval > 0.1f ? wave.spawnInterval : 1f;
                        waveTimers[i] = interval;
                    }
                }
            }
        }

        private void CheckBossSpawn()
        {
            if (!bossSpawned && gameTimer >= currentLevelData.levelDuration)
            {
                if (currentLevelData.bossData != null)
                {
                    Debug.Log($"[WaveManager] Level duration reached ({currentLevelData.levelDuration}s). Spawning BOSS: {currentLevelData.bossData.enemyName}!");
                    SpawnEnemy(currentLevelData.bossData, true);
                    bossSpawned = true;
                }
            }
        }

        public GameObject SpawnEnemy(EnemyData enemyData, bool isBoss = false)
        {
            if (enemyData == null || playerTransform == null) return null;

            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            Vector3 spawnOffset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * spawnRadius;
            Vector3 spawnPos = playerTransform.position + spawnOffset;
            spawnPos.y = isBoss ? 1.5f : 1.0f;

            GameObject enemyObj;
            if (isBoss)
            {
                enemyObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                enemyObj.name = $"BOSS_{enemyData.enemyName}";
            }
            else
            {
                enemyObj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                enemyObj.name = $"Enemy_{enemyData.enemyName}";
            }

            enemyObj.transform.position = spawnPos;

            Collider col = enemyObj.GetComponent<Collider>();
            if (col != null) col.isTrigger = false;

            Rigidbody rb = enemyObj.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = enemyObj.AddComponent<Rigidbody>();
                rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezePositionY;
            }

            EnemyController enemyCtrl = enemyObj.AddComponent<EnemyController>();
            EnemyVisuals visuals = enemyObj.AddComponent<EnemyVisuals>();
            enemyCtrl.Initialize(enemyData);
            visuals.ApplyEnemyVisuals(enemyData);

            return enemyObj;
        }
    }
}
