using System;
using UnityEngine;

namespace LootboundIsles.Combat
{
    public class CombatState : MonoBehaviour
    {
        [Header("Combat State")]
        [SerializeField, Min(0f)]
        private float exitDelay = 5f;

        [Header("DEBUG")]
        [SerializeField]
        private bool isDebug;

        private float lastCombatEventTime;

        public bool IsInCombat { get; private set; }

        public event Action<bool> CombatStateChanged;

        private void Update()
        {
            if (!IsInCombat)
                return;

            if (Time.time - lastCombatEventTime < exitDelay)
                return;

            SetCombatState(false);
        }

        public void RegisterCombatEvent()
        {
            lastCombatEventTime = Time.time;
            SetCombatState(true);
        }

        public void ResetCombatState()
        {
            lastCombatEventTime = 0f;
            SetCombatState(false);
        }

        private void SetCombatState(bool isInCombat)
        {
            if (IsInCombat == isInCombat)
                return;

            IsInCombat = isInCombat;
            CombatStateChanged?.Invoke(IsInCombat);

            if (isDebug)
            {
                Debug.Log(
                    IsInCombat
                        ? "Player entered Combat State."
                        : "Player left Combat State.",
                    this
                );
            }
        }
    }
}
