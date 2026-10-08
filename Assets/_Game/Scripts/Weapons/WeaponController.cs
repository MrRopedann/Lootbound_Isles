using System;
using System.Collections.Generic;
using LootboundIsles.Combat;
using LootboundIsles.Enemies;
using LootboundIsles.Player;
using UnityEngine;

namespace LootboundIsles.Weapons
{
    [RequireComponent(typeof(PlayerTargeting))]
    [RequireComponent(typeof(PlayerStats))]
    [RequireComponent(typeof(CombatState))]
    [RequireComponent(typeof(CombatResolver))]
    public class WeaponController : MonoBehaviour
    {
        [Header("Weapon")]
        [SerializeField]
        private WeaponDefinition weaponDefinition;

        [SerializeField]
        private Transform projectileOrigin;

        [Header("DEBUG")]
        [SerializeField]
        private bool isDebug;

        private PlayerTargeting targeting;
        private PlayerStats playerStats;
        private CombatState combatState;
        private CombatResolver combatResolver;
        private PlayerDeath playerDeath;

        private readonly List<Enemy> greatSwordTargets = new(8);

        private float attackTimer;

        public WeaponDefinition CurrentWeapon => weaponDefinition;

        public float RemainingAttackCooldown =>
            Mathf.Max(0f, attackTimer);

        public event Action<WeaponDefinition> WeaponChanged;

        private void Awake()
        {
            targeting =
                GetComponent<PlayerTargeting>();

            playerStats =
                GetComponent<PlayerStats>();

            combatState =
                GetComponent<CombatState>();

            combatResolver =
                GetComponent<CombatResolver>();

            playerDeath =
                GetComponent<PlayerDeath>();

            if (combatState == null ||
                combatResolver == null ||
                playerDeath == null)
            {
                Debug.LogError(
                    "WeaponController: отсутствует CombatState, CombatResolver или PlayerDeath.",
                    this
                );

                enabled = false;
            }
        }

        private void Update()
        {
            if (weaponDefinition == null)
                return;

            attackTimer -= Time.deltaTime;

            TryAttack();
        }

        private void TryAttack()
        {
            if (attackTimer > 0f)
                return;

            float finalAttackRange =
                playerStats.GetFinalAttackRange(
                    weaponDefinition.AttackRange
                );

            Enemy target =
                targeting.FindNearestEnemy(
                    finalAttackRange
                );

            if (target == null)
                return;

            switch (weaponDefinition.WeaponType)
            {
                case WeaponType.Sword:
                    PerformSwordAttack(
                        target,
                        finalAttackRange
                    );
                    break;

                case WeaponType.GreatSword:
                    PerformGreatSwordAttack(
                        target,
                        finalAttackRange
                    );
                    break;

                case WeaponType.Bow:
                    PerformBowAttack(target);
                    break;
            }
        }

        public bool EquipWeapon(
            WeaponDefinition newWeaponDefinition)
        {
            if (newWeaponDefinition == null)
            {
                Debug.LogError(
                    "WeaponController: нельзя экипировать пустой WeaponDefinition.",
                    this
                );

                return false;
            }

            if (weaponDefinition == newWeaponDefinition)
                return false;

            weaponDefinition = newWeaponDefinition;
            WeaponChanged?.Invoke(weaponDefinition);

            if (isDebug)
            {
                Debug.Log(
                    $"Weapon equipped: {weaponDefinition.WeaponType} | " +
                    $"Remaining cooldown: {RemainingAttackCooldown:F2}s",
                    this
                );
            }

            return true;
        }

        private void PerformSwordAttack(
            Enemy target,
            float finalAttackRange)
        {
            Health targetHealth =
                target.GetComponent<Health>();

            EnemyStats targetStats =
                target.GetComponent<EnemyStats>();

            if (targetHealth == null ||
                targetHealth.IsDead ||
                targetStats == null)
            {
                return;
            }

            combatState.RegisterCombatEvent();

            float finalDamage =
                playerStats.GetFinalDamage(
                    weaponDefinition.Damage
                );

            attackTimer =
                playerStats.GetFinalAttackCooldown(
                    weaponDefinition.AttackCooldown
                );

            AttackResult result =
                combatResolver.ResolveAttack(
                    finalDamage,
                    playerStats.Accuracy,
                    playerStats.CriticalChance,
                    targetStats.Defense,
                    targetStats.Evasion
                );

            if (!result.IsHit)
            {
                if (isDebug)
                {
                    Debug.Log(
                        $"{weaponDefinition.WeaponType} промахнулся " +
                        $"по {target.gameObject.name} | " +
                        $"Hit Chance: {result.HitChance:P0}",
                        this
                    );
                }

                return;
            }

            targetHealth.TakeDamage(result.Damage);

            if (isDebug)
            {
                string criticalText =
                    result.IsCritical
                        ? " | CRITICAL!"
                        : string.Empty;

                Debug.Log(
                    $"{weaponDefinition.WeaponType} атакует " +
                    $"{target.gameObject.name}: " +
                    $"{result.Damage} урона" +
                    $"{criticalText} | " +
                    $"Enemy HP: " +
                    $"{targetHealth.CurrentHealth:F2}/" +
                    $"{targetHealth.MaxHealth:F2} | " +
                    $"Range: {finalAttackRange:F2} | " +
                    $"Cooldown: {attackTimer:F2}s",
                    this
                );
            }
        }

        private void PerformGreatSwordAttack(
            Enemy primaryTarget,
            float finalAttackRange)
        {
            Vector3 attackDirection =
                primaryTarget.transform.position -
                transform.position;

            targeting.FindEnemiesInFront(
                finalAttackRange,
                attackDirection,
                weaponDefinition.GreatSwordAngle,
                weaponDefinition.MaxTargets,
                greatSwordTargets
            );

            if (greatSwordTargets.Count == 0)
                return;

            combatState.RegisterCombatEvent();

            attackTimer =
                playerStats.GetFinalAttackCooldown(
                    weaponDefinition.AttackCooldown
                );

            float finalDamage =
                playerStats.GetFinalDamage(
                    weaponDefinition.Damage
                );

            bool sharedCritical =
                combatResolver.RollCriticalHit(
                    playerStats.CriticalChance
                );

            for (int i = 0; i < greatSwordTargets.Count; i++)
            {
                Enemy target = greatSwordTargets[i];

                if (target == null ||
                    target.Health == null ||
                    target.Health.IsDead)
                {
                    continue;
                }

                EnemyStats targetStats =
                    target.GetComponent<EnemyStats>();

                if (targetStats == null)
                    continue;

                AttackResult result =
                    combatResolver.ResolveAttack(
                        finalDamage,
                        playerStats.Accuracy,
                        playerStats.CriticalChance,
                        targetStats.Defense,
                        targetStats.Evasion,
                        sharedCritical
                    );

                if (result.IsHit)
                {
                    target.Health.TakeDamage(result.Damage);
                }

                if (isDebug)
                {
                    string outcome = result.IsHit
                        ? $"{result.Damage} damage" +
                          (result.IsCritical ? " | CRITICAL!" : string.Empty)
                        : "MISS";

                    Debug.Log(
                        $"GreatSword → {target.gameObject.name}: {outcome}",
                        this
                    );
                }
            }
        }

        private void PerformBowAttack(Enemy target)
        {
            if (weaponDefinition.ProjectilePrefab == null)
            {
                Debug.LogError(
                    "WeaponController: у Bow не назначен Projectile Prefab.",
                    weaponDefinition
                );

                enabled = false;
                return;
            }

            if (target == null ||
                target.Health == null ||
                target.Health.IsDead)
            {
                return;
            }

            Vector3 originPosition = projectileOrigin != null
                ? projectileOrigin.position
                : transform.position;

            PlayerProjectile projectile = Instantiate(
                weaponDefinition.ProjectilePrefab,
                originPosition,
                Quaternion.identity
            );

            float snapshotDamage =
                playerStats.GetFinalDamage(
                    weaponDefinition.Damage
                );

            projectile.Initialize(
                target,
                combatState,
                combatResolver,
                playerDeath,
                weaponDefinition.ProjectileSpeed,
                weaponDefinition.ProjectileLifetime,
                snapshotDamage,
                playerStats.Accuracy,
                playerStats.CriticalChance
            );

            combatState.RegisterCombatEvent();

            attackTimer =
                playerStats.GetFinalAttackCooldown(
                    weaponDefinition.AttackCooldown
                );

            if (isDebug)
            {
                Debug.Log(
                    $"Bow fired at {target.gameObject.name} | " +
                    $"Damage snapshot: {snapshotDamage:F2} | " +
                    $"Accuracy snapshot: {playerStats.Accuracy:F2} | " +
                    $"Crit snapshot: {playerStats.CriticalChance:P0}",
                    this
                );
            }
        }
    }
}
