using System;
using System.Collections.Generic;

namespace LootboundIsles.Save
{
    [Serializable]
    public class PlayerProgressSaveData
    {
        public const int CurrentVersion = 1;

        public int saveVersion = CurrentVersion;
        public long savedAtUtcTicks;
        public List<string> unlockedZoneIds = new();
        public List<string> defeatedMainBossIds = new();
        public int gold;
        public int piastres;
    }
}
