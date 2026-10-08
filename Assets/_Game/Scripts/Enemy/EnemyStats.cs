using LootboundIsles.Combat;
using UnityEngine;

namespace LootboundIsles.Enemies
{
    [RequireComponent(typeof(Enemy))]
    [RequireComponent(typeof(Health))]
    public class EnemyStats : MonoBehaviour
    {
        private Enemy enemy;
        private Health health;

        public EnemyDefinition Definition => enemy.Definition;
        public EnemyType EnemyType => enemy.Definition.EnemyType;
        public float Accuracy => enemy.Definition.Accuracy;
        public float Evasion => enemy.Definition.Evasion;
        public float Defense => enemy.Definition.Defense;
        public float CriticalChance => enemy.Definition.CriticalChance;

        private void Awake()
        {
            enemy = GetComponent<Enemy>();
            health = GetComponent<Health>();
        }

        private void Start()
        {
            if (enemy.Definition == null)
            {
                enabled = false;
                return;
            }

            health.Initialize(enemy.Definition.MaxHealth);
        }
    }
}
