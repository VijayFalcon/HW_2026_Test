using System;
using System.Collections.Generic;
using UnityEngine;
using DoofusDiaries.Core;

namespace DoofusDiaries.Pulpits
{
    /// <summary>
    /// Spawns square tiles ("pulpits") on a grid. At most two tiles exist at
    /// once, matching the brief ("Only two Pulpits can exist simultaneously").
    /// A tile spawns the next one -- adjacent to itself, never on top of the
    /// other active tile -- once its own age reaches config.PulpitSpawnTime.
    /// That produces the intended chain: TileA spawns; a couple of seconds
    /// later TileB appears next to it (now 2 active); TileA eventually
    /// collapses on its own random timer (back to 1 active); TileB spawns
    /// TileC; and so on.
    /// </summary>
    public class PulpitSpawner : MonoBehaviour
    {
        [Tooltip("World size of one square tile, and the grid spacing between tile centers.")]
        public float TileSize = 9f;

        [Tooltip("How many grid cells from the centre the play area extends in each direction, keeping the level inside the enclosed room.")]
        public int GridHalfExtent = 8;

        /// <summary>Optional prefab to spawn instead of a bare primitive cube.</summary>
        public GameObject PulpitPrefabTemplate;

        public event Action<Pulpit> OnPulpitCollapsed;
        public event Action<Pulpit> OnPulpitSpawned;

        private static readonly Vector2Int[] NeighborOffsets =
        {
            new Vector2Int(1, 0), new Vector2Int(-1, 0),
            new Vector2Int(0, 1), new Vector2Int(0, -1)
        };

        private GameConfig _config;
        private readonly List<Pulpit> _activeTiles = new List<Pulpit>();
        private bool _running;

        public void Initialize(GameConfig config)
        {
            _config = config;
        }

        /// <summary>Grid cell -> world position (tiles sit at y = 0; PulpitSpawner doesn't own tile thickness/visuals beyond that).</summary>
        public Vector3 GridToWorld(Vector2Int gridPos) => new Vector3(gridPos.x * TileSize, 0f, gridPos.y * TileSize);

        /// <summary>Destroys any leftover tiles so a restart begins from a clean grid.</summary>
        public void ResetGrid()
        {
            _running = false;
            foreach (Pulpit tile in _activeTiles)
            {
                if (tile == null) continue;
                tile.OnCollapsed -= HandleTileCollapsed;
                Destroy(tile.gameObject);
            }
            _activeTiles.Clear();
        }

        /// <summary>Spawns the very first tile at the grid origin and starts the chain. Returns that tile so the caller can position the player on it.</summary>
        public Pulpit BeginSpawning()
        {
            _running = true;
            return SpawnAt(Vector2Int.zero);
        }

        public void StopSpawning() => _running = false;

        private void Update()
        {
            if (!_running || _activeTiles.Count == 0 || _activeTiles.Count >= 2) return;

            // The most recently spawned (and still active) tile is the one
            // responsible for triggering the next spawn, once it reaches
            // config.PulpitSpawnTime seconds of its own age.
            Pulpit mostRecent = _activeTiles[_activeTiles.Count - 1];
            if (mostRecent == null || mostRecent.HasTriggeredNextSpawn) return;

            if (mostRecent.Age >= _config.PulpitSpawnTime)
            {
                mostRecent.HasTriggeredNextSpawn = true;
                SpawnAdjacentTo(mostRecent.GridPosition);
            }
        }

        private void SpawnAdjacentTo(Vector2Int origin)
        {
            List<Vector2Int> candidates = new List<Vector2Int>();
            foreach (Vector2Int offset in NeighborOffsets)
            {
                Vector2Int candidate = origin + offset;
                if (!InBounds(candidate)) continue;
                if (IsOccupied(candidate)) continue;
                candidates.Add(candidate);
            }

            if (candidates.Count == 0)
            {
                // Extremely unlikely with only ever 2 tiles active, but if
                // every neighbor is blocked or off the edge of the room,
                // relax the occupancy rule rather than silently skip the
                // player's next platform.
                Debug.LogWarning("[PulpitSpawner] No unoccupied neighbor found for the next tile; relaxing the occupancy rule.");
                foreach (Vector2Int offset in NeighborOffsets)
                {
                    Vector2Int candidate = origin + offset;
                    if (InBounds(candidate)) candidates.Add(candidate);
                }
            }

            if (candidates.Count == 0)
            {
                Debug.LogWarning("[PulpitSpawner] GridHalfExtent is too small to place another tile at all; skipping this spawn.");
                return;
            }

            Vector2Int chosen = candidates[UnityEngine.Random.Range(0, candidates.Count)];
            SpawnAt(chosen);
        }

        private bool InBounds(Vector2Int pos) =>
            Mathf.Abs(pos.x) <= GridHalfExtent && Mathf.Abs(pos.y) <= GridHalfExtent;

        private bool IsOccupied(Vector2Int pos)
        {
            foreach (Pulpit tile in _activeTiles)
            {
                if (tile != null && tile.GridPosition == pos) return true;
            }
            return false;
        }

        private Pulpit SpawnAt(Vector2Int gridPos)
        {
            GameObject go = PulpitPrefabTemplate != null
                ? Instantiate(PulpitPrefabTemplate)
                : GameObject.CreatePrimitive(PrimitiveType.Cube);

            go.name = $"Pulpit_{gridPos.x}_{gridPos.y}";
            go.transform.SetParent(transform, false);
            go.transform.position = GridToWorld(gridPos);
            // Slightly larger than the grid spacing so adjacent tiles'
            // colliders overlap a hair rather than leaving a seam a moving
            // Rigidbody could catch its collider on.
            go.transform.localScale = new Vector3(TileSize * 1.02f, 1f, TileSize * 1.02f);

            Pulpit pulpit = go.GetComponent<Pulpit>();
            if (pulpit == null) pulpit = go.AddComponent<Pulpit>();

            pulpit.Initialize(gridPos, _config.RandomDestroyTime());
            pulpit.OnCollapsed += HandleTileCollapsed;

            _activeTiles.Add(pulpit);
            OnPulpitSpawned?.Invoke(pulpit);
            return pulpit;
        }

        private void HandleTileCollapsed(Pulpit pulpit)
        {
            _activeTiles.Remove(pulpit);
            OnPulpitCollapsed?.Invoke(pulpit);
        }

        public Pulpit GetTileAt(Vector2Int gridPos)
        {
            foreach (Pulpit tile in _activeTiles)
            {
                if (tile != null && tile.GridPosition == gridPos) return tile;
            }
            return null;
        }
    }
}
