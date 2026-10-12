using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FoodSurvivors.Boss;
using FoodSurvivors.Data;
using FoodSurvivors.Player;
using FoodSurvivors.UI;

namespace FoodSurvivors.Core
{
    public class CompetitionManager : MonoBehaviour
    {
        public static CompetitionManager Instance { get; private set; }

        [Header("Match Timing")]
        public float prepPhaseDuration = 90f;   // 1.5 минуты подготовки
        public float bossPhaseDuration = 30f;   // 30 секунд битва за туристов
        public float matchTimer = 0f;

        [Header("Score Tracker")]
        public int playerScore = 0;
        public int bossScore = 0;

        [Header("Boss Balancing")]
        public int bossLevelOffset = 1; // Уровень блюд босса на 1 ниже игрока
        public float playerBonusSpawnPercent = 0.15f; // На 15% больше мобов возле игрока

        [Header("State")]
        public bool isBossPhase = false;
        private bool bossSpawned = false;
        private RivalBossController activeBoss;

        public float TotalMatchDuration => prepPhaseDuration + bossPhaseDuration;
        public float RemainingTime => Mathf.Max(0f, TotalMatchDuration - matchTimer);

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Update()
        {
            matchTimer += Time.deltaTime;

            // На 1.5 минуте спавним соперника
            if (!bossSpawned && matchTimer >= prepPhaseDuration)
            {
                bossSpawned = true;
                StartCoroutine(TriggerBossIntroCutscene());
            }

            // Окончание матча (2:00)
            if (matchTimer >= TotalMatchDuration)
            {
                EndMatch();
            }
        }

        private IEnumerator TriggerBossIntroCutscene()
        {
            isBossPhase = true;
            Debug.Log("[CompetitionManager] 🚨 ПРИБЫТИЕ ШЕФА-СОПЕРНИКА!");

            Time.timeScale = 0f; // Заморозка игры

            // Спавним босса
            SpawnRivalBoss();

            // Перевод камеры на босса
            CameraFollow cam = Object.FindFirstObjectByType<CameraFollow>();
            Transform originalTarget = cam != null ? cam.target : null;

            if (cam != null && activeBoss != null)
            {
                cam.target = activeBoss.transform;
                cam.transform.position = activeBoss.transform.position + cam.offset;
            }

            yield return new WaitForSecondsRealtime(2.8f); // Показ босса 2.8 сек

            // Возврат камеры к игроку
            if (cam != null && originalTarget != null)
            {
                cam.target = originalTarget;
            }

            Time.timeScale = 1f; // Возобновление игры
        }

        private void SpawnRivalBoss()
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            Vector3 spawnPos = Vector3.zero;

            if (MapBoundsManager.Instance != null && playerObj != null)
            {
                spawnPos = MapBoundsManager.Instance.GetRandomPositionNearPlayerChunk(playerObj.transform.position);
            }
            else if (playerObj != null)
            {
                spawnPos = playerObj.transform.position + playerObj.transform.forward * 20f;
            }

            BossArchetype archetype = (BossArchetype)Random.Range(0, 3);

            if (archetype == BossArchetype.TripleDelivery)
            {
                // Спавним 3 курьеров
                for (int i = 0; i < 3; i++)
                {
                    Vector3 pos = spawnPos + new Vector3(Random.Range(-8f, 8f), 0f, Random.Range(-8f, 8f));
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
            GameObject bossObj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            bossObj.name = $"RivalChef_{archetype}";
            bossObj.transform.position = pos;
            bossObj.transform.localScale = Vector3.one * 1.6f;

            Renderer r = bossObj.GetComponent<Renderer>();
            if (r != null) r.sharedMaterial = DefaultMaterialsGenerator.CreateSolidColorMaterial("M_Rival", new Color(0.9f, 0.2f, 0.8f)); // Пурпурный шеф

            activeBoss = bossObj.AddComponent<RivalBossController>();
            activeBoss.InitializeBoss(archetype, dmgMult, bossLevelOffset);
        }

        public void AddPlayerScore(int amount = 1)
        {
            playerScore += amount;
        }

        public void AddBossScore(int amount = 1)
        {
            bossScore += amount;
        }

        private void EndMatch()
        {
            enabled = false;
            Time.timeScale = 0f;

            bool isVictory = playerScore > bossScore;
            Debug.Log($"[CompetitionManager] 🏁 МАТЧ ЗАВЕРШЕН! Игрок: {playerScore} | Босс: {bossScore} -> Победа: {isVictory}");

            if (GameOverUI.Instance != null)
            {
                GameOverUI.Instance.ShowGameOver(isVictory);
            }
        }
    }
}