using System;
using System.Collections.Generic;
using LootboundIsles.Combat;
using LootboundIsles.Player;
using LootboundIsles.Progression;
using UnityEngine;

namespace LootboundIsles.Upgrades
{
    [RequireComponent(typeof(LevelSystem))]
    [RequireComponent(typeof(PlayerStats))]
    [RequireComponent(typeof(Health))]
    public class UpgradeSystem : MonoBehaviour
    {
        [Header("Upgrade Pool")]
        [SerializeField]
        private List<UpgradeDefinition> availableUpgrades = new();

        [Header("Settings")]
        [SerializeField, Min(1)]
        private int choicesCount = 3;

        [Header("DEBUG")]
        [SerializeField]
        private bool isDebug = true;

        private LevelSystem levelSystem;
        private PlayerStats playerStats;
        private Health playerHealth;

        private readonly List<UpgradeDefinition>
            currentChoices = new();

        private bool isWaitingForChoice;

        // Сколько выборов улучшения игрок ещё должен сделать.
        private int pendingChoices;

        public IReadOnlyList<UpgradeDefinition> CurrentChoices =>
            currentChoices;

        public bool IsWaitingForChoice =>
            isWaitingForChoice;

        public int PendingChoices =>
            pendingChoices;

        public event Action<IReadOnlyList<UpgradeDefinition>>
            ChoicesGenerated;

        public event Action<UpgradeDefinition>
            UpgradeSelected;

        private void Awake()
        {
            levelSystem =
                GetComponent<LevelSystem>();

            playerStats =
                GetComponent<PlayerStats>();

            playerHealth =
                GetComponent<Health>();
        }

        private void OnEnable()
        {
            levelSystem.LeveledUp +=
                HandleLevelUp;
        }

        private void OnDisable()
        {
            levelSystem.LeveledUp -=
                HandleLevelUp;
        }

        private void HandleLevelUp(int newLevel)
        {
            pendingChoices++;

            if (isDebug)
            {
                Debug.Log(
                    $"UpgradeSystem: получен Level Up " +
                    $"{newLevel}. " +
                    $"Ожидающих выборов: {pendingChoices}",
                    this
                );
            }

            // Если окно уже открыто, ничего не перезаписываем.
            // Новый выбор просто остаётся в очереди.
            if (isWaitingForChoice)
                return;

            GenerateChoices();
        }

        private void GenerateChoices()
        {
            currentChoices.Clear();

            if (availableUpgrades.Count == 0)
            {
                Debug.LogError(
                    "UpgradeSystem: список Available Upgrades пуст.",
                    this
                );

                return;
            }

            List<UpgradeDefinition> candidates =
                new List<UpgradeDefinition>();

            foreach (UpgradeDefinition upgrade
                     in availableUpgrades)
            {
                if (upgrade != null)
                {
                    candidates.Add(upgrade);
                }
            }

            if (candidates.Count == 0)
            {
                Debug.LogError(
                    "UpgradeSystem: в Available Upgrades " +
                    "нет корректных UpgradeDefinition.",
                    this
                );

                return;
            }

            int count =
                Mathf.Min(
                    choicesCount,
                    candidates.Count
                );

            for (int i = 0; i < count; i++)
            {
                int randomIndex =
                    UnityEngine.Random.Range(
                        0,
                        candidates.Count
                    );

                UpgradeDefinition selected =
                    candidates[randomIndex];

                currentChoices.Add(selected);

                candidates.RemoveAt(randomIndex);
            }

            isWaitingForChoice =
                currentChoices.Count > 0;

            if (isDebug)
            {
                DebugChoices();
            }

            ChoicesGenerated?.Invoke(CurrentChoices);
        }

        public void SelectUpgrade(int index)
        {
            if (!isWaitingForChoice)
                return;

            if (index < 0 ||
                index >= currentChoices.Count)
            {
                return;
            }

            UpgradeDefinition selectedUpgrade =
                currentChoices[index];

            ApplyUpgrade(selectedUpgrade);

            isWaitingForChoice = false;

            if (pendingChoices > 0)
            {
                pendingChoices--;
            }

            UpgradeSelected?.Invoke(selectedUpgrade);

            if (isDebug)
            {
                Debug.Log(
                    $"Выбрано улучшение: " +
                    $"{selectedUpgrade.DisplayName} | " +
                    $"Осталось выборов: {pendingChoices}",
                    this
                );
            }

            currentChoices.Clear();

            // Если за один большой EXP-набор получили
            // несколько уровней, сразу показываем следующий выбор.
            if (pendingChoices > 0)
            {
                GenerateChoices();
            }
        }

        private void ApplyUpgrade(
    UpgradeDefinition upgrade)
        {
            switch (upgrade.UpgradeType)
            {
                case UpgradeType.Damage:
                    playerStats.AddDamageMultiplier(
                        upgrade.Value
                    );
                    break;

                case UpgradeType.AttackSpeed:
                    playerStats.AddAttackSpeedMultiplier(
                        upgrade.Value
                    );
                    break;

                case UpgradeType.MaxHealth:
                    ApplyMaxHealthUpgrade(
                        upgrade.Value
                    );
                    break;

                case UpgradeType.MovementSpeed:
                    playerStats.AddMovementSpeedMultiplier(
                        upgrade.Value
                    );
                    break;

                case UpgradeType.CriticalChance:
                    playerStats.AddCriticalChance(
                        upgrade.Value
                    );
                    break;

                case UpgradeType.AttackRange:
                    playerStats.AddAttackRangeMultiplier(
                        upgrade.Value
                    );
                    break;

                default:
                    if (isDebug)
                    {
                        Debug.Log(
                            $"Улучшение " +
                            $"{upgrade.UpgradeType} " +
                            $"пока не реализовано.",
                            this
                        );
                    }
                    break;
            }
        }

        private void ApplyMaxHealthUpgrade(
            float value)
        {
            playerStats.AddMaxHealthMultiplier(
                value
            );

            float finalMaxHealth =
                playerStats.GetFinalMaxHealth(
                    playerHealth.BaseMaxHealth
                );

            playerHealth.SetMaxHealth(
                finalMaxHealth
            );

            if (isDebug)
            {
                Debug.Log(
                    $"Player HP: " +
                    $"{playerHealth.CurrentHealth:F2}/" +
                    $"{playerHealth.MaxHealth:F2}",
                    this
                );
            }
        }

        private void DebugChoices()
        {
            Debug.Log(
                $"Предложено улучшений: " +
                $"{currentChoices.Count} | " +
                $"Ожидающих выборов: {pendingChoices}",
                this
            );

            for (int i = 0;
                 i < currentChoices.Count;
                 i++)
            {
                UpgradeDefinition upgrade =
                    currentChoices[i];

                Debug.Log(
                    $"Вариант {i + 1}: " +
                    $"{upgrade.DisplayName} | " +
                    $"{upgrade.Description}",
                    this
                );
            }
        }
    }
}
