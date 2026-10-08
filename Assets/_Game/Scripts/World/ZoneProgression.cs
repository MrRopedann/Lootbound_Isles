using System;
using System.Collections.Generic;
using LootboundIsles.Enemies;
using LootboundIsles.Save;
using UnityEngine;

namespace LootboundIsles.World
{
    public class ZoneProgression : MonoBehaviour
    {
        [Header("Zones")]
        [SerializeField]
        private List<ZoneDefinition> zones = new();

        [SerializeField]
        private LocalSaveSystem saveSystem;

        [Header("DEBUG")]
        [SerializeField]
        private bool isDebug;

        private readonly HashSet<string>
            unlockedZoneIds = new();

        private readonly HashSet<string>
            defeatedMainBossIds = new();

        private bool isInitialized;

        public event Action<ZoneDefinition> ZoneUnlocked;
        public event Action<EnemyDefinition> MainBossFirstDefeated;

        private void Awake()
        {
            if (saveSystem == null)
                saveSystem = GetComponent<LocalSaveSystem>();

            EnsureInitialized();
        }

        public bool IsZoneUnlocked(
            ZoneDefinition zone)
        {
            EnsureInitialized();

            return zone != null &&
                   !string.IsNullOrWhiteSpace(zone.ZoneId) &&
                   unlockedZoneIds.Contains(zone.ZoneId);
        }

        public bool RegisterEnemyDefeat(
            EnemyDefinition enemyDefinition)
        {
            EnsureInitialized();

            if (enemyDefinition == null ||
                enemyDefinition.EnemyType != EnemyType.Boss)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(
                    enemyDefinition.EnemyId))
            {
                Debug.LogError(
                    $"ZoneProgression: у Boss definition " +
                    $"{enemyDefinition.name} не указан Enemy ID.",
                    enemyDefinition
                );

                return false;
            }

            bool isFirstDefeat =
                defeatedMainBossIds.Add(
                    enemyDefinition.EnemyId
                );

            if (!isFirstDefeat)
                return false;

            ZoneDefinition zoneToUnlock =
                enemyDefinition.ZoneUnlockedOnFirstKill;

            if (zoneToUnlock != null)
                UnlockZoneInternal(
                    zoneToUnlock,
                    false
                );

            SaveProgression();

            MainBossFirstDefeated?.Invoke(
                enemyDefinition
            );

            if (isDebug)
            {
                Debug.Log(
                    $"ZoneProgression: Main Boss " +
                    $"{enemyDefinition.EnemyId} побеждён впервые.",
                    this
                );
            }

            return true;
        }

        public bool UnlockZone(
            ZoneDefinition zone)
        {
            EnsureInitialized();

            return UnlockZoneInternal(
                zone,
                true
            );
        }

        private bool UnlockZoneInternal(
            ZoneDefinition zone,
            bool saveImmediately)
        {
            if (!ValidateZone(zone))
                return false;

            if (!unlockedZoneIds.Add(zone.ZoneId))
                return false;

            ZoneUnlocked?.Invoke(zone);

            if (saveImmediately)
                SaveProgression();

            if (isDebug)
            {
                Debug.Log(
                    $"ZoneProgression: открыта зона " +
                    $"{zone.DisplayName} ({zone.ZoneId}).",
                    this
                );
            }

            return true;
        }

        private void EnsureInitialized()
        {
            if (isInitialized)
                return;

            isInitialized = true;

            if (saveSystem != null &&
                saveSystem.TryLoadProgression(
                    out PlayerProgressSaveData saveData))
            {
                AddValidIds(
                    unlockedZoneIds,
                    saveData.unlockedZoneIds
                );

                AddValidIds(
                    defeatedMainBossIds,
                    saveData.defeatedMainBossIds
                );
            }

            foreach (ZoneDefinition zone in zones)
            {
                if (zone == null ||
                    !zone.UnlockedByDefault)
                {
                    continue;
                }

                if (ValidateZone(zone))
                    unlockedZoneIds.Add(zone.ZoneId);
            }
        }

        private void AddValidIds(
            HashSet<string> destination,
            List<string> source)
        {
            if (source == null)
                return;

            foreach (string id in source)
            {
                if (!string.IsNullOrWhiteSpace(id))
                    destination.Add(id);
            }
        }

        private void SaveProgression()
        {
            if (saveSystem == null)
            {
                if (isDebug)
                {
                    Debug.LogWarning(
                        "ZoneProgression: LocalSaveSystem не назначен; " +
                        "изменения не переживут перезапуск.",
                        this
                    );
                }

                return;
            }

            saveSystem.SaveProgression(
                unlockedZoneIds,
                defeatedMainBossIds
            );
        }

        private bool ValidateZone(
            ZoneDefinition zone)
        {
            if (zone == null)
                return false;

            if (!string.IsNullOrWhiteSpace(
                    zone.ZoneId))
            {
                return true;
            }

            Debug.LogError(
                $"ZoneProgression: у Zone Definition " +
                $"{zone.name} не указан Zone ID.",
                zone
            );

            return false;
        }
    }
}
