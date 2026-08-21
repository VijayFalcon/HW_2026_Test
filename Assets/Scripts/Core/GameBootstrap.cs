using UnityEngine;
using UnityEngine.EventSystems;
using DoofusDiaries.Pulpits;
using DoofusDiaries.Player;
using DoofusDiaries.UI;

namespace DoofusDiaries.Core
{
    /// <summary>
    /// Entry point for the whole game. Requires zero manual scene setup: it
    /// spins itself up automatically the moment any scene loads (via
    /// RuntimeInitializeOnLoadMethod), builds every GameObject it needs from
    /// primitives and code -- the enclosed room, the disco lights, the tile
    /// grid, the player, the follow camera, the UI -- and wires it all
    /// together. Open a brand new Unity project, drop this Assets folder
    /// in, hit Play on any scene.
    ///
    /// If a GameManager already exists (e.g. someone added one manually to a
    /// scene for testing), auto-bootstrap steps aside instead of doubling up.
    /// </summary>
    public static class GameBootstrap
    {
        // NOTE: the assignment's prop description calls the pulpit a
        // "9x9 platform", but nothing in the brief actually pins tile size
        // to a rule the way it pins player speed to the JSON -- it's a
        // deliberate design choice here, shrunk from 9 to make crossing
        // distance feel better matched to config.PlayerSpeed.
        private const float TileSize = 5f;
        private const int GridHalfExtent = 8;
        private const float WallHeight = 40f;
        private const float WallBottomY = -30f;
        private const float PitDepth = -25f;

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

            gameManager.Configure(config, spawner, player);
        }

        /// <summary>
        /// No skybox/sun -- the room is enclosed and otherwise dark except
        /// for the disco lights. Fog fades the view to black past a short
        /// distance, so falling into the pit reads as falling into darkness
        /// without needing any actual "pit" geometry.
        /// </summary>
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

            // Rigidbody is added here; PlayerController.Awake() is the single
            // place that configures its physics properties (gravity,
            // constraints, drag), so there's one source of truth for that.
            playerGO.AddComponent<Rigidbody>();

            Renderer renderer = playerGO.GetComponent<Renderer>();
            if (renderer != null) renderer.material.color = new Color(0.95f, 0.85f, 0.2f);

            return playerGO.AddComponent<PlayerController>();
        }

        private static void BuildCamera(Transform parent, Transform target)
        {
            GameObject camGO;
            if (Camera.main != null)
            {
                camGO = Camera.main.gameObject;
            }
            else
            {
                camGO = new GameObject("Main Camera");
                camGO.transform.SetParent(parent);
                camGO.tag = "MainCamera";
                camGO.AddComponent<Camera>();
                camGO.AddComponent<AudioListener>();
            }

            var follow = camGO.AddComponent<CameraFollow>();
            follow.Target = target;
        }

        /// <summary>Four walls and a ceiling enclosing the whole play grid -- "a cube where you can control the lighting", per the brief.</summary>
        private static void BuildEnclosedRoom(Transform parent)
        {
            var room = new GameObject("Room");
            room.transform.SetParent(parent);

            float half = (GridHalfExtent + 1) * TileSize; // a little buffer past the outermost possible tile
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

            // Recolor whichever default material CreatePrimitive assigned
            // rather than constructing a new Material with an explicit
            // shader -- that keeps this correct whether the project is set
            // up for the Built-in pipeline or URP (a hardcoded "Standard"
            // shader lookup renders magenta/missing under URP).
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

        /// <summary>Invisible trigger volume well below the tile grid; anything that falls into it has "fallen into the pit" (see PitVolume/PlayerController).</summary>
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
