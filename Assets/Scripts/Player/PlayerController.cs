using System;
using System.Collections;
using UnityEngine;
using DoofusDiaries.Core;
using DoofusDiaries.Pulpits;

namespace DoofusDiaries.Player
{
    /// <summary>
    /// Handles input and movement for "Doofus". The player hops between
    /// adjacent ring slots; travel time is derived from world distance and
    /// config.PlayerSpeed, so the JSON-configured speed directly drives how
    /// long a hop is exposed to risk (the target pulpit could collapse
    /// mid-flight -- handled below).
    /// </summary>
    public class PlayerController : MonoBehaviour
    {
        /// <summary>Raised after a successful landing: (fromSlot, toSlot).</summary>
        public event Action<int, int> OnMovedToPulpit;

        /// <summary>Raised once when the player falls (game over).</summary>
        public event Action OnFell;

        public int CurrentSlot { get; private set; }

        private PulpitSpawner _spawner;
        private GameConfig _config;
        private bool _isMoving;
        private bool _alive;
        private bool _inputEnabled = true;
        private Coroutine _moveRoutine;

        /// <summary>(Re)initializes the player onto a starting slot. Safe to call again on restart.</summary>
        public void Initialize(PulpitSpawner spawner, GameConfig config, int startSlot)
        {
            if (_spawner != null)
            {
                _spawner.OnPulpitCollapsed -= HandlePulpitCollapsed;
            }

            _spawner = spawner;
            _config = config;
            CurrentSlot = startSlot;
            _alive = true;
            _isMoving = false;

            if (_moveRoutine != null)
            {
                StopCoroutine(_moveRoutine);
                _moveRoutine = null;
            }

            transform.position = _spawner.SlotPositions[startSlot] + Vector3.up * 0.6f;
            _spawner.OnPulpitCollapsed += HandlePulpitCollapsed;
        }

        public void SetInputEnabled(bool enabled) => _inputEnabled = enabled;

        private void OnDestroy()
        {
            if (_spawner != null)
            {
                _spawner.OnPulpitCollapsed -= HandlePulpitCollapsed;
            }
        }

        private void Update()
        {
            if (!_alive || !_inputEnabled || _isMoving || _spawner == null) return;

            int direction = 0;
            if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) direction = 1;
            else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) direction = -1;

            if (direction != 0)
            {
                TryMove(direction);
            }
        }

        private void TryMove(int direction)
        {
            int targetSlot = direction > 0
                ? _spawner.GetSlotToRight(CurrentSlot)
                : _spawner.GetSlotToLeft(CurrentSlot);

            Pulpit targetPulpit = _spawner.GetPulpitAt(targetSlot);
            if (targetPulpit == null)
            {
                // Nothing to land on right now -- reject the move silently
                // rather than letting the player walk off into thin air.
                return;
            }

            _moveRoutine = StartCoroutine(MoveTo(targetSlot));
        }

        private IEnumerator MoveTo(int targetSlot)
        {
            _isMoving = true;
            int fromSlot = CurrentSlot;
            Vector3 start = transform.position;
            Vector3 end = _spawner.SlotPositions[targetSlot] + Vector3.up * 0.6f;
            float distance = Vector3.Distance(start, end);
            float duration = distance / Mathf.Max(0.01f, _config.PlayerSpeed);
            float elapsed = 0f;

            while (elapsed < duration)
            {
                if (!_alive) yield break; // died mid-flight, e.g. the origin pulpit isn't relevant anymore

                elapsed += Time.deltaTime;
                transform.position = Vector3.Lerp(start, end, Mathf.Clamp01(elapsed / duration));
                yield return null;
            }

            // The destination may have collapsed while we were travelling to it.
            if (_spawner.GetPulpitAt(targetSlot) == null)
            {
                Die();
                yield break;
            }

            transform.position = end;
            CurrentSlot = targetSlot;
            _isMoving = false;
            _moveRoutine = null;

            if (fromSlot != targetSlot)
            {
                OnMovedToPulpit?.Invoke(fromSlot, targetSlot);
            }
        }

        private void HandlePulpitCollapsed(Pulpit pulpit)
        {
            if (!_alive) return;

            // Only fatal if it's the pulpit we are currently standing still
            // on. Mid-air travel is handled separately in MoveTo, and a
            // collapsing pulpit we've already left behind is irrelevant.
            if (!_isMoving && pulpit.SlotIndex == CurrentSlot)
            {
                Die();
            }
        }

        private void Die()
        {
            if (!_alive) return;
            _alive = false;

            if (_moveRoutine != null)
            {
                StopCoroutine(_moveRoutine);
                _moveRoutine = null;
            }

            OnFell?.Invoke();
        }
    }
}
