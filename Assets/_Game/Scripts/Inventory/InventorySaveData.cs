using System;
using System.Collections.Generic;

namespace LootboundIsles.Inventory
{
    [Serializable]
    public sealed class InventorySaveData
    {
        public List<InventoryStackSaveData> stacks = new();
        public List<InventoryItemSaveData> uniqueItems = new();
        public List<EquipmentSaveData> equipment = new();
        public List<GemSaveData> gems = new();
    }

    [Serializable]
    public sealed class InventoryStackSaveData
    {
        public string itemId;
        public int quantity;
    }

    [Serializable]
    public class InventoryItemSaveData
    {
        public string instanceId;
        public string itemId;
    }

    [Serializable]
    public sealed class EquipmentSaveData : InventoryItemSaveData
    {
        public int level;
        public float efficiency;
        public bool isLocked;
        public GemSaveData[] socketedGems = new GemSaveData[EquipmentInstance.SocketCount];
    }

    [Serializable]
    public sealed class GemSaveData : InventoryItemSaveData
    {
        public int level = 1;
    }
}
