using UnityEngine;

namespace FoodSurvivors.Core
{
    public class CameraFollow : MonoBehaviour
    {
        [Header("Target Follow")]
        public Transform target;

        [Header("Offset Settings")]
        public Vector3 offset = new Vector3(0f, 18f, -9f); // Высота и отдаление
        public float smoothSpeed = 8f;

        [Header("Rotation Settings")]
        public bool lockRotation = true;
        public Vector3 rotationEuler = new Vector3(65f, 0f, 0f); // Угол обзора сверху

        private void Start()
        {
            if (lockRotation)
            {
                transform.rotation = Quaternion.Euler(rotationEuler);
            }
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null) target = playerObj.transform;
                return;
            }

            // Плавное следование за поваром
            Vector3 desiredPosition = target.position + offset;
            transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

            // Фиксация нужного угла обзора (больше нет мешающего LookAt!)
            if (lockRotation)
            {
                transform.rotation = Quaternion.Euler(rotationEuler);
            }
        }
    }
}