using UnityEngine;
using LootboundIsles.Weapons;

namespace LootboundIsles.Player
{
    public sealed class PlayerAnimationDriver : MonoBehaviour
    {
        private static readonly int SpeedParameter =
            Animator.StringToHash("Speed");

        private static readonly int AttackTrigger =
            Animator.StringToHash("Attack");

        private static readonly int DeathTrigger =
            Animator.StringToHash("Death");

        private static readonly int DieTrigger =
            Animator.StringToHash("Die");

        private static readonly int IdleState =
            Animator.StringToHash("Base Layer.Idle");

        private static readonly int WaitingState =
            Animator.StringToHash("Base Layer.0000_01_waiting");

        private static readonly int[] BowAttackStates =
        {
            Animator.StringToHash("Base Layer.0000_277"),
            Animator.StringToHash("Base Layer.0000_278"),
            Animator.StringToHash("Base Layer.0000_279")
        };

        [Header("Animation")]
        [SerializeField] private Animator animator;
        [SerializeField, Min(0f)] private float speedDampTime = 0.08f;

        private CharacterController characterController;
        private WeaponController weaponController;
        private PlayerDeath playerDeath;
        private int deathTriggerParameter;
        private int idleState;
        private int nextBowAttackState;

        private void Awake()
        {
            if (animator == null)
                animator = GetComponentInChildren<Animator>(true);

            characterController =
                GetComponentInParent<CharacterController>();

            weaponController =
                GetComponentInParent<WeaponController>();

            playerDeath =
                GetComponentInParent<PlayerDeath>();

            deathTriggerParameter = HasParameter(
                DieTrigger,
                AnimatorControllerParameterType.Trigger)
                ? DieTrigger
                : DeathTrigger;

            idleState = animator != null && animator.HasState(0, WaitingState)
                ? WaitingState
                : IdleState;

            if (animator == null ||
                characterController == null ||
                weaponController == null ||
                playerDeath == null)
            {
                Debug.LogError(
                    "PlayerAnimationDriver requires an Animator, " +
                    "CharacterController, WeaponController and PlayerDeath.",
                    this
                );

                enabled = false;
                return;
            }

            if (!HasParameter(SpeedParameter, AnimatorControllerParameterType.Float) ||
                !HasParameter(AttackTrigger, AnimatorControllerParameterType.Trigger) ||
                !HasParameter(
                    deathTriggerParameter,
                    AnimatorControllerParameterType.Trigger))
            {
                Debug.LogError(
                    "PlayerAnimationDriver Animator Controller is missing " +
                    "Speed, Attack or Death parameters.",
                    this
                );

                enabled = false;
            }
        }

        private void OnEnable()
        {
            if (weaponController != null)
                weaponController.AttackStarted += HandleAttackStarted;

            if (playerDeath != null)
            {
                playerDeath.DeathStarted += HandleDeathStarted;
                playerDeath.Respawned += HandleRespawned;
            }
        }

        private void OnDisable()
        {
            if (weaponController != null)
                weaponController.AttackStarted -= HandleAttackStarted;

            if (playerDeath != null)
            {
                playerDeath.DeathStarted -= HandleDeathStarted;
                playerDeath.Respawned -= HandleRespawned;
            }
        }

        private void LateUpdate()
        {
            if (animator == null || characterController == null)
                return;

            Vector3 horizontalVelocity = characterController.velocity;
            horizontalVelocity.y = 0f;

            animator.SetFloat(
                SpeedParameter,
                horizontalVelocity.magnitude,
                speedDampTime,
                Time.deltaTime
            );
        }

        private bool HasParameter(
            int parameterHash,
            AnimatorControllerParameterType parameterType)
        {
            foreach (AnimatorControllerParameter parameter in animator.parameters)
            {
                if (parameter.nameHash == parameterHash &&
                    parameter.type == parameterType)
                {
                    return true;
                }
            }

            return false;
        }

        private void HandleAttackStarted(WeaponType weaponType)
        {
            if (weaponType == WeaponType.Bow && HasBowAttackStates())
            {
                int state = BowAttackStates[nextBowAttackState];
                nextBowAttackState =
                    (nextBowAttackState + 1) % BowAttackStates.Length;

                animator.ResetTrigger(AttackTrigger);
                animator.CrossFadeInFixedTime(state, 0.06f, 0, 0f);
                return;
            }

            animator.ResetTrigger(AttackTrigger);
            animator.SetTrigger(AttackTrigger);
        }

        private bool HasBowAttackStates()
        {
            for (int i = 0; i < BowAttackStates.Length; i++)
            {
                if (!animator.HasState(0, BowAttackStates[i]))
                    return false;
            }

            return true;
        }

        private void HandleDeathStarted()
        {
            animator.ResetTrigger(AttackTrigger);
            animator.SetTrigger(deathTriggerParameter);
        }

        private void HandleRespawned()
        {
            animator.ResetTrigger(deathTriggerParameter);
            animator.ResetTrigger(AttackTrigger);
            animator.SetFloat(SpeedParameter, 0f);
            animator.Play(idleState, 0, 0f);
        }
    }
}
