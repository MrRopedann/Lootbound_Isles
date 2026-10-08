using UnityEngine;

namespace LootboundIsles.Combat
{
    public readonly struct AttackResult
    {
        public AttackResult(
            bool isHit,
            bool isCritical,
            int damage,
            float hitChance)
        {
            IsHit = isHit;
            IsCritical = isCritical;
            Damage = damage;
            HitChance = hitChance;
        }

        public bool IsHit { get; }
        public bool IsCritical { get; }
        public int Damage { get; }
        public float HitChance { get; }
    }

    public class CombatResolver : MonoBehaviour
    {
        private const float CriticalDamageMultiplier = 2f;

        [Header("Rules")]
        [SerializeField]
        private CombatRulesDefinition rules;

        public bool IsConfigured => rules != null;

        private void Awake()
        {
            if (rules != null)
                return;

            Debug.LogError(
                "CombatResolver: CombatRulesDefinition не назначен.",
                this
            );

            enabled = false;
        }

        public AttackResult ResolveAttack(
            float rawDamage,
            float accuracy,
            float criticalChance,
            float targetDefense,
            float targetEvasion,
            bool? criticalOverride = null)
        {
            if (rules == null)
                return new AttackResult(false, false, 0, 0f);

            float hitChance = CalculateHitChance(
                accuracy,
                targetEvasion
            );

            if (Random.value > hitChance)
            {
                return new AttackResult(
                    false,
                    false,
                    0,
                    hitChance
                );
            }

            bool isCritical =
                criticalOverride ??
                RollCriticalHit(criticalChance);

            float damageBeforeDefense =
                Mathf.Max(0f, rawDamage);

            if (isCritical)
            {
                damageBeforeDefense *=
                    CriticalDamageMultiplier;
            }

            float defense = Mathf.Max(0f, targetDefense);

            float damageAfterDefense =
                damageBeforeDefense *
                rules.DefenseConstant /
                (rules.DefenseConstant + defense);

            int roundedDamage =
                Mathf.FloorToInt(damageAfterDefense + 0.5f);

            int finalDamage = Mathf.Max(
                rules.MinimumDamage,
                roundedDamage
            );

            return new AttackResult(
                true,
                isCritical,
                finalDamage,
                hitChance
            );
        }

        public bool RollCriticalHit(float criticalChance)
        {
            return Random.value < Mathf.Clamp01(criticalChance);
        }

        private float CalculateHitChance(
            float accuracy,
            float targetEvasion)
        {
            float statDifference =
                Mathf.Max(0f, accuracy) -
                Mathf.Max(0f, targetEvasion);

            float hitChance =
                rules.EqualStatsHitChance +
                statDifference *
                rules.HitChancePerStatPoint;

            return Mathf.Clamp(
                hitChance,
                rules.MinimumHitChance,
                rules.MaximumHitChance
            );
        }
    }
}
