using System;
using UnityEngine;
using DoofusDiaries.Core;
using DoofusDiaries.Pulpits;

namespace DoofusDiaries.Player
{
    /// <summary>
    /// Free-roaming movement for "Doofus", driven by an actual Rigidbody
    /// rather than a scripted hop-to-slot animation. WASD/arrow input sets
    /// horizontal velocity directly (config.PlayerSpeed units/sec); gravity
    /// handles everything vertical, so when a tile disappears from under the
    /// player, physics -- not game-logic bookkeeping -- makes them fall.
    ///
    /// Landing on a new tile is detected via physical collision with that
    /// tile's collider (both are solid, so this is an ordinary
    /// OnCollisionEnter, not a trigger). Falling into the pit is detected
    /// via a dedicated trigger volume (see PitVolume) far below the grid.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerController : MonoBehaviour
    {
        /// <summary>Raised the first time the player lands on a tile different from the last one it landed on.</summary>
        public event Action<Vector2Int> OnLandedOnNewTile;

        /// <summary>Raised once when the player falls into the pit (game over).</summary>
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
            // NOTE: Rigidbody.drag/.velocity were renamed to linearDamping/
            // linearVelocity starting with Unity 6; the old names used here
            // still compile (obsolete-but-functional) on that version and
            // are the only names available on older Editors, so they're the
            // safer choice for a project whose exact Unity version may vary.
            _rigidbody.drag = 0.5f;
        }

        /// <summary>(Re)initializes the player at a starting world position. Safe to call again on restart.</summary>
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

            // Raw axes: full speed the instant a key is pressed, zero the
            // instant it's released. The smoothed "Horizontal"/"Vertical"
            // axes ramp up over a few frames (Input Manager's default
            // sensitivity/gravity settings), which reads as input lag on
            // top of whatever config.PlayerSpeed already is.
            float x = Input.GetAxisRaw("Horizontal"); // A/D and Left/Right arrows by default
            float z = Input.GetAxisRaw("Vertical");   // W/S and Up/Down arrows by default

            Vector3 move = new Vector3(x, 0f, z);
            if (move.sqrMagnitude > 1f) move.Normalize(); // don't let diagonal movement exceed configured speed

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
