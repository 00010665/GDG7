using UnityEngine;
using FoodSurvivors.Core;
using FoodSurvivors.Data;
using FoodSurvivors.Player;
using FoodSurvivors.UI;

namespace FoodSurvivors.Enemies
{
    public class EnemyController : MonoBehaviour
    {
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

        public void Initialize(EnemyData data)
        {
            enemyData = data;
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

                transform.localScale = enemyData.scale != Vector3.zero ? enemyData.scale : (isBoss ? Vector3.one * 2f : Vector3.one);

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

            EnemyVisuals visuals = GetComponent<EnemyVisuals>();
            if (visuals == null)
            {
                visuals = gameObject.AddComponent<EnemyVisuals>();
            }
            visuals.ApplyEnemyVisuals(enemyData);
        }

        private void Start()
        {
            if (playerTransform == null)
            {
                FindPlayer();
            }
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
            if (playerTransform == null)
            {
                FindPlayer();
                return;
            }

            Vector3 direction = (playerTransform.position - transform.position);
            direction.y = 0f;

            if (direction.sqrMagnitude > 0.01f)
            {
                Vector3 moveDir = direction.normalized;
                transform.position += moveDir * (moveSpeed * Time.deltaTime);

                Quaternion targetRotation = Quaternion.LookRotation(moveDir, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                if (playerController != null)
                {
                    playerController.TakeDamage(damage * Time.deltaTime);
                }
            }
        }

        private void OnCollisionStay(Collision collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                if (playerController != null)
                {
                    playerController.TakeDamage(damage * Time.deltaTime);
                }
            }
        }

        public void TakeDamage(float dmg)
        {
            float effectiveDamage = Mathf.Max(1f, dmg - armor);
            currentHealth -= effectiveDamage;

            if (currentHealth <= 0f)
            {
                currentHealth = 0f;
                Die();
            }
        }

        public void Die()
        {
            Debug.Log($"[EnemyController] {gameObject.name} (XP: {xpValue}) defeated!");

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
            if (rend != null)
            {
                rend.material.color = Color.cyan;
            }

            Collider col = gemObj.GetComponent<Collider>();
            if (col != null) col.isTrigger = true;

            ExperienceGem gem = gemObj.AddComponent<ExperienceGem>();
            gem.Setup(xpValue > 0 ? xpValue : 10);
        }
    }
}
