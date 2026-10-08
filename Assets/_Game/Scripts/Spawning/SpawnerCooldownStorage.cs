using System;
using UnityEngine;

namespace LootboundIsles.Spawning
{
    public static class SpawnerCooldownStorage
    {
        private const string KeyPrefix =
            "SpawnerCooldown_";

        public static void Save(
            string spawnerId,
            DateTime nextCycleTimeUtc)
        {
            string key =
                GetKey(spawnerId);

            long timestamp =
                nextCycleTimeUtc.ToBinary();

            PlayerPrefs.SetString(
                key,
                timestamp.ToString()
            );

            PlayerPrefs.Save();
        }

        public static bool TryLoad(
            string spawnerId,
            out DateTime nextCycleTimeUtc)
        {
            string key =
                GetKey(spawnerId);

            if (!PlayerPrefs.HasKey(key))
            {
                nextCycleTimeUtc =
                    DateTime.MinValue;

                return false;
            }

            string savedValue =
                PlayerPrefs.GetString(key);

            if (!long.TryParse(
                    savedValue,
                    out long timestamp))
            {
                Debug.LogError(
                    $"SpawnerCooldownStorage: " +
                    $"некорректные данные для " +
                    $"Spawner ID '{spawnerId}'."
                );

                nextCycleTimeUtc =
                    DateTime.MinValue;

                return false;
            }

            try
            {
                nextCycleTimeUtc =
                    DateTime.FromBinary(timestamp);

                return true;
            }
            catch
            {
                Debug.LogError(
                    $"SpawnerCooldownStorage: " +
                    $"не удалось прочитать время для " +
                    $"Spawner ID '{spawnerId}'."
                );

                nextCycleTimeUtc =
                    DateTime.MinValue;

                return false;
            }
        }

        public static void Clear(
            string spawnerId)
        {
            string key =
                GetKey(spawnerId);

            PlayerPrefs.DeleteKey(key);
            PlayerPrefs.Save();
        }

        private static string GetKey(
            string spawnerId)
        {
            return KeyPrefix + spawnerId;
        }
    }
}