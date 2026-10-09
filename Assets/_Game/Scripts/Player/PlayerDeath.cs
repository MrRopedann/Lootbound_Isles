using System;
using System.Collections;
using LootboundIsles.Combat;
using LootboundIsles.Weapons;
using UnityEngine;

namespace LootboundIsles.Player
{
    [RequireComponent(typeof(Health))]
    [RequireComponent(typeof(PlayerMovement))]
    [RequireComponent(typeof(WeaponController))]
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(CombatState))]
    public class PlayerDeath : MonoBehaviour
    {
        [Header("Respawn")]
        [SerializeField]
        private Transform spawnPoint;

        [SerializeField, Min(0f)]
        private float respawnDelay = 2f;

        [SerializeField, Range(0.01f, 1f)]
        private float respawnHealthFraction = 0.1f;

        [Header("DEBUG")]
        [SerializeField]
        private bool isDebug = true;

        private Health health;
        private PlayerMovement playerMovement;
        private WeaponController weaponController;
        private CharacterController characterController;
        private CombatState combatState;

        private bool isDeathHandled;

        public bool IsDead => isDeathHandled;

        public event Action DeathStarted;
        public event Action Respawned;

        private void Awake()
        {
            health =
                GetComponent<Health>();

            playerMovement =
                GetComponent<PlayerMovement>();

            weaponController =
                GetComponent<WeaponController>();

            characterController =
                GetComponent<CharacterController>();

            combatState =
                GetComponent<CombatState>();
        }

        private void OnEnable()
        {
            health.Died += HandleDeath;
        }

        private void OnDisable()
        {
            health.Died -= HandleDeath;
        }

        private void HandleDeath()
        {
            if (isDeathHandled)
                return;

            isDeathHandled = true;

            playerMovement.enabled = false;
            weaponController.enabled = false;

            combatState.ResetCombatState();

            DeathStarted?.Invoke();

            if (isDebug)
            {
                Debug.Log(
                    $"PLAYER DIED | Respawn через " +
                    $"{respawnDelay:F1} сек.",
                    this
                );
            }

            StartCoroutine(RespawnRoutine());
        }

        private IEnumerator RespawnRoutine()
        {
            yield return new WaitForSeconds(respawnDelay);

            Respawn();
        }

        private void Respawn()
        {
            if (spawnPoint == null)
            {
                Debug.LogError(
                    "PlayerDeath: Spawn Point не назначен.",
                    this
                );

                isDeathHandled = false;
                return;
            }

            characterController.enabled = false;

            transform.position =
                spawnPoint.position;

            transform.rotation =
                spawnPoint.rotation;

            characterController.enabled = true;

            float respawnHealth =
                health.MaxHealth *
                respawnHealthFraction;

            health.Revive(respawnHealth);

            playerMovement.enabled = true;
            weaponController.enabled = true;

            isDeathHandled = false;

            Respawned?.Invoke();

            if (isDebug)
            {
                Debug.Log(
                    $"PLAYER RESPAWNED | " +
                    $"HP: {health.CurrentHealth}/" +
                    $"{health.MaxHealth}",
                    this
                );
            }
        }
    }
}
