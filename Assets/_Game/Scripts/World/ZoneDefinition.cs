using UnityEngine;

namespace LootboundIsles.World
{
    [CreateAssetMenu(
        fileName = "ZoneDefinition",
        menuName = "Lootbound Isles/World/Zone Definition"
    )]
    public class ZoneDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField]
        private string zoneId;

        [SerializeField]
        private string displayName;

        [Header("Progression")]
        [SerializeField]
        private bool unlockedByDefault;

        public string ZoneId => zoneId;
        public string DisplayName => displayName;
        public bool UnlockedByDefault => unlockedByDefault;
    }
}
