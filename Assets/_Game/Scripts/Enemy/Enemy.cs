using System.Collections.Generic;
using LootboundIsles.Combat;
using UnityEngine;

namespace LootboundIsles.Enemies
{
    [RequireComponent(typeof(Health))]
    public class Enemy : MonoBehaviour
    {
        private static readonly HashSet<Enemy>
            activeEnemies = new();

        [Header("DEBUG")]
        [SerializeField] private bool isDebug = true;

        [Header("Definition")]
        [SerializeField] private EnemyDefinition definition;

        public EnemyDefinition Definition => definition;
        public Health Health { get; private set; }

        public static IEnumerable<Enemy> ActiveEnemies =>
            activeEnemies;

        [RuntimeInitializeOnLoadMethod(
            RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetRegistry()
        {
            activeEnemies.Clear();
        }

        private void Awake()
        {
            Health = GetComponent<Health>();

            if (definition == null)
            {
                if (isDebug)
                {
                    Debug.LogError(
                        $"Enemy: EnemyDefinition не назначен на объекте {gameObject.name}.",
                        this
                    );
                }

                enabled = false;
            }
        }

        private void OnEnable()
        {
            activeEnemies.Add(this);
        }

        private void OnDisable()
        {
            activeEnemies.Remove(this);
        }
    }
}
