using LootboundIsles.Combat;
using LootboundIsles.Progression;
using LootboundIsles.World;
using UnityEngine;
using UnityEngine.AI;

namespace LootboundIsles.Enemies
{
    [RequireComponent(typeof(Enemy))]
    [RequireComponent(typeof(Health))]
    public class EnemyDeath : MonoBehaviour
    {
        [Header("Death")]
        [SerializeField, Min(0f)] private float destroyDelay = 0.2f;

        [Header("DEBUG")]
        [SerializeField] private bool isDebug = true;

        private Enemy enemy;
        private Health health;

        private NavMeshAgent agent;
        private EnemyAI enemyAI;
        private EnemyCombat enemyCombat;

        private ExperienceSystem playerExperience;
        private ZoneProgression playerZoneProgression;

        private bool isDeathHandled;

        private void Awake()
        {
            enemy = GetComponent<Enemy>();
            health = GetComponent<Health>();

            agent = GetComponent<NavMeshAgent>();
            enemyAI = GetComponent<EnemyAI>();
            enemyCombat = GetComponent<EnemyCombat>();
        }

        private void Start()
        {
            FindPlayerSystems();
        }

        private void OnEnable()
        {
            health.Died += HandleDeath;
        }

        private void OnDisable()
        {
            health.Died -= HandleDeath;
        }

        private void FindPlayerSystems()
        {
            GameObject player =
                GameObject.FindGameObjectWithTag("Player");

            if (player == null)
            {
                Debug.LogError(
                    "EnemyDeath: Player с тегом 'Player' не найден.",
                    this
                );

                return;
            }

            playerExperience =
                player.GetComponent<ExperienceSystem>();

            if (playerExperience == null)
            {
                Debug.LogError(
                    "EnemyDeath: у Player отсутствует ExperienceSystem.",
                    player
                );
            }

            playerZoneProgression =
                player.GetComponent<ZoneProgression>();

            if (playerZoneProgression == null)
            {
                Debug.LogError(
                    "EnemyDeath: у Player отсутствует ZoneProgression.",
                    player
                );
            }
        }

        private void HandleDeath()
        {
            if (isDeathHandled)
                return;

            isDeathHandled = true;

            GiveExperienceReward();

            if (playerZoneProgression != null &&
                enemy.Definition != null)
            {
                playerZoneProgression.RegisterEnemyDefeat(
                    enemy.Definition
                );
            }

            if (isDebug)
            {
                Debug.Log(
                    $"{gameObject.name} погиб.",
                    this
                );
            }

            StopEnemy();

            Destroy(gameObject, destroyDelay);
        }

        private void GiveExperienceReward()
        {
            if (playerExperience == null)
                return;

            if (enemy.Definition == null)
                return;

            playerExperience.AddExperience(
                enemy.Definition.ExperienceReward
            );
        }

        private void StopEnemy()
        {
            if (agent != null)
            {
                if (agent.isOnNavMesh)
                {
                    agent.isStopped = true;
                    agent.ResetPath();
                }

                agent.enabled = false;
            }

            if (enemyAI != null)
            {
                enemyAI.enabled = false;
            }

            if (enemyCombat != null)
            {
                enemyCombat.enabled = false;
            }
        }
    }
}
