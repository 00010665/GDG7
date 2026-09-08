using System.Collections;
using UnityEngine;

namespace FoodSurvivors.Core
{
    public class DamageFlash : MonoBehaviour
    {
        [Header("Flash Settings")]
        public Color flashColor = Color.red;
        public float flashDuration = 0.2f;

        private Renderer[] renderers;
        private SpriteRenderer[] spriteRenderers;
        private Color[] originalColors;
        private Color[] originalSpriteColors;
        private Coroutine flashCoroutine;

        private void Awake()
        {
            CacheOriginalColors();
        }

        public void CacheOriginalColors()
        {
            renderers = GetComponentsInChildren<Renderer>();
            originalColors = new Color[renderers.Length];

            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null && renderers[i].material != null)
                {
                    if (renderers[i].material.HasProperty("_BaseColor"))
                        originalColors[i] = renderers[i].material.GetColor("_BaseColor");
                    else if (renderers[i].material.HasProperty("_Color"))
                        originalColors[i] = renderers[i].material.color;
                    else
                        originalColors[i] = Color.white;
                }
            }

            spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
            originalSpriteColors = new Color[spriteRenderers.Length];

            for (int i = 0; i < spriteRenderers.Length; i++)
            {
                if (spriteRenderers[i] != null)
                    originalSpriteColors[i] = spriteRenderers[i].color;
            }
        }

        public void CallFlash()
        {
            if (flashCoroutine != null)
            {
                StopCoroutine(flashCoroutine);
            }

            flashCoroutine = StartCoroutine(DoFlash());
        }

        private IEnumerator DoFlash()
        {
            // Устанавливаем вспышку красного цвета
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null && renderers[i].material != null)
                {
                    if (renderers[i].material.HasProperty("_BaseColor"))
                        renderers[i].material.SetColor("_BaseColor", flashColor);
                    else if (renderers[i].material.HasProperty("_Color"))
                        renderers[i].material.color = flashColor;
                }
            }

            for (int i = 0; i < spriteRenderers.Length; i++)
            {
                if (spriteRenderers[i] != null)
                    spriteRenderers[i].color = flashColor;
            }

            // Длительность вспышки (0.2 сек)
            yield return new WaitForSecondsRealtime(flashDuration);

            // Возвращаем исходный цвет
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null && renderers[i].material != null)
                {
                    if (renderers[i].material.HasProperty("_BaseColor"))
                        renderers[i].material.SetColor("_BaseColor", originalColors[i]);
                    else if (renderers[i].material.HasProperty("_Color"))
                        renderers[i].material.color = originalColors[i];
                }
            }

            for (int i = 0; i < spriteRenderers.Length; i++)
            {
                if (spriteRenderers[i] != null)
                    spriteRenderers[i].color = originalSpriteColors[i];
            }

            flashCoroutine = null;
        }
    }
}