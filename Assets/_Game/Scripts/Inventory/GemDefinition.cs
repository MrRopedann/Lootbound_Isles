using System.Collections.Generic;
using UnityEngine;

namespace LootboundIsles.Inventory
{
    public enum GemStatFamily
    {
        Attack,
        Defense,
        MaxHealth,
        AttackSpeed,
        CriticalChance,
        HealthRegeneration
    }

    [CreateAssetMenu(fileName = "GemDefinition", menuName = "Lootbound Isles/Items/Gem Definition")]
    public sealed class GemDefinition : ItemDefinition
    {
        [SerializeField] private GemStatFamily statFamily;
        [SerializeField] private List<string> allowedEquipmentSlotIds = new();

        public GemStatFamily StatFamily => statFamily;
        public IReadOnlyList<string> AllowedEquipmentSlotIds => allowedEquipmentSlotIds;

        public bool AllowsEquipmentSlot(string equipmentSlotId)
        {
            if (string.IsNullOrWhiteSpace(equipmentSlotId) || allowedEquipmentSlotIds == null)
                return false;

            string requestedId = equipmentSlotId.Trim();
            foreach (string allowedId in allowedEquipmentSlotIds)
            {
                if (string.Equals(allowedId?.Trim(), requestedId, System.StringComparison.Ordinal))
                    return true;
            }
            return false;
        }

        public bool IsCompatibleWith(ItemDefinition equipmentDefinition)
        {
            return equipmentDefinition != null &&
                   equipmentDefinition.Category == ItemCategory.Equipment &&
                   AllowsEquipmentSlot(equipmentDefinition.EquipmentSlotId);
        }

        private void OnValidate()
        {
            if (allowedEquipmentSlotIds == null)
                allowedEquipmentSlotIds = new List<string>();

            for (int i = 0; i < allowedEquipmentSlotIds.Count; i++)
                allowedEquipmentSlotIds[i] = allowedEquipmentSlotIds[i]?.Trim();
        }
    }
}
