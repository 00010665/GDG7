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

        public void Initialize(EnemyData data, EnemyBehaviorState state = EnemyBehaviorState.Hungry)
        {
            enemyData = data;
            behaviorState = state;
            gameObject.tag = "Enemy";

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
                if (enemyRenderer != null)
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
            float effectiveDamage = Mathf.Max(1f, dmg - armor);
            currentHealth -= effectiveDamage;

            DamageFlash flasher = GetComponent<DamageFlash>();
            if (flasher == null) flasher = gameObject.AddComponent<DamageFlash>();
            flasher.CallFlash();

            if (currentHealth <= 0f)
            {
                currentHealth = 0f;
                Die();
            }
        }

        public void Die()
        {
            if (ExperienceManager.Instance != null)
            {
                ExperienceManager.Instance.enemiesKilledCount++;
            }

            if (AudioManager.Instance != null && enemyData != null)
            {
                AudioManager.Instance.PlaySFX(enemyData.deathSound);
            }

            SpawnExperienceGem();

            if (isBoss && GameOverUI.Instance != null)
            {
                GameOverUI.Instance.ShowGameOver(true);
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