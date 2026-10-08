using System;
using UnityEngine;

namespace LootboundIsles.Progression
{
    [RequireComponent(typeof(ExperienceSystem))]
    public class LevelSystem : MonoBehaviour
    {
        [Header("Level")]
        [SerializeField, Min(1)]
        private int startingLevel = 1;

        [SerializeField, Min(1)]
        private int maxLevel = 100;

        [Header("Experience")]
        [SerializeField, Min(1)]
        private int baseExperienceRequired = 30;

        [SerializeField, Min(0)]
        private int experienceIncreasePerLevel = 10;

        [Header("DEBUG")]
        [SerializeField]
        private bool isDebug = true;

        private ExperienceSystem experienceSystem;

        public int CurrentLevel { get; private set; }

        public int MaxLevel => maxLevel;

        public bool IsAtMaxLevel =>
            CurrentLevel >= maxLevel;

        public int ExperienceToNextLevel =>
            IsAtMaxLevel
                ? 0
                : CalculateExperienceRequired(CurrentLevel);

        public event Action<int> LevelChanged;
        public event Action<int> LeveledUp;

        private void Awake()
        {
            experienceSystem =
                GetComponent<ExperienceSystem>();

            maxLevel = Mathf.Max(1, maxLevel);

            CurrentLevel = Mathf.Clamp(
                startingLevel,
                1,
                maxLevel
            );
        }

        private void OnEnable()
        {
            experienceSystem.ExperienceChanged +=
                HandleExperienceChanged;
        }

        private void OnDisable()
        {
            experienceSystem.ExperienceChanged -=
                HandleExperienceChanged;
        }

        private void HandleExperienceChanged(
            int currentExperience)
        {
            CheckForLevelUp();
        }

        private void CheckForLevelUp()
        {
            while (
                !IsAtMaxLevel &&
                experienceSystem.CurrentExperience >=
                ExperienceToNextLevel)
            {
                int requiredExperience =
                    ExperienceToNextLevel;

                experienceSystem.SpendExperience(
                    requiredExperience
                );

                CurrentLevel++;

                LevelChanged?.Invoke(CurrentLevel);
                LeveledUp?.Invoke(CurrentLevel);

                if (isDebug)
                {
                    Debug.Log(
                        $"LEVEL UP! " +
                        $"Новый уровень: {CurrentLevel} | " +
                        $"Остаток EXP: " +
                        $"{experienceSystem.CurrentExperience} | " +
                        $"До следующего уровня: " +
                        $"{ExperienceToNextLevel}",
                        this
                    );
                }
            }

            DiscardExperienceAtLevelCap();
        }

        private void DiscardExperienceAtLevelCap()
        {
            if (!IsAtMaxLevel ||
                experienceSystem.CurrentExperience <= 0)
            {
                return;
            }

            int discardedExperience =
                experienceSystem.CurrentExperience;

            experienceSystem.SpendExperience(
                discardedExperience
            );

            if (isDebug)
            {
                Debug.Log(
                    $"LevelSystem: достигнут Level Cap " +
                    $"{maxLevel}. Лишний EXP сгорел: " +
                    $"{discardedExperience}.",
                    this
                );
            }
        }

        private int CalculateExperienceRequired(
            int level)
        {
            return baseExperienceRequired +
                   (level - 1) *
                   experienceIncreasePerLevel;
        }
    }
}
