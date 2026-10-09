using LootboundIsles.Economy;
using UnityEngine;

namespace LootboundIsles.Inventory
{
    [RequireComponent(typeof(PlayerInventory), typeof(PlayerWallet))]
    public sealed class GemUpgradeService : MonoBehaviour
    {
        [SerializeField] private GemUpgradeRules rules;

        private PlayerInventory inventory;
        private PlayerWallet wallet;

        public GemUpgradeRules Rules => rules;

        private void Awake()
        {
            inventory = GetComponent<PlayerInventory>();
            wallet = GetComponent<PlayerWallet>();
        }

        public bool TryGetUpgradeInfo(string targetId, out GemInstance target, out GemInstance donor,
            out int goldCost, out float successChance)
        {
            EnsureReferences();
            target = FindGem(targetId);
            donor = null;
            goldCost = 0;
            successChance = 0f;
            if (target == null || target.Level >= 9 || rules == null ||
                !rules.TryGetGoldCost(target.Level, out goldCost))
                return false;

            foreach (GemInstance candidate in inventory.Gems)
            {
                if (candidate != null && candidate != target && candidate.Level == 1 &&
                    candidate.Definition == target.Definition)
                {
                    donor = candidate;
                    break;
                }
            }

            if (donor == null)
                return false;

            successChance = rules.GetSuccessChance(target.Level, null);
            return true;
        }

        public bool CanUseProtectionStone()
        {
            EnsureReferences();
            return rules != null && rules.ProtectionStone != null &&
                   IsValidUpgradeMaterial(rules.ProtectionStone) &&
                   inventory.GetQuantity(rules.ProtectionStone) > 0;
        }

        public int GetCatalystCount()
        {
            EnsureReferences();
            return rules == null ? 0 : rules.CatalystCount;
        }

        public ItemDefinition GetCatalyst(int index)
        {
            EnsureReferences();
            return rules == null ? null : rules.GetCatalyst(index);
        }

        public bool TryUpgrade(string targetId, bool useProtectionStone, ItemDefinition catalyst,
            out GemUpgradeResult result, out float successChance)
        {
            result = GemUpgradeResult.Unavailable;
            successChance = 0f;
            if (!TryGetUpgradeInfo(targetId, out GemInstance target, out GemInstance donor,
                    out int goldCost, out _))
                return false;

            ItemDefinition protection = useProtectionStone ? rules.ProtectionStone : null;
            if (useProtectionStone && (!CanUseProtectionStone() || target.Level == 1))
                return false;
            if (target.Level == 1)
                catalyst = null;
            if (catalyst != null && (!rules.TryGetCatalystBonus(catalyst, out _) ||
                                     !IsValidUpgradeMaterial(catalyst) || inventory.GetQuantity(catalyst) <= 0))
                return false;

            successChance = rules.GetSuccessChance(target.Level, catalyst);
            if (wallet == null || wallet.Gold < goldCost)
                return false;

            bool succeeded = target.Level == 1 || Random.value < successChance;
            bool saved = inventory.TryUpgradeGem(target.InstanceId, donor.InstanceId, goldCost,
                protection, catalyst, succeeded, wallet, out bool wasProtected);
            if (!saved)
            {
                result = GemUpgradeResult.SaveFailed;
                return false;
            }

            result = succeeded
                ? GemUpgradeResult.Success
                : wasProtected ? GemUpgradeResult.FailedProtected : GemUpgradeResult.FailedDowngraded;
            return true;
        }

        private GemInstance FindGem(string instanceId)
        {
            EnsureReferences();
            if (inventory == null || string.IsNullOrEmpty(instanceId))
                return null;
            foreach (GemInstance gem in inventory.Gems)
                if (gem != null && gem.InstanceId == instanceId)
                    return gem;
            return null;
        }

        private bool IsValidUpgradeMaterial(ItemDefinition definition)
        {
            return definition != null && definition.Category == ItemCategory.Materials &&
                   definition.IsStackable;
        }

        private void EnsureReferences()
        {
            inventory ??= GetComponent<PlayerInventory>();
            wallet ??= GetComponent<PlayerWallet>();
        }
    }

    public enum GemUpgradeResult
    {
        Unavailable,
        SaveFailed,
        Success,
        FailedProtected,
        FailedDowngraded
    }
}
