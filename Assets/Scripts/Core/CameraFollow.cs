using UnityEngine;

namespace DoofusDiaries.Core
{
    /// <summary>
    /// Simple chase camera: holds a fixed world-space offset above and
    /// behind the target (Doofus) -- "more to the top and back", per the
    /// brief, so the player can see tiles appearing ahead of them -- and
    /// smooths toward that position each frame so movement doesn't feel
    /// jittery. This is a fixed-offset follow rather than one that orbits
    /// behind the player's facing direction, which keeps it simple and
    /// robust; a direction-aware orbit camera would be a reasonable
    /// follow-up enhancement.
    /// </summary>
    public class CameraFollow : MonoBehaviour
    {
        public Transform Target;
        public Vector3 Offset = new Vector3(0f, 7f, -9f);
        public float LookAheadHeight = 1.5f;
        public float SmoothTime = 0.15f;

        private Vector3 _velocity;

        private void LateUpdate()
        {
            if (Target == null) return;

            Vector3 desiredPosition = Target.position + Offset;
            transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref _velocity, SmoothTime);
            transform.LookAt(Target.position + Vector3.up * LookAheadHeight);
        }
    }
}
