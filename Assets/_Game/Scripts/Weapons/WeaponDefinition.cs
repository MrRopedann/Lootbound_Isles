using UnityEngine;

namespace LootboundIsles.Weapons
{
    [CreateAssetMenu(
        fileName = "WeaponDefinition",
        menuName = "Lootbound Isles/Weapons/Weapon Definition"
    )]
    public class WeaponDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private WeaponType weaponType = WeaponType.Sword;

        [Header("Combat")]
        [SerializeField, Min(0f)] private float damage = 10f;
        [SerializeField, Min(0.1f)] private float attackRange = 2f;
        [SerializeField, Min(0.01f)] private float attackCooldown = 1f;

        [Header("GreatSword")]
        [SerializeField, Range(0f, 360f)]
        private float greatSwordAngle = 90f;

        [SerializeField, Min(1)]
        private int maxTargets = 3;

        [Header("Bow")]
        [SerializeField]
        private PlayerProjectile projectilePrefab;

        [SerializeField, Min(0.01f)]
        private float projectileSpeed = 12f;

        [SerializeField, Min(0.01f)]
        private float projectileLifetime = 10f;

        public WeaponType WeaponType => weaponType;
        public float Damage => damage;
        public float AttackRange => attackRange;
        public float AttackCooldown => attackCooldown;
        public float GreatSwordAngle => greatSwordAngle;
        public int MaxTargets => maxTargets;
        public PlayerProjectile ProjectilePrefab => projectilePrefab;
        public float ProjectileSpeed => projectileSpeed;
        public float ProjectileLifetime => projectileLifetime;
    }
}
