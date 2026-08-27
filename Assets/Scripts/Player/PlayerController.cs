using UnityEngine;
using UnityEngine.InputSystem;
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
            Vector2 input = Vector2.zero;

            if (Keyboard.current != null)
            {
                if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) input.y += 1f;
                if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) input.y -= 1f;
                if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) input.x -= 1f;
                if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) input.x += 1f;
            }

            if (Gamepad.current != null)
            {
                Vector2 stick = Gamepad.current.leftStick.ReadValue();
                if (stick.sqrMagnitude > 0.01f)
                {
                    input += stick;
                }
            }

            input = Vector2.ClampMagnitude(input, 1f);
            Vector3 moveDirection = new Vector3(input.x, 0f, input.y);

            if (moveDirection.sqrMagnitude > 0.01f)
            {
                // Move position
                transform.position += moveDirection * (currentMoveSpeed * Time.deltaTime);

                // Rotate towards movement direction smoothly
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection.normalized, Vector3.up);
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
