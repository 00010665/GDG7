using UnityEngine;

namespace FoodSurvivors.Core
{
    public class CameraFollow : MonoBehaviour
    {
        [Header("Target Follow")]
        public Transform target;

        [Header("Offset Settings")]
        public Vector3 offset = new Vector3(0, 10, -5);
        public float smoothSpeed = 5f;

        private void LateUpdate()
        {
            if (target == null) return;

            Vector3 desiredPosition = target.position + offset;
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
            transform.position = smoothedPosition;

            transform.LookAt(target.position);
        }
    }
}
