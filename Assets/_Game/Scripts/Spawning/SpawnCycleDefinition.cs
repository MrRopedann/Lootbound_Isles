using LootboundIsles.Enemies;
using UnityEngine;

namespace LootboundIsles.Spawning
{
    [CreateAssetMenu(
        fileName = "SpawnCycleDefinition",
        menuName = "Lootbound Isles/Spawning/Spawn Cycle Definition"
    )]
    public class SpawnCycleDefinition : ScriptableObject
    {
        [Header("Enemy")]
        [SerializeField]
        private Enemy enemyPrefab;

        [Header("Wave Settings")]
        [SerializeField, Min(1)]
        private int enemiesPerWave = 5;

        [SerializeField, Min(1)]
        private int wavesPerCycle = 3;

        [SerializeField, Min(0f)]
        private float enemySpawnDelay = 0.5f;

        [SerializeField, Min(0f)]
        private float waveDelay = 10f;

        [Header("Cycle Settings")]
        [SerializeField, Min(0f)]
        private float cycleCooldownMinutes = 30f;

        [Header("Spawn Settings")]
        [SerializeField, Min(0f)]
        private float spawnRadius = 2f;

        [Header("NavMesh")]
        [SerializeField, Min(0.1f)]
        private float navMeshSampleDistance = 3f;

        public Enemy EnemyPrefab => enemyPrefab;
        public int EnemiesPerWave => enemiesPerWave;
        public int WavesPerCycle => wavesPerCycle;
        public float EnemySpawnDelay => enemySpawnDelay;
        public float WaveDelay => waveDelay;
        public float CycleCooldownMinutes => cycleCooldownMinutes;
        public float SpawnRadius => spawnRadius;
        public float NavMeshSampleDistance => navMeshSampleDistance;
    }
}
