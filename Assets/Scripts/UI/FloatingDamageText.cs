using UnityEngine;

namespace NeonArena
{
    /// <summary>
    /// Lightweight world-space floating neon damage numbers.
    /// </summary>
    public class FloatingDamageText : MonoBehaviour
    {
        private TextMesh _tm;
        private float _life;
        private float _maxLife;
        private Vector3 _velocity;
        private Color _baseColor;

        public static void Spawn(Vector3 position, float damage, bool isCrit = false)
        {
            SpawnLabel(
                position,
                isCrit ? $"{Mathf.RoundToInt(damage)}!" : Mathf.RoundToInt(damage).ToString(),
                isCrit ? new Color(1f, 0.92f, 0.2f) : new Color(0.75f, 0.98f, 1f),
                isCrit ? 46 : 34,
                isCrit ? 0.65f : 0.48f
            );
        }

        public static void SpawnLabel(Vector3 position, string label, Color color, int fontSize = 38, float duration = 0.72f)
        {
            var go = new GameObject("DmgText");
            go.transform.position = position + new Vector3(Random.Range(-0.25f, 0.25f), 0.38f, 0f);

            var tm = go.AddComponent<TextMesh>();
            tm.text = label;
            tm.fontSize = fontSize;
            tm.characterSize = 0.085f;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            tm.fontStyle = FontStyle.Bold;
            tm.color = color;

            var mr = go.GetComponent<MeshRenderer>();
            mr.sortingOrder = 50;

            var fx = go.AddComponent<FloatingDamageText>();
            fx._tm = tm;
            fx._maxLife = duration;
            fx._life = duration;
            fx._velocity = new Vector3(Random.Range(-0.4f, 0.4f), 2.1f, 0f);
            fx._baseColor = color;
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

            transform.position += _velocity * dt;
            _velocity *= Mathf.Clamp01(1f - 3.5f * dt);

            float t = _life / _maxLife;
            var c = _baseColor;
            c.a = Mathf.Clamp01(t * 1.4f);
            _tm.color = c;
        }
    }
}
