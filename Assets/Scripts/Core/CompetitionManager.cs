using System.Collections;
using UnityEngine;
using FoodSurvivors.Boss;
using FoodSurvivors.Player;
using FoodSurvivors.UI;
using FoodSurvivors.Data;

namespace FoodSurvivors.Core
{
    public class CompetitionManager : MonoBehaviour
    {
        private static CompetitionManager _instance;

        // САМОСОЗДАЮЩИЙСЯ SINGLETON: Создается моментально в ту же миллисекунду при первом запросе!
        public static CompetitionManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Object.FindFirstObjectByType<CompetitionManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("[CompetitionManager]");
                        _instance = go.AddComponent<CompetitionManager>();
                        Debug.Log("[CompetitionManager] ⚡ Автоматически создан на сцене!");
                    }
                }
                return _instance;
            }
        }

        [Header("Match Timing (Настройки таймера)")]
        public float prepPhaseDuration = 15f;   // 15 СЕКУНД ДО БОССА
        public float bossPhaseDuration = 15f;   // 15 СЕКУНД БАТЛА
        public float matchTimer = 0f;

        [Header("Score Tracker")]
        public int playerScore = 0;
        public int bossScore = 0;

        [Header("Boss Balancing")]
        public int bossLevelOffset = 1;
        public float playerBonusSpawnPercent = 0.15f;

        [Header("State")]
        public bool isBossPhase = false;
        private bool bossSpawned = false;
        private RivalBossController activeBoss;
        private int lastLoggedSecond = -1;

        public float TotalMatchDuration => prepPhaseDuration + bossPhaseDuration;
        public float RemainingTime => Mathf.Max(0f, TotalMatchDuration - matchTimer);

        public string ActiveBossName => activeBoss != null ? activeBoss.DisplayName : "СОПЕРНИК";

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            Time.timeScale = 1f;
        }

        private void Start()
        {
            Time.timeScale = 1f;
            matchTimer = 0f;
            bossSpawned = false;
            isBossPhase = false;
            playerScore = 0;
            bossScore = 0;
            Debug.Log($"[CompetitionManager] 🏁 СТАРТ ТАЙМЕРА! До босса: {prepPhaseDuration}с. Всего матч: {TotalMatchDuration}с.");
        }

        private void Update()
        {
            matchTimer += Time.deltaTime;

            // Ежесекундный лог прямо в консоль Unity!
            int curSec = Mathf.FloorToInt(matchTimer);
            if (curSec != lastLoggedSecond)
            {
                lastLoggedSecond = curSec;
                if (!bossSpawned)
                {
                    int toBoss = Mathf.CeilToInt(prepPhaseDuration - matchTimer);
                    Debug.Log($"[ТАЙМЕР] ⏳ До Шефа-соперника осталось: {toBoss} сек...");
                }
                else
                {
                    int toEnd = Mathf.CeilToInt(TotalMatchDuration - matchTimer);
                    Debug.Log($"[БАТЛ] 🔥 До конца матча: {toEnd} сек! Счет: ВЫ {playerScore} : {bossScore} БОСС");
                }
            }

            // Ровно на 15 секунде спавним босса
            if (!bossSpawned && matchTimer >= prepPhaseDuration)
            {
                bossSpawned = true;
                Debug.Log("[CompetitionManager] 🚨 15 СЕКУНД ПРОШЛО! СПАВНИМ БОССА И ЗАПУСКАЕМ КАТСЦЕНУ!");
                StartCoroutine(TriggerBossIntroCutscene());
            }

            // На 30 секунде финал
            if (matchTimer >= TotalMatchDuration)
            {
                EndMatch();
            }
        }

        /// <summary>Starts the boss phase immediately from the developer menu.</summary>
        public void ForceBossPhase()
        {
            if (bossSpawned || !enabled) return;
            bossSpawned = true;
            matchTimer = Mathf.Max(matchTimer, prepPhaseDuration);
            StartCoroutine(TriggerBossIntroCutscene());
        }

        /// <summary>Ends the current match immediately and shows its result screen.</summary>
        public void ForceEndMatch()
        {
            if (!enabled) return;
            EndMatch();
        }

        private IEnumerator TriggerBossIntroCutscene()
        {
            isBossPhase = true;
            Time.timeScale = 0f;

            // Сброс счета в 0 для обоих в начале батла!
            playerScore = 0;
            bossScore = 0;

            SpawnRivalBoss();

            CameraFollow cam = Object.FindFirstObjectByType<CameraFollow>();
            Transform originalTarget = cam != null ? cam.target : null;

            if (cam != null && activeBoss != null)
            {
                cam.target = activeBoss.transform;
                cam.transform.position = activeBoss.transform.position + cam.offset;
                Debug.Log($"[CompetitionManager] 🎥 Камера показывает Шефа-Соперника!");
            }

            yield return new WaitForSecondsRealtime(2.5f); // 2.5 секунды показа босса

            if (cam != null && originalTarget != null)
            {
                cam.target = originalTarget;
            }

            Time.timeScale = CheatMenu.Instance != null && CheatMenu.Instance.showMenu ? 0f : 1f; // Не размораживаем игру поверх открытого чит-меню.
            Debug.Log("[CompetitionManager] ⚔️ КУЛИНАРНЫЙ БАТЛ НАЧАЛСЯ!");
        }

        private void SpawnRivalBoss()
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            Vector3 playerPos = playerObj != null ? playerObj.transform.position : Vector3.zero;
            Vector3 forward = playerObj != null ? playerObj.transform.forward : Vector3.forward;

            // Спавним прямо ПЕРЕД ЛИЦОМ игрока на дороге в 12 метрах!
            Vector3 spawnPos = playerPos + forward * 12f;
            spawnPos.y = playerPos.y;

            BossArchetype archetype = (BossArchetype)Random.Range(0, 3);
            AudioManager.Instance?.BeginBossEncounter(archetype);
            Debug.Log($"[CompetitionManager] 👹 Заспавнен Шеф-Соперник: {archetype} на позиции {spawnPos}");

            if (archetype == BossArchetype.TripleDelivery)
            {
                for (int i = 0; i < 3; i++)
                {
                    Vector3 pos = spawnPos + new Vector3((i - 1) * 3f, 0f, (i - 1) * 2f);
                    CreateBossInstance(pos, archetype, 0.30f);
                }
            }
            else
            {
                CreateBossInstance(spawnPos, archetype, 1.0f);
            }
        }

        private void CreateBossInstance(Vector3 pos, BossArchetype archetype, float dmgMult)
        {
            CharacterPrefabRegistry registry = Resources.Load<CharacterPrefabRegistry>("FoodSurvivors/CharacterPrefabRegistry");
            GameObject bossPrefab = registry != null ? registry.GetBossPrefab(archetype) : null;
            GameObject bossObj = bossPrefab != null
                ? Instantiate(bossPrefab, pos, Quaternion.identity)
                : GameObject.CreatePrimitive(PrimitiveType.Capsule);
            bossObj.name = $"RivalChef_{archetype}";
            bossObj.transform.position = pos;

            if (bossPrefab == null)
            {
                bossObj.transform.localScale = Vector3.one * 1.8f;
                Renderer r = bossObj.GetComponent<Renderer>();
                if (r != null) r.sharedMaterial = DefaultMaterialsGenerator.CreateSolidColorMaterial("M_Rival", new Color(0.95f, 0.1f, 0.85f));
            }

            if (bossObj.GetComponent<Collider>() == null) bossObj.AddComponent<CapsuleCollider>();

            activeBoss = bossObj.GetComponent<RivalBossController>();
            if (activeBoss == null) activeBoss = bossObj.AddComponent<RivalBossController>();
            activeBoss.InitializeBoss(archetype, dmgMult, bossLevelOffset);
        }

        public void AddPlayerScore(int amount = 1) => playerScore += amount;
        public void AddBossScore(int amount = 1) => bossScore += amount;

        private void EndMatch()
        {
            enabled = false;
            Time.timeScale = 0f;

            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) p.GetComponent<PlayerController>()?.SaveCurrentBuildForNextBoss();

            bool isVictory = playerScore > bossScore;
            Debug.Log($"[CompetitionManager] 🏁 ФИНАЛ МАТЧА! Счет: Игрок {playerScore} : {bossScore} Босс -> Победа: {isVictory}");

            if (GameOverUI.Instance != null)
            {
                GameOverUI.Instance.ShowGameOver(isVictory);
            }

        }
    }
}
