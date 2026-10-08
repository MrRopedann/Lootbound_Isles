using System;
using System.Collections;
using System.Collections.Generic;
using LootboundIsles.Enemies;
using UnityEngine;
using UnityEngine.AI;

namespace LootboundIsles.Spawning
{
    public class EnemySpawnPoint : MonoBehaviour
    {
        [Header("Identity")]
        [SerializeField]
        private string spawnerId;

        [Header("Definition")]
        [SerializeField]
        private SpawnCycleDefinition spawnDefinition;

        [Header("Fallback Enemy")]
        [SerializeField]
        private Enemy enemyPrefab;

        [Header("Fallback Wave Settings")]
        [SerializeField, Min(1)]
        private int enemiesPerWave = 5;

        [SerializeField, Min(1)]
        private int wavesPerCycle = 3;

        [SerializeField, Min(0f)]
        private float enemySpawnDelay = 0.5f;

        [SerializeField, Min(0f)]
        private float waveDelay = 10f;

        [Header("Fallback Cycle Settings")]
        [SerializeField, Min(0f)]
        private float cycleCooldownMinutes = 30f;

        [Header("Fallback Spawn Settings")]
        [SerializeField, Min(0f)]
        private float spawnRadius = 2f;

        [Header("Fallback NavMesh")]
        [SerializeField, Min(0.1f)]
        private float navMeshSampleDistance = 3f;

        [Header("DEBUG")]
        [SerializeField]
        private bool isDebug;

        private readonly List<Enemy>
            spawnedEnemies = new();

        private int currentWave;
        private float waveTimer;

        private DateTime nextCycleTimeUtc;

        private SpawnerState state;

        private Enemy ConfiguredEnemyPrefab =>
            spawnDefinition != null
                ? spawnDefinition.EnemyPrefab
                : enemyPrefab;

        private int ConfiguredEnemiesPerWave =>
            spawnDefinition != null
                ? spawnDefinition.EnemiesPerWave
                : enemiesPerWave;

        private int ConfiguredWavesPerCycle =>
            spawnDefinition != null
                ? spawnDefinition.WavesPerCycle
                : wavesPerCycle;

        private float ConfiguredEnemySpawnDelay =>
            spawnDefinition != null
                ? spawnDefinition.EnemySpawnDelay
                : enemySpawnDelay;

        private float ConfiguredWaveDelay =>
            spawnDefinition != null
                ? spawnDefinition.WaveDelay
                : waveDelay;

        private float ConfiguredCycleCooldownMinutes =>
            spawnDefinition != null
                ? spawnDefinition.CycleCooldownMinutes
                : cycleCooldownMinutes;

        private float ConfiguredSpawnRadius =>
            spawnDefinition != null
                ? spawnDefinition.SpawnRadius
                : spawnRadius;

        private float ConfiguredNavMeshSampleDistance =>
            spawnDefinition != null
                ? spawnDefinition.NavMeshSampleDistance
                : navMeshSampleDistance;

        public int AliveCount =>
            spawnedEnemies.Count;

        public int CurrentWave =>
            currentWave;

        public int WavesPerCycle =>
            ConfiguredWavesPerCycle;

        public bool IsOnCooldown =>
            state == SpawnerState.Cooldown;

        public TimeSpan RemainingCooldown
        {
            get
            {
                if (!IsOnCooldown)
                    return TimeSpan.Zero;

                TimeSpan remaining =
                    nextCycleTimeUtc -
                    DateTime.UtcNow;

                if (remaining <= TimeSpan.Zero)
                    return TimeSpan.Zero;

                return remaining;
            }
        }

        private enum SpawnerState
        {
            Ready,
            SpawningWave,
            WaveActive,
            WaitingForNextWave,
            Cooldown
        }

        private void Start()
        {
            if (!ValidateConfiguration())
                return;

            LoadState();
        }

        private void Update()
        {
            CleanupDestroyedEnemies();

            switch (state)
            {
                case SpawnerState.WaveActive:
                    UpdateActiveWave();
                    break;

                case SpawnerState.WaitingForNextWave:
                    UpdateWaveDelay();
                    break;

                case SpawnerState.Cooldown:
                    UpdateCycleCooldown();
                    break;
            }
        }

        private bool ValidateConfiguration()
        {
            if (string.IsNullOrWhiteSpace(
                    spawnerId))
            {
                Debug.LogError(
                    $"EnemySpawnPoint: Spawner ID " +
                    $"не указан на {gameObject.name}.",
                    this
                );

                enabled = false;
                return false;
            }

            if (ConfiguredEnemyPrefab == null)
            {
                Debug.LogError(
                    $"EnemySpawnPoint: Enemy Prefab " +
                    $"не назначен на {gameObject.name}.",
                    this
                );

                enabled = false;
                return false;
            }

            if (spawnDefinition == null &&
                isDebug)
            {
                Debug.LogWarning(
                    $"{gameObject.name}: Spawn Cycle Definition " +
                    "не назначен, используются fallback-поля компонента.",
                    this
                );
            }

            return true;
        }

        private void LoadState()
        {
            bool hasSavedCooldown =
                SpawnerCooldownStorage.TryLoad(
                    spawnerId,
                    out DateTime savedTime
                );

            if (!hasSavedCooldown)
            {
                StartNewCycle();
                return;
            }

            if (DateTime.UtcNow >= savedTime)
            {
                SpawnerCooldownStorage.Clear(
                    spawnerId
                );

                if (isDebug)
                {
                    Debug.Log(
                        $"{gameObject.name}: сохранённый " +
                        $"cooldown уже завершён.",
                        this
                    );
                }

                StartNewCycle();
                return;
            }

            nextCycleTimeUtc = savedTime;
            state = SpawnerState.Cooldown;

            if (isDebug)
            {
                TimeSpan remaining =
                    RemainingCooldown;

                Debug.Log(
                    $"{gameObject.name}: cooldown " +
                    $"восстановлен | Осталось: " +
                    $"{remaining.Minutes:D2}:" +
                    $"{remaining.Seconds:D2}.",
                    this
                );
            }
        }

        private void StartNewCycle()
        {
            currentWave = 0;
            nextCycleTimeUtc =
                DateTime.MinValue;

            SpawnerCooldownStorage.Clear(
                spawnerId
            );

            state = SpawnerState.Ready;

            if (isDebug)
            {
                Debug.Log(
                    $"{gameObject.name}: новый цикл начат.",
                    this
                );
            }

            StartNextWave();
        }

        private void StartNextWave()
        {
            currentWave++;

            state =
                SpawnerState.SpawningWave;

            if (isDebug)
            {
                Debug.Log(
                    $"{gameObject.name}: волна " +
                    $"{currentWave}/" +
                    $"{ConfiguredWavesPerCycle} началась.",
                    this
                );
            }

            StartCoroutine(
                SpawnWaveRoutine()
            );
        }

        private IEnumerator SpawnWaveRoutine()
        {
            int spawnedCount = 0;

            for (int i = 0;
                 i < ConfiguredEnemiesPerWave;
                 i++)
            {
                if (TrySpawnEnemy())
                    spawnedCount++;

                if (i <
                        ConfiguredEnemiesPerWave - 1 &&
                    ConfiguredEnemySpawnDelay > 0f)
                {
                    yield return
                        new WaitForSeconds(
                            ConfiguredEnemySpawnDelay
                        );
                }
            }

            if (spawnedCount <= 0)
            {
                Debug.LogError(
                    $"{gameObject.name}: не удалось " +
                    $"создать ни одного противника " +
                    $"для волны {currentWave}.",
                    this
                );

                currentWave--;

                state =
                    SpawnerState
                        .WaitingForNextWave;

                waveTimer =
                    ConfiguredWaveDelay;

                yield break;
            }

            state =
                SpawnerState.WaveActive;

            if (isDebug)
            {
                Debug.Log(
                    $"{gameObject.name}: формирование " +
                    $"волны {currentWave}/" +
                    $"{ConfiguredWavesPerCycle} завершено | " +
                    $"Создано: {spawnedCount}.",
                    this
                );
            }
        }

        private bool TrySpawnEnemy()
        {
            if (!TryGetSpawnPosition(
                    out Vector3 spawnPosition))
            {
                Debug.LogError(
                    $"{gameObject.name}: не удалось " +
                    $"найти NavMesh для создания " +
                    $"противника.",
                    this
                );

                return false;
            }

            Enemy spawnedEnemy =
                Instantiate(
                    ConfiguredEnemyPrefab,
                    spawnPosition,
                    transform.rotation
                );

            spawnedEnemies.Add(
                spawnedEnemy
            );

            if (isDebug)
            {
                Debug.Log(
                    $"{gameObject.name}: создан " +
                    $"{spawnedEnemy.gameObject.name} | " +
                    $"Сейчас создано: " +
                    $"{spawnedEnemies.Count}/" +
                    $"{ConfiguredEnemiesPerWave}",
                    this
                );
            }

            return true;
        }

        private void UpdateActiveWave()
        {
            if (spawnedEnemies.Count > 0)
                return;

            CompleteWave();
        }

        private void CompleteWave()
        {
            if (isDebug)
            {
                Debug.Log(
                    $"{gameObject.name}: волна " +
                    $"{currentWave}/" +
                    $"{ConfiguredWavesPerCycle} завершена.",
                    this
                );
            }

            if (currentWave >=
                ConfiguredWavesPerCycle)
            {
                StartCooldown();
                return;
            }

            state =
                SpawnerState
                    .WaitingForNextWave;

            waveTimer =
                ConfiguredWaveDelay;

            if (isDebug)
            {
                Debug.Log(
                    $"{gameObject.name}: следующая " +
                    $"волна через {ConfiguredWaveDelay:F1} сек.",
                    this
                );
            }
        }

        private void UpdateWaveDelay()
        {
            waveTimer -=
                Time.deltaTime;

            if (waveTimer > 0f)
                return;

            StartNextWave();
        }

        private void StartCooldown()
        {
            state =
                SpawnerState.Cooldown;

            nextCycleTimeUtc =
                DateTime.UtcNow.AddMinutes(
                    ConfiguredCycleCooldownMinutes
                );

            SpawnerCooldownStorage.Save(
                spawnerId,
                nextCycleTimeUtc
            );

            if (isDebug)
            {
                Debug.Log(
                    $"{gameObject.name}: цикл завершён | " +
                    $"Cooldown: " +
                    $"{ConfiguredCycleCooldownMinutes:F2} мин. | " +
                    $"Следующий цикл UTC: " +
                    $"{nextCycleTimeUtc}:" +
                    "yyyy-MM-dd HH:mm:ss",
                    this
                );
            }
        }

        private void UpdateCycleCooldown()
        {
            if (DateTime.UtcNow <
                nextCycleTimeUtc)
            {
                return;
            }

            if (isDebug)
            {
                Debug.Log(
                    $"{gameObject.name}: " +
                    $"cooldown завершён.",
                    this
                );
            }

            StartNewCycle();
        }

        private void CleanupDestroyedEnemies()
        {
            for (int i =
                     spawnedEnemies.Count - 1;
                 i >= 0;
                 i--)
            {
                if (spawnedEnemies[i] != null)
                    continue;

                spawnedEnemies.RemoveAt(i);
            }
        }

        private bool TryGetSpawnPosition(
            out Vector3 spawnPosition)
        {
            Vector2 randomCircle =
                UnityEngine.Random
                    .insideUnitCircle *
                ConfiguredSpawnRadius;

            Vector3 randomPosition =
                transform.position +
                new Vector3(
                    randomCircle.x,
                    0f,
                    randomCircle.y
                );

            if (NavMesh.SamplePosition(
                    randomPosition,
                    out NavMeshHit hit,
                    ConfiguredNavMeshSampleDistance,
                    NavMesh.AllAreas))
            {
                spawnPosition =
                    hit.position;

                return true;
            }

            spawnPosition =
                transform.position;

            return false;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireSphere(
                transform.position,
                ConfiguredSpawnRadius
            );
        }
    }
}
