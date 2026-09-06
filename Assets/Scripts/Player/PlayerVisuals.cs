using UnityEngine;
using FoodSurvivors.Data;

namespace FoodSurvivors.Player
{
    [RequireComponent(typeof(PlayerController))]
    public class PlayerVisuals : MonoBehaviour
    {
        [Header("Visual Components")]
        public SpriteRenderer spriteRenderer;
        public Animator animator;

        private PlayerController playerController;

        private void Awake()
        {
            playerController = GetComponent<PlayerController>();

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

        private void Start()
        {
            ApplyChefVisuals();
        }

        public void ApplyChefVisuals()
        {
            if (playerController == null) return;

            ChefData chefData = playerController.chefData;
            if (chefData == null) return;

            if (spriteRenderer != null && chefData.characterSprite != null)
            {
                spriteRenderer.sprite = chefData.characterSprite;
                spriteRenderer.color = Color.white;
            }

            if (animator != null && chefData.animatorController != null)
            {
                animator.runtimeAnimatorController = chefData.animatorController;
            }
        }

        private void Update()
        {
            if (playerController == null) return;

            bool isWalking = playerController.IsWalking;
            float facingX = playerController.MoveInput.x;

            if (spriteRenderer != null && Mathf.Abs(facingX) > 0.01f)
            {
                spriteRenderer.flipX = facingX < 0f;
            }

            if (animator == null || animator.runtimeAnimatorController == null) return;

            if (HasAnimatorParameter("IsWalking"))
            {
                animator.SetBool("IsWalking", isWalking);
            }

            if (HasAnimatorParameter("FlipX") && Mathf.Abs(facingX) > 0.01f)
            {
                animator.SetBool("FlipX", facingX < 0f);
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
