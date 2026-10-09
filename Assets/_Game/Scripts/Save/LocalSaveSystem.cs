using System;
using System.Collections.Generic;
using System.IO;
using LootboundIsles.Inventory;
using UnityEngine;

namespace LootboundIsles.Save
{
    public class LocalSaveSystem : MonoBehaviour
    {
        private const string SaveFileName =
            "lootbound-isles-save.json";

        private const string BackupFileName =
            "lootbound-isles-save.backup.json";

        [Header("DEBUG")]
        [SerializeField]
        private bool isDebug;

        private PlayerProgressSaveData cachedSaveData;
        private bool hasLoadedData;
        private bool hasValidSave;

        private string SavePath =>
            Path.Combine(
                Application.persistentDataPath,
                SaveFileName
            );

        private string BackupPath =>
            Path.Combine(
                Application.persistentDataPath,
                BackupFileName
            );

        private string TemporaryPath =>
            SavePath + ".tmp";

        public bool TryLoadProgression(
            out PlayerProgressSaveData saveData)
        {
            if (hasLoadedData)
            {
                saveData = Clone(cachedSaveData);
                return hasValidSave;
            }

            bool hasCurrent = TryReadSave(
                SavePath,
                out PlayerProgressSaveData current
            );

            bool hasBackup = TryReadSave(
                BackupPath,
                out PlayerProgressSaveData backup
            );

            if (!hasCurrent && !hasBackup)
            {
                cachedSaveData =
                    new PlayerProgressSaveData();
                hasLoadedData = true;
                saveData = Clone(cachedSaveData);
                return false;
            }

            cachedSaveData =
                hasCurrent &&
                (!hasBackup ||
                 current.savedAtUtcTicks >=
                 backup.savedAtUtcTicks)
                    ? current
                    : backup;
            hasLoadedData = true;
            hasValidSave = true;
            saveData = Clone(cachedSaveData);

            if (isDebug)
            {
                Debug.Log(
                    $"LocalSaveSystem: progression загружен, " +
                    $"версия {cachedSaveData.saveVersion}.",
                    this
                );
            }

            return true;
        }

        public bool SaveProgression(
            ICollection<string> unlockedZoneIds,
            ICollection<string> defeatedMainBossIds)
        {
            if (unlockedZoneIds == null ||
                defeatedMainBossIds == null)
            {
                Debug.LogError(
                    "LocalSaveSystem: списки progression не назначены.",
                    this
                );

                return false;
            }

            EnsureDataLoaded();

            PlayerProgressSaveData previousData =
                Clone(cachedSaveData);
            cachedSaveData.unlockedZoneIds =
                new List<string>(unlockedZoneIds);
            cachedSaveData.defeatedMainBossIds =
                new List<string>(defeatedMainBossIds);

            return WriteCachedData(previousData);
        }

        public bool SaveWallet(
            int gold,
            int piastres)
        {
            EnsureDataLoaded();

            PlayerProgressSaveData previousData =
                Clone(cachedSaveData);
            cachedSaveData.gold =
                Mathf.Max(0, gold);
            cachedSaveData.piastres =
                Mathf.Max(0, piastres);

            return WriteCachedData(previousData);
        }

        public bool SaveInventory(InventorySaveData inventory)
        {
            if (inventory == null || inventory.stacks == null ||
                inventory.uniqueItems == null || inventory.equipment == null || inventory.gems == null)
                return false;

            EnsureDataLoaded();
            PlayerProgressSaveData previousData = Clone(cachedSaveData);
            cachedSaveData.inventory = CloneInventory(inventory);
            return WriteCachedData(previousData);
        }

        public bool SaveWalletAndInventory(int gold, int piastres, InventorySaveData inventory)
        {
            if (inventory == null || inventory.stacks == null ||
                inventory.uniqueItems == null || inventory.equipment == null || inventory.gems == null)
                return false;

            EnsureDataLoaded();
            PlayerProgressSaveData previousData = Clone(cachedSaveData);
            cachedSaveData.gold = Mathf.Max(0, gold);
            cachedSaveData.piastres = Mathf.Max(0, piastres);
            cachedSaveData.inventory = CloneInventory(inventory);
            return WriteCachedData(previousData);
        }

        private void EnsureDataLoaded()
        {
            if (hasLoadedData)
                return;

            TryLoadProgression(out _);
        }

        private bool WriteCachedData(
            PlayerProgressSaveData previousData)
        {
            cachedSaveData.saveVersion =
                PlayerProgressSaveData.CurrentVersion;
            cachedSaveData.savedAtUtcTicks =
                DateTime.UtcNow.Ticks;

            try
            {
                Directory.CreateDirectory(
                    Application.persistentDataPath
                );

                if (TryReadSave(SavePath, out _))
                {
                    File.Copy(
                        SavePath,
                        BackupPath,
                        true
                    );
                }

                string json =
                    JsonUtility.ToJson(
                        cachedSaveData,
                        true
                    );

                File.WriteAllText(
                    TemporaryPath,
                    json
                );

                File.Copy(
                    TemporaryPath,
                    SavePath,
                    true
                );

                try
                {
                    File.Delete(TemporaryPath);
                }
                catch (IOException exception)
                {
                    if (isDebug)
                    {
                        Debug.LogWarning(
                            "LocalSaveSystem: временный файл " +
                            $"не удалён: {exception.Message}",
                            this
                        );
                    }
                }

                if (isDebug)
                {
                    Debug.Log(
                        "LocalSaveSystem: progression сохранён.",
                        this
                    );
                }

                hasValidSave = true;
                return true;
            }
            catch (Exception exception)
            {
                cachedSaveData = previousData;

                Debug.LogError(
                    $"LocalSaveSystem: ошибка сохранения: " +
                    $"{exception.Message}",
                    this
                );

                return false;
            }
        }

        private static PlayerProgressSaveData Clone(
            PlayerProgressSaveData source)
        {
            return new PlayerProgressSaveData
            {
                saveVersion = source.saveVersion,
                savedAtUtcTicks = source.savedAtUtcTicks,
                unlockedZoneIds =
                    new List<string>(source.unlockedZoneIds),
                defeatedMainBossIds =
                    new List<string>(source.defeatedMainBossIds),
                gold = source.gold,
                piastres = source.piastres,
                inventory = CloneInventory(source.inventory)
            };
        }

        private static InventorySaveData CloneInventory(InventorySaveData source)
        {
            InventorySaveData clone = new();
            if (source == null) return clone;
            foreach (InventoryStackSaveData stack in source.stacks ?? new List<InventoryStackSaveData>())
                if (stack != null) clone.stacks.Add(new InventoryStackSaveData { itemId = stack.itemId, quantity = stack.quantity });
            foreach (InventoryItemSaveData item in source.uniqueItems ?? new List<InventoryItemSaveData>())
                if (item != null) clone.uniqueItems.Add(new InventoryItemSaveData { instanceId = item.instanceId, itemId = item.itemId });
            foreach (EquipmentSaveData item in source.equipment ?? new List<EquipmentSaveData>())
                if (item != null) clone.equipment.Add(new EquipmentSaveData { instanceId = item.instanceId, itemId = item.itemId, level = item.level, efficiency = item.efficiency, isLocked = item.isLocked, socketedGems = CloneGemSlots(item.socketedGems) });
            foreach (GemSaveData item in source.gems ?? new List<GemSaveData>())
                if (item != null) clone.gems.Add(new GemSaveData { instanceId = item.instanceId, itemId = item.itemId, level = item.level });
            return clone;
        }

        private static GemSaveData[] CloneGemSlots(GemSaveData[] source)
        {
            GemSaveData[] clone = new GemSaveData[EquipmentInstance.SocketCount];
            if (source == null) return clone;
            for (int i = 0; i < clone.Length && i < source.Length; i++)
            {
                GemSaveData gem = source[i];
                if (gem != null)
                    clone[i] = new GemSaveData { instanceId = gem.instanceId, itemId = gem.itemId, level = gem.level };
            }
            return clone;
        }

        private bool TryReadSave(
            string path,
            out PlayerProgressSaveData saveData)
        {
            saveData = null;

            if (!File.Exists(path))
                return false;

            try
            {
                string json =
                    File.ReadAllText(path);

                PlayerProgressSaveData candidate =
                    JsonUtility.FromJson<PlayerProgressSaveData>(
                        json
                    );

                if (candidate == null || candidate.saveVersion < 1 ||
                    candidate.saveVersion > PlayerProgressSaveData.CurrentVersion ||
                    candidate.savedAtUtcTicks <= 0 ||
                    candidate.unlockedZoneIds == null ||
                    candidate.defeatedMainBossIds == null ||
                    candidate.gold < 0 ||
                    candidate.piastres < 0)
                {
                    return false;
                }

                // v1 did not contain inventory data. Preserve every
                // progression/wallet field while migrating to an empty bag.
                if (candidate.saveVersion == 1)
                {
                    candidate.inventory = new InventorySaveData();
                }
                if (candidate.inventory == null) return false;
                // v2 predates persisted Gem instances; preserve its other
                // inventory collections and add an empty Gem collection.
                candidate.inventory.gems ??= new List<GemSaveData>();
                // v3 predates Equipment socket data. Preserve each Equipment
                // instance and normalize its new fixed-size socket array.
                foreach (EquipmentSaveData item in candidate.inventory.equipment ?? new List<EquipmentSaveData>())
                    if (item != null) item.socketedGems = CloneGemSlots(item.socketedGems);
                candidate.saveVersion = PlayerProgressSaveData.CurrentVersion;
                if (candidate.inventory == null || candidate.inventory.stacks == null ||
                    candidate.inventory.uniqueItems == null || candidate.inventory.equipment == null ||
                    candidate.inventory.gems == null)
                    return false;

                saveData = candidate;
                return true;
            }
            catch (Exception exception)
            {
                if (isDebug)
                {
                    Debug.LogWarning(
                        $"LocalSaveSystem: файл " +
                        $"{Path.GetFileName(path)} не прочитан: " +
                        $"{exception.Message}",
                        this
                    );
                }

                return false;
            }
        }
    }
}
