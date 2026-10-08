using UnityEngine;

namespace LootboundIsles.Combat
{
    [CreateAssetMenu(
        fileName = "CombatRulesDefinition",
        menuName = "Lootbound Isles/Combat/Combat Rules Definition"
    )]
    public class CombatRulesDefinition : ScriptableObject
    {
        [Header("Hit Chance")]
        [SerializeField, Range(0f, 1f)]
        private float equalStatsHitChance = 0.9f;

        [SerializeField, Range(0f, 1f)]
        private float minimumHitChance = 0.2f;

        [SerializeField, Range(0f, 1f)]
        private float maximumHitChance = 1f;

        [SerializeField, Min(0f)]
        private float hitChancePerStatPoint = 0.005f;

        [Header("Damage")]
        [SerializeField, Min(0.01f)]
        private float defenseConstant = 100f;

        [SerializeField, Min(1)]
        private int minimumDamage = 1;

        public float EqualStatsHitChance => equalStatsHitChance;
        public float MinimumHitChance => minimumHitChance;
        public float MaximumHitChance => maximumHitChance;
        public float HitChancePerStatPoint => hitChancePerStatPoint;
        public float DefenseConstant => defenseConstant;
        public int MinimumDamage => minimumDamage;

        private void OnValidate()
        {
            maximumHitChance = Mathf.Max(
                minimumHitChance,
                maximumHitChance
            );

            equalStatsHitChance = Mathf.Clamp(
                equalStatsHitChance,
                minimumHitChance,
                maximumHitChance
            );

            defenseConstant = Mathf.Max(0.01f, defenseConstant);
            minimumDamage = Mathf.Max(1, minimumDamage);
        }
    }
}
