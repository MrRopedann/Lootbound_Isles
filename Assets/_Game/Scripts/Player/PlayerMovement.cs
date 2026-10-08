using UnityEngine;
using UnityEngine.InputSystem;

namespace LootboundIsles.Player
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerStats))]
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float rotationSpeed = 12f;

        [Header("Gravity")]
        [SerializeField] private float gravity = -20f;

        [Header("References")]
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private Transform visual;

        [Header("DEBUG")]
        [SerializeField] private bool isDebug = true;

        private CharacterController characterController;
        private PlayerStats playerStats;

        private InputAction moveAction;
        private float verticalVelocity;

        private void Awake()
        {
            characterController =
                GetComponent<CharacterController>();

            playerStats =
                GetComponent<PlayerStats>();

            if (cameraTransform == null &&
                Camera.main != null)
            {
                cameraTransform =
                    Camera.main.transform;
            }
        }

        private void OnEnable()
        {
            moveAction =
                InputSystem.actions.FindAction(
                    "Player/Move"
                );

            if (moveAction == null)
            {
                Debug.LogError(
                    "PlayerMovement: Input Action " +
                    "'Player/Move' не найден.",
                    this
                );

                enabled = false;
                return;
            }

            moveAction.Enable();
        }

        private void OnDisable()
        {
            moveAction?.Disable();
        }

        private void Update()
        {
            Move();
        }

        private void Move()
        {
            Vector2 input =
                moveAction.ReadValue<Vector2>();

            Vector3 cameraForward =
                cameraTransform.forward;

            Vector3 cameraRight =
                cameraTransform.right;

            cameraForward.y = 0f;
            cameraRight.y = 0f;

            cameraForward.Normalize();
            cameraRight.Normalize();

            Vector3 movement =
                cameraForward * input.y +
                cameraRight * input.x;

            if (movement.sqrMagnitude > 1f)
                movement.Normalize();

            RotateVisual(movement);

            if (characterController.isGrounded &&
                verticalVelocity < 0f)
            {
                verticalVelocity = -2f;
            }

            verticalVelocity +=
                gravity * Time.deltaTime;

            float finalMoveSpeed =
                playerStats.GetFinalMovementSpeed(
                    moveSpeed
                );

            Vector3 velocity =
                movement * finalMoveSpeed;

            velocity.y = verticalVelocity;

            characterController.Move(
                velocity * Time.deltaTime
            );
        }

        private void RotateVisual(Vector3 direction)
        {
            if (visual == null)
                return;

            if (direction.sqrMagnitude < 0.001f)
                return;

            Quaternion targetRotation =
                Quaternion.LookRotation(
                    direction,
                    Vector3.up
                );

            visual.rotation =
                Quaternion.Slerp(
                    visual.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime
                );
        }
    }
}