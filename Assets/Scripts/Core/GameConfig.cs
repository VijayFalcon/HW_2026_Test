// Defines the JSON shape (RawGameConfig/RawPlayerData/RawPulpitData) used to
// deserialize doofus_diary.json, the hardcoded fallback values
// (DefaultGameConfig), and the validated GameConfig struct that the rest of
// the game actually reads from.

using System;
using UnityEngine;

namespace DoofusDiaries.Core
{
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

    internal static class DefaultGameConfig
    {
        public const float PlayerSpeed = 3f;
        public const float MinPulpitDestroyTime = 4f;
        public const float MaxPulpitDestroyTime = 5f;
        public const float PulpitSpawnTime = 2.5f;
    }

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

        public float RandomDestroyTime() => UnityEngine.Random.Range(MinPulpitDestroyTime, MaxPulpitDestroyTime);
    }
}
