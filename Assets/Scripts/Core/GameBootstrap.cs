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
    /// primitives and code, wires the systems together, and shows the Start
    /// screen. This keeps the project resilient to missing prefab/scene
    /// wiring, since there simply isn't any to miss -- open a brand new
    /// Unity project, drop this Assets folder in, hit Play on any scene.
    ///
    /// If a GameManager already exists (e.g. someone added one manually to a
    /// scene for testing), auto-bootstrap steps aside instead of doubling up.
    /// </summary>
    public static class GameBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoBootstrap()
        {
            if (Object.FindObjectOfType<GameManager>() != null) return;

            var root = new GameObject("DoofusDiaries");
            Object.DontDestroyOnLoad(root);

            GameConfig config = ConfigLoader.Load();

            EnsureCamera(root.transform);
            EnsureLight(root.transform);
            EnsureEventSystem(root.transform);

            var spawnerGO = new GameObject("PulpitSpawner");
            spawnerGO.transform.SetParent(root.transform);
            var spawner = spawnerGO.AddComponent<PulpitSpawner>();
            spawner.Initialize(config);

            var playerGO = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            playerGO.name = "Doofus";
            playerGO.transform.SetParent(root.transform);
            playerGO.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
            var player = playerGO.AddComponent<PlayerController>();

            var gmGO = new GameObject("GameManager");
            gmGO.transform.SetParent(root.transform);
            var gameManager = gmGO.AddComponent<GameManager>();

            var uiGO = new GameObject("UIManager");
            uiGO.transform.SetParent(root.transform);
            var uiManager = uiGO.AddComponent<UIManager>();
            uiManager.Bind(gameManager);

            gameManager.Configure(config, spawner, player);
        }

        private static void EnsureCamera(Transform parent)
        {
            if (Camera.main != null) return;

            var camGO = new GameObject("Main Camera");
            camGO.transform.SetParent(parent);
            camGO.tag = "MainCamera";

            var cam = camGO.AddComponent<Camera>();
            camGO.transform.position = new Vector3(0f, 8f, -8f);
            camGO.transform.LookAt(Vector3.zero);

            camGO.AddComponent<AudioListener>();
        }

        private static void EnsureLight(Transform parent)
        {
            if (Object.FindObjectOfType<Light>() != null) return;

            var lightGO = new GameObject("Directional Light");
            lightGO.transform.SetParent(parent);

            var light = lightGO.AddComponent<Light>();
            light.type = LightType.Directional;
            lightGO.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
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
