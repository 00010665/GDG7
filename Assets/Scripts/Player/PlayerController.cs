using UnityEngine;
using UnityEngine.InputSystem;
using FoodSurvivors.Core;
using FoodSurvivors.Data;
using FoodSurvivors.UI;
using FoodSurvivors.Weapons;

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
        public float magnetRadius = 3.5f;

        [Header("Offensive & Utility Multipliers")]
        public float damageMultiplier = 1f;
        public float cooldownReduction = 0f;
        public float areaMultiplier = 1f;
        public float hpRegenPerSec = 0f;

        private Renderer playerRenderer;
        private WeaponManager weaponManager;

        public Vector2 MoveInput { get; private set; }
        public bool IsWalking => MoveInput.sqrMagnitude > 0.01f;

        private void Start()
        {
            Time.timeScale = 1f;
            playerRenderer = GetComponentInChildren<Renderer>();
            weaponManager = GetComponent<WeaponManager>();
            InitializeChefStats();
            ApplyDefaultPlayerMaterial();
            EnsureStartingWeaponSpawned();
        }

        private void ApplyDefaultPlayerMaterial()
        {
            if (playerRenderer == null) return;
            // Применяем ярко-голубой материал по умолчанию
            playerRenderer.sharedMaterial = DefaultMaterialsGenerator.GetPlayerMaterial();
        }

        private void EnsureStartingWeaponSpawned()
        {
            // Если есть WeaponManager, он сам спавнит оружие в Start(),
            // но в редких случаях ChefData может не иметь startingWeapon,
            // тогда создаём дефолтное оружие.
            if (weaponManager == null) return;

            // Немного ждём пока WeaponManager.InitializeStartingWeapon() отработает
            // Если ничего не заспавнилось, пытаемся создать дефолтное
            if (weaponManager.activeWeapons == null || weaponManager.activeWeapons.Count == 0)
            {
#if UNITY_EDITOR
                if (chefData == null)
                {
                    string[] guids = UnityEditor.AssetDatabase.FindAssets("t:WeaponData");
                    if (guids.Length > 0)
                    {
                        string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                        var defaultWeapon = UnityEditor.AssetDatabase.LoadAssetAtPath<WeaponData>(path);
                        if (defaultWeapon != null)
                        {
                            Debug.LogWarning($"[PlayerController] No starting weapon found. Spawning default: {defaultWeapon.weaponName}");
                            weaponManager.AddWeapon(defaultWeapon);
                        }
                    }
                }
#endif
            }
        }

        public void InitializeChefStats()
        {
            if (GameManager.Instance != null && GameManager.Instance.selectedChef != null)
            {
                chefData = GameManager.Instance.selectedChef;
            }

#if UNITY_EDITOR
            if (chefData == null)
            {
                string[] guids = UnityEditor.AssetDatabase.FindAssets("t:ChefData");
                if (guids.Length > 0)
                {
                    string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                    chefData = UnityEditor.AssetDatabase.LoadAssetAtPath<ChefData>(path);
                }
            }
#endif

            if (chefData != null)
            {
                maxHealth = chefData.maxHealth;
                currentHealth = maxHealth;
                currentMoveSpeed = chefData.moveSpeed;
                currentArmor = chefData.armor;

                Debug.Log($"[PlayerController] Initialized as {chefData.chefName} (HP: {currentHealth}, Speed: {currentMoveSpeed}, Armor: {currentArmor})");
            }
            else
            {
                maxHealth = 100f;
                currentHealth = maxHealth;
                currentMoveSpeed = 5f;
                currentArmor = 0f;
                Debug.LogWarning("[PlayerController] No ChefData found, initialized with fallback default stats (Diego-style: HP=100, Speed=5, Armor=0).");
            }

            Debug.Log("[PlayerController] Player spawned. WeaponManager will attach starting weapon shortly.");
        }

        private void Update()
        {
            HandleMovement();
            HandleRegeneration();
        }

        private void HandleRegeneration()
        {
            if (hpRegenPerSec > 0f && currentHealth > 0f && currentHealth < maxHealth)
            {
                currentHealth = Mathf.Min(maxHealth, currentHealth + hpRegenPerSec * Time.deltaTime);
            }
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
            MoveInput = input;
            Vector3 moveDirection = new Vector3(input.x, 0f, input.y);

            if (moveDirection.sqrMagnitude > 0.01f)
            {
                transform.position += moveDirection * (currentMoveSpeed * Time.deltaTime);

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
            if (GameOverUI.Instance != null)
            {
                GameOverUI.Instance.ShowGameOver(false);
            }
        }
    }
}
