using LootboundIsles.Combat;
using LootboundIsles.Player;
using UnityEngine;

namespace LootboundIsles.Enemies
{
    [RequireComponent(typeof(Enemy))]
    [RequireComponent(typeof(EnemyAI))]

    public class EnemyCombat : MonoBehaviour
    {
        [Header("DEBUG")]
        [SerializeField] private bool isDebug = true;

        private Enemy enemy;
        private EnemyAI enemyAI;

        private Health targetHealth;
        private CombatState targetCombatState;
        private CombatResolver targetCombatResolver;
        private PlayerStats targetStats;
        private float attackTimer;

        private void Awake()
        {
            enemy = GetComponent<Enemy>();
            enemyAI = GetComponent<EnemyAI>();
        }

        private void Update()
        {
            if (enemy.Definition == null)
                return;

            if (enemyAI.IsReturningHome)
                return;

            TryFindTargetHealth();

            if (targetHealth == null || targetHealth.IsDead)
                return;

            attackTimer -= Time.deltaTime;

            TryAttack();
        }

        private void TryFindTargetHealth()
        {
            if (targetHealth != null)
                return;

            Transform target = enemyAI.Target;

            if (target == null)
                return;

            targetHealth = target.GetComponent<Health>();

            targetCombatState =
                target.GetComponent<CombatState>();

            targetCombatResolver =
                target.GetComponent<CombatResolver>();

            targetStats =
                target.GetComponent<PlayerStats>();

            if (targetHealth == null)
            {
                if (isDebug)
                {
                    Debug.LogError(
                        "EnemyCombat: у цели отсутствует компонент Health.",
                        target
                    );
                }

                enabled = false;
                return;
            }

            if (targetCombatState == null)
            {
                Debug.LogError(
                    "EnemyCombat: у цели отсутствует компонент CombatState.",
                    target
                );

                enabled = false;
                return;
            }

            if (targetCombatResolver == null ||
                targetStats == null)
            {
                Debug.LogError(
                    "EnemyCombat: у цели отсутствует CombatResolver или PlayerStats.",
                    target
                );

                enabled = false;
            }
        }

        private void TryAttack()
        {
            Transform target = enemyAI.Target;

            if (target == null)
                return;

            Vector3 enemyPosition = transform.position;
            Vector3 targetPosition = target.position;

            enemyPosition.y = 0f;
            targetPosition.y = 0f;

            float distance = Vector3.Distance(
                enemyPosition,
                targetPosition
            );

            if (distance > enemy.Definition.AttackRange)
                return;

            if (attackTimer > 0f)
                return;

            Attack();
        }

        private void Attack()
        {
            targetCombatState.RegisterCombatEvent();

            attackTimer = enemy.Definition.AttackCooldown;

            AttackResult result =
                targetCombatResolver.ResolveAttack(
                    enemy.Definition.Damage,
                    enemy.Definition.Accuracy,
                    enemy.Definition.CriticalChance,
                    targetStats.Defense,
                    targetStats.Evasion
                );

            if (!result.IsHit)
            {
                if (isDebug)
                {
                    Debug.Log(
                        $"{gameObject.name} промахнулся по Player | " +
                        $"Hit Chance: {result.HitChance:P0}",
                        this
                    );
                }

                return;
            }

            targetHealth.TakeDamage(result.Damage);

            if (isDebug)
            {
                Debug.Log(
                    $"{gameObject.name} атакует Player: " +
                    $"{result.Damage} урона" +
                    (result.IsCritical ? " | CRITICAL!" : string.Empty) +
                    " | " +
                    $"Player HP: {targetHealth.CurrentHealth}/{targetHealth.MaxHealth}"
                );
            }
        }
    }
}
