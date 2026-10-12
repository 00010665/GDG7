using UnityEngine;
using FoodSurvivors.Core;
using FoodSurvivors.Data;
using FoodSurvivors.Player;
using FoodSurvivors.UI;

namespace FoodSurvivors.Enemies
{
    public enum EnemyBehaviorState
    {
        Hungry, // Голодный: бежит к повару
        Full,   // Сытый: стоит на месте
        Busy    // Занятой: бегает по случайным точкам
    }

    public class EnemyController : MonoBehaviour
    {
        [Header("Behavior State")]
        public EnemyBehaviorState behaviorState = EnemyBehaviorState.Hungry;

        [Header("Enemy Data")]
        public EnemyData enemyData;

        [Header("Runtime Stats")]
        public float currentHealth;
        public float maxHealth;
        public float moveSpeed;
        public float damage;
        public float armor;
        public int xpValue;
        public bool isBoss;

        private Transform playerTransform;
        private PlayerController playerController;
        private Renderer enemyRenderer;

        // Переменные для состояния "Занятой"
        private Vector3 busyTargetPos;
        private float busyChangeTimer = 0f;

        private Vector3 lastStuckCheckPos;
        private float stuckTimer = 0f;

        public void Initialize(EnemyData data, EnemyBehaviorState state = EnemyBehaviorState.Hungry)
        {
            enemyData = data;
            behaviorState = state;
            gameObject.tag = "Enemy";

            Animator animator = GetComponentInChildren<Animator>(true);
            if (animator != null && animator.runtimeAnimatorController != null)
            {
                foreach (AnimatorControllerParameter parameter in animator.parameters)
                {
                    if (parameter.name == "State" && parameter.type == AnimatorControllerParameterType.Int)
                    {
                        animator.SetInteger("State", (int)state);
                        break;
                    }
                }
            }

            if (enemyData != null)
            {
                maxHealth = enemyData.maxHealth;
                currentHealth = maxHealth;
                moveSpeed = enemyData.moveSpeed;
                damage = enemyData.damage;
                armor = enemyData.armor;
                xpValue = enemyData.xpValue;
                isBoss = enemyData.isBoss;

                transform.localScale = enemyData.scale != Vector3.zero ? enemyData.scale : (isBoss ? Vector3.one * 2.5f : Vector3.one);

                enemyRenderer = GetComponentInChildren<Renderer>();
                bool hasSkinnedCharacter = GetComponentInChildren<SkinnedMeshRenderer>() != null;
                if (enemyRenderer != null && !hasSkinnedCharacter)
                {
                    enemyRenderer.material.color = enemyData.enemyColor;
                }
            }
            else
            {
                maxHealth = 20f;
                currentHealth = maxHealth;
                moveSpeed = 3f;
                damage = 10f;
                armor = 0f;
                xpValue = 10;
                isBoss = false;
            }

            FindPlayer();
            PickNewBusyTarget();

            EnemyVisuals visuals = GetComponent<EnemyVisuals>();
            if (visuals == null) visuals = gameObject.AddComponent<EnemyVisuals>();
            visuals.ApplyEnemyVisuals(enemyData);
        }

        private void Start()
        {
            if (playerTransform == null) FindPlayer();
            lastStuckCheckPos = transform.position;
        }

        private void FindPlayer()
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerTransform = playerObj.transform;
                playerController = playerObj.GetComponent<PlayerController>();
            }
        }

        private void Update()
        {
            HandleBehaviorMovement();

            // Проверяем телепортацию через край карты (Pac-Man)
            if (MapBoundsManager.Instance != null && MapBoundsManager.Instance.enablePacmanTeleport)
            {
                Vector3 pos = transform.position;
                if (MapBoundsManager.Instance.CheckAndWrapPosition(ref pos))
                {
                    transform.position = pos;
                }
            }

            if (behaviorState != EnemyBehaviorState.Full)
            {
                if (Vector3.Distance(transform.position, lastStuckCheckPos) < 0.2f)
                {
                    stuckTimer += Time.deltaTime;
                    if (stuckTimer >= 10f)
                    {
                        Debug.Log($"[EnemyController] Моб {gameObject.name} застрял в геометрии карты на 10 сек. Устранение...");
                        // Пытаемся вытолкнуть в сторону
                        transform.position += Random.insideUnitSphere * 2.5f;
                        transform.position = new Vector3(transform.position.x, 1f, transform.position.z);

                        // Если всё еще на месте — удаляем без зависания
                        if (stuckTimer >= 12f)
                        {
                            Destroy(gameObject);
                        }
                    }
                }
                else
                {
                    stuckTimer = 0f;
                    lastStuckCheckPos = transform.position;
                }
            }
        }

        private void HandleBehaviorMovement()
        {
            if (moveSpeed <= 0.01f) return;

            switch (behaviorState)
            {
                case EnemyBehaviorState.Full:
                    // Сытые стоят на месте (скорость 0)
                    break;

                case EnemyBehaviorState.Hungry:
                    // Голодные бегут к игроку
                    if (playerTransform != null)
                    {
                        Vector3 dir = (playerTransform.position - transform.position);
                        dir.y = 0f;
                        if (dir.sqrMagnitude > 0.05f)
                        {
                            MoveInDirection(dir.normalized);
                        }
                    }
                    break;

                case EnemyBehaviorState.Busy:
                    // Занятые бегают в случайных направлениях по таймеру
                    busyChangeTimer -= Time.deltaTime;
                    if (busyChangeTimer <= 0f || Vector3.Distance(transform.position, busyTargetPos) < 1f)
                    {
                        PickNewBusyTarget();
                    }

                    Vector3 busyDir = (busyTargetPos - transform.position);
                    busyDir.y = 0f;
                    if (busyDir.sqrMagnitude > 0.05f)
                    {
                        MoveInDirection(busyDir.normalized);
                    }
                    break;
            }
        }

        private void MoveInDirection(Vector3 moveDir)
        {
            transform.position += moveDir * (moveSpeed * Time.deltaTime);
            Quaternion targetRotation = Quaternion.LookRotation(moveDir, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }

        private void PickNewBusyTarget()
        {
            busyChangeTimer = Random.Range(3f, 6f);
            Vector2 randCircle = Random.insideUnitCircle * Random.Range(8f, 16f);
            busyTargetPos = transform.position + new Vector3(randCircle.x, 0f, randCircle.y);
        }

        private void OnTriggerStay(Collider other)
        {
            if (other.CompareTag("Player") && playerController != null)
            {
                playerController.TakeDamage(damage * Time.deltaTime);
            }
        }

        private void OnCollisionStay(Collision collision)
        {
            if (collision.gameObject.CompareTag("Player") && playerController != null)
            {
                playerController.TakeDamage(damage * Time.deltaTime);
            }
        }

        public void TakeDamage(float dmg)
        {
            // Если поблизости босс наносит урон своим триггером или оружием
            ApplyDamage(dmg, false);
        }

        public void TakeDamageFromBoss(float dmg)
        {
            ApplyDamage(dmg, true);
        }

        private void ApplyDamage(float dmg, bool isFromBoss)
        {
            float effectiveDamage = Mathf.Max(1f, dmg - armor);
            currentHealth -= effectiveDamage;

            DamageFlash flasher = GetComponent<DamageFlash>();
            if (flasher == null) flasher = gameObject.AddComponent<DamageFlash>();
            flasher.CallFlash();

            if (currentHealth <= 0f)
            {
                currentHealth = 0f;
                Die(isFromBoss);
            }
        }

        public void Die(bool isFromBoss = false)
        {
            // УМНЫЙ ФИЛЬТР: Если рядом есть Шеф-Соперник — это 100% убийство босса!
            if (!isFromBoss)
            {
                FoodSurvivors.Boss.RivalBossController[] bosses = Object.FindObjectsByType<FoodSurvivors.Boss.RivalBossController>(FindObjectsSortMode.None);
                foreach (var b in bosses)
                {
                    if (b == null) continue;
                    float checkRadius = (b.archetype == FoodSurvivors.Boss.BossArchetype.FoodCart) ? b.pullRadius + 3f : 10f;
                    if (Vector3.Distance(transform.position, b.transform.position) <= checkRadius)
                    {
                        isFromBoss = true; // Моб накормлен в зоне действия босса!
                        break;
                    }
                }
            }

            // Начисление очков в соревнование
            if (CompetitionManager.Instance != null && CompetitionManager.Instance.isBossPhase)
            {
                if (isFromBoss) CompetitionManager.Instance.AddBossScore(1);
                else CompetitionManager.Instance.AddPlayerScore(1);
            }

            // Статистика игрока (только если накормил игрок)
            if (ExperienceManager.Instance != null && !isFromBoss)
            {
                ExperienceManager.Instance.enemiesKilledCount++;
            }

            // ГЕМ ВЫПАДАЕТ СТРОГО ЕСЛИ ЭТО НЕ БОСС!
            if (!isFromBoss)
            {
                SpawnExperienceGem();
            }

            Destroy(gameObject);
        }

        private void SpawnExperienceGem()
        {
            GameObject gemObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            gemObj.name = "ExperienceGem";
            gemObj.transform.position = transform.position + Vector3.up * 0.3f;
            gemObj.transform.localScale = Vector3.one * 0.35f;

            Renderer rend = gemObj.GetComponent<Renderer>();
            if (rend != null) rend.sharedMaterial = DefaultMaterialsGenerator.GetExpGemMaterial();

            Collider col = gemObj.GetComponent<Collider>();
            if (col != null) col.isTrigger = true;

            ExperienceGem gem = gemObj.AddComponent<ExperienceGem>();
            gem.Setup(xpValue > 0 ? xpValue : 10);
        }
    }
}
