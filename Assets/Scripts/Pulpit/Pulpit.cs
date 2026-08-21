// A single collapsible square tile ("pulpit"). Counts down its own
// lifetime regardless of whether the player is on it, then disables its
// collider and renderer so anything standing on it starts falling
// immediately (real physics, not scripted bookkeeping).

using System;
using UnityEngine;
using DoofusDiaries.Core;

namespace DoofusDiaries.Pulpits
{
    [RequireComponent(typeof(Collider))]
    public class Pulpit : MonoBehaviour
    {
        private static readonly Color NormalColor = new Color(0.1f, 0.85f, 0.35f);
        private static readonly Color WarningColor = new Color(0.9f, 0.15f, 0.15f);
        private const float WarningThresholdFraction = 0.7f;

        public Vector2Int GridPosition { get; private set; }
        public bool HasTriggeredNextSpawn { get; set; }
        public float Age { get; private set; }

        public event Action<Pulpit> OnCollapsed;

        private float _lifetime;
        private bool _collapsing;
        private bool _warned;
        private Renderer _cachedRenderer;
        private Collider _cachedCollider;

        public void Initialize(Vector2Int gridPosition, float lifetime)
        {
            GridPosition = gridPosition;
            _lifetime = Mathf.Max(0.25f, lifetime);
            Age = 0f;
            _collapsing = false;
            _warned = false;
            HasTriggeredNextSpawn = false;

            _cachedRenderer = GetComponentInChildren<Renderer>();
            _cachedCollider = GetComponent<Collider>();
            if (_cachedCollider != null)
            {
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

            if (_cachedCollider != null) _cachedCollider.enabled = false;
            if (_cachedRenderer != null) _cachedRenderer.enabled = false;

            OnCollapsed?.Invoke(this);
            Destroy(gameObject, 0.5f);
        }
    }
}
