using System.Collections.Generic;
using UnityEngine;

namespace LootboundIsles.Inventory
{
    [CreateAssetMenu(fileName = "ItemDefinitionCatalog", menuName = "Lootbound Isles/Items/Item Catalog")]
    public sealed class ItemDefinitionCatalog : ScriptableObject
    {
        [SerializeField] private List<ItemDefinition> definitions = new();

        public IReadOnlyList<ItemDefinition> Definitions => definitions;

        public ItemDefinition Find(string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId)) return null;
            foreach (ItemDefinition definition in definitions)
                if (definition != null && definition.ItemId == itemId) return definition;
            return null;
        }

        private void OnValidate()
        {
            if (definitions == null) return;
            var ids = new HashSet<string>();
            foreach (ItemDefinition definition in definitions)
            {
                if (definition == null || string.IsNullOrWhiteSpace(definition.ItemId)) continue;
                if (!ids.Add(definition.ItemId))
                    Debug.LogError($"ItemDefinitionCatalog contains duplicate item ID '{definition.ItemId}'.", this);
            }
        }
    }
}
