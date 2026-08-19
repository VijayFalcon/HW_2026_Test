using System;
using UnityEngine;

namespace DoofusDiaries.Pulpits
{
    /// <summary>
    /// A single platform ("pulpit") the player can stand on. Once spawned it
    /// counts down its own lifetime and self-destructs, independent of
    /// whether the player is currently standing on it -- that ticking clock
    /// is the core pressure of the game. It visually warns (color change)
    /// before collapsing so the player has a fair chance to react.
    /// </summary>
    public class Pulpit : MonoBehaviour
    {
        private static readonly Color NormalColor = new Color(0.55f, 0.35f, 0.18f);
        private static readonly Color WarningColor = new Color(0.85f, 0.2f, 0.15f);
        private const float WarningThresholdFraction = 0.75f;

        public int SlotIndex { get; private set; }

        /// <summary>Fired once, shortly before Collapse(), so listeners can play a warning cue.</summary>
        public event Action<Pulpit> OnAboutToCollapse;

        /// <summary>Fired exactly once when this pulpit's lifetime expires.</summary>
        public event Action<Pulpit> OnCollapsed;

        private float _lifetime;
        private float _timer;
        private bool _collapsing;
        private bool _warned;
        private Renderer _cachedRenderer;

        /// <summary>Must be called immediately after instantiation, before Update() runs.</summary>
        public void Initialize(int slotIndex, float lifetime)
        {
            SlotIndex = slotIndex;
            // Guard against a bad/zero lifetime slipping through so a pulpit
            // never lives forever or collapses on the same frame it spawns.
            _lifetime = Mathf.Max(0.25f, lifetime);
            _timer = 0f;
            _collapsing = false;
            _warned = false;

            _cachedRenderer = GetComponentInChildren<Renderer>();
            if (_cachedRenderer != null)
            {
                _cachedRenderer.material.color = NormalColor;
            }
        }

        private void Update()
        {
            if (_collapsing) return;

            _timer += Time.deltaTime;

            if (!_warned && _timer >= _lifetime * WarningThresholdFraction)
            {
                _warned = true;
                OnAboutToCollapse?.Invoke(this);
                if (_cachedRenderer != null)
                {
                    _cachedRenderer.material.color = WarningColor;
                }
            }

            if (_timer >= _lifetime)
            {
                Collapse();
            }
        }

        private void Collapse()
        {
            if (_collapsing) return;
            _collapsing = true;
            OnCollapsed?.Invoke(this);
            // Small delay leaves room for a future collapse animation/VFX;
            // logically the pulpit is already gone (listeners were notified above).
            Destroy(gameObject, 0.15f);
        }
    }
}
