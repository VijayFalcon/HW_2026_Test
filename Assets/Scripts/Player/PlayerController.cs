// Free Rigidbody-based movement for Doofus: WASD/arrow input sets
// horizontal velocity directly, gravity handles everything vertical,
// landing on a new tile is detected via collision, and falling into the
// pit is detected via a trigger volume (PitVolume).

using System;
using UnityEngine;
using DoofusDiaries.Core;
using DoofusDiaries.Pulpits;

namespace DoofusDiaries.Player
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerController : MonoBehaviour
    {
        public event Action<Vector2Int> OnLandedOnNewTile;
        public event Action OnFell;

        private Rigidbody _rigidbody;
        private GameConfig _config;
        private bool _alive;
        private bool _inputEnabled = true;
        private Vector2Int? _lastScoredTile;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _rigidbody.useGravity = true;
            _rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
            _rigidbody.drag = 0.5f;
        }

        public void Initialize(GameConfig config, Vector3 startPosition)
        {
            _config = config;
            _alive = true;
            _lastScoredTile = null;

            _rigidbody.velocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;
            transform.position = startPosition;
        }

        public void SetInputEnabled(bool enabled) => _inputEnabled = enabled;

        private void FixedUpdate()
        {
            if (!_alive || !_inputEnabled) return;

            float x = Input.GetAxisRaw("Horizontal");
            float z = Input.GetAxisRaw("Vertical");

            Vector3 move = new Vector3(x, 0f, z);
            if (move.sqrMagnitude > 1f) move.Normalize();

            Vector3 horizontal = move * _config.PlayerSpeed;
            _rigidbody.velocity = new Vector3(horizontal.x, _rigidbody.velocity.y, horizontal.z);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!_alive) return;

            Pulpit tile = collision.collider.GetComponent<Pulpit>();
            if (tile == null) return;

            if (_lastScoredTile == null || _lastScoredTile.Value != tile.GridPosition)
            {
                _lastScoredTile = tile.GridPosition;
                OnLandedOnNewTile?.Invoke(tile.GridPosition);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!_alive) return;

            if (other.GetComponent<PitVolume>() != null)
            {
                Die();
            }
        }

        private void Die()
        {
            if (!_alive) return;
            _alive = false;
            _rigidbody.velocity = Vector3.zero;
            OnFell?.Invoke();
        }
    }
}
