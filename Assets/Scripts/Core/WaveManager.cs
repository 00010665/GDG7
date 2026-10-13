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

            AudioManager.Instance?.PlayInGameMusic();

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
                        // Используем состав волны из LevelData, чтобы тайминг и численность совпадали с настройками уровня.
                        int groupCount = Mathf.Max(1, wave.spawnCountPerTick);
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

            const float minimumSpawnSpacing = 1.25f;
            EnemyController[] existingEnemies = Object.FindObjectsByType<EnemyController>(FindObjectsSortMode.None);
            List<Vector3> groupPositions = new List<Vector3>(count);

            for (int c = 0; c < count; c++)
            {
                Vector3 spawnPos = groupCenterPos;
                bool hasSeparatedPosition = false;
                for (int attempt = 0; attempt < 16; attempt++)
                {
                    Vector2 offset = Random.insideUnitCircle * 4f;
                    spawnPos = groupCenterPos + new Vector3(offset.x, 0f, offset.y);
                    hasSeparatedPosition = true;

                    foreach (EnemyController existing in existingEnemies)
                    {
                        if (existing == null) continue;
                        Vector3 delta = existing.transform.position - spawnPos;
                        delta.y = 0f;
                        if (delta.sqrMagnitude < minimumSpawnSpacing * minimumSpawnSpacing)
                        {
                            hasSeparatedPosition = false;
                            break;
                        }
                    }

                    if (hasSeparatedPosition)
                    {
                        foreach (Vector3 previousSpawn in groupPositions)
                        {
                            Vector3 delta = previousSpawn - spawnPos;
                            delta.y = 0f;
                            if (delta.sqrMagnitude < minimumSpawnSpacing * minimumSpawnSpacing)
                            {
                                hasSeparatedPosition = false;
                                break;
                            }
                        }
                    }

                    if (hasSeparatedPosition) break;
                }

                // Dense areas still get a spawn, but farther from the group center.
                if (!hasSeparatedPosition)
                {
                    Vector2 offset = Random.insideUnitCircle.normalized * (4f + c * minimumSpawnSpacing);
                    spawnPos = groupCenterPos + new Vector3(offset.x, 0f, offset.y);
                }
                groupPositions.Add(spawnPos);

                // Случайное поведение для каждого моба в группе
                EnemyBehaviorState state = (EnemyBehaviorState)Random.Range(0, 3); // Hungry, Full или Busy

                SpawnEnemyAtPosition(enemyData, spawnPos, state, false);
            }
        }

        public GameObject SpawnEnemyAtPosition(EnemyData enemyData, Vector3 spawnPos, EnemyBehaviorState state, bool isBoss = false)
        {
            CharacterPrefabRegistry registry = Resources.Load<CharacterPrefabRegistry>("FoodSurvivors/CharacterPrefabRegistry");
            GameObject visualPrefab = isBoss ? registry?.levelBossPrefab : registry?.GetEnemyPrefab(enemyData);
            GameObject enemyObj = visualPrefab != null
                ? Instantiate(visualPrefab, spawnPos, Quaternion.identity)
                : GameObject.CreatePrimitive(isBoss ? PrimitiveType.Cube : PrimitiveType.Capsule);
            enemyObj.name = $"Enemy_{enemyData.enemyName}_{state}";
            enemyObj.transform.position = spawnPos;

            if (isBoss) enemyObj.transform.localScale = Vector3.one * 2.5f;

            Renderer rend = enemyObj.GetComponentInChildren<Renderer>();
            if (rend != null && visualPrefab == null)
            {
                // Небольшой цветовой оттенок по поведению:
                // Голодные — красные, Занятые — оранжево-желтые, Сытые — зеленовато-серые
                Color col = enemyData.enemyColor;
                if (state == EnemyBehaviorState.Full) col = Color.gray;
                else if (state == EnemyBehaviorState.Busy) col = new Color(1f, 0.6f, 0.1f);
                rend.sharedMaterial = DefaultMaterialsGenerator.CreateSolidColorMaterial($"M_{state}", col);
            }

            Rigidbody rb = enemyObj.GetComponent<Rigidbody>();
            if (rb == null) rb = enemyObj.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.isKinematic = true;
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezePositionY;

            EnemyController enemyCtrl = enemyObj.GetComponent<EnemyController>();
            if (enemyCtrl == null) enemyCtrl = enemyObj.AddComponent<EnemyController>();
            enemyCtrl.Initialize(enemyData, state);

            // Character source prefabs are render hierarchies; give each spawned NPC one body collider aligned to the actual model bounds.
            CapsuleCollider bodyCollider = enemyObj.GetComponent<CapsuleCollider>();
            if (bodyCollider == null) bodyCollider = enemyObj.AddComponent<CapsuleCollider>();
            FitColliderToModel(enemyObj.transform, bodyCollider);

            return enemyObj;
        }

        private static void FitColliderToModel(Transform root, CapsuleCollider capsule)
        {
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            bool foundRenderer = false;
            Bounds worldBounds = new Bounds(root.position, Vector3.zero);
            foreach (Renderer renderer in renderers)
            {
                if (renderer == null || !renderer.enabled || !renderer.gameObject.activeInHierarchy) continue;
                if (!foundRenderer)
                {
                    worldBounds = renderer.bounds;
                    foundRenderer = true;
                }
                else worldBounds.Encapsulate(renderer.bounds);
            }

            if (!foundRenderer) return;

            Vector3 min = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
            Vector3 max = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);
            for (int i = 0; i < 8; i++)
            {
                Vector3 corner = new Vector3(
                    (i & 1) == 0 ? worldBounds.min.x : worldBounds.max.x,
                    (i & 2) == 0 ? worldBounds.min.y : worldBounds.max.y,
                    (i & 4) == 0 ? worldBounds.min.z : worldBounds.max.z);
                Vector3 local = root.InverseTransformPoint(corner);
                min = Vector3.Min(min, local);
                max = Vector3.Max(max, local);
            }

            Vector3 size = max - min;
            capsule.direction = 1;
            capsule.center = (min + max) * 0.5f;
            capsule.radius = Mathf.Max(0.2f, Mathf.Min(size.x, size.z) * 0.4f);
            capsule.height = Mathf.Max(size.y, capsule.radius * 2f);
            capsule.isTrigger = false;
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
            spawnPos.y = 0.5f;

            GameObject enemyObj = SpawnEnemyAtPosition(enemyData, spawnPos, EnemyBehaviorState.Hungry, isBoss);
            if (enemyObj == null) return null;
            enemyObj.name = isBoss ? $"BOSS_{enemyData.enemyName}" : $"Enemy_{enemyData.enemyName}";

            Debug.Log($"[WaveManager] Spawned enemy: {enemyObj.name} at {spawnPos} (Radius: {spawnRadius})");

            return enemyObj;
        }
    }
}
