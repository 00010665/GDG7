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

        private float fallbackSpawnTimer = 0f;
        private float fallbackSpawnInterval = 2f;

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

            // Если всё равно нет LevelData — создаём дефолтный уровень 1 (synthetic runtime level)
            if (currentLevelData == null)
            {
                Debug.LogWarning("[WaveManager] No LevelData found, creating synthetic default Level 1.");
                currentLevelData = CreateDefaultLevel1();
            }

            GridGroundSetup grid = Object.FindFirstObjectByType<GridGroundSetup>();
            if (grid != null)
            {
                grid.UpdateColorsForLevel(currentLevelData);
            }

            InitWaveTimers();

            if (AudioManager.Instance != null && currentLevelData != null && currentLevelData.backgroundMusic != null)
            {
                AudioManager.Instance.PlayBGM(currentLevelData.backgroundMusic);
            }

            Debug.Log($"[WaveManager] Started. Level: {currentLevelData.levelName}, Waves: {(currentLevelData.waves != null ? currentLevelData.waves.Count : 0)}");
        }

        // Создаёт дефолтный runtime Level 1 с одним типом врага
        private LevelData CreateDefaultLevel1()
        {
            LevelData level = ScriptableObject.CreateInstance<LevelData>();
            level.levelName = "Уровень 1 (По умолчанию)";
            level.description = "Автоматически созданный уровень при отсутствии данных";
            level.levelDuration = 60f;

            // Создаём дефолтного врага
            EnemyData defaultEnemy = ScriptableObject.CreateInstance<EnemyData>();
            defaultEnemy.enemyName = "Турист";
            defaultEnemy.maxHealth = 20f;
            defaultEnemy.moveSpeed = 3f;
            defaultEnemy.damage = 5f;
            defaultEnemy.armor = 0f;
            defaultEnemy.xpValue = 5;
            defaultEnemy.isBoss = false;
            defaultEnemy.enemyColor = Color.red;

            WaveData wave = new WaveData
            {
                enemy = defaultEnemy,
                startTime = 0f,
                endTime = 9999f, // бесконечная волна
                spawnInterval = 2f,
                spawnCountPerTick = 1
            };

            level.waves = new List<WaveData> { wave };
            return level;
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
            else
            {
                // Fallback: спавним по таймеру если currentLevelData не загружен
                fallbackSpawnTimer -= Time.deltaTime;
                if (fallbackSpawnTimer <= 0f && GameManager.Instance != null)
                {
                    fallbackSpawnTimer = fallbackSpawnInterval;
                    Debug.Log($"[WaveManager] Fallback spawn tick around player (Radius: {spawnRadius})");
                }
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
                        // Спавним группой (от 3 до 5 человек) в соседнем чанке
                        int groupCount = Random.Range(3, 6);
                        SpawnGroupInNearbyChunk(wave.enemy, groupCount);

                        float interval = wave.spawnInterval > 0.1f ? wave.spawnInterval : 2.5f;
                        waveTimers[i] = interval;
                    }
                }
            }
        }

        /// <summary>
        /// Спавнит группу мобов со смешанным поведением в соседнем чанке
        /// </summary>
        private void SpawnGroupInNearbyChunk(EnemyData enemyData, int count)
        {
            if (playerTransform == null || enemyData == null) return;

            Vector3 groupCenterPos;
            if (MapBoundsManager.Instance != null)
            {
                groupCenterPos = MapBoundsManager.Instance.GetRandomPositionNearPlayerChunk(playerTransform.position);
            }
            else
            {
                float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                groupCenterPos = playerTransform.position + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * spawnRadius;
            }

            for (int c = 0; c < count; c++)
            {
                Vector2 offset = Random.insideUnitCircle * 2.5f;
                Vector3 spawnPos = groupCenterPos + new Vector3(offset.x, 0.5f, offset.y);

                // Случайное поведение для каждого моба в группе
                EnemyBehaviorState state = (EnemyBehaviorState)Random.Range(0, 3); // Hungry, Full или Busy

                SpawnEnemyAtPosition(enemyData, spawnPos, state, false);
            }
        }

        public GameObject SpawnEnemyAtPosition(EnemyData enemyData, Vector3 spawnPos, EnemyBehaviorState state, bool isBoss = false)
        {
            GameObject enemyObj = GameObject.CreatePrimitive(isBoss ? PrimitiveType.Cube : PrimitiveType.Capsule);
            enemyObj.name = $"Enemy_{enemyData.enemyName}_{state}";
            enemyObj.transform.position = spawnPos;

            if (isBoss) enemyObj.transform.localScale = Vector3.one * 2.5f;

            Renderer rend = enemyObj.GetComponent<Renderer>();
            if (rend != null)
            {
                // Небольшой цветовой оттенок по поведению:
                // Голодные — красные, Занятые — оранжево-желтые, Сытые — зеленовато-серые
                Color col = enemyData.enemyColor;
                if (state == EnemyBehaviorState.Full) col = Color.gray;
                else if (state == EnemyBehaviorState.Busy) col = new Color(1f, 0.6f, 0.1f);
                rend.sharedMaterial = DefaultMaterialsGenerator.CreateSolidColorMaterial($"M_{state}", col);
            }

            Rigidbody rb = enemyObj.AddComponent<Rigidbody>();
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezePositionY;

            EnemyController enemyCtrl = enemyObj.AddComponent<EnemyController>();
            enemyCtrl.Initialize(enemyData, state);

            return enemyObj;
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

            // Масштаб босса — 2.5x для заметности
            if (isBoss)
            {
                enemyObj.transform.localScale = Vector3.one * 2.5f;
            }

            // Назначаем яркие материалы по типу
            Renderer rend = enemyObj.GetComponent<Renderer>();
            if (rend != null)
            {
                if (isBoss)
                {
                    rend.sharedMaterial = DefaultMaterialsGenerator.GetBossMaterial();
                }
                else if (enemyData != null && enemyData.moveSpeed >= 5f)
                {
                    // Быстрый враг — оранжевый
                    rend.sharedMaterial = DefaultMaterialsGenerator.GetEnemyFastMaterial();
                }
                else
                {
                    // Обычный враг — красный
                    rend.sharedMaterial = DefaultMaterialsGenerator.GetEnemyNormalMaterial();
                }
            }

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

            // Переопределяем материал ещё раз после инициализации (на случай если EnemyController перезаписал)
            if (rend != null)
            {
                if (isBoss)
                {
                    rend.sharedMaterial = DefaultMaterialsGenerator.GetBossMaterial();
                }
                else if (enemyData.moveSpeed >= 5f)
                {
                    rend.sharedMaterial = DefaultMaterialsGenerator.GetEnemyFastMaterial();
                }
                else
                {
                    rend.sharedMaterial = DefaultMaterialsGenerator.GetEnemyNormalMaterial();
                }
            }

            Debug.Log($"[WaveManager] Spawned enemy: {enemyObj.name} at {spawnPos} (Radius: {spawnRadius})");

            return enemyObj;
        }
    }
}
