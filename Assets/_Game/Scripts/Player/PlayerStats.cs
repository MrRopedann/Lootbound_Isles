using System;
using UnityEngine;

namespace LootboundIsles.Player
{
    public class PlayerStats : MonoBehaviour
    {
        public float DamageMultiplier { get; private set; } = 1f;
        public float AttackSpeedMultiplier { get; private set; } = 1f;
        public float MaxHealthMultiplier { get; private set; } = 1f;
        public float MovementSpeedMultiplier { get; private set; } = 1f;

        public float CriticalChance { get; private set; } = 0f;
        public float AttackRangeMultiplier { get; private set; } = 1f;

        [Header("Combat")]
        [SerializeField, Min(0f)]
        private float accuracy = 100f;

        [SerializeField, Min(0f)]
        private float evasion = 100f;

        [SerializeField, Min(0f)]
        private float defense;

        [Header("DEBUG")]
        [SerializeField]
        private bool isDebug = true;

        public event Action StatsChanged;

        public float Accuracy => accuracy;
        public float Evasion => evasion;
        public float Defense => defense;

        public float GetFinalDamage(float baseDamage)
        {
            return baseDamage * DamageMultiplier;
        }

        public float GetFinalAttackCooldown(float baseCooldown)
        {
            return baseCooldown / AttackSpeedMultiplier;
        }

        public float GetFinalMaxHealth(float baseMaxHealth)
        {
            return baseMaxHealth * MaxHealthMultiplier;
        }

        public float GetFinalMovementSpeed(float baseMoveSpeed)
        {
            return baseMoveSpeed * MovementSpeedMultiplier;
        }

        public float GetFinalAttackRange(float baseAttackRange)
        {
            return baseAttackRange * AttackRangeMultiplier;
        }

        public void AddDamageMultiplier(float value)
        {
            if (value <= 0f)
                return;

            DamageMultiplier *= 1f + value;

            NotifyStatsChanged();

            if (isDebug)
            {
                Debug.Log(
                    $"Damage +{value * 100f:F0}% | " +
                    $"Damage Multiplier: {DamageMultiplier:F3}",
                    this
                );
            }
        }

        public void AddAttackSpeedMultiplier(float value)
        {
            if (value <= 0f)
                return;

            AttackSpeedMultiplier *= 1f + value;

            NotifyStatsChanged();

            if (isDebug)
            {
                Debug.Log(
                    $"Attack Speed +{value * 100f:F0}% | " +
                    $"Attack Speed Multiplier: " +
                    $"{AttackSpeedMultiplier:F3}",
                    this
                );
            }
        }

        public void AddMaxHealthMultiplier(float value)
        {
            if (value <= 0f)
                return;

            MaxHealthMultiplier *= 1f + value;

            NotifyStatsChanged();

            if (isDebug)
            {
                Debug.Log(
                    $"Max HP +{value * 100f:F0}% | " +
                    $"Max Health Multiplier: " +
                    $"{MaxHealthMultiplier:F3}",
                    this
                );
            }
        }

        public void AddMovementSpeedMultiplier(float value)
        {
            if (value <= 0f)
                return;

            MovementSpeedMultiplier *= 1f + value;

            NotifyStatsChanged();

            if (isDebug)
            {
                Debug.Log(
                    $"Movement Speed +{value * 100f:F0}% | " +
                    $"Movement Speed Multiplier: " +
                    $"{MovementSpeedMultiplier:F3}",
                    this
                );
            }
        }

        public void AddCriticalChance(float value)
        {
            if (value <= 0f)
                return;

            CriticalChance += value;

            CriticalChance =
                Mathf.Clamp01(CriticalChance);

            NotifyStatsChanged();

            if (isDebug)
            {
                Debug.Log(
                    $"Critical Chance +{value * 100f:F0}% | " +
                    $"Critical Chance: " +
                    $"{CriticalChance * 100f:F0}%",
                    this
                );
            }
        }

        public void AddAttackRangeMultiplier(float value)
        {
            if (value <= 0f)
                return;

            AttackRangeMultiplier *= 1f + value;

            NotifyStatsChanged();

            if (isDebug)
            {
                Debug.Log(
                    $"Attack Range +{value * 100f:F0}% | " +
                    $"Attack Range Multiplier: " +
                    $"{AttackRangeMultiplier:F3}",
                    this
                );
            }
        }

        private void NotifyStatsChanged()
        {
            StatsChanged?.Invoke();
        }
    }
}
