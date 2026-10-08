using LootboundIsles.Combat;
using LootboundIsles.Player;
using UnityEngine;
using UnityEngine.AI;

namespace LootboundIsles.Enemies
{
    [RequireComponent(typeof(Enemy))]
    [RequireComponent(typeof(Health))]
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyAI : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField]
        private Transform target;

        [Header("DEBUG")]
        [SerializeField]
        private bool isDebug = true;

        private Enemy enemy;
        private Health health;
        private NavMeshAgent agent;
        private PlayerDeath playerDeath;

        private bool isChasing;
        private bool isReturningHome;
        private bool isSubscribedToPlayerDeath;

        private Vector3 homePosition;
        private float lastReturnProgressDistance;
        private float lastReturnProgressTime;

        public Transform Target => target;

        public bool IsChasing => isChasing;

        public bool IsReturningHome => isReturningHome;

        private void Awake()
        {
            enemy =
                GetComponent<Enemy>();

            health =
                GetComponent<Health>();

            agent =
                GetComponent<NavMeshAgent>();

            homePosition = transform.position;
        }

        private void OnEnable()
        {
            SubscribeToPlayerDeath();
        }

        private void OnDisable()
        {
            UnsubscribeFromPlayerDeath();
        }

        private void Start()
        {
            if (enemy.Definition == null)
            {
                enabled = false;
                return;
            }

            if (target == null)
            {
                GameObject player =
                    GameObject.FindGameObjectWithTag("Player");

                if (player != null)
                {
                    target =
                        player.transform;
                }
            }

            if (target == null)
            {
                Debug.LogError(
                    "EnemyAI: Player с тегом 'Player' не найден.",
                    this
                );

                enabled = false;
                return;
            }

            playerDeath =
                target.GetComponent<PlayerDeath>();

            if (playerDeath == null)
            {
                Debug.LogError(
                    "EnemyAI: у Player отсутствует PlayerDeath.",
                    target
                );

                enabled = false;
                return;
            }

            SubscribeToPlayerDeath();

            agent.speed =
                enemy.Definition.MoveSpeed;

            agent.stoppingDistance =
                enemy.Definition.AttackRange;
        }

        private void Update()
        {
            if (isReturningHome)
            {
                UpdateReturningHome();
                return;
            }

            UpdateChasing();
        }

        private void UpdateChasing()
        {
            if (!agent.isOnNavMesh ||
                target == null)
            {
                return;
            }

            if (GetDistanceToHomeSqr() >
                enemy.Definition.LeashRange *
                enemy.Definition.LeashRange)
            {
                BeginReturningHome();
                return;
            }

            float distanceSqr =
                GetDistanceToTargetSqr();

            float detectionRange =
                enemy.Definition.DetectionRange;

            float detectionRangeSqr =
                detectionRange * detectionRange;

            if (distanceSqr <= detectionRangeSqr)
            {
                StartChasing();
                FollowTarget();

                return;
            }

            StopChasing();
        }

        private float GetDistanceToTargetSqr()
        {
            Vector3 enemyPosition =
                transform.position;

            Vector3 targetPosition =
                target.position;

            enemyPosition.y = 0f;
            targetPosition.y = 0f;

            return (
                targetPosition -
                enemyPosition
            ).sqrMagnitude;
        }

        private float GetDistanceToHomeSqr()
        {
            Vector3 currentPosition = transform.position;
            Vector3 destination = homePosition;

            currentPosition.y = 0f;
            destination.y = 0f;

            return (destination - currentPosition).sqrMagnitude;
        }

        private void StartChasing()
        {
            if (isChasing)
                return;

            isChasing = true;

            agent.isStopped = false;

            if (isDebug)
            {
                Debug.Log(
                    $"{gameObject.name}: Player обнаружен.",
                    this
                );
            }
        }

        private void FollowTarget()
        {
            agent.SetDestination(
                target.position
            );
        }

        private void StopChasing()
        {
            if (!isChasing)
                return;

            isChasing = false;

            agent.isStopped = true;
            agent.ResetPath();

            if (isDebug)
            {
                Debug.Log(
                    $"{gameObject.name}: Player вышел " +
                    $"из радиуса обнаружения.",
                    this
                );
            }
        }

        public void BeginReturningHome()
        {
            if (isReturningHome || health.IsDead)
                return;

            isChasing = false;
            isReturningHome = true;

            lastReturnProgressDistance =
                Mathf.Sqrt(GetDistanceToHomeSqr());
            lastReturnProgressTime = Time.time;

            if (agent.isOnNavMesh)
            {
                agent.isStopped = false;
                agent.stoppingDistance =
                    enemy.Definition.HomeArrivalRadius;

                agent.SetDestination(homePosition);
            }

            if (isDebug)
            {
                Debug.Log(
                    $"{gameObject.name}: Returning Home.",
                    this
                );
            }
        }

        private void UpdateReturningHome()
        {
            if (health.IsDead)
                return;

            float arrivalRadius =
                enemy.Definition.HomeArrivalRadius;

            if (GetDistanceToHomeSqr() <=
                arrivalRadius * arrivalRadius)
            {
                CompleteReturnHome();
                return;
            }

            if (!agent.isOnNavMesh)
            {
                TeleportHomeAndReset();
                return;
            }

            if (!agent.hasPath && !agent.pathPending)
            {
                agent.SetDestination(homePosition);
            }

            UpdateReturnProgress();
        }

        private void UpdateReturnProgress()
        {
            float requiredProgress =
                enemy.Definition.ReturnProgressDistance;

            float currentDistanceToHome =
                Mathf.Sqrt(GetDistanceToHomeSqr());

            if (lastReturnProgressDistance -
                    currentDistanceToHome >=
                requiredProgress)
            {
                lastReturnProgressDistance =
                    currentDistanceToHome;
                lastReturnProgressTime = Time.time;
                return;
            }

            if (Time.time - lastReturnProgressTime <
                enemy.Definition.ReturnStuckTimeout)
            {
                return;
            }

            TeleportHomeAndReset();
        }

        private void CompleteReturnHome()
        {
            if (agent.isOnNavMesh)
            {
                agent.isStopped = true;
                agent.ResetPath();
                agent.stoppingDistance =
                    enemy.Definition.AttackRange;
            }

            health.RestoreFullHealth();
            isReturningHome = false;
            isChasing = false;

            if (isDebug)
            {
                Debug.Log(
                    $"{gameObject.name}: вернулся домой и восстановил HP.",
                    this
                );
            }
        }

        private void TeleportHomeAndReset()
        {
            if (agent.isOnNavMesh)
            {
                bool warpSucceeded =
                    agent.Warp(homePosition);

                if (!warpSucceeded)
                {
                    transform.position = homePosition;
                }
            }
            else
            {
                transform.position = homePosition;
            }

            CompleteReturnHome();
        }

        private void HandlePlayerDeath()
        {
            if (!isChasing)
                return;

            BeginReturningHome();
        }

        private void SubscribeToPlayerDeath()
        {
            if (playerDeath == null ||
                isSubscribedToPlayerDeath)
            {
                return;
            }

            playerDeath.DeathStarted += HandlePlayerDeath;
            isSubscribedToPlayerDeath = true;
        }

        private void UnsubscribeFromPlayerDeath()
        {
            if (playerDeath == null ||
                !isSubscribedToPlayerDeath)
            {
                return;
            }

            playerDeath.DeathStarted -= HandlePlayerDeath;
            isSubscribedToPlayerDeath = false;
        }
    }
}
