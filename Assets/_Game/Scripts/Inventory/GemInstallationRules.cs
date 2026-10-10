using UnityEngine;

namespace LootboundIsles.Inventory
{
    [CreateAssetMenu(fileName = "GemInstallationRules", menuName = "Lootbound Isles/Gems/Installation Rules")]
    public sealed class GemInstallationRules : ScriptableObject
    {
        [SerializeField, Min(1)] private int goldCost = 50;

        public int GoldCost => Mathf.Max(1, goldCost);
    }
}
