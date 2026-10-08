using LootboundIsles.Combat;
using LootboundIsles.Enemies;
using LootboundIsles.Player;
using UnityEngine;

namespace LootboundIsles.Weapons
{
    public class PlayerProjectile : MonoBehaviour
    {
        [Header("Impact")]
        [SerializeField, Min(0.01f)]
        private float impactDistance = 0.15f;

        [Header("DEBUG")]
        [SerializeField]
        private bool isDebug;

        private Enemy target;
        private CombatState ownerCombatState;
        private CombatResolver combatResolver;
        private PlayerDeath ownerDeath;

        private float speed;
        private float lifetime;
        private float elapsedTime;

        private float snapshotDamage;
        private float snapshotAccuracy;
        private float snapshotCriticalChance;

        private bool isInitialized;

        private void Start()
        {
            if (isInitialized)
                return;

            Debug.LogError(
                "PlayerProjectile: projectile создан без Initialize.",
                this
            );

            Destroy(gameObject);
        }

        public void Initialize(
            Enemy newTarget,
            CombatState newOwnerCombatState,
            CombatResolver newCombatResolver,
            PlayerDeath newOwnerDeath,
            float newSpeed,
            float newLifetime,
            float damage,
            float accuracy,
            float criticalChance)
        {
            target = newTarget;
            ownerCombatState = newOwnerCombatState;
            combatResolver = newCombatResolver;
            ownerDeath = newOwnerDeath;

            speed = Mathf.Max(0.01f, newSpeed);
            lifetime = Mathf.Max(0.01f, newLifetime);

            snapshotDamage = Mathf.Max(0f, damage);
            snapshotAccuracy = Mathf.Max(0f, accuracy);
            snapshotCriticalChance = Mathf.Clamp01(criticalChance);

            if (ownerDeath != null)
            {
                ownerDeath.DeathStarted += HandleOwnerDeath;
            }

            isInitialized = true;
        }

        private void Update()
        {
            if (!isInitialized)
                return;

            if (!IsTargetValid())
            {
                Destroy(gameObject);
                return;
            }

            elapsedTime += Time.deltaTime;

            if (elapsedTime >= lifetime)
            {
                Destroy(gameObject);
                return;
            }

            Vector3 targetPosition = target.transform.position;
            Vector3 direction = targetPosition - transform.position;

            float movementDistance = speed * Time.deltaTime;

            if (direction.sqrMagnitude <=
                impactDistance * impactDistance ||
                movementDistance >= direction.magnitude)
            {
                ResolveImpact();
                return;
            }

            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPosition,
                movementDistance
            );

            if (direction.sqrMagnitude > 0f)
            {
                transform.rotation = Quaternion.LookRotation(
                    direction,
                    Vector3.up
                );
            }
        }

        private bool IsTargetValid()
        {
            return target != null &&
                   target.isActiveAndEnabled &&
                   target.Health != null &&
                   !target.Health.IsDead;
        }

        private void ResolveImpact()
        {
            if (!IsTargetValid())
            {
                Destroy(gameObject);
                return;
            }

            EnemyStats targetStats =
                target.GetComponent<EnemyStats>();

            if (targetStats == null ||
                combatResolver == null ||
                ownerCombatState == null)
            {
                Debug.LogError(
                    "PlayerProjectile: отсутствуют зависимости для impact.",
                    this
                );

                Destroy(gameObject);
                return;
            }

            ownerCombatState.RegisterCombatEvent();

            AttackResult result =
                combatResolver.ResolveAttack(
                    snapshotDamage,
                    snapshotAccuracy,
                    snapshotCriticalChance,
                    targetStats.Defense,
                    targetStats.Evasion
                );

            if (result.IsHit)
            {
                target.Health.TakeDamage(result.Damage);
            }

            if (isDebug)
            {
                string outcome = result.IsHit
                    ? $"{result.Damage} damage" +
                      (result.IsCritical ? " | CRITICAL!" : string.Empty)
                    : "MISS";

                Debug.Log(
                    $"Bow projectile → {target.gameObject.name}: {outcome}",
                    this
                );
            }

            Destroy(gameObject);
        }

        private void HandleOwnerDeath()
        {
            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            if (ownerDeath != null)
            {
                ownerDeath.DeathStarted -= HandleOwnerDeath;
            }
        }
    }
}
