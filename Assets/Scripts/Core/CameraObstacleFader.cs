using System.Collections.Generic;
using UnityEngine;

namespace FoodSurvivors.Core
{
    /// <summary>
    /// Ультра-легкий скрипт без создания материалов и без падения FPS.
    /// Проверяет перекрытия с оптимизированной частотой и скрывает только меши над головой.
    /// </summary>
    public class CameraObstacleFader : MonoBehaviour
    {
        [Header("Settings")]
        public float checkInterval = 0.08f; // Проверка 12 раз в секунду вместо каждого кадра (0 лагов!)
        public float sphereRadius = 0.7f;
        public float heightThreshold = 2.0f; // Скрывать только то, что выше головы игрока

        private Transform playerTransform;
        private float timer = 0f;

        // Кэш скрытых объектов
        private HashSet<Renderer> currentlyHidden = new HashSet<Renderer>();
        private HashSet<Renderer> hitsThisTick = new HashSet<Renderer>();

        private void Start()
        {
            FindPlayer();
        }

        private void FindPlayer()
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) playerTransform = p.transform;
        }

        private void LateUpdate()
        {
            if (playerTransform == null)
            {
                FindPlayer();
                return;
            }

            timer += Time.deltaTime;
            if (timer < checkInterval) return;
            timer = 0f;

            Vector3 camPos = transform.position;
            Vector3 targetPos = playerTransform.position + Vector3.up * 1.0f;
            Vector3 direction = targetPos - camPos;
            float distance = direction.magnitude;

            hitsThisTick.Clear();

            // Быстрый каст сферы
            RaycastHit[] hits = Physics.SphereCastAll(camPos, sphereRadius, direction.normalized, distance);
            float minHeight = playerTransform.position.y + heightThreshold;

            foreach (var hit in hits)
            {
                if (hit.collider.CompareTag("Player") || hit.collider.CompareTag("Enemy") ||
                    hit.collider.gameObject.name.Contains("Ground") || hit.collider.gameObject.name.Contains("Road") ||
                    hit.collider.gameObject.name.Contains("Floor"))
                    continue;

                // Берем рендереры перекрытия
                Renderer rend = hit.collider.GetComponent<Renderer>();
                if (rend == null) rend = hit.collider.GetComponentInChildren<Renderer>();

                if (rend != null && rend.bounds.center.y >= minHeight)
                {
                    hitsThisTick.Add(rend);

                    // Скрываем без клонирования материалов (0 лагов!)
                    if (rend.enabled)
                    {
                        rend.enabled = false;
                        currentlyHidden.Add(rend);
                    }
                }
            }

            // Возвращаем все объекты, которые больше не заслоняют обзор
            List<Renderer> toRestore = new List<Renderer>();
            foreach (var rend in currentlyHidden)
            {
                if (rend != null && !hitsThisTick.Contains(rend))
                {
                    rend.enabled = true;
                    toRestore.Add(rend);
                }
            }

            foreach (var r in toRestore)
            {
                currentlyHidden.Remove(r);
            }
        }

        private void OnDisable()
        {
            // Возвращаем все меши при отключении скрипта
            foreach (var rend in currentlyHidden)
            {
                if (rend != null) rend.enabled = true;
            }
            currentlyHidden.Clear();
        }
    }
}