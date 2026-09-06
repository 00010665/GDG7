using UnityEngine;
using FoodSurvivors.Data;

namespace FoodSurvivors.Enemies
{
    [RequireComponent(typeof(EnemyController))]
    public class EnemyVisuals : MonoBehaviour
    {
        [Header("Visual Components")]
        public SpriteRenderer spriteRenderer;
        public Animator animator;

        private EnemyController enemyController;
        private Transform playerTransform;

        private void Awake()
        {
            enemyController = GetComponent<EnemyController>();

            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }

            if (spriteRenderer == null)
            {
                Transform visualRoot = transform.Find("Visual");
                GameObject visualObj = visualRoot != null ? visualRoot.gameObject : new GameObject("Visual");
                visualObj.transform.SetParent(transform, false);
                spriteRenderer = visualObj.GetComponent<SpriteRenderer>();
                if (spriteRenderer == null)
                {
                    spriteRenderer = visualObj.AddComponent<SpriteRenderer>();
                }
            }

            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }

            if (animator == null && spriteRenderer != null)
            {
                animator = spriteRenderer.GetComponent<Animator>();
                if (animator == null)
                {
                    animator = spriteRenderer.gameObject.AddComponent<Animator>();
                }
            }
        }

        public void ApplyEnemyVisuals(EnemyData data)
        {
            if (data == null) return;

            if (spriteRenderer != null && data.enemySprite != null)
            {
                spriteRenderer.sprite = data.enemySprite;
                spriteRenderer.color = Color.white;
            }

            if (animator != null && data.animatorController != null)
            {
                animator.runtimeAnimatorController = data.animatorController;
            }
        }

        private void Update()
        {
            if (playerTransform == null)
            {
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null) playerTransform = playerObj.transform;
            }

            bool facingLeft = false;
            if (playerTransform != null)
            {
                facingLeft = playerTransform.position.x < transform.position.x;
                if (spriteRenderer != null)
                {
                    spriteRenderer.flipX = facingLeft;
                }
            }

            if (animator == null || animator.runtimeAnimatorController == null) return;

            bool isWalking = enemyController != null && enemyController.moveSpeed > 0.01f;

            if (HasAnimatorParameter("IsWalking"))
            {
                animator.SetBool("IsWalking", isWalking);
            }

            if (HasAnimatorParameter("FlipX"))
            {
                animator.SetBool("FlipX", facingLeft);
            }
        }

        private bool HasAnimatorParameter(string paramName)
        {
            if (animator == null || animator.runtimeAnimatorController == null) return false;

            foreach (AnimatorControllerParameter param in animator.parameters)
            {
                if (param.name == paramName) return true;
            }

            return false;
        }
    }
}
