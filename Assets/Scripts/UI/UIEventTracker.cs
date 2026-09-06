using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.InputSystem;

namespace FoodSurvivors.UI
{
    /// <summary>
    /// Диагностический скрипт отслеживания кликов для Нового Input System.
    /// Повесь этот скрипт на Canvas в MainMenuScene.
    /// </summary>
    public class UIEventTracker : MonoBehaviour
    {
        private void Start()
        {
            Debug.Log("=============================================");
            Debug.Log("[UIEventTracker] ЗАПУСК ДИАГНОСТИКИ ИНТЕРФЕЙСА");
            Debug.Log("=============================================");

            CheckEventSystem();
            CheckGraphicRaycaster();
            AttachLoggersToAllButtons();
        }

        private void CheckEventSystem()
        {
            EventSystem es = Object.FindFirstObjectByType<EventSystem>();
            if (es == null)
            {
                Debug.LogError("[UIEventTracker] ❌ КРИТИЧЕСКАЯ ОШИБКА: На сцене отсутствует EventSystem! Кнопки НЕ будут реагировать на мышь.");
            }
            else
            {
                Debug.Log($"[UIEventTracker] ✅ EventSystem найден: '{es.gameObject.name}'. Включен: {es.enabled}");
            }
        }

        private void CheckGraphicRaycaster()
        {
            GraphicRaycaster raycaster = GetComponent<GraphicRaycaster>();
            if (raycaster == null)
            {
                Debug.LogError("[UIEventTracker] ❌ ОШИБКА: На Canvas отсутствует компонент GraphicRaycaster!");
            }
            else
            {
                Debug.Log($"[UIEventTracker] ✅ GraphicRaycaster найден на Canvas. Включен: {raycaster.enabled}");
            }
        }

        private void AttachLoggersToAllButtons()
        {
            Button[] buttons = GetComponentsInChildren<Button>(true);
            Debug.Log($"[UIEventTracker] Найдено кнопок на Canvas: {buttons.Length}");

            for (int i = 0; i < buttons.Length; i++)
            {
                Button btn = buttons[i];
                string buttonName = btn.gameObject.name;

                // Динамический логгер при клике
                btn.onClick.AddListener(() =>
                {
                    Debug.Log($"[UIEventTracker] 🖱️ КЛИК УСПЕШНО ЗАЗАРЕГИСТРИРОВАН по кнопке: '{buttonName}'!");
                });

                Debug.Log($"   -> Кнопка [{i + 1}/{buttons.Length}]: '{buttonName}' | Активна: {btn.gameObject.activeInHierarchy} | Interactable: {btn.interactable}");
            }
        }

        private void Update()
        {
            if (Mouse.current == null) return;

            // Считывание нажатия мыши через новый Input System
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                if (EventSystem.current == null)
                {
                    Debug.LogError("[UIEventTracker] EventSystem.current равен NULL!");
                    return;
                }

                PointerEventData eventData = new PointerEventData(EventSystem.current);
                eventData.position = Mouse.current.position.ReadValue();

                List<RaycastResult> results = new List<RaycastResult>();
                EventSystem.current.RaycastAll(eventData, results);

                if (results.Count > 0)
                {
                    Debug.Log($"[UIEventTracker] 🔍 ЛКМ нажата над UI-объектом: '{results[0].gameObject.name}' (Всего перехвачено слоев: {results.Count})");
                    for (int i = 0; i < results.Count; i++)
                    {
                        Debug.Log($"      - Слой [{i}]: '{results[i].gameObject.name}'");
                    }
                }
                else
                {
                    Debug.LogWarning("[UIEventTracker] ⚠️ ЛКМ нажата, но Raycast НЕ зацепил ни один UI-элемент!");
                }
            }
        }
    }
}