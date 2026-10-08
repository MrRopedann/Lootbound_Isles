using UnityEngine;

namespace LootboundIsles.Combat
{
    [RequireComponent(typeof(Health))]
    [RequireComponent(typeof(CombatState))]
    public class HealthRegeneration : MonoBehaviour
    {
        [Header("Regeneration")]
        [SerializeField, Min(0f)]
        private float healthPerSecond = 1f;

        private Health health;
        private CombatState combatState;
        private float accumulatedRegeneration;

        private void Awake()
        {
            health = GetComponent<Health>();
            combatState = GetComponent<CombatState>();
        }

        private void OnEnable()
        {
            health.Died += ResetAccumulator;
        }

        private void OnDisable()
        {
            health.Died -= ResetAccumulator;
        }

        private void Update()
        {
            if (health.IsDead || combatState.IsInCombat)
                return;

            if (health.CurrentHealth >= health.MaxHealth)
            {
                ResetAccumulator();
                return;
            }

            accumulatedRegeneration +=
                healthPerSecond * Time.deltaTime;

            int wholeHealth =
                Mathf.FloorToInt(accumulatedRegeneration);

            if (wholeHealth <= 0)
                return;

            accumulatedRegeneration -= wholeHealth;
            health.Heal(wholeHealth);

            if (health.CurrentHealth >= health.MaxHealth)
            {
                ResetAccumulator();
            }
        }

        private void ResetAccumulator()
        {
            accumulatedRegeneration = 0f;
        }
    }
}
