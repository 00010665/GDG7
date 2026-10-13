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
        private static readonly Collider[] NearbyColliderBuffer = new Collider[64];
        private static readonly int[] NearbyEnemyIds = new int[64];

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
        private Transform cartFacingTarget;
        private float cartFacingUntil;
        private Transform idleGroupTarget;
        private float nextIdleLookSearch;

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

            if (behaviorState == EnemyBehaviorState.Full)
                UpdateIdleGroupFacing();

            if (cartFacingTarget != null && Time.time <= cartFacingUntil)
                FaceTarget(cartFacingTarget.position);
            else
                cartFacingTarget = null;

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

        private void LateUpdate()
        {
            // Cart attraction and other scripts can move several NPCs onto the same point during Update.
            // Resolve the overlap afterwards so every behavior state keeps a visible gap between bodies.
            ResolveEnemyOverlap();
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
            Vector3 separation = GetEnemySeparation();
            if (separation.sqrMagnitude > 0.001f)
                moveDir = (moveDir + separation * 2f).normalized;

            transform.position += moveDir * (moveSpeed * Time.deltaTime);
            Quaternion targetRotation = Quaternion.LookRotation(moveDir, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }

        private Vector3 GetEnemySeparation()
        {
            CapsuleCollider ownCollider = GetComponent<CapsuleCollider>();
            float ownScale = Mathf.Max(transform.lossyScale.x, transform.lossyScale.z);
            float ownRadius = Mathf.Max(0.45f, ownCollider != null ? ownCollider.radius * ownScale : 0.45f);
            float queryRadius = ownRadius * 2f + 0.35f;
            int count = Physics.OverlapSphereNonAlloc(
                transform.position,
                queryRadius,
                NearbyColliderBuffer,
                Physics.AllLayers,
                QueryTriggerInteraction.Ignore);

            Vector3 separation = Vector3.zero;
            int seenCount = 0;
            for (int i = 0; i < count; i++)
            {
                Collider otherCollider = NearbyColliderBuffer[i];
                if (otherCollider == null) continue;
                EnemyController other = otherCollider.GetComponentInParent<EnemyController>();
                if (other == null || other == this) continue;

                int id = other.GetInstanceID();
                bool alreadySeen = false;
                for (int seen = 0; seen < seenCount; seen++)
                {
                    if (NearbyEnemyIds[seen] == id)
                    {
                        alreadySeen = true;
                        break;
                    }
                }
                if (alreadySeen) continue;
                NearbyEnemyIds[seenCount++] = id;

                CapsuleCollider otherColliderShape = other.GetComponent<CapsuleCollider>();
                float otherScale = Mathf.Max(other.transform.lossyScale.x, other.transform.lossyScale.z);
                float otherRadius = Mathf.Max(0.45f, otherColliderShape != null ? otherColliderShape.radius * otherScale : 0.45f);
                float minimumDistance = ownRadius + otherRadius + 0.2f;

                Vector3 away = transform.position - other.transform.position;
                away.y = 0f;
                float distance = away.magnitude;
                if (distance >= minimumDistance) continue;

                if (distance < 0.001f)
                {
                    away = GetPairDirectionAwayFrom(id);
                    distance = 0f;
                }

                separation += away.normalized * (1f - distance / minimumDistance);
            }

            separation.y = 0f;
            return separation.sqrMagnitude > 1f ? separation.normalized : separation;
        }

        private void ResolveEnemyOverlap()
        {
            CapsuleCollider ownCollider = GetComponent<CapsuleCollider>();
            float ownScale = Mathf.Max(transform.lossyScale.x, transform.lossyScale.z);
            float ownRadius = Mathf.Max(0.45f, ownCollider != null ? ownCollider.radius * ownScale : 0.45f);
            float queryRadius = ownRadius * 2f + 0.5f;
            int count = Physics.OverlapSphereNonAlloc(
                transform.position,
                queryRadius,
                NearbyColliderBuffer,
                Physics.AllLayers,
                QueryTriggerInteraction.Ignore);

            Vector3 correction = Vector3.zero;
            int seenCount = 0;
            for (int i = 0; i < count; i++)
            {
                Collider otherCollider = NearbyColliderBuffer[i];
                if (otherCollider == null) continue;
                EnemyController other = otherCollider.GetComponentInParent<EnemyController>();
                if (other == null || other == this) continue;

                int id = other.GetInstanceID();
                bool alreadySeen = false;
                for (int seen = 0; seen < seenCount; seen++)
                {
                    if (NearbyEnemyIds[seen] == id)
                    {
                        alreadySeen = true;
                        break;
                    }
                }
                if (alreadySeen) continue;
                NearbyEnemyIds[seenCount++] = id;

                CapsuleCollider otherColliderShape = other.GetComponent<CapsuleCollider>();
                float otherScale = Mathf.Max(other.transform.lossyScale.x, other.transform.lossyScale.z);
                float otherRadius = Mathf.Max(0.45f, otherColliderShape != null ? otherColliderShape.radius * otherScale : 0.45f);
                float minimumDistance = ownRadius + otherRadius + 0.12f;

                Vector3 away = transform.position - other.transform.position;
                away.y = 0f;
                float distance = away.magnitude;
                if (distance >= minimumDistance) continue;
                if (distance < 0.001f) away = GetPairDirectionAwayFrom(id);

                // Correct half of the penetration each frame; both NPCs participate, keeping groups apart smoothly.
                correction += away.normalized * ((minimumDistance - distance) * 0.5f);
            }

            correction.y = 0f;
            float maxCorrection = 3f * Time.deltaTime;
            if (correction.magnitude > maxCorrection)
                correction = correction.normalized * maxCorrection;
            transform.position += correction;
        }

        private Vector3 GetPairDirectionAwayFrom(int otherId)
        {
            int ownId = GetInstanceID();
            int lowerId = Mathf.Min(ownId, otherId);
            int upperId = Mathf.Max(ownId, otherId);
            uint hash = (uint)(lowerId * 73856093) ^ (uint)(upperId * 19349663);
            float angle = (hash % 3600u) * 0.1f;
            Vector3 direction = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;
            return ownId < otherId ? direction : -direction;
        }

        public void SetCartFacingTarget(Transform target, float keepAlive = 0.2f)
        {
            cartFacingTarget = target;
            cartFacingUntil = Time.time + keepAlive;
        }

        private void UpdateIdleGroupFacing()
        {
            if (Time.time >= nextIdleLookSearch || idleGroupTarget == null)
            {
                nextIdleLookSearch = Time.time + Random.Range(1.5f, 3f);
                idleGroupTarget = null;
                EnemyController[] nearby = Object.FindObjectsByType<EnemyController>(FindObjectsSortMode.None);
                float nearestDistance = 5f;
                foreach (EnemyController candidate in nearby)
                {
                    if (candidate == this || candidate == null || candidate.behaviorState != EnemyBehaviorState.Full) continue;
                    float distance = (candidate.transform.position - transform.position).sqrMagnitude;
                    if (distance < nearestDistance * nearestDistance)
                    {
                        nearestDistance = Mathf.Sqrt(distance);
                        idleGroupTarget = candidate.transform;
                    }
                }
            }

            if (idleGroupTarget != null)
                FaceTarget(idleGroupTarget.position);
        }

        private void FaceTarget(Vector3 targetPosition)
        {
            Vector3 direction = targetPosition - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude <= 0.001f) return;
            Quaternion targetRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 8f);
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
