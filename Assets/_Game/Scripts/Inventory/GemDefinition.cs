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

        private void OnValidate()
        {
            if (allowedEquipmentSlotIds == null)
                allowedEquipmentSlotIds = new List<string>();

            for (int i = 0; i < allowedEquipmentSlotIds.Count; i++)
                allowedEquipmentSlotIds[i] = allowedEquipmentSlotIds[i]?.Trim();
        }
    }
}
