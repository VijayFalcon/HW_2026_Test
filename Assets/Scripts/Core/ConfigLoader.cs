// Reads and validates doofus_diary.json from StreamingAssets, falling back
// to DefaultGameConfig (with a logged warning) for every failure mode:
// missing file, unreadable file, invalid JSON, missing fields, non-positive
// values, or min/max destroy times given in the wrong order.

using System;
using System.IO;
using UnityEngine;

namespace DoofusDiaries.Core
{
    public static class ConfigLoader
    {
        private const string ConfigFileName = "doofus_diary.json";

        public static GameConfig Load()
        {
            string json = ReadFileSafely();

            RawGameConfig raw = null;
            if (!string.IsNullOrWhiteSpace(json))
            {
                try
                {
                    raw = JsonUtility.FromJson<RawGameConfig>(json);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[ConfigLoader] '{ConfigFileName}' is not valid JSON ({e.Message}). Falling back to defaults.");
                }
            }

            RawPlayerData playerData = raw?.player_data ?? new RawPlayerData();
            RawPulpitData pulpitData = raw?.pulpit_data ?? new RawPulpitData();

            if (raw != null && raw.player_data == null)
                Debug.LogWarning($"[ConfigLoader] '{ConfigFileName}' is missing \"player_data\". Using default player speed.");
            if (raw != null && raw.pulpit_data == null)
                Debug.LogWarning($"[ConfigLoader] '{ConfigFileName}' is missing \"pulpit_data\". Using default pulpit timings.");

            return Validate(playerData, pulpitData);
        }

        private static string ReadFileSafely()
        {
            string path = Path.Combine(Application.streamingAssetsPath, ConfigFileName);
            try
            {
                if (!File.Exists(path))
                {
                    Debug.LogWarning($"[ConfigLoader] Config file not found at '{path}'. Falling back to defaults.");
                    return null;
                }
                return File.ReadAllText(path);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[ConfigLoader] Failed to read '{path}': {e.Message}. Falling back to defaults.");
                return null;
            }
        }

        private static GameConfig Validate(RawPlayerData playerData, RawPulpitData pulpitData)
        {
            float speed = playerData.speed > 0f ? playerData.speed : Warn(
                "player_data.speed", playerData.speed, DefaultGameConfig.PlayerSpeed);

            float min = pulpitData.min_pulpit_destroy_time > 0f ? pulpitData.min_pulpit_destroy_time : Warn(
                "pulpit_data.min_pulpit_destroy_time", pulpitData.min_pulpit_destroy_time, DefaultGameConfig.MinPulpitDestroyTime);

            float max = pulpitData.max_pulpit_destroy_time > 0f ? pulpitData.max_pulpit_destroy_time : Warn(
                "pulpit_data.max_pulpit_destroy_time", pulpitData.max_pulpit_destroy_time, DefaultGameConfig.MaxPulpitDestroyTime);

            float spawnTime = pulpitData.pulpit_spawn_time > 0f ? pulpitData.pulpit_spawn_time : Warn(
                "pulpit_data.pulpit_spawn_time", pulpitData.pulpit_spawn_time, DefaultGameConfig.PulpitSpawnTime);

            if (min > max)
            {
                Debug.LogWarning($"[ConfigLoader] min_pulpit_destroy_time ({min}) is greater than max_pulpit_destroy_time ({max}). Swapping them so pulpits still behave sensibly.");
                (min, max) = (max, min);
            }

            return new GameConfig(speed, min, max, spawnTime);
        }

        private static float Warn(string fieldName, float badValue, float fallback)
        {
            Debug.LogWarning($"[ConfigLoader] '{fieldName}' is missing or non-positive ({badValue}). Using default ({fallback}).");
            return fallback;
        }
    }
}
