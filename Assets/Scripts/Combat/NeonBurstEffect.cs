using UnityEngine;

namespace NeonArena
{
    /// <summary>
    /// Spawns lightweight procedural neon spark bursts on enemy death or laser hits.
    /// </summary>
    public class NeonBurstEffect : MonoBehaviour
    {
        private Vector2 _velocity;
        private float _life;
        private float _maxLife;
        private SpriteRenderer _sr;
        private Color _startColor;

        public static void Spawn(Vector3 position, Color color, int count = 8, float speed = 5f, float scale = 0.22f)
        {
            for (int i = 0; i < count; i++)
            {
                var go = new GameObject("NeonSpark");
                go.transform.position = position;
                go.transform.localScale = Vector3.one * Random.Range(scale * 0.6f, scale * 1.25f);

                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = NeonSpriteFactory.Circle;
                sr.color = color;
                sr.sortingOrder = 15;

                var spark = go.AddComponent<NeonBurstEffect>();
                float angle = Random.Range(0f, Mathf.PI * 2f);
                float spd = Random.Range(speed * 0.45f, speed * 1.15f);
                spark._velocity = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * spd;
                spark._maxLife = Random.Range(0.18f, 0.38f);
                spark._life = spark._maxLife;
                spark._sr = sr;
                spark._startColor = color;
            }
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            _life -= dt;
            if (_life <= 0f)
            {
                Destroy(gameObject);
                return;
            }

            transform.position += (Vector3)(_velocity * dt);
            _velocity *= Mathf.Clamp01(1f - 5f * dt);

            float t = _life / _maxLife;
            var c = _startColor;
            c.a = t;
            _sr.color = c;
            transform.localScale *= (1f - 1.5f * dt);
        }
    }
}
