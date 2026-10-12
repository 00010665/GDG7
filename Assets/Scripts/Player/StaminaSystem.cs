using UnityEngine;

namespace FoodSurvivors.Player
{
    public enum StaminaMode
    {
        DamageBoostOnly, // Стамина = бонус урона, тратится при ударах мобов
        DashOnly,        // Стамина тратится на рывки, пассивно восстанавливается
        Hybrid           // И рывки, и бонус к урону
    }

    public class StaminaSystem : MonoBehaviour
    {
        [Header("Mode Selection")]
        public StaminaMode currentMode = StaminaMode.Hybrid;

        [Header("Stamina Stats")]
        public float maxStamina = 100f;
        public float currentStamina = 100f;
        public float staminaRegenPerSec = 15f;

        [Header("Damage Boost Mode Settings")]
        [Tooltip("Делитель стамины: 100 / 20 = 5 бонусного урона")]
        public float damageDivisor = 20f;
        public float staminaLostOnHit = 10f;

        [Header("Dash Mode Settings")]
        public float dashCost = 25f;
        public float dashDistance = 7f;
        public float dashDuration = 0.2f;
        public float dashCooldown = 0.5f;

        private PlayerController player;
        private Rigidbody rb;
        private bool isDashing = false;
        private float dashCooldownTimer = 0f;

        public bool IsDashing => isDashing;

        private void Awake()
        {
            player = GetComponent<PlayerController>();
            rb = GetComponent<Rigidbody>();
            currentStamina = maxStamina;
        }

        private void Update()
        {
            dashCooldownTimer -= Time.deltaTime;

            // Пассивная регенерация стамины (в режимах с рывком)
            if (currentMode != StaminaMode.DamageBoostOnly && !isDashing)
            {
                if (currentStamina < maxStamina)
                {
                    currentStamina = Mathf.Min(maxStamina, currentStamina + staminaRegenPerSec * Time.deltaTime);
                }
            }
        }

        /// <summary>
        /// Расчет целочисленного бонуса к урону от стамины:
        /// 100 / 20 = 5. Если 90 / 20 = 4.5 -> округляем вверх до 5
        /// </summary>
        public int GetFlatDamageBonus()
        {
            if (currentMode == StaminaMode.DashOnly) return 0;
            if (damageDivisor <= 0.01f) return 0;

            float rawBonus = currentStamina / damageDivisor;
            return Mathf.CeilToInt(rawBonus);
        }

        /// <summary>
        /// Вызывается мобами при касании игрока вместо нанесения урона
        /// </summary>
        public void OnMobHitPlayer(float damageImpact)
        {
            if (isDashing) return; // Неуязвимость во время рывка

            if (currentMode == StaminaMode.DamageBoostOnly || currentMode == StaminaMode.Hybrid)
            {
                currentStamina = Mathf.Max(0f, currentStamina - staminaLostOnHit);
                Debug.Log($"[StaminaSystem] Удар моба! Стамина: {currentStamina}/{maxStamina} (Бонус урона: +{GetFlatDamageBonus()})");
            }
        }

        /// <summary>
        /// Выполнение физического рывка без прохождения сквозь стены
        /// </summary>
        public bool TryPerformDash(Vector3 inputDirection)
        {
            if (currentMode == StaminaMode.DamageBoostOnly) return false;
            if (isDashing || dashCooldownTimer > 0f || currentStamina < dashCost) return false;

            currentStamina -= dashCost;
            dashCooldownTimer = dashCooldown;
            StartCoroutine(DoDashRoutine(inputDirection));
            return true;
        }

        private System.Collections.IEnumerator DoDashRoutine(Vector3 direction)
        {
            isDashing = true;

            // Вектор рывка (если стоим на месте — рывок вперед)
            Vector3 dashDir = direction.sqrMagnitude > 0.01f ? direction.normalized : transform.forward;
            float speed = dashDistance / dashDuration;

            // Игнорируем коллизии с врагами на время рывка
            SetEnemyCollisionsIgnored(true);

            float elapsed = 0f;
            while (elapsed < dashDuration)
            {
                if (rb != null)
                {
                    rb.MovePosition(rb.position + dashDir * (speed * Time.deltaTime));
                }
                else
                {
                    transform.position += dashDir * (speed * Time.deltaTime);
                }

                elapsed += Time.deltaTime;
                yield return null;
            }

            // Возвращаем коллизии назад
            SetEnemyCollisionsIgnored(false);
            isDashing = false;
        }

        private void SetEnemyCollisionsIgnored(bool ignore)
        {
            Collider playerCol = GetComponent<Collider>();
            if (playerCol == null) return;

            // Игнорируем коллайдеры мобов при рывке, но стены блокируют!
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            foreach (var e in enemies)
            {
                if (e != null)
                {
                    Collider eCol = e.GetComponent<Collider>();
                    if (eCol != null)
                    {
                        Physics.IgnoreCollision(playerCol, eCol, ignore);
                    }
                }
            }
        }
    }
}