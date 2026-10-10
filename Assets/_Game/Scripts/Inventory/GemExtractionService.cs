using System.Collections.Generic;
using LootboundIsles.Economy;
using UnityEngine;

namespace LootboundIsles.Inventory
{
    [RequireComponent(typeof(PlayerInventory), typeof(PlayerWallet))]
    public sealed class GemExtractionService : MonoBehaviour
    {
        [SerializeField] private GemExtractionRules rules;

        private PlayerInventory inventory;
        private PlayerWallet wallet;

        public GemExtractionRules Rules => rules;

        private void Awake()
        {
            EnsureReferences();
        }

        public bool TryGetExtractionInfo(string equipmentInstanceId, IReadOnlyList<int> socketIndexes,
            ItemDefinition catalyst, out EquipmentInstance equipment, out int goldCost, out string reason)
        {
            EnsureReferences();
            equipment = FindEquipment(equipmentInstanceId);
            goldCost = 0;
            reason = null;

            if (equipment == null) reason = "Select Equipment from inventory.";
            else if (socketIndexes == null || socketIndexes.Count == 0) reason = "Select one or more occupied sockets.";
            else if (rules == null || !rules.TryGetGoldCost(socketIndexes.Count, out goldCost)) reason = "Extraction rules are not configured.";
            else if (wallet == null || wallet.Gold < goldCost) reason = "Not enough Gold.";
            else if (catalyst != null && (!rules.TryGetCatalyst(catalyst, out _) ||
                                           catalyst.Category != ItemCategory.Materials || !catalyst.IsStackable ||
                                           inventory.GetQuantity(catalyst) < 1)) reason = "Selected Extraction Catalyst is unavailable.";
            else
            {
                HashSet<int> uniqueSockets = new();
                foreach (int socketIndex in socketIndexes)
                {
                    if (socketIndex < 0 || socketIndex >= EquipmentInstance.SocketCount ||
                        !uniqueSockets.Add(socketIndex) || !equipment.HasSocketContent(socketIndex) ||
                        equipment.SocketedGems[socketIndex] == null)
                    {
                        reason = "Selected socket is empty, unavailable, or contains unresolved Gem data.";
                        break;
                    }
                    if (!rules.TryGetChances(equipment.SocketedGems[socketIndex].Level, catalyst, out _))
                    {
                        reason = "Extraction chances are missing for a selected Gem level.";
                        break;
                    }
                }
            }

            return reason == null;
        }

        public bool TryExtract(string equipmentInstanceId, IReadOnlyList<int> socketIndexes,
            ItemDefinition catalyst, out IReadOnlyList<GemExtractionOutcome> outcomes, out string reason)
        {
            outcomes = null;
            if (!TryGetExtractionInfo(equipmentInstanceId, socketIndexes, catalyst,
                    out EquipmentInstance equipment, out int goldCost, out reason))
                return false;

            List<GemExtractionOutcome> rolls = new(socketIndexes.Count);
            foreach (int socketIndex in socketIndexes)
            {
                GemInstance gem = equipment.SocketedGems[socketIndex];
                rules.TryGetChances(gem.Level, catalyst, out GemExtractionChances chances);
                rolls.Add(new GemExtractionOutcome(socketIndex, gem, Roll(chances)));
            }

            if (!inventory.TryExtractGems(equipmentInstanceId, socketIndexes, rolls, goldCost, catalyst, wallet))
            {
                reason = "Extraction could not be saved; no extraction was applied.";
                return false;
            }

            outcomes = rolls;
            reason = null;
            return true;
        }

        public int GetCatalystCount()
        {
            return rules == null ? 0 : rules.CatalystCount;
        }

        public ItemDefinition GetCatalyst(int index)
        {
            return rules == null ? null : rules.GetCatalyst(index);
        }

        private GemExtractionResult Roll(GemExtractionChances chances)
        {
            float roll = Random.value;
            if (roll < chances.Success) return GemExtractionResult.Success;
            if (roll < chances.Success + chances.Downgrade) return GemExtractionResult.Downgrade;
            return GemExtractionResult.Destroy;
        }

        private EquipmentInstance FindEquipment(string instanceId)
        {
            if (inventory == null || string.IsNullOrEmpty(instanceId)) return null;
            foreach (EquipmentInstance item in inventory.Equipment)
                if (item != null && item.InstanceId == instanceId) return item;
            return null;
        }

        private void EnsureReferences()
        {
            inventory ??= GetComponent<PlayerInventory>();
            wallet ??= GetComponent<PlayerWallet>();
        }
    }
}
