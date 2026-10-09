using System;
using UnityEngine;

namespace LootboundIsles.Inventory
{
    [Serializable]
    public struct GemUpgradeCatalyst
    {
        [SerializeField] private ItemDefinition definition;
        [SerializeField, Range(0f, 1f)] private float successChanceBonus;

        public ItemDefinition Definition => definition;
        public float SuccessChanceBonus => successChanceBonus;
    }

    [CreateAssetMenu(fileName = "GemUpgradeRules", menuName = "Lootbound Isles/Gems/Upgrade Rules")]
    public sealed class GemUpgradeRules : ScriptableObject
    {
        [Header("Target levels 2 through 9")]
        [SerializeField] private int[] goldCostByTargetLevel = { 100, 200, 400, 800, 1600, 3200, 6400, 12800 };
        [SerializeField, Range(0f, 1f)] private float[] successChanceByTargetLevel = { 1f, .85f, .75f, .65f, .55f, .45f, .35f, .25f };
        [SerializeField, Range(0f, 1f)] private float maximumSuccessChance = .95f;

        [Header("Optional upgrade materials")]
        [SerializeField] private ItemDefinition protectionStone;
        [SerializeField] private GemUpgradeCatalyst[] catalysts = Array.Empty<GemUpgradeCatalyst>();

        public ItemDefinition ProtectionStone => protectionStone;
        public int CatalystCount => catalysts?.Length ?? 0;

        public bool TryGetGoldCost(int mainGemLevel, out int goldCost)
        {
            int targetLevel = mainGemLevel + 1;
            int index = targetLevel - 2;
            if (mainGemLevel < 1 || mainGemLevel >= 9 || goldCostByTargetLevel == null ||
                index < 0 || index >= goldCostByTargetLevel.Length)
            {
                goldCost = 0;
                return false;
            }

            goldCost = goldCostByTargetLevel[index];
            return goldCost > 0;
        }

        public float GetSuccessChance(int mainGemLevel, ItemDefinition catalyst)
        {
            if (mainGemLevel <= 1)
                return 1f;
            if (mainGemLevel >= 9 || successChanceByTargetLevel == null)
                return 0f;

            int index = mainGemLevel + 1 - 2;
            if (index < 0 || index >= successChanceByTargetLevel.Length)
                return 0f;

            float chance = Mathf.Clamp01(successChanceByTargetLevel[index]);
            if (catalyst != null && TryGetCatalystBonus(catalyst, out float bonus))
                chance += bonus;

            float cap = Mathf.Clamp(maximumSuccessChance, 0f, .99f);
            return Mathf.Min(chance, cap);
        }

        public ItemDefinition GetCatalyst(int index)
        {
            return catalysts != null && index >= 0 && index < catalysts.Length
                ? catalysts[index].Definition
                : null;
        }

        public bool TryGetCatalystBonus(ItemDefinition definition, out float bonus)
        {
            if (definition != null && catalysts != null)
            {
                foreach (GemUpgradeCatalyst catalyst in catalysts)
                {
                    if (catalyst.Definition == definition)
                    {
                        bonus = Mathf.Clamp01(catalyst.SuccessChanceBonus);
                        return true;
                    }
                }
            }

            bonus = 0f;
            return false;
        }

        private void OnValidate()
        {
            if (goldCostByTargetLevel == null || goldCostByTargetLevel.Length != 8)
                Array.Resize(ref goldCostByTargetLevel, 8);
            if (successChanceByTargetLevel == null || successChanceByTargetLevel.Length != 8)
                Array.Resize(ref successChanceByTargetLevel, 8);
            if (successChanceByTargetLevel != null && successChanceByTargetLevel.Length > 0)
                successChanceByTargetLevel[0] = 1f;
            maximumSuccessChance = Mathf.Clamp(maximumSuccessChance, 0f, .99f);
        }
    }
}
