using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using FoodSurvivors.Core;
using FoodSurvivors.Data;
using FoodSurvivors.Weapons;

namespace FoodSurvivors.Player
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Chef Profile")]
        public ChefData chefData;

        [Header("Movement")]
        public float currentMoveSpeed = 6f;
        public float magnetRadius = 4f;

        [Header("Offensive Multipliers")]
        public float damageMultiplier = 1f;
        public float cooldownReduction = 0f;
        public float areaMultiplier = 1f;

        [Header("Defense & Legacy Stats")]
        public float currentArmor = 0f;
        public float hpRegenPerSec = 0f;

        // Здоровье перенаправлено на Стамину (с поддержкой чтения и записи)
        public float maxHealth
        {
            get => staminaSystem != null ? staminaSystem.maxStamina : 100f;
            set { if (staminaSystem != null) staminaSystem.maxStamina = value; }
        }

        public float currentHealth
        {
            get => staminaSystem != null ? staminaSystem.currentStamina : 100f;
            set { if (staminaSystem != null) staminaSystem.currentStamina = value; }
        }

        private Rigidbody rb;
        private WeaponManager weaponManager;
        private StaminaSystem staminaSystem;

        public Vector2 MoveInput { get; private set; }
        public bool IsWalking => MoveInput.sqrMagnitude > 0.01f;
        public StaminaSystem Stamina => staminaSystem;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            staminaSystem = GetComponent<StaminaSystem>();
            if (staminaSystem == null) staminaSystem = gameObject.AddComponent<StaminaSystem>();

            // Настройка физики
            rb.useGravity = false;
            rb.isKinematic = false;
            rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
        }

        private void Start()
        {
            Time.timeScale = 1f;
            weaponManager = GetComponent<WeaponManager>();
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
                currentMoveSpeed = chefData.moveSpeed;
                currentArmor = chefData.armor;
            }
        }

        private void Update()
        {
            ReadInput();

            // Рывок на Shift или Space
            if (Keyboard.current != null)
            {
                if (Keyboard.current.leftShiftKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame)
                {
                    Vector3 moveDirection = new Vector3(MoveInput.x, 0f, MoveInput.y);
                    staminaSystem.TryPerformDash(moveDirection);
                }
            }
        }

        private void FixedUpdate()
        {
            if (staminaSystem != null && staminaSystem.IsDashing) return;

            // Честное физическое движение (с учетом стен и препятствий)
            Vector3 moveDir = new Vector3(MoveInput.x, 0f, MoveInput.y);
            if (moveDir.sqrMagnitude > 0.01f)
            {
                Vector3 targetPos = rb.position + moveDir.normalized * (currentMoveSpeed * Time.fixedDeltaTime);
                rb.MovePosition(targetPos);

                Quaternion targetRot = Quaternion.LookRotation(moveDir.normalized, Vector3.up);
                rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, Time.fixedDeltaTime * 15f));
            }
        }

        private void ReadInput()
        {
            Vector2 input = Vector2.zero;
            if (Keyboard.current != null)
            {
                if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) input.y += 1f;
                if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) input.y -= 1f;
                if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) input.x -= 1f;
                if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) input.x += 1f;
            }
            MoveInput = Vector2.ClampMagnitude(input, 1f);
        }

        public void TakeDamage(float damage)
        {
            if (staminaSystem != null)
            {
                staminaSystem.OnMobHitPlayer(damage);
            }

            DamageFlash flasher = GetComponent<DamageFlash>();
            if (flasher != null) flasher.CallFlash();
        }

        public void Heal(float amount)
        {
            if (staminaSystem != null)
            {
                staminaSystem.currentStamina = Mathf.Min(staminaSystem.maxStamina, staminaSystem.currentStamina + amount);
            }
        }

        public void Die() { }

        public void SaveCurrentBuildForNextBoss()
        {
            WeaponManager wm = GetComponent<WeaponManager>();
            PassiveManager pm = GetComponent<PassiveManager>();

            if (wm != null && wm.activeWeapons.Count > 0)
            {
                List<string> wList = new List<string>();
                foreach (var w in wm.activeWeapons)
                {
                    if (w != null && w.weaponData != null)
                        wList.Add($"{w.weaponData.name}:{w.currentLevel}");
                }
                PlayerPrefs.SetString("PrevRun_Weapons", string.Join(";", wList));
            }

            if (pm != null && pm.activePassives.Count > 0)
            {
                List<string> pList = new List<string>();
                foreach (var p in pm.activePassives)
                {
                    if (p != null)
                        pList.Add($"{p.name}:{pm.GetPassiveLevel(p)}");
                }
                PlayerPrefs.SetString("PrevRun_Passives", string.Join(";", pList));
            }
            PlayerPrefs.Save();
        }
    }
}