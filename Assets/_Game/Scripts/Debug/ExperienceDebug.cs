using LootboundIsles.Progression;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LootboundIsles.Debugging
{
    [RequireComponent(typeof(ExperienceSystem))]
    public class ExperienceDebug : MonoBehaviour
    {
        [Header("Test")]
        [SerializeField, Min(1)]
        private int experienceAmount = 150;

        [Header("DEBUG")]
        [SerializeField]
        private bool isDebug = true;

        private ExperienceSystem experienceSystem;

        private void Awake()
        {
            experienceSystem =
                GetComponent<ExperienceSystem>();
        }

        private void Update()
        {
            if (Keyboard.current == null)
                return;

            if (!Keyboard.current.f6Key.wasPressedThisFrame)
                return;

            experienceSystem.AddExperience(
                experienceAmount
            );

            if (isDebug)
            {
                Debug.Log(
                    $"ExperienceDebug: добавлено " +
                    $"{experienceAmount} EXP.",
                    this
                );
            }
        }
    }
}