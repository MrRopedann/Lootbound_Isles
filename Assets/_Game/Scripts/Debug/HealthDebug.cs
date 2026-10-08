using LootboundIsles.Combat;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LootboundIsles.Debugging
{
    public class HealthDebug : MonoBehaviour
    {
        [Header("DEBUG")]
        [SerializeField] private bool isDebug = true;

        [SerializeField] private Health health;

        private void Update()
        {
            if (Keyboard.current == null || health == null)
                return;

            if (Keyboard.current.hKey.wasPressedThisFrame)
            {
                health.TakeDamage(10f);

                if (isDebug)
                {
                    Debug.Log(
                        $"{health.gameObject.name} | Damage: 10 | " +
                        $"HP: {health.CurrentHealth}/{health.MaxHealth}"
                    );
                }
            }

            if (Keyboard.current.jKey.wasPressedThisFrame)
            {
                health.Heal(10f);

                if (isDebug)
                {
                    Debug.Log(
                        $"{health.gameObject.name} | Heal: 10 | " +
                        $"HP: {health.CurrentHealth}/{health.MaxHealth}"
                    );
                }
            }
        }
    }
}