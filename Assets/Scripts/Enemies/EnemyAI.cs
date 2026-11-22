using System.Collections.Generic;
using UnityEngine;

namespace NeonArena
{
    public enum RobotType
    {
        Drone,
        Swarmer,
        Tank,
        Sniper,
        Splitter,
        Boss
    }

    /// <summary>
    /// Base cyberpunk enemy robot AI supporting 5 distinct robot archetypes + Boss subclassing.
    /// </summary>
    public class EnemyAI : MonoBehaviour
    {
        public static readonly List<EnemyAI> ActiveEnemies = new List<EnemyAI>();

        public RobotType Type;
        public int EnemyLevel = 1;
        public float MaxHealth = 25f;
        public float CurrentHealth { get; protected set; }
        public float MoveSpeed = 2.8f;
        public float ContactDamage = 12f;
        public int XpDrop = 10;
        public int ScoreValue = 25;
        public bool IsBoss => Type == RobotType.Boss;

        protected Rigidbody2D Rb;
        protected SpriteRenderer Sr;
        protected Color BaseColor;
        protected float FlashTimer;
        protected Vector2 Knockback;
        protected float AttackCooldown;

        private float _sniperShotTimer;
        private TextMesh _levelLabel;

        public static EnemyAI Spawn(RobotType type, Vector3 position, int level)
        {
            var go = new GameObject($"Robot_{type}_Lv{level}");
            go.transform.position = position;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 5;

            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            rb.linearDamping = 4f;

            var col = go.AddComponent<CircleCollider2D>();

            var ai = go.AddComponent<EnemyAI>();
            ai.Type = type;
            ai.EnemyLevel = Mathf.Max(1, level);
            ai.Rb = rb;
            ai.Sr = sr;

            float lvlScale = 1f + (ai.EnemyLevel - 1) * 0.20f;
            float spdBonus = Mathf.Min(2.0f, (ai.EnemyLevel - 1) * 0.12f);

            switch (type)
            {
                case RobotType.Drone:
                    sr.sprite = NeonSpriteFactory.Drone;
                    ai.BaseColor = new Color(1f, 0.22f, 0.52f);
                    ai.MaxHealth = 24f * lvlScale;
                    ai.MoveSpeed = 3.05f + spdBonus;
                    ai.ContactDamage = 10f + ai.EnemyLevel * 1.2f;
                    ai.XpDrop = 12 + ai.EnemyLevel * 2;
                    ai.ScoreValue = 25 + ai.EnemyLevel * 5;
                    go.transform.localScale = Vector3.one * 0.9f;
                    col.radius = 0.4f;
                    break;

                case RobotType.Swarmer:
                    sr.sprite = NeonSpriteFactory.Swarmer;
                    ai.BaseColor = new Color(1f, 0.58f, 0.14f);
                    ai.MaxHealth = 14f * lvlScale;
                    ai.MoveSpeed = 4.55f + spdBonus * 1.15f;
                    ai.ContactDamage = 7f + ai.EnemyLevel * 0.9f;
                    ai.XpDrop = 9 + ai.EnemyLevel;
                    ai.ScoreValue = 20 + ai.EnemyLevel * 4;
                    go.transform.localScale = Vector3.one * 0.72f;
                    col.radius = 0.34f;
                    break;

                case RobotType.Tank:
                    sr.sprite = NeonSpriteFactory.Tank;
                    ai.BaseColor = new Color(0.8f, 0.25f, 1f);
                    ai.MaxHealth = 88f * lvlScale;
                    ai.MoveSpeed = 1.9f + spdBonus * 0.5f;
                    ai.ContactDamage = 18f + ai.EnemyLevel * 2f;
                    ai.XpDrop = 32 + ai.EnemyLevel * 4;
                    ai.ScoreValue = 80 + ai.EnemyLevel * 12;
                    go.transform.localScale = Vector3.one * 1.25f;
                    col.radius = 0.48f;
                    break;

                case RobotType.Sniper:
                    sr.sprite = NeonSpriteFactory.Sniper;
                    ai.BaseColor = new Color(1f, 0.25f, 0.28f);
                    ai.MaxHealth = 32f * lvlScale;
                    ai.MoveSpeed = 2.6f + spdBonus * 0.6f;
                    ai.ContactDamage = 10f + ai.EnemyLevel * 1.2f;
                    ai.XpDrop = 22 + ai.EnemyLevel * 3;
                    ai.ScoreValue = 55 + ai.EnemyLevel * 8;
                    go.transform.localScale = Vector3.one * 0.95f;
                    col.radius = 0.4f;
                    ai._sniperShotTimer = Random.Range(1.4f, 2.5f);
                    break;

                case RobotType.Splitter:
                    sr.sprite = NeonSpriteFactory.Splitter;
                    ai.BaseColor = new Color(0.82f, 1f, 0.15f);
                    ai.MaxHealth = 48f * lvlScale;
                    ai.MoveSpeed = 2.55f + spdBonus * 0.7f;
                    ai.ContactDamage = 14f + ai.EnemyLevel * 1.5f;
                    ai.XpDrop = 24 + ai.EnemyLevel * 3;
                    ai.ScoreValue = 65 + ai.EnemyLevel * 10;
                    go.transform.localScale = Vector3.one * 1.05f;
                    col.radius = 0.44f;
                    break;
            }

            sr.color = ai.BaseColor;
            ai.CurrentHealth = ai.MaxHealth;
            ai.CreateLevelBadge($"Lv.{ai.EnemyLevel}", new Vector3(0f, 0.72f, 0f));
            return ai;
        }

        protected void CreateLevelBadge(string text, Vector3 localOffset)
        {
            var labelGo = new GameObject("LvBadge");
            labelGo.transform.SetParent(transform, false);
            labelGo.transform.localPosition = localOffset;
            _levelLabel = labelGo.AddComponent<TextMesh>();
            _levelLabel.text = text;
            _levelLabel.fontSize = 26;
            _levelLabel.characterSize = 0.068f;
            _levelLabel.anchor = TextAnchor.MiddleCenter;
            _levelLabel.alignment = TextAlignment.Center;
            _levelLabel.color = new Color(0.85f, 0.92f, 1f, 0.85f);
            labelGo.GetComponent<MeshRenderer>().sortingOrder = 25;
        }

        protected virtual void OnEnable()
        {
            ActiveEnemies.Add(this);
        }

        protected virtual void OnDisable()
        {
            ActiveEnemies.Remove(this);
        }

        protected virtual void Update()
        {
            if (AttackCooldown > 0f) AttackCooldown -= Time.deltaTime;

            if (FlashTimer > 0f)
            {
                FlashTimer -= Time.deltaTime;
                if (FlashTimer <= 0f && Sr != null) Sr.color = BaseColor;
            }

            if (_levelLabel != null)
            {
                _levelLabel.transform.rotation = Quaternion.identity;
            }

            OnEnemyUpdate(Time.deltaTime);
        }

        protected virtual void OnEnemyUpdate(float dt)
        {
            if (Type != RobotType.Sniper) return;
            var player = PlayerController.Instance;
            if (player == null || !player.IsAlive || (GameManager.Instance != null && !GameManager.Instance.IsPlaying)) return;

            _sniperShotTimer -= dt;
            if (_sniperShotTimer <= 0f)
            {
                float dist = Vector2.Distance(transform.position, player.transform.position);
                if (dist < 13f)
                {
                    Vector2 dir = (player.transform.position - transform.position).normalized;
                    EnemyProjectile.Spawn(transform.position + (Vector3)(dir * 0.6f), dir, 8.2f, 11f + EnemyLevel * 1.2f, new Color(1f, 0.25f, 0.25f), 0.44f);
                    NeonBurstEffect.Spawn(transform.position + (Vector3)(dir * 0.5f), new Color(1f, 0.4f, 0.2f), 4, 3f, 0.12f);
                    _sniperShotTimer = Random.Range(2.2f, 3.1f);
                }
            }
        }

        protected virtual void FixedUpdate()
        {
            var player = PlayerController.Instance;
            if (player == null || !player.IsAlive || (GameManager.Instance != null && !GameManager.Instance.IsPlaying))
            {
                SetVelocity(Vector2.zero);
                return;
            }

            MoveStep(player);
        }

        protected virtual void MoveStep(PlayerController player)
        {
            Vector2 toPlayer = (Vector2)(player.transform.position - transform.position);
            float dist = toPlayer.magnitude;
            Vector2 dir = dist > 0.01f ? toPlayer / dist : Vector2.zero;

            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);

            float moveFactor = 1f;
            if (Type == RobotType.Sniper && dist < 6.8f)
            {
                moveFactor = dist < 4.5f ? -0.6f : 0.15f; // Keeps sniper distance
            }

            Vector2 vel = dir * (MoveSpeed * moveFactor) + Knockback;
            Knockback = Vector2.Lerp(Knockback, Vector2.zero, 10f * Time.fixedDeltaTime);
            SetVelocity(vel);

            if (dist < 0.85f * transform.localScale.x + 0.35f && AttackCooldown <= 0f)
            {
                player.TakeDamage(ContactDamage);
                AttackCooldown = 0.65f;
            }
        }

        protected void SetVelocity(Vector2 v)
        {
#if UNITY_6000_0_OR_NEWER
            Rb.linearVelocity = v;
#else
            Rb.velocity = v;
#endif
        }

        public virtual void TakeDamage(float amount, Vector2 knockbackImpulse, bool isCrit = false)
        {
            CurrentHealth -= amount;
            FlashTimer = 0.08f;
            if (Sr != null) Sr.color = Color.white;
            float kbMult = (Type == RobotType.Tank || IsBoss) ? 0.18f : 1f;
            Knockback += knockbackImpulse * kbMult;

            FloatingDamageText.Spawn(transform.position, amount, isCrit);
            NeonAudio.Instance?.PlayHit();
            OnAfterDamage();

            if (CurrentHealth <= 0f)
            {
                Die();
            }
        }

        protected virtual void OnAfterDamage() { }

        protected virtual void Die()
        {
            NeonBurstEffect.Spawn(transform.position, BaseColor, Type == RobotType.Tank ? 16 : 9, 6f, 0.24f);
            NeonAudio.Instance?.PlayExplosion(Type == RobotType.Tank ? 0.8f : 0.5f);
            NeonCameraFollow.Instance?.Shake(Type == RobotType.Tank ? 0.15f : 0.05f, 0.08f);

            // Splitter spawns 2 rapid mini-swarmers on death
            if (Type == RobotType.Splitter)
            {
                for (int i = 0; i < 2; i++)
                {
                    Vector3 offset = (Vector3)(Random.insideUnitCircle.normalized * 0.7f);
                    Spawn(RobotType.Swarmer, transform.position + offset, Mathf.Max(1, EnemyLevel - 1));
                }
            }

            SpawnXpGem(transform.position, XpDrop, Type == RobotType.Tank ? 0.65f : 0.48f);
            GameManager.Instance?.OnEnemyKilled(ScoreValue, false);
            Destroy(gameObject);
        }

        protected static void SpawnXpGem(Vector3 pos, int xp, float scale)
        {
            var gemGo = new GameObject("XpGem");
            gemGo.transform.position = pos;
            gemGo.transform.localScale = Vector3.one * scale;
            var gemSr = gemGo.AddComponent<SpriteRenderer>();
            gemSr.sprite = NeonSpriteFactory.XpGem;
            gemSr.color = scale > 0.6f ? new Color(0.2f, 0.92f, 1f) : new Color(0.15f, 1f, 0.68f);
            gemSr.sortingOrder = 3;
            var gem = gemGo.AddComponent<XpGem>();
            gem.XpValue = xp;
        }
    }
}
