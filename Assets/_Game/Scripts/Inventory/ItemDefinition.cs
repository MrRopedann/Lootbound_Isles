using UnityEngine;

namespace LootboundIsles.Inventory
{
    public enum ItemCategory
    {
        Equipment,
        Materials,
        Gems,
        Consumables,
        Blueprints
    }

    public enum ItemRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }

    [CreateAssetMenu(
        fileName = "ItemDefinition",
        menuName = "Lootbound Isles/Items/Item Definition"
    )]
    public class ItemDefinition : ScriptableObject
    {
        [SerializeField]
        private string itemId;

        [SerializeField]
        private string displayName;

        [SerializeField]
        private ItemCategory category;

        [SerializeField]
        private ItemRarity rarity;

        [SerializeField]
        private bool isStackable = true;

        public string ItemId => itemId;
        public string DisplayName => displayName;
        public ItemCategory Category => category;
        public ItemRarity Rarity => rarity;

        // Equipment and Gems carry per-instance state and are never stacked.
        public bool IsStackable =>
            category != ItemCategory.Equipment && category != ItemCategory.Gems && isStackable;

        private void OnValidate()
        {
            itemId = itemId?.Trim();
            displayName = displayName?.Trim();

            if (category == ItemCategory.Equipment || category == ItemCategory.Gems)
            {
                isStackable = false;
            }
        }
    }
}
