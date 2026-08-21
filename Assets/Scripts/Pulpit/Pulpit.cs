using System;
using UnityEngine;
using DoofusDiaries.Core;

namespace DoofusDiaries.Pulpits
{
    /// <summary>
    /// A single square tile ("pulpit") the player can stand on, sitting at a
    /// fixed grid cell. It counts down its own lifetime and collapses
    /// independent of whether the player is on it -- that ticking clock is
    /// the core pressure of the game.
    ///
    /// Collapsing disables the collider and renderer on the same frame the
    /// lifetime expires, rather than waiting on a destroy animation. That
    /// means a Rigidbody player standing on this tile loses support and
    /// starts falling on the very next physics step, purely from gravity --
    /// no manual "was I still supported" bookkeeping required anywhere else.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class Pulpit : MonoBehaviour
    {
        private static readonly Color NormalColor = new Color(0.1f, 0.85f, 0.35f); // green, per the brief
        private static readonly Color WarningColor = new Color(0.9f, 0.15f, 0.15f);
        private const float WarningThresholdFraction = 0.7f;

        /// <summary>This tile's position on the spawn grid (see PulpitSpawner.GridToWorld).</summary>
        public Vector2Int GridPosition { get; private set; }

        /// <summary>
        /// Set by PulpitSpawner once this tile has already spawned the next
        /// one in the chain, so it doesn't try to trigger a second spawn.
        /// </summary>
        public bool HasTriggeredNextSpawn { get; set; }

        /// <summary>How long, in seconds, this tile has been alive.</summary>
        public float Age { get; private set; }

        /// <summary>Fired exactly once when this tile's lifetime expires.</summary>
        public event Action<Pulpit> OnCollapsed;

        private float _lifetime;
        private bool _collapsing;
        private bool _warned;
        private Renderer _cachedRenderer;
        private Collider _cachedCollider;

        /// <summary>Must be called immediately after instantiation, before Update() runs.</summary>
        public void Initialize(Vector2Int gridPosition, float lifetime)
        {
            GridPosition = gridPosition;
            // Guard against a bad/zero lifetime slipping through so a tile
            // never lives forever or collapses on the same frame it spawns.
            _lifetime = Mathf.Max(0.25f, lifetime);
            Age = 0f;
            _collapsing = false;
            _warned = false;
            HasTriggeredNextSpawn = false;

            _cachedRenderer = GetComponentInChildren<Renderer>();
            _cachedCollider = GetComponent<Collider>();
            if (_cachedCollider != null)
            {
                // Same frictionless material as the player -- keeps Doofus
                // from snagging on the seam between this tile and its neighbor.
                _cachedCollider.material = PhysicsMaterials.Frictionless;
            }
            if (_cachedRenderer != null)
            {
                _cachedRenderer.material.color = NormalColor;
            }
        }

        private void Update()
        {
            if (_collapsing) return;

            Age += Time.deltaTime;

            if (!_warned && Age >= _lifetime * WarningThresholdFraction)
            {
                _warned = true;
                if (_cachedRenderer != null)
                {
                    _cachedRenderer.material.color = WarningColor;
                }
            }

            if (Age >= _lifetime)
            {
                Collapse();
            }
        }

        private void Collapse()
        {
            if (_collapsing) return;
            _collapsing = true;

            // Remove support and visibility on the same frame -- anything
            // standing here (physics) starts falling immediately, and the
            // tile visually vanishes rather than lingering.
            if (_cachedCollider != null) _cachedCollider.enabled = false;
            if (_cachedRenderer != null) _cachedRenderer.enabled = false;

            OnCollapsed?.Invoke(this);
            Destroy(gameObject, 0.5f);
        }
    }
}
