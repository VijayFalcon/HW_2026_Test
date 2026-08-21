// Spawns square tiles onto a grid, keeping at most two active at once. The
// most recently spawned tile triggers the next one -- adjacent to itself,
// never on the other active tile -- once its own age reaches
// config.PulpitSpawnTime.

using System;
using System.Collections.Generic;
using UnityEngine;
using DoofusDiaries.Core;

namespace DoofusDiaries.Pulpits
{
    public class PulpitSpawner : MonoBehaviour
    {
        [Tooltip("World size of one square tile, and the grid spacing between tile centers.")]
        public float TileSize = 5f;

        [Tooltip("How many grid cells from the centre the play area extends in each direction, keeping the level inside the enclosed room.")]
        public int GridHalfExtent = 8;

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

        public Vector3 GridToWorld(Vector2Int gridPos) => new Vector3(gridPos.x * TileSize, 0f, gridPos.y * TileSize);

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

        public Pulpit BeginSpawning()
        {
            _running = true;
            return SpawnAt(Vector2Int.zero);
        }

        public void StopSpawning() => _running = false;

        private void Update()
        {
            if (!_running || _activeTiles.Count == 0 || _activeTiles.Count >= 2) return;

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
