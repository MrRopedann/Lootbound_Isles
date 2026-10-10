using System;
using UnityEngine;

namespace LootboundIsles.Inventory
{
    [Serializable]
    public struct GemExtractionChances
    {
        [SerializeField, Range(0f, 1f)] private float success;
        [SerializeField, Range(0f, 1f)] private float downgrade;
        [SerializeField, Range(0f, 1f)] private float destroy;

        public float Success => success;
        public float Downgrade => downgrade;
        public float Destroy => destroy;

        public GemExtractionChances(float success, float downgrade, float destroy)
        {
            this.success = success;
            this.downgrade = downgrade;
            this.destroy = destroy;
        }
    }

    [Serializable]
    public struct GemExtractionCatalyst
    {
        [SerializeField] private ItemDefinition definition;
        [SerializeField, Range(0f, 1f)] private float successChanceBonus;
        [SerializeField, Range(0f, 1f)] private float destroyChanceReduction;

        public ItemDefinition Definition => definition;
        public float SuccessChanceBonus => successChanceBonus;
        public float DestroyChanceReduction => destroyChanceReduction;
    }

    [CreateAssetMenu(fileName = "GemExtractionRules", menuName = "Lootbound Isles/Gems/Extraction Rules")]
    public sealed class GemExtractionRules : ScriptableObject
    {
        [Header("Prototype tuning; balance is not fixed by the GDD")]
        [SerializeField, Min(1)] private int goldCostPerSocket = 250;
        [SerializeField] private GemExtractionChances[] chancesByGemLevel =
        {
            new(.70f, .20f, .10f), new(.68f, .22f, .10f), new(.66f, .23f, .11f),
            new(.64f, .24f, .12f), new(.62f, .25f, .13f), new(.60f, .26f, .14f),
            new(.58f, .27f, .15f), new(.56f, .28f, .16f), new(.54f, .29f, .17f)
        };
        [SerializeField] private GemExtractionCatalyst[] catalysts = Array.Empty<GemExtractionCatalyst>();

        public int GoldCostPerSocket => Mathf.Max(1, goldCostPerSocket);
        public int CatalystCount => catalysts?.Length ?? 0;

        public bool TryGetGoldCost(int socketCount, out int goldCost)
        {
            long cost = (long)GoldCostPerSocket * socketCount;
            if (socketCount <= 0 || cost > int.MaxValue)
            {
                goldCost = 0;
                return false;
            }
            goldCost = (int)cost;
            return true;
        }

        public bool TryGetChances(int gemLevel, ItemDefinition catalyst, out GemExtractionChances chances)
        {
            chances = default;
            int index = gemLevel - 1;
            if (index < 0 || index >= 9 || chancesByGemLevel == null || index >= chancesByGemLevel.Length ||
                (catalyst != null && !TryGetCatalyst(catalyst, out _)))
                return false;

            GemExtractionChances source = chancesByGemLevel[index];
            float total = Mathf.Max(0f, source.Success) + Mathf.Max(0f, source.Downgrade) + Mathf.Max(0f, source.Destroy);
            if (total <= 0f) return false;

            float success = Mathf.Max(0f, source.Success) / total;
            float downgrade = Mathf.Max(0f, source.Downgrade) / total;
            float destroy = Mathf.Max(0f, source.Destroy) / total;
            if (catalyst != null)
            {
                TryGetCatalyst(catalyst, out GemExtractionCatalyst modifier);
                destroy = Mathf.Max(0f, destroy - Mathf.Clamp01(modifier.DestroyChanceReduction));
                success = Mathf.Min(success + Mathf.Clamp01(modifier.SuccessChanceBonus), 1f - destroy);
            }

            // Extraction must remain risky, even when a catalyst removes Destroy chance.
            success = Mathf.Min(success, .99f);
            downgrade = Mathf.Max(0f, 1f - success - destroy);
            chances = new GemExtractionChances(success, downgrade, destroy);
            return true;
        }

        public bool TryGetCatalyst(ItemDefinition definition, out GemExtractionCatalyst catalyst)
        {
            if (definition != null && catalysts != null)
            {
                foreach (GemExtractionCatalyst candidate in catalysts)
                {
                    if (candidate.Definition == definition)
                    {
                        catalyst = candidate;
                        return true;
                    }
                }
            }
            catalyst = default;
            return false;
        }

        public ItemDefinition GetCatalyst(int index)
        {
            return catalysts != null && index >= 0 && index < catalysts.Length
                ? catalysts[index].Definition
                : null;
        }

        private void OnValidate()
        {
            goldCostPerSocket = Mathf.Max(1, goldCostPerSocket);
            if (chancesByGemLevel == null || chancesByGemLevel.Length != 9)
                Array.Resize(ref chancesByGemLevel, 9);
        }
    }

    public enum GemExtractionResult
    {
        Success,
        Downgrade,
        Destroy
    }

    public sealed class GemExtractionOutcome
    {
        public int SocketIndex { get; }
        public GemInstance OriginalGem { get; }
        public GemExtractionResult Result { get; }
        public int ReturnedLevel { get; }
        public bool ReturnsGem => (Result == GemExtractionResult.Success || Result == GemExtractionResult.Downgrade) &&
                                  ReturnedLevel > 0;
        public bool IsValid => SocketIndex >= 0 && OriginalGem != null && Enum.IsDefined(typeof(GemExtractionResult), Result);

        public GemExtractionOutcome(int socketIndex, GemInstance originalGem, GemExtractionResult result)
        {
            SocketIndex = socketIndex;
            OriginalGem = originalGem;
            Result = result;
            ReturnedLevel = result == GemExtractionResult.Downgrade && originalGem != null
                ? Mathf.Max(0, originalGem.Level - 1)
                : originalGem?.Level ?? 0;
        }
    }
}
