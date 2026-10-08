using System;
using UnityEngine;

namespace LootboundIsles.Progression
{
    public class ExperienceSystem : MonoBehaviour
    {
        public int CurrentExperience { get; private set; }

        public event Action<int> ExperienceChanged;

        [Header("DEBUG")]
        [SerializeField] private bool isDebug = true;

        public void AddExperience(int amount)
        {
            if (amount <= 0)
                return;

            CurrentExperience += amount;

            if (isDebug)
            {
                Debug.Log(
                    $"Player получил {amount} EXP | " +
                    $"Текущий EXP: {CurrentExperience}",
                    this
                );
            }

            ExperienceChanged?.Invoke(CurrentExperience);
        }

        public void SpendExperience(int amount)
        {
            if (amount <= 0)
                return;

            if (amount > CurrentExperience)
                return;

            CurrentExperience -= amount;

            if (isDebug)
            {
                Debug.Log(
                    $"Потрачено {amount} EXP | " +
                    $"Осталось EXP: {CurrentExperience}",
                    this
                );
            }

            // ВАЖНО:
            // ExperienceChanged здесь НЕ вызываем.
            //
            // SpendExperience используется LevelSystem для внутреннего
            // расходования накопленного EXP.
            // Если вызвать событие здесь, LevelSystem повторно войдет
            // в CheckForLevelUp() до завершения текущей проверки.
        }
    }
}