using UnityEngine;
using LootboundIsles.World;

namespace LootboundIsles.Enemies
{
    [CreateAssetMenu(
        fileName = "EnemyDefinition",
        menuName = "Lootbound Isles/Enemies/Enemy Definition"
    )]
    public class EnemyDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField]
        private string enemyId;

        [SerializeField]
        private EnemyType enemyType = EnemyType.Normal;

        [Header("Stats")]
        [SerializeField, Min(1f)]
        private float maxHealth = 30f;

        [SerializeField, Min(0f)]
        private float moveSpeed = 3f;

        [SerializeField, Min(0f)]
        private float damage = 10f;

        [Header("Detection")]
        [SerializeField, Min(0f)]
        private float detectionRange = 6f;

        [Header("Combat")]
        [SerializeField, Min(0f)]
        private float attackRange = 1.5f;

        [SerializeField, Min(0.01f)]
        private float attackCooldown = 1f;

        [SerializeField, Min(0f)]
        private float accuracy = 100f;

        [SerializeField, Min(0f)]
        private float evasion = 100f;

        [SerializeField, Min(0f)]
        private float defense;

        [SerializeField, Range(0f, 1f)]
        private float criticalChance;

        [Header("Leash")]
        [SerializeField, Min(0.1f)]
        private float leashRange = 12f;

        [SerializeField, Min(0.05f)]
        private float homeArrivalRadius = 0.5f;

        [SerializeField, Min(0.1f)]
        private float returnStuckTimeout = 3f;

        [SerializeField, Min(0.01f)]
        private float returnProgressDistance = 0.1f;

        [Header("Rewards")]
        [SerializeField, Min(0)]
        private int experienceReward = 10;

        [Header("Boss Progression")]
        [SerializeField]
        private ZoneDefinition zoneUnlockedOnFirstKill;

        public string EnemyId => enemyId;
        public EnemyType EnemyType => enemyType;

        public float MaxHealth => maxHealth;
        public float MoveSpeed => moveSpeed;
        public float Damage => damage;

        public float DetectionRange => detectionRange;

        public float AttackRange => attackRange;
        public float AttackCooldown => attackCooldown;
        public float Accuracy => accuracy;
        public float Evasion => evasion;
        public float Defense => defense;
        public float CriticalChance => criticalChance;

        public float LeashRange => leashRange;
        public float HomeArrivalRadius => homeArrivalRadius;
        public float ReturnStuckTimeout => returnStuckTimeout;
        public float ReturnProgressDistance => returnProgressDistance;

        public int ExperienceReward => experienceReward;
        public ZoneDefinition ZoneUnlockedOnFirstKill =>
            zoneUnlockedOnFirstKill;
    }
}
