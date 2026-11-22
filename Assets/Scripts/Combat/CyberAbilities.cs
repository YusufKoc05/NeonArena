using System.Collections.Generic;
using UnityEngine;

namespace NeonArena
{
    public enum UpgradeId
    {
        LaserPower,
        FireRate,
        MultiLaser,
        PierceBeam,
        OrbitalBlade,
        ChainLightning,
        EmpShockwave,
        CritSystem,
        CyberDash,
        MagnetField,
        NanoRepair
    }

    public enum UpgradeRarity
    {
        Rare,       // Nadir (Cyan)
        Epic,       // Epik (Purple/Magenta)
        Legendary   // Efsanevi (Gold)
    }

    /// <summary>
    /// Jagged neon lightning bolt jumping between multiple robots.
    /// </summary>
    public class ChainLightningEffect : MonoBehaviour
    {
        private LineRenderer _lr;
        private float _life = 0.24f;
        private Color _color;

        public static void Cast(Vector3 startPos, int jumps, float damage)
        {
            var hitSet = new HashSet<EnemyAI>();
            var points = new List<Vector3> { startPos };
            Vector3 currentPos = startPos;

            for (int j = 0; j < jumps; j++)
            {
                EnemyAI best = null;
                float bestDistSq = 8.5f * 8.5f;

                var list = EnemyAI.ActiveEnemies;
                for (int i = 0; i < list.Count; i++)
                {
                    var e = list[i];
                    if (e == null || hitSet.Contains(e)) continue;
                    float dSq = (e.transform.position - currentPos).sqrMagnitude;
                    if (dSq < bestDistSq)
                    {
                        bestDistSq = dSq;
                        best = e;
                    }
                }

                if (best == null) break;

                hitSet.Add(best);
                Vector3 targetPos = best.transform.position;

                // Add jagged midpoints for authentic electric arc look
                Vector3 mid1 = Vector3.Lerp(currentPos, targetPos, 0.33f) + (Vector3)(Random.insideUnitCircle * 0.45f);
                Vector3 mid2 = Vector3.Lerp(currentPos, targetPos, 0.66f) + (Vector3)(Random.insideUnitCircle * 0.45f);
                points.Add(mid1);
                points.Add(mid2);
                points.Add(targetPos);

                best.TakeDamage(damage, (targetPos - currentPos).normalized * 2.5f, false);
                NeonBurstEffect.Spawn(targetPos, new Color(0.55f, 0.45f, 1f), 6, 5f, 0.16f);
                currentPos = targetPos;
            }

            if (points.Count <= 1) return;

            var go = new GameObject("ChainLightning");
            var lr = go.AddComponent<LineRenderer>();
            lr.positionCount = points.Count;
            lr.SetPositions(points.ToArray());
            lr.startWidth = 0.16f;
            lr.endWidth = 0.08f;
            lr.sortingOrder = 14;
            lr.useWorldSpace = true;

            var mat = new Material(Shader.Find("Sprites/Default"));
            lr.material = mat;
            Color col = new Color(0.55f, 0.78f, 1f, 1f);
            lr.startColor = Color.white;
            lr.endColor = col;

            var fx = go.AddComponent<ChainLightningEffect>();
            fx._lr = lr;
            fx._color = col;
            NeonAudio.Instance?.PlayLaser();
        }

        private void Update()
        {
            _life -= Time.deltaTime;
            if (_life <= 0f)
            {
                Destroy(gameObject);
                return;
            }

            float t = _life / 0.24f;
            _lr.startWidth = 0.16f * t;
            _lr.endWidth = 0.08f * t;
            var c = _color;
            c.a = t;
            _lr.startColor = new Color(1f, 1f, 1f, t);
            _lr.endColor = c;
        }
    }

    /// <summary>
    /// Expanding neon EMP ring that damages & knocks back robots and clears enemy projectiles.
    /// </summary>
    public class EmpShockwaveEffect : MonoBehaviour
    {
        private float _maxRadius;
        private float _currentRadius;
        private float _expandSpeed;
        private float _damage;
        private SpriteRenderer _sr;
        private readonly HashSet<EnemyAI> _hitEnemies = new HashSet<EnemyAI>();

        public static void Cast(Vector3 center, float maxRadius, float damage)
        {
            var go = new GameObject("EmpShockwave");
            go.transform.position = center;
            go.transform.localScale = Vector3.one * 0.4f;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = NeonSpriteFactory.Ring;
            sr.color = new Color(0.1f, 0.95f, 1f, 0.95f);
            sr.sortingOrder = 12;

            var emp = go.AddComponent<EmpShockwaveEffect>();
            emp._sr = sr;
            emp._maxRadius = maxRadius;
            emp._currentRadius = 0.4f;
            emp._expandSpeed = maxRadius * 3.2f;
            emp._damage = damage;

            NeonCameraFollow.Instance?.Shake(0.14f, 0.14f);
            NeonAudio.Instance?.PlayDash();
        }

        private void Update()
        {
            _currentRadius += _expandSpeed * Time.deltaTime;
            float progress = Mathf.Clamp01(_currentRadius / _maxRadius);

            transform.localScale = Vector3.one * (_currentRadius * 2f);
            var c = _sr.color;
            c.a = (1f - progress) * 0.92f;
            _sr.color = c;

            // Damage enemies inside expanding wave
            float radSq = _currentRadius * _currentRadius;
            var enemies = EnemyAI.ActiveEnemies;
            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                var e = enemies[i];
                if (e == null || _hitEnemies.Contains(e)) continue;
                Vector2 delta = e.transform.position - transform.position;
                if (delta.sqrMagnitude <= radSq)
                {
                    _hitEnemies.Add(e);
                    e.TakeDamage(_damage, delta.normalized * 8.5f, false);
                }
            }

            // Vaporize enemy plasma projectiles caught in EMP wave!
            var projectiles = Object.FindObjectsByType<EnemyProjectile>(FindObjectsSortMode.None);
            for (int i = 0; i < projectiles.Length; i++)
            {
                if (projectiles[i] == null) continue;
                if ((projectiles[i].transform.position - transform.position).sqrMagnitude <= radSq)
                {
                    NeonBurstEffect.Spawn(projectiles[i].transform.position, new Color(0.2f, 1f, 1f), 4, 3f, 0.12f);
                    Destroy(projectiles[i].gameObject);
                }
            }

            if (_currentRadius >= _maxRadius)
            {
                Destroy(gameObject);
            }
        }
    }
}
