// Chase camera: holds a fixed world-space offset above and behind the
// target (Doofus) and smoothly follows it every frame.

using UnityEngine;

namespace DoofusDiaries.Core
{
    public class CameraFollow : MonoBehaviour
    {
        public Transform Target;
        public Vector3 Offset = new Vector3(0f, 4f, -5f);
        public float LookAheadHeight = 1.5f;
        public float SmoothTime = 0.08f;

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
