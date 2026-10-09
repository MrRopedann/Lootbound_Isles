using UnityEngine;

namespace LootboundIsles.Inventory
{
    [CreateAssetMenu(fileName = "EquipmentUpgradeRules", menuName = "Lootbound Isles/Equipment/Upgrade Rules")]
    public sealed class EquipmentUpgradeRules : ScriptableObject
    {
        [SerializeField] private ItemDefinition[] rarityFragments = new ItemDefinition[5];
        [SerializeField] private int[] goldCostByTargetLevel = { 25, 50, 100, 200, 400, 800, 1600, 3200, 6400 };
        [SerializeField] private int[] fragmentCostByTargetLevel = { 1, 1, 2, 2, 3, 3, 4, 4, 5 };
        [SerializeField, Min(2)] private int[] fragmentExchangeRate = { 100, 100, 100, 100 };
        [SerializeField] private int[] sellGoldByRarityAndLevel =
        {
            5, 7, 9, 11, 13, 15, 17, 19, 21, 23,
            10, 14, 18, 22, 26, 30, 34, 38, 42, 46,
            20, 28, 36, 44, 52, 60, 68, 76, 84, 92,
            40, 56, 72, 88, 104, 120, 136, 152, 168, 184,
            80, 112, 144, 176, 208, 240, 272, 304, 336, 368
        };
        [SerializeField] private int[] dismantleFragmentYieldByRarityAndLevel =
        {
            1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
            1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
            1, 1, 2, 2, 2, 2, 2, 2, 2, 2,
            2, 2, 2, 2, 3, 3, 3, 3, 3, 3,
            3, 3, 3, 4, 4, 4, 4, 5, 5, 5
        };

        public ItemDefinition GetFragment(ItemRarity rarity)
        {
            int index = (int)rarity;
            return rarityFragments != null && index >= 0 && index < rarityFragments.Length
                ? rarityFragments[index]
                : null;
        }

        public bool TryGetCost(int targetLevel, out int gold, out int fragments)
        {
            int index = targetLevel - 2;
            if (goldCostByTargetLevel == null || fragmentCostByTargetLevel == null ||
                index < 0 || index >= goldCostByTargetLevel.Length || index >= fragmentCostByTargetLevel.Length)
            {
                gold = 0;
                fragments = 0;
                return false;
            }
            gold = goldCostByTargetLevel[index];
            fragments = fragmentCostByTargetLevel[index];
            return gold > 0 && fragments > 0;
        }

        public int GetExchangeRate(ItemRarity fromRarity)
        {
            int index = (int)fromRarity;
            return fragmentExchangeRate != null && index >= 0 && index < fragmentExchangeRate.Length
                ? Mathf.Max(2, fragmentExchangeRate[index])
                : 0;
        }

        public int GetSellGold(ItemRarity rarity, int level) => GetLevelValue(sellGoldByRarityAndLevel, rarity, level);
        public int GetDismantleYield(ItemRarity rarity, int level) => GetLevelValue(dismantleFragmentYieldByRarityAndLevel, rarity, level);

        private static int GetLevelValue(int[] values, ItemRarity rarity, int level)
        {
            int index = (int)rarity * 10 + Mathf.Clamp(level, 1, 10) - 1;
            return values != null && index >= 0 && index < values.Length ? Mathf.Max(0, values[index]) : 0;
        }
    }
}
