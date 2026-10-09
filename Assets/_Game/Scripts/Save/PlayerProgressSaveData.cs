using System;
using System.Collections.Generic;
using LootboundIsles.Inventory;

namespace LootboundIsles.Save
{
    [Serializable]
    public class PlayerProgressSaveData
    {
        public const int CurrentVersion = 4;

        public int saveVersion = CurrentVersion;
        public long savedAtUtcTicks;
        public List<string> unlockedZoneIds = new();
        public List<string> defeatedMainBossIds = new();
        public int gold;
        public int piastres;
        public InventorySaveData inventory = new();
    }
}
