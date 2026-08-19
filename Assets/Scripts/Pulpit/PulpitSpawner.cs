using System;
using System.Collections.Generic;
using UnityEngine;
using DoofusDiaries.Core;

namespace DoofusDiaries.Pulpits
{
    /// <summary>
    /// Owns a fixed ring of "slots" arranged in a circle around the origin.
    /// At a fixed interval (config.PulpitSpawnTime) it spawns a new pulpit
    /// into a random currently-empty slot. Each pulpit is handed a random
    /// lifetime within [MinPulpitDestroyTime, MaxPulpitDestroyTime] and
    /// manages its own countdown/destruction (see Pulpit.cs).
    ///
    /// This is the "platform placement" system for the level: placement is
    /// procedural rather than a fixed layout, driven entirely by the timing
    /// values in doofus_diary.json (see README for why -- the provided JSON
    /// has no explicit position/layout array).
    /// </summary>
    public class PulpitSpawner : MonoBehaviour
    {
        [Tooltip("Number of fixed positions arranged around the ring.")]
        public int SlotCount = 8;

        [Tooltip("Radius of the ring, in world units.")]
        public float RingRadius = 4f;

        /// <summary>Optional prefab to spawn instead of a bare primitive cylinder.</summary>
        public GameObject PulpitPrefabTemplate;

        public event Action<Pulpit> OnPulpitCollapsed;

        public IReadOnlyList<Vector3> SlotPositions => _slotPositions;

        private GameConfig _config;
        private Vector3[] _slotPositions;
        private Pulpit[] _slotOccupants;
        private float _spawnTimer;
        private bool _running;

        public void Initialize(GameConfig config)
        {
            _config = config;

            SlotCount = Mathf.Max(2, SlotCount); // a ring needs at least 2 slots to mean anything
            _slotPositions = new Vector3[SlotCount];
            _slotOccupants = new Pulpit[SlotCount];

            for (int i = 0; i < SlotCount; i++)
            {
                float angle = i * Mathf.PI * 2f / SlotCount;
                _slotPositions[i] = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * RingRadius;
            }
        }

        /// <summary>Clears any leftover pulpits from a previous run so a restart starts from a clean ring.</summary>
        public void ResetRing()
        {
            _running = false;
            _spawnTimer = 0f;

            if (_slotOccupants == null) return;

            for (int i = 0; i < _slotOccupants.Length; i++)
            {
                Pulpit occupant = _slotOccupants[i];
                if (occupant == null) continue;

                occupant.OnCollapsed -= HandlePulpitCollapsed;
                Destroy(occupant.gameObject);
                _slotOccupants[i] = null;
            }
        }

        public void BeginSpawning(int guaranteedStartSlot)
        {
            _running = true;
            _spawnTimer = 0f;
            SpawnAt(Mathf.Clamp(guaranteedStartSlot, 0, SlotCount - 1));
        }

        public void StopSpawning() => _running = false;

        private void Update()
        {
            if (!_running) return;

            _spawnTimer += Time.deltaTime;
            if (_spawnTimer >= _config.PulpitSpawnTime)
            {
                _spawnTimer = 0f;
                TrySpawnInRandomEmptySlot();
            }
        }

        private void TrySpawnInRandomEmptySlot()
        {
            List<int> emptySlots = new List<int>();
            for (int i = 0; i < SlotCount; i++)
            {
                if (_slotOccupants[i] == null) emptySlots.Add(i);
            }

            if (emptySlots.Count == 0) return; // ring is full this tick; nothing to do

            int slot = emptySlots[UnityEngine.Random.Range(0, emptySlots.Count)];
            SpawnAt(slot);
        }

        private Pulpit SpawnAt(int slot)
        {
            GameObject go = PulpitPrefabTemplate != null
                ? Instantiate(PulpitPrefabTemplate)
                : GameObject.CreatePrimitive(PrimitiveType.Cylinder);

            go.name = $"Pulpit_{slot}";
            go.transform.SetParent(transform, false);
            go.transform.localPosition = _slotPositions[slot];
            go.transform.localScale = new Vector3(1.2f, 0.2f, 1.2f);

            Pulpit pulpit = go.GetComponent<Pulpit>();
            if (pulpit == null) pulpit = go.AddComponent<Pulpit>();

            pulpit.Initialize(slot, _config.RandomDestroyTime());
            pulpit.OnCollapsed += HandlePulpitCollapsed;

            _slotOccupants[slot] = pulpit;
            return pulpit;
        }

        private void HandlePulpitCollapsed(Pulpit pulpit)
        {
            if (_slotOccupants[pulpit.SlotIndex] == pulpit)
            {
                _slotOccupants[pulpit.SlotIndex] = null;
            }
            OnPulpitCollapsed?.Invoke(pulpit);
        }

        public Pulpit GetPulpitAt(int slot)
        {
            if (_slotOccupants == null || slot < 0 || slot >= _slotOccupants.Length) return null;
            return _slotOccupants[slot];
        }

        public int GetSlotToLeft(int slot) => (slot - 1 + SlotCount) % SlotCount;
        public int GetSlotToRight(int slot) => (slot + 1) % SlotCount;
    }
}
