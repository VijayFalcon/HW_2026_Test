using System;
using UnityEngine;

namespace DoofusDiaries.Core
{
    /// <summary>
    /// Raw shape of doofus_diary.json. Field names intentionally mirror the JSON
    /// keys (snake_case) because Unity's JsonUtility maps fields by exact name
    /// match and does not support per-field renaming attributes the way
    /// Newtonsoft.Json does. Keeping this project dependency-free (no extra
    /// package) means JsonUtility is the right tool, so the naming trade-off
    /// is contained entirely to this one internal file.
    /// </summary>
    [Serializable]
    internal class RawGameConfig
    {
        public RawPlayerData player_data;
        public RawPulpitData pulpit_data;
    }

    [Serializable]
    internal class RawPlayerData
    {
        public float speed = DefaultGameConfig.PlayerSpeed;
    }

    [Serializable]
    internal class RawPulpitData
    {
        public float min_pulpit_destroy_time = DefaultGameConfig.MinPulpitDestroyTime;
        public float max_pulpit_destroy_time = DefaultGameConfig.MaxPulpitDestroyTime;
        public float pulpit_spawn_time = DefaultGameConfig.PulpitSpawnTime;
    }

    /// <summary>
    /// Hard-coded fallbacks used whenever doofus_diary.json is missing,
    /// unreadable, malformed, or missing individual fields. These currently
    /// mirror the values shipped in the provided JSON so gameplay is
    /// identical whether the file loads or not; they exist so the game never
    /// crashes or behaves undefined-ly just because an asset failed to load.
    /// </summary>
    internal static class DefaultGameConfig
    {
        public const float PlayerSpeed = 3f;
        public const float MinPulpitDestroyTime = 4f;
        public const float MaxPulpitDestroyTime = 5f;
        public const float PulpitSpawnTime = 2.5f;
    }

    /// <summary>
    /// Clean, validated config consumed by the rest of the game. Unlike
    /// RawGameConfig this is never partially-populated or null-checked by
    /// callers -- ConfigLoader guarantees every field is a sane, positive
    /// number before handing this out.
    /// </summary>
    [Serializable]
    public struct GameConfig
    {
        public float PlayerSpeed;
        public float MinPulpitDestroyTime;
        public float MaxPulpitDestroyTime;
        public float PulpitSpawnTime;

        public GameConfig(float playerSpeed, float minPulpitDestroyTime, float maxPulpitDestroyTime, float pulpitSpawnTime)
        {
            PlayerSpeed = playerSpeed;
            MinPulpitDestroyTime = minPulpitDestroyTime;
            MaxPulpitDestroyTime = maxPulpitDestroyTime;
            PulpitSpawnTime = pulpitSpawnTime;
        }

        /// <summary>A random lifetime for a newly spawned pulpit, in seconds.</summary>
        public float RandomDestroyTime() => UnityEngine.Random.Range(MinPulpitDestroyTime, MaxPulpitDestroyTime);
    }
}
