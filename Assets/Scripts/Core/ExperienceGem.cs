using UnityEngine;
using FoodSurvivors.Player;

namespace FoodSurvivors.Core
{
    public class ExperienceGem : MonoBehaviour
    {
        public int xpAmount = 10;
        public float magnetSpeed = 12f;
        public AudioClip pickupSound;

        private Transform playerTransform;
        private PlayerController playerController;
        private bool isMagnetized = false;

        public void Setup(int amount)
        {
            xpAmount = amount;
        }

        private void Start()
        {
            FindPlayer();
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

            float distance = Vector3.Distance(transform.position, playerTransform.position);
            float radius = playerController != null ? playerController.magnetRadius : 3.5f;

            if (distance <= radius)
            {
                isMagnetized = true;
            }

            if (isMagnetized)
            {
                Vector3 targetPos = playerTransform.position + Vector3.up * 0.5f;
                transform.position = Vector3.MoveTowards(transform.position, targetPos, magnetSpeed * Time.deltaTime);

                if (Vector3.Distance(transform.position, targetPos) < 0.8f)
                {
                    Collect();
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                Collect();
            }
        }

        private void Collect()
        {
            Debug.Log($"[ExperienceGem] Collected {xpAmount} XP.");

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(pickupSound);
            }

            if (ExperienceManager.Instance != null)
            {
                ExperienceManager.Instance.AddXP(xpAmount);
            }
            else
            {
                Debug.Log($"[ExperienceGem] Collected {xpAmount} XP. (No ExperienceManager)");
            }

            Destroy(gameObject);
        }
    }
}
