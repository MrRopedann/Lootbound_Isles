using System;
using System.Collections.Generic;
using System.IO;
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
                piastres = source.piastres
            };
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

                if (candidate == null ||
                    candidate.saveVersion !=
                        PlayerProgressSaveData.CurrentVersion ||
                    candidate.savedAtUtcTicks <= 0 ||
                    candidate.unlockedZoneIds == null ||
                    candidate.defeatedMainBossIds == null ||
                    candidate.gold < 0 ||
                    candidate.piastres < 0)
                {
                    return false;
                }

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
