using LootboundIsles.Economy;
using UnityEngine;

namespace LootboundIsles.Inventory
{
    [RequireComponent(typeof(PlayerInventory), typeof(PlayerWallet))]
    public sealed class GemInstallationService : MonoBehaviour
    {
        [SerializeField] private GemInstallationRules rules;

        private PlayerInventory inventory;
        private PlayerWallet wallet;

        public GemInstallationRules Rules => rules;

        private void Awake()
        {
            inventory = GetComponent<PlayerInventory>();
            wallet = GetComponent<PlayerWallet>();
        }

        public bool TryGetInstallationInfo(string equipmentInstanceId, int socketIndex, string gemInstanceId,
            out EquipmentInstance equipment, out GemInstance gem, out int goldCost, out string reason)
        {
            equipment = FindEquipment(equipmentInstanceId);
            gem = FindGem(gemInstanceId);
            goldCost = rules == null ? 0 : rules.GoldCost;
            reason = null;

            if (equipment == null) reason = "Select an Equipment item.";
            else if (socketIndex < 0 || socketIndex >= EquipmentInstance.SocketCount) reason = "Select a socket.";
            else if (gem == null) reason = "Select a Gem from inventory.";
            else if (!equipment.CanInstallGem(socketIndex, gem)) reason = "Socket is occupied, slot is incompatible, or this exact Gem type is already installed.";
            else if (rules == null || goldCost <= 0) reason = "Gem installation rules are not configured.";
            else if (wallet == null || wallet.Gold < goldCost) reason = "Not enough Gold.";

            return reason == null;
        }

        public bool TryInstall(string equipmentInstanceId, int socketIndex, string gemInstanceId)
        {
            if (!TryGetInstallationInfo(equipmentInstanceId, socketIndex, gemInstanceId,
                    out _, out _, out int goldCost, out _))
                return false;

            return inventory.TryInstallGem(equipmentInstanceId, socketIndex, gemInstanceId, goldCost, wallet);
        }

        private EquipmentInstance FindEquipment(string instanceId)
        {
            if (inventory == null || string.IsNullOrEmpty(instanceId)) return null;
            foreach (EquipmentInstance item in inventory.Equipment)
                if (item != null && item.InstanceId == instanceId) return item;
            return null;
        }

        private GemInstance FindGem(string instanceId)
        {
            if (inventory == null || string.IsNullOrEmpty(instanceId)) return null;
            foreach (GemInstance gem in inventory.Gems)
                if (gem != null && gem.InstanceId == instanceId) return gem;
            return null;
        }
    }
}
