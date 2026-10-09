using System;
using System.Collections.Generic;
using LootboundIsles.Inventory;
using UnityEngine;

namespace LootboundIsles.Loot
{
    public enum LootGroupType
    {
        Currency,
        Materials,
        Gems,
        Equipment,
        Blueprints
    }

    public enum LootCurrency
    {
        Gold,
        Piastres
    }

    [Serializable]
    public sealed class LootEntry
    {
        [SerializeField] private ItemDefinition item;
        [SerializeField] private LootCurrency currency;
        [SerializeField, Min(0.0001f)] private float weight = 1f;
        [SerializeField, Min(1)] private int minimumQuantity = 1;
        [SerializeField, Min(1)] private int maximumQuantity = 1;

        public ItemDefinition Item => item;
        public LootCurrency Currency => currency;
        public float Weight => weight;
        public int MinimumQuantity => minimumQuantity;
        public int MaximumQuantity => maximumQuantity;
    }

    [Serializable]
    public sealed class LootGroup
    {
        [SerializeField] private LootGroupType type;
        [SerializeField, Range(0f, 1f)] private float dropChance = 1f;
        [SerializeField, Min(1)] private int rolls = 1;
        [SerializeField] private List<LootEntry> entries = new();

        public LootGroupType Type => type;
        public float DropChance => dropChance;
        public int Rolls => rolls;
        public IReadOnlyList<LootEntry> Entries => entries;

        internal bool TryRoll(LootGroupType groupType, out LootEntry entry, out int quantity)
        {
            entry = null;
            quantity = 0;
            if (entries == null || entries.Count == 0)
                return false;
            if (dropChance <= 0f || (dropChance < 1f && UnityEngine.Random.value >= dropChance))
                return false;

            float totalWeight = 0f;
            foreach (LootEntry candidate in entries)
                if (IsCompatible(groupType, candidate) && candidate.Weight > 0f)
                    totalWeight += candidate.Weight;

            if (totalWeight <= 0f)
                return false;

            float roll = UnityEngine.Random.value * totalWeight;
            foreach (LootEntry candidate in entries)
            {
                if (!IsCompatible(groupType, candidate) || candidate.Weight <= 0f)
                    continue;

                roll -= candidate.Weight;
                if (roll > 0f)
                    continue;

                entry = candidate;
                quantity = UnityEngine.Random.Range(
                    candidate.MinimumQuantity,
                    Mathf.Max(candidate.MinimumQuantity, candidate.MaximumQuantity) + 1
                );
                return quantity > 0;
            }

            return false;
        }

        private static bool IsCompatible(LootGroupType groupType, LootEntry entry)
        {
            if (entry == null)
                return false;
            if (groupType == LootGroupType.Currency)
                return entry.Item == null;

            if (entry.Item == null)
                return false;
            return groupType switch
            {
                LootGroupType.Materials => entry.Item.Category == ItemCategory.Materials,
                LootGroupType.Gems => entry.Item.Category == ItemCategory.Gems,
                LootGroupType.Equipment => entry.Item.Category == ItemCategory.Equipment,
                LootGroupType.Blueprints => entry.Item.Category == ItemCategory.Blueprints,
                _ => false
            };
        }

    }

    [CreateAssetMenu(fileName = "LootTable", menuName = "Lootbound Isles/Loot/Loot Table")]
    public sealed class LootTable : ScriptableObject
    {
        [SerializeField] private List<LootGroup> groups = new();

        public IReadOnlyList<LootGroup> Groups => groups;

        public List<LootDrop> Roll()
        {
            List<LootDrop> drops = new();
            if (groups == null)
                return drops;

            foreach (LootGroup group in groups)
            {
                if (group == null)
                    continue;
                for (int roll = 0; roll < Mathf.Max(1, group.Rolls); roll++)
                {
                    if (!group.TryRoll(group.Type, out LootEntry entry, out int quantity))
                        continue;
                    drops.Add(new LootDrop(group.Type, entry.Item, entry.Currency, quantity));
                }
            }

            return drops;
        }
    }

    public readonly struct LootDrop
    {
        public LootGroupType Group { get; }
        public ItemDefinition Item { get; }
        public LootCurrency Currency { get; }
        public int Quantity { get; }

        public LootDrop(LootGroupType group, ItemDefinition item, LootCurrency currency, int quantity)
        {
            Group = group;
            Item = item;
            Currency = currency;
            Quantity = quantity;
        }
    }
}
