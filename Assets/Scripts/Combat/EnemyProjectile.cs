using UnityEngine;

namespace NeonArena
{
    /// <summary>
    /// Glowing plasma projectile fired by Sniper robots and Final Bosses.
    /// </summary>
    public class EnemyProjectile : MonoBehaviour
    {
        private Vector2 _direction;
        private float _speed;
        private float _damage;
        private float _lifeTimer = 5f;
        private Color _color;

        public static EnemyProjectile Spawn(Vector3 position, Vector2 direction, float speed, float damage, Color color, float scale = 0.52f)
        {
            var go = new GameObject("EnemyPlasma");
            go.transform.position = position;
            go.transform.localScale = Vector3.one * scale;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = NeonSpriteFactory.PlasmaOrb;
            sr.color = color;
            sr.sortingOrder = 9;

            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.32f;

            var proj = go.AddComponent<EnemyProjectile>();
            proj._direction = direction.normalized;
            proj._speed = speed;
            proj._damage = damage;
            proj._color = color;
            return proj;
        }

        private void Update()
        {
            transform.position += (Vector3)(_direction * (_speed * Time.deltaTime));
            transform.Rotate(0f, 0f, 240f * Time.deltaTime);

            _lifeTimer -= Time.deltaTime;
            if (_lifeTimer <= 0f)
            {
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent<PlayerController>(out var player))
            {
                if (!player.IsDashing)
                {
                    player.TakeDamage(_damage);
                    NeonBurstEffect.Spawn(transform.position, _color, 6, 4.5f, 0.16f);
                    Destroy(gameObject);
                }
            }
        }
    }
}
