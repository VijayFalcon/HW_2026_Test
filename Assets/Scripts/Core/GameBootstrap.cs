// Builds the entire game world at runtime the moment a scene loads (no
// manual scene setup needed): config, the enclosed room, disco lights, the
// pit trigger, the tile grid, the player, the follow camera, the UI, and
// the game manager -- all created from code and wired together.

using UnityEngine;
using UnityEngine.EventSystems;
using DoofusDiaries.Pulpits;
using DoofusDiaries.Player;
using DoofusDiaries.UI;

namespace DoofusDiaries.Core
{
    public static class GameBootstrap
    {
        private const float TileSize = 5f;
        private const int GridHalfExtent = 8;
        private const float WallHeight = 40f;
        private const float WallBottomY = -30f;
        private const float PitDepth = -25f;
        private const float CameraFieldOfView = 75f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoBootstrap()
        {
            if (Object.FindObjectOfType<GameManager>() != null) return;

            var root = new GameObject("DoofusDiaries");
            Object.DontDestroyOnLoad(root);

            GameConfig config = ConfigLoader.Load();

            ConfigureAmbience();

            var spawnerGO = new GameObject("PulpitSpawner");
            spawnerGO.transform.SetParent(root.transform);
            var spawner = spawnerGO.AddComponent<PulpitSpawner>();
            spawner.TileSize = TileSize;
            spawner.GridHalfExtent = GridHalfExtent;
            spawner.Initialize(config);

            PlayerController player = BuildPlayer(root.transform);
            BuildCamera(root.transform, player.transform);
            BuildEnclosedRoom(root.transform);
            BuildDiscoLights(root.transform);
            BuildPitVolume(root.transform);
            EnsureEventSystem(root.transform);

            var gmGO = new GameObject("GameManager");
            gmGO.transform.SetParent(root.transform);
            var gameManager = gmGO.AddComponent<GameManager>();

            var uiGO = new GameObject("UIManager");
            uiGO.transform.SetParent(root.transform);
            var uiManager = uiGO.AddComponent<UIManager>();
            uiManager.Bind(gameManager);

            BuildMusic(root.transform, gameManager);

            gameManager.Configure(config, spawner, player);
        }

        private static void BuildMusic(Transform parent, GameManager gameManager)
        {
            var musicGO = new GameObject("MusicController");
            musicGO.transform.SetParent(parent);
            musicGO.AddComponent<AudioSource>();
            var music = musicGO.AddComponent<MusicController>();
            music.Bind(gameManager);
        }

        private static void ConfigureAmbience()
        {
            RenderSettings.skybox = null;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.03f, 0.03f, 0.05f);
            RenderSettings.fog = true;
            RenderSettings.fogColor = Color.black;
            RenderSettings.fogMode = FogMode.Exponential;
            RenderSettings.fogDensity = 0.02f;
        }

        private static PlayerController BuildPlayer(Transform parent)
        {
            var playerGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
            playerGO.name = "Doofus";
            playerGO.transform.SetParent(parent);
            playerGO.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);

            Object.Destroy(playerGO.GetComponent<BoxCollider>());
            var capsule = playerGO.AddComponent<CapsuleCollider>();
            capsule.direction = 1;
            capsule.radius = 0.5f;
            capsule.height = 1.1f;
            capsule.material = PhysicsMaterials.Frictionless;

            playerGO.AddComponent<Rigidbody>();

            Renderer renderer = playerGO.GetComponent<Renderer>();
            if (renderer != null) renderer.material.color = new Color(0.95f, 0.85f, 0.2f);

            return playerGO.AddComponent<PlayerController>();
        }

        private static void BuildCamera(Transform parent, Transform target)
        {
            GameObject camGO;
            Camera cam;
            if (Camera.main != null)
            {
                camGO = Camera.main.gameObject;
                cam = Camera.main;
            }
            else
            {
                camGO = new GameObject("Main Camera");
                camGO.transform.SetParent(parent);
                camGO.tag = "MainCamera";
                cam = camGO.AddComponent<Camera>();
                camGO.AddComponent<AudioListener>();
            }

            cam.fieldOfView = CameraFieldOfView;

            var follow = camGO.AddComponent<CameraFollow>();
            follow.Target = target;
        }

        private static void BuildEnclosedRoom(Transform parent)
        {
            var room = new GameObject("Room");
            room.transform.SetParent(parent);

            float half = (GridHalfExtent + 1) * TileSize;
            float wallSpan = WallHeight - WallBottomY;
            float wallCenterY = (WallHeight + WallBottomY) / 2f;
            Color wallColor = new Color(0.05f, 0.05f, 0.07f);

            CreateWall(room.transform, "Wall_PosX", new Vector3(half, wallCenterY, 0f), new Vector3(2f, wallSpan, half * 2f), wallColor);
            CreateWall(room.transform, "Wall_NegX", new Vector3(-half, wallCenterY, 0f), new Vector3(2f, wallSpan, half * 2f), wallColor);
            CreateWall(room.transform, "Wall_PosZ", new Vector3(0f, wallCenterY, half), new Vector3(half * 2f, wallSpan, 2f), wallColor);
            CreateWall(room.transform, "Wall_NegZ", new Vector3(0f, wallCenterY, -half), new Vector3(half * 2f, wallSpan, 2f), wallColor);
            CreateWall(room.transform, "Ceiling", new Vector3(0f, WallHeight, 0f), new Vector3(half * 2f, 2f, half * 2f), wallColor);
        }

        private static void CreateWall(Transform parent, string name, Vector3 position, Vector3 scale, Color color)
        {
            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = name;
            wall.transform.SetParent(parent, false);
            wall.transform.position = position;
            wall.transform.localScale = scale;

            Renderer renderer = wall.GetComponent<Renderer>();
            if (renderer != null) renderer.material.color = color;
        }

        private static void BuildDiscoLights(Transform parent)
        {
            var lightsRoot = new GameObject("DiscoLights");
            lightsRoot.transform.SetParent(parent);
            var controller = lightsRoot.AddComponent<DiscoLightController>();
            controller.BuildLights(lightsRoot.transform, WallHeight - 4f);
        }

        private static void BuildPitVolume(Transform parent)
        {
            var pit = new GameObject("PitVolume");
            pit.transform.SetParent(parent);
            pit.transform.position = new Vector3(0f, PitDepth, 0f);

            float half = (GridHalfExtent + 2) * TileSize;
            var collider = pit.AddComponent<BoxCollider>();
            collider.isTrigger = true;
            collider.size = new Vector3(half * 2f, 2f, half * 2f);

            pit.AddComponent<PitVolume>();
        }

        private static void EnsureEventSystem(Transform parent)
        {
            if (Object.FindObjectOfType<EventSystem>() != null) return;

            var esGO = new GameObject("EventSystem");
            esGO.transform.SetParent(parent);
            esGO.AddComponent<EventSystem>();
            esGO.AddComponent<StandaloneInputModule>();
        }
    }
}
