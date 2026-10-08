using System;
using UnityEngine;

namespace LootboundIsles.Combat
{
    public class Health : MonoBehaviour
    {
        [Header("Health")]
        [SerializeField, Min(1f)]
        private float maxHealth = 100f;

        public float CurrentHealth { get; private set; }
        public float MaxHealth => maxHealth;
        public float BaseMaxHealth { get; private set; }
        public bool IsDead => CurrentHealth <= 0f;

        public event Action<float, float> HealthChanged;
        public event Action Died;

        private void Awake()
        {
            BaseMaxHealth = maxHealth;
            CurrentHealth = maxHealth;
        }

        public void Initialize(float newMaxHealth)
        {
            maxHealth = Mathf.Max(
                1f,
                newMaxHealth
            );

            CurrentHealth = maxHealth;

            HealthChanged?.Invoke(
                CurrentHealth,
                MaxHealth
            );
        }

        public void SetMaxHealth(float newMaxHealth)
        {
            newMaxHealth =
                Mathf.Max(1f, newMaxHealth);

            maxHealth = newMaxHealth;

            CurrentHealth =
                Mathf.Clamp(
                    CurrentHealth,
                    0f,
                    maxHealth
                );

            HealthChanged?.Invoke(
                CurrentHealth,
                MaxHealth
            );
        }

        public void TakeDamage(float amount)
        {
            if (IsDead || amount <= 0f)
                return;

            CurrentHealth =
                Mathf.Max(
                    CurrentHealth - amount,
                    0f
                );

            HealthChanged?.Invoke(
                CurrentHealth,
                MaxHealth
            );

            if (IsDead)
                Died?.Invoke();
        }

        public void Heal(float amount)
        {
            if (IsDead || amount <= 0f)
                return;

            CurrentHealth =
                Mathf.Min(
                    CurrentHealth + amount,
                    MaxHealth
                );

            HealthChanged?.Invoke(
                CurrentHealth,
                MaxHealth
            );
        }

        public void RestoreFullHealth()
        {
            CurrentHealth = MaxHealth;

            HealthChanged?.Invoke(
                CurrentHealth,
                MaxHealth
            );
        }

        public void Revive(float healthAmount)
        {
            CurrentHealth = Mathf.Clamp(
                healthAmount,
                1f,
                MaxHealth
            );

            HealthChanged?.Invoke(
                CurrentHealth,
                MaxHealth
            );
        }
    }
}
