using UnityEngine;

namespace LootboundIsles.World
{
    public class ZoneAccessGate : MonoBehaviour
    {
        [Header("Progression")]
        [SerializeField]
        private ZoneProgression zoneProgression;

        [SerializeField]
        private ZoneDefinition requiredZone;

        [Header("Locked State")]
        [SerializeField]
        private Collider[] blockingColliders;

        [SerializeField]
        private GameObject[] lockedVisuals;

        [Header("DEBUG")]
        [SerializeField]
        private bool isDebug;

        private void OnEnable()
        {
            if (!ValidateConfiguration())
                return;

            zoneProgression.ZoneUnlocked +=
                HandleZoneUnlocked;

            RefreshState();
        }

        private void OnDisable()
        {
            if (zoneProgression != null)
            {
                zoneProgression.ZoneUnlocked -=
                    HandleZoneUnlocked;
            }
        }

        private bool ValidateConfiguration()
        {
            if (zoneProgression != null &&
                requiredZone != null)
            {
                return true;
            }

            Debug.LogError(
                $"ZoneAccessGate: на {gameObject.name} " +
                "не назначены Zone Progression или Required Zone.",
                this
            );

            enabled = false;
            return false;
        }

        private void HandleZoneUnlocked(
            ZoneDefinition zone)
        {
            if (zone != requiredZone)
                return;

            RefreshState();
        }

        private void RefreshState()
        {
            bool isLocked =
                !zoneProgression.IsZoneUnlocked(
                    requiredZone
                );

            SetLockedState(isLocked);

            if (isDebug)
            {
                Debug.Log(
                    $"{gameObject.name}: доступ к " +
                    $"{requiredZone.DisplayName} — " +
                    $"{(isLocked ? "закрыт" : "открыт")}.",
                    this
                );
            }
        }

        private void SetLockedState(
            bool isLocked)
        {
            if (blockingColliders != null)
            {
                foreach (Collider blockingCollider
                         in blockingColliders)
                {
                    if (blockingCollider != null)
                        blockingCollider.enabled = isLocked;
                }
            }

            if (lockedVisuals == null)
                return;

            foreach (GameObject lockedVisual
                     in lockedVisuals)
            {
                if (lockedVisual != null)
                    lockedVisual.SetActive(isLocked);
            }
        }
    }
}
