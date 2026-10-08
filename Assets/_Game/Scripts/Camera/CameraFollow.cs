using UnityEngine;
using UnityEngine.InputSystem;

namespace LootboundIsles.CameraSystem
{
    public class CameraFollow : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField]
        private Transform target;

        [Header("Follow Settings")]
        [SerializeField]
        private float distance = 12.8f;

        [SerializeField]
        private float height = 10f;

        [SerializeField, Min(0f)]
        private float smoothSpeed = 8f;

        [Header("Rotation")]
        [SerializeField]
        private float rotationSpeed = 0.15f;

        [SerializeField]
        private bool requireRightMouseButton = true;

        [SerializeField]
        private float startYaw = 0f;

        [Header("DEBUG")]
        [SerializeField]
        private bool isDebug;

        private float currentYaw;

        public Transform Target => target;

        private void Awake()
        {
            currentYaw = startYaw;
        }

        private void Start()
        {
            SnapToTarget();
        }

        private void Update()
        {
            ReadRotationInput();
        }

        private void LateUpdate()
        {
            if (target == null)
                return;

            Vector3 targetPosition =
                GetCameraPosition();

            transform.position =
                Vector3.Lerp(
                    transform.position,
                    targetPosition,
                    smoothSpeed * Time.deltaTime
                );

            LookAtTarget();
        }

        private void ReadRotationInput()
        {
            if (Mouse.current == null)
                return;

            if (requireRightMouseButton &&
                !Mouse.current.rightButton.isPressed)
            {
                return;
            }

            float mouseX =
                Mouse.current.delta.ReadValue().x;

            currentYaw +=
                mouseX * rotationSpeed;
        }

        private Vector3 GetCameraPosition()
        {
            Quaternion rotation =
                Quaternion.Euler(
                    0f,
                    currentYaw,
                    0f
                );

            Vector3 direction =
                rotation *
                Vector3.back;

            Vector3 horizontalOffset =
                direction * distance;

            return target.position +
                   horizontalOffset +
                   Vector3.up * height;
        }

        private void LookAtTarget()
        {
            Vector3 lookDirection =
                target.position -
                transform.position;

            if (lookDirection.sqrMagnitude <= 0f)
                return;

            transform.rotation =
                Quaternion.LookRotation(
                    lookDirection,
                    Vector3.up
                );
        }

        public void SnapToTarget()
        {
            if (target == null)
            {
                Debug.LogError(
                    "CameraFollow: Target не назначен.",
                    this
                );

                return;
            }

            transform.position =
                GetCameraPosition();

            LookAtTarget();

            if (isDebug)
            {
                Debug.Log(
                    $"CameraFollow: камера мгновенно " +
                    $"синхронизирована с {target.name}.",
                    this
                );
            }
        }
    }
}