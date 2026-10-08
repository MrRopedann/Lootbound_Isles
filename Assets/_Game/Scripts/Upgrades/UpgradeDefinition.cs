using UnityEngine;

namespace LootboundIsles.Upgrades
{
    [CreateAssetMenu(
        fileName = "UpgradeDefinition",
        menuName = "Lootbound Isles/Upgrades/Upgrade Definition"
    )]
    public class UpgradeDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private UpgradeType upgradeType;

        [Header("Display")]
        [SerializeField] private string displayName;

        [TextArea]
        [SerializeField] private string description;

        [Header("Value")]
        [SerializeField] private float value;

        public UpgradeType UpgradeType => upgradeType;
        public string DisplayName => displayName;
        public string Description => description;
        public float Value => value;
    }
}