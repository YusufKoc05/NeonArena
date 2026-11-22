using UnityEngine;

namespace NeonArena
{
    /// <summary>
    /// Smoothly follows the player inside the neon arena with micro screen-shake support.
    /// </summary>
    public class NeonCameraFollow : MonoBehaviour
    {
        public static NeonCameraFollow Instance { get; private set; }

        public Transform Target;
        public float SmoothSpeed = 8f;
        public Vector2 ArenaHalfBounds = new Vector2(20f, 14f);

        private float _shakeTimer;
        private float _shakeIntensity;

        private void Awake()
        {
            Instance = this;
        }

        public void Shake(float intensity = 0.14f, float duration = 0.12f)
        {
            _shakeIntensity = Mathf.Max(_shakeIntensity, intensity);
            _shakeTimer = Mathf.Max(_shakeTimer, duration);
        }

        private void LateUpdate()
        {
            if (Target == null) return;

            Vector3 desired = new Vector3(
                Mathf.Clamp(Target.position.x, -ArenaHalfBounds.x, ArenaHalfBounds.x),
                Mathf.Clamp(Target.position.y, -ArenaHalfBounds.y, ArenaHalfBounds.y),
                -10f
            );

            Vector3 pos = Vector3.Lerp(transform.position, desired, SmoothSpeed * Time.deltaTime);

            if (_shakeTimer > 0f)
            {
                _shakeTimer -= Time.deltaTime;
                pos += (Vector3)(Random.insideUnitCircle * _shakeIntensity);
                if (_shakeTimer <= 0f) _shakeIntensity = 0f;
            }

            transform.position = pos;
        }
    }
}
