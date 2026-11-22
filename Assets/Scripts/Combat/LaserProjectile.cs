using UnityEngine;

namespace NeonArena
{
    /// <summary>
    /// Fast glowing neon laser bolt fired automatically at the nearest enemy.
    /// Supports piercing and critical strikes.
    /// </summary>
    public class LaserProjectile : MonoBehaviour
    {
        private Vector2 _direction;
        private float _speed;
        private float _damage;
        private int _pierceRemaining;
        private bool _isCrit;
        private float _lifeTimer = 2.2f;

        public void Initialize(Vector2 direction, float speed, float damage, int pierce, bool isCrit = false)
        {
            _direction = direction.normalized;
            _speed = speed;
            _damage = damage;
            _pierceRemaining = pierce;
            _isCrit = isCrit;

            float angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        private void Update()
        {
            transform.position += (Vector3)(_direction * (_speed * Time.deltaTime));
            _lifeTimer -= Time.deltaTime;
            if (_lifeTimer <= 0f)
            {
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent<EnemyAI>(out var enemy))
            {
                enemy.TakeDamage(_damage, _direction * 3.5f, _isCrit);
                Color sparkColor = _isCrit ? new Color(1f, 0.92f, 0.2f) : new Color(0.2f, 0.95f, 1f);
                NeonBurstEffect.Spawn(transform.position, sparkColor, _isCrit ? 7 : 4, 4.5f, 0.15f);

                if (_pierceRemaining > 0)
                {
                    _pierceRemaining--;
                }
                else
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}
