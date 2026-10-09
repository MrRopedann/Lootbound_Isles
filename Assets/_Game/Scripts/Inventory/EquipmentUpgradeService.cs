using UnityEngine;
using System.Text;
using LootboundIsles.Economy;

namespace LootboundIsles.Inventory
{
    [RequireComponent(typeof(PlayerInventory), typeof(PlayerWallet))]
    public sealed class EquipmentUpgradeService : MonoBehaviour
    {
        [SerializeField] private EquipmentUpgradeRules rules;
        private PlayerInventory inventory;
        private PlayerWallet wallet;

        private void Awake()
        {
            inventory = GetComponent<PlayerInventory>();
            wallet = GetComponent<PlayerWallet>();
        }

        public bool TryGetUpgradeInfo(string targetId, out EquipmentInstance target,
            out EquipmentInstance donor, out ItemDefinition fragment, out int goldCost, out int fragmentCost)
        {
            target = FindEquipment(targetId);
            donor = null;
            fragment = null;
            goldCost = 0;
            fragmentCost = 0;
            if (target == null || target.Level >= 10 || rules == null ||
                !rules.TryGetCost(target.Level + 1, out goldCost, out fragmentCost))
                return false;

            fragment = rules.GetFragment(target.Definition.Rarity);
            foreach (EquipmentInstance candidate in inventory.Equipment)
            {
                if (candidate != null && candidate != target && !candidate.IsLocked &&
                    candidate.Definition == target.Definition)
                {
                    donor = candidate;
                    break;
                }
            }
            return donor != null && fragment != null;
        }

        public bool TryUpgrade(string targetId)
        {
            if (!TryGetUpgradeInfo(targetId, out EquipmentInstance target, out EquipmentInstance donor,
                    out ItemDefinition fragment, out int goldCost, out int fragmentCost))
                return false;
            return inventory.TryUpgradeEquipment(target.InstanceId, donor.InstanceId, fragment,
                fragmentCost, goldCost, wallet);
        }

        public int GetSellValue(string instanceId)
        {
            EquipmentInstance item = FindEquipment(instanceId);
            return item == null || item.IsLocked || rules == null ? 0 : rules.GetSellGold(item.Definition.Rarity, item.Level);
        }

        public int GetDismantleYield(string instanceId)
        {
            EquipmentInstance item = FindEquipment(instanceId);
            return item == null || item.IsLocked || rules == null ? 0 : rules.GetDismantleYield(item.Definition.Rarity, item.Level);
        }

        public bool TrySell(string instanceId)
        {
            int value = GetSellValue(instanceId);
            return value > 0 && inventory.TrySellEquipment(instanceId, value, wallet);
        }

        public bool TryDismantle(string instanceId)
        {
            EquipmentInstance item = FindEquipment(instanceId);
            if (item == null || rules == null) return false;
            ItemDefinition fragment = rules.GetFragment(item.Definition.Rarity);
            int yield = rules.GetDismantleYield(item.Definition.Rarity, item.Level);
            return yield > 0 && inventory.TryDismantleEquipment(instanceId, fragment, yield);
        }

        public bool TryGetExchangeInfo(ItemDefinition selected, out string summary)
        {
            summary = null;
            if (selected == null || selected.Category != ItemCategory.Materials || rules == null || inventory == null) return false;
            ItemDefinition[] fragments = new ItemDefinition[5];
            int selectedIndex = -1;
            for (int i = 0; i < fragments.Length; i++)
            {
                fragments[i] = rules.GetFragment((ItemRarity)i);
                if (fragments[i] == selected) selectedIndex = i;
            }
            if (selectedIndex < 0 || inventory.GetQuantity(selected) <= 0 ||
                fragments.Length != 5 || System.Array.Exists(fragments, item => item == null)) return false;

            long carry = 0;
            long[] resultingCounts = new long[fragments.Length];
            bool willExchange = false;
            for (int i = 0; i < fragments.Length; i++)
            {
                int currentCount = inventory.GetQuantity(fragments[i]);
                long total = currentCount + carry;
                if (i == fragments.Length - 1)
                {
                    resultingCounts[i] = total;
                    if (resultingCounts[i] > int.MaxValue) return false;
                    willExchange |= resultingCounts[i] != currentCount;
                    break;
                }
                int rate = rules.GetExchangeRate((ItemRarity)i);
                if (rate < 2) return false;
                resultingCounts[i] = total % rate;
                carry = total / rate;
                if (resultingCounts[i] > int.MaxValue) return false;
                willExchange |= resultingCounts[i] != currentCount;
            }
            StringBuilder result = new("After exchange: ");
            for (int i = 0; i < resultingCounts.Length; i++)
            {
                if (resultingCounts[i] <= 0) continue;
                if (result[result.Length - 1] != ' ') result.Append(" · ");
                result.Append($"{resultingCounts[i]:N0} {fragments[i].DisplayName}");
            }
            summary = result.ToString();
            return willExchange && !string.IsNullOrEmpty(summary);
        }

        public bool TryExchange(ItemDefinition selected)
        {
            if (!TryGetExchangeInfo(selected, out _)) return false;
            ItemDefinition[] fragments = new ItemDefinition[5];
            int[] rates = new int[4];
            for (int i = 0; i < fragments.Length; i++)
            {
                fragments[i] = rules.GetFragment((ItemRarity)i);
                if (i < rates.Length) rates[i] = rules.GetExchangeRate((ItemRarity)i);
            }
            return inventory.TryExchangeFragments(fragments, rates);
        }

        private EquipmentInstance FindEquipment(string id)
        {
            if (inventory == null || string.IsNullOrEmpty(id)) return null;
            foreach (EquipmentInstance item in inventory.Equipment)
                if (item != null && item.InstanceId == id) return item;
            return null;
        }
    }
}
