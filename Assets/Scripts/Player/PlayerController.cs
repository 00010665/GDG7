using UnityEngine;
using FoodSurvivors.Core;
using FoodSurvivors.Data;

namespace FoodSurvivors.Player
{
    public class PlayerController : MonoBehaviour
    {
        [Header("Chef Profile")]
        public ChefData chefData;

        [Header("Runtime Stats")]
        public float currentHealth;
        public float maxHealth;
        public float currentMoveSpeed;
        public float currentArmor;

        private Renderer playerRenderer;

        private void Start()
        {
            playerRenderer = GetComponentInChildren<Renderer>();
            InitializeChefStats();
        }

        public void InitializeChefStats()
        {
            if (GameManager.Instance != null && GameManager.Instance.selectedChef != null)
            {
                chefData = GameManager.Instance.selectedChef;
            }

            if (chefData != null)
            {
                maxHealth = chefData.maxHealth;
                currentHealth = maxHealth;
                currentMoveSpeed = chefData.moveSpeed;
                currentArmor = chefData.armor;

                if (playerRenderer != null)
                {
                    playerRenderer.material.color = chefData.chefColor;
                }

                Debug.Log($"[PlayerController] Initialized as {chefData.chefName} (HP: {currentHealth}, Speed: {currentMoveSpeed}, Armor: {currentArmor})");
            }
            else
            {
                maxHealth = 100f;
                currentHealth = maxHealth;
                currentMoveSpeed = 5f;
                currentArmor = 0f;
                Debug.LogWarning("[PlayerController] No ChefData found, initialized with fallback default stats.");
            }
        }

        private void Update()
        {
            HandleMovement();
        }

        private void HandleMovement()
        {
            float moveX = Input.GetAxisRaw("Horizontal");
            float moveZ = Input.GetAxisRaw("Vertical");

            Vector3 moveDirection = new Vector3(moveX, 0f, moveZ).normalized;

            if (moveDirection.magnitude > 0.1f)
            {
                // Move position
                transform.position += moveDirection * (currentMoveSpeed * Time.deltaTime);

                // Rotate towards movement direction smoothly
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 15f);
            }
        }

        public void TakeDamage(float damage)
        {
            float effectiveDamage = Mathf.Max(1f, damage - currentArmor);
            currentHealth -= effectiveDamage;

            Debug.Log($"[PlayerController] Took {effectiveDamage} damage (Armor: {currentArmor}). Remaining HP: {currentHealth}/{maxHealth}");

            if (currentHealth <= 0)
            {
                currentHealth = 0;
                Die();
            }
        }

        public void Heal(float amount)
        {
            if (amount <= 0 || currentHealth <= 0) return;

            currentHealth += amount;
            if (currentHealth > maxHealth) currentHealth = maxHealth;

            Debug.Log($"[PlayerController] Healed {amount}. HP: {currentHealth}/{maxHealth}");
        }

        public void Die()
        {
            Debug.Log("[PlayerController] Player Died!");
            // Future Game Over handling
        }
    }
}
