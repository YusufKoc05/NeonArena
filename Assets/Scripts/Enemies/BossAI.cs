using UnityEngine;

namespace NeonArena
{
    /// <summary>
    /// High-level Sector Final Bosses (Sentinel-X Lv.10, Hydra-V9 Lv.20, Omega-Overlord Lv.35+).
    /// Features rotating energy aura, multi-pattern plasma attacks, Phase 2 enrage at 50% HP,
    /// and huge XP reward showers.
    /// </summary>
    public class BossAI : EnemyAI
    {
        public static BossAI ActiveBoss { get; private set; }

        public string BossName { get; private set; }
        public int SectorNumber { get; private set; }
        public bool IsEnraged { get; private set; }

        private Transform _auraRing;
        private SpriteRenderer _auraSr;
        private float _patternTimer = 2.2f;
        private int _patternStep;
        private float _dashTimer;
        private Vector2 _dashDir;
        private float _spiralAngle;

        public static BossAI SpawnBoss(int sector, Vector3 position, int playerLevel)
        {
            // Final Bosses are ALWAYS significantly high level!
            int baseBossLevel = sector == 1 ? 10 : (sector == 2 ? 20 : 35 + (sector - 3) * 15);
            int bossLevel = Mathf.Max(baseBossLevel, playerLevel + 6 + sector * 2);

            string bossName;
            Color bossColor;
            if (sector == 1)
            {
                bossName = "SENTINEL-X";
                bossColor = new Color(1f, 0.2f, 0.48f);
            }
            else if (sector == 2)
            {
                bossName = "HYDRA-V9";
                bossColor = new Color(0.82f, 0.22f, 1f);
            }
            else if (sector == 3)
            {
                bossName = "OMEGA-OVERLORD";
                bossColor = new Color(1f, 0.35f, 0.1f);
            }
            else
            {
                bossName = $"OMEGA-PRIME MK.{sector - 2}";
                bossColor = new Color(1f, 0.15f, 0.25f);
            }

            var go = new GameObject($"BOSS_{bossName}_Lv{bossLevel}");
            go.transform.position = position;
            go.transform.localScale = Vector3.one * 2.15f;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = NeonSpriteFactory.BossCore;
            sr.color = bossColor;
            sr.sortingOrder = 7;

            // Rotating neon energy aura ring behind Boss
            var auraGo = new GameObject("BossAura");
            auraGo.transform.SetParent(go.transform, false);
            auraGo.transform.localScale = Vector3.one * 1.45f;
            var auraSr = auraGo.AddComponent<SpriteRenderer>();
            auraSr.sprite = NeonSpriteFactory.BossAura;
            auraSr.color = new Color(bossColor.r, bossColor.g, bossColor.b, 0.78f);
            auraSr.sortingOrder = 6;

            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            rb.linearDamping = 5f;

            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.62f;

            var boss = go.AddComponent<BossAI>();
            boss.Type = RobotType.Boss;
            boss.BossName = bossName;
            boss.SectorNumber = sector;
            boss.EnemyLevel = bossLevel;
            boss.Rb = rb;
            boss.Sr = sr;
            boss.BaseColor = bossColor;
            boss._auraRing = auraGo.transform;
            boss._auraSr = auraSr;

            // High-level Boss stats
            boss.MaxHealth = 680f + (sector - 1) * 750f + bossLevel * 28f;
            boss.CurrentHealth = boss.MaxHealth;
            boss.MoveSpeed = 2.45f + sector * 0.25f;
            boss.ContactDamage = 24f + sector * 6f;
            boss.XpDrop = 160 + sector * 80;
            boss.ScoreValue = 600 + sector * 450;

            ActiveBoss = boss;
            NeonBurstEffect.Spawn(position, bossColor, 26, 8f, 0.35f);
            NeonCameraFollow.Instance?.Shake(0.35f, 0.4f);
            NeonAudio.Instance?.PlayBossWarning();
            return boss;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (ActiveBoss == this) ActiveBoss = null;
        }

        protected override void OnEnemyUpdate(float dt)
        {
            if (_auraRing != null)
            {
                _auraRing.Rotate(0f, 0f, (IsEnraged ? -170f : 90f) * dt);
            }

            if (GameManager.Instance != null && !GameManager.Instance.IsPlaying) return;
            var player = PlayerController.Instance;
            if (player == null || !player.IsAlive) return;

            if (_dashTimer > 0f)
            {
                _dashTimer -= dt;
                return;
            }

            _patternTimer -= dt;
            if (_patternTimer <= 0f)
            {
                ExecuteBossPattern(player);
                float baseCd = IsEnraged ? 1.55f : 2.35f;
                _patternTimer = Mathf.Max(1.05f, baseCd - (SectorNumber - 1) * 0.15f);
            }
        }

        protected override void MoveStep(PlayerController player)
        {
            Vector2 toPlayer = (Vector2)(player.transform.position - transform.position);
            float dist = toPlayer.magnitude;
            Vector2 dir = dist > 0.01f ? toPlayer / dist : Vector2.zero;

            transform.Rotate(0f, 0f, (IsEnraged ? 95f : 42f) * Time.fixedDeltaTime);

            Vector2 vel;
            if (_dashTimer > 0f)
            {
                vel = _dashDir * (MoveSpeed * 3.8f);
            }
            else
            {
                vel = dir * MoveSpeed + Knockback;
            }

            Knockback = Vector2.Lerp(Knockback, Vector2.zero, 12f * Time.fixedDeltaTime);
            SetVelocity(vel);

            // Clamp inside arena
            Vector3 p = transform.position;
            p.x = Mathf.Clamp(p.x, -22f, 22f);
            p.y = Mathf.Clamp(p.y, -14f, 14f);
            transform.position = p;

            if (dist < 1.55f && AttackCooldown <= 0f)
            {
                player.TakeDamage(ContactDamage);
                AttackCooldown = 0.7f;
            }
        }

        private void ExecuteBossPattern(PlayerController player)
        {
            Vector2 toPlayer = (player.transform.position - transform.position).normalized;
            float dmg = 14f + SectorNumber * 4f;
            Color shotColor = IsEnraged ? new Color(1f, 0.75f, 0.15f) : BaseColor;

            int pattern = _patternStep % 4;
            _patternStep++;

            switch (pattern)
            {
                case 0:
                    // Aimed fan volley
                    int count = IsEnraged ? 7 : 5;
                    float baseAng = Mathf.Atan2(toPlayer.y, toPlayer.x) * Mathf.Rad2Deg;
                    float step = 14f;
                    float start = baseAng - step * (count - 1) * 0.5f;
                    for (int i = 0; i < count; i++)
                    {
                        float rad = (start + i * step) * Mathf.Deg2Rad;
                        Vector2 d = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
                        EnemyProjectile.Spawn(transform.position + (Vector3)(d * 1.1f), d, 7.6f, dmg, shotColor, 0.56f);
                    }
                    break;

                case 1:
                    // 360-degree neon plasma ring
                    int ringCount = IsEnraged ? 16 : 12;
                    _spiralAngle += 18f;
                    for (int i = 0; i < ringCount; i++)
                    {
                        float rad = (_spiralAngle + i * (360f / ringCount)) * Mathf.Deg2Rad;
                        Vector2 d = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
                        EnemyProjectile.Spawn(transform.position + (Vector3)(d * 1.1f), d, 6.2f, dmg, shotColor, 0.52f);
                    }
                    break;

                case 2:
                    // High-speed cyber charge dash
                    _dashDir = toPlayer;
                    _dashTimer = 0.52f;
                    NeonBurstEffect.Spawn(transform.position, shotColor, 14, 7f, 0.28f);
                    NeonAudio.Instance?.PlayDash();
                    break;

                case 3:
                    // Summon elite escort robots (Sector 2+ or Enraged)
                    if (SectorNumber >= 2 || IsEnraged)
                    {
                        int minions = IsEnraged ? 3 : 2;
                        for (int i = 0; i < minions; i++)
                        {
                            Vector3 off = (Vector3)(Random.insideUnitCircle.normalized * 2.2f);
                            Spawn(RobotType.Swarmer, transform.position + off, Mathf.Max(2, SectorNumber * 3));
                        }
                    }
                    else
                    {
                        // Extra aimed triple shot for Sector 1 Phase 1
                        for (int i = -1; i <= 1; i++)
                        {
                            float rad = (Mathf.Atan2(toPlayer.y, toPlayer.x) * Mathf.Rad2Deg + i * 16f) * Mathf.Deg2Rad;
                            Vector2 d = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
                            EnemyProjectile.Spawn(transform.position + (Vector3)(d * 1.1f), d, 8.2f, dmg, shotColor, 0.55f);
                        }
                    }
                    break;
            }
        }

        protected override void OnAfterDamage()
        {
            if (!IsEnraged && CurrentHealth <= MaxHealth * 0.5f)
            {
                IsEnraged = true;
                BaseColor = new Color(1f, 0.12f, 0.15f);
                if (_auraSr != null) _auraSr.color = new Color(1f, 0.78f, 0.12f, 0.95f);
                MoveSpeed *= 1.28f;
                NeonBurstEffect.Spawn(transform.position, new Color(1f, 0.75f, 0.1f), 24, 9f, 0.34f);
                NeonCameraFollow.Instance?.Shake(0.32f, 0.35f);
                NeonHUD.Instance?.ShowRadioMessage($"{BossName} [LV.{EnemyLevel}]", "AŞIRI YÜKLEME AKTİF! SİSTEMLERİNİ SİLECEĞİM!", new Color(1f, 0.25f, 0.25f), 4.5f);
            }
        }

        protected override void Die()
        {
            NeonBurstEffect.Spawn(transform.position, BaseColor, 42, 11f, 0.42f);
            NeonBurstEffect.Spawn(transform.position, Color.white, 20, 7f, 0.28f);
            NeonAudio.Instance?.PlayBossDefeat();
            NeonCameraFollow.Instance?.Shake(0.45f, 0.5f);

            // Massive XP shower!
            for (int i = 0; i < 10; i++)
            {
                Vector3 offset = (Vector3)(Random.insideUnitCircle * 2.2f);
                SpawnXpGem(transform.position + offset, XpDrop / 10, 0.72f);
            }

            if (ActiveBoss == this) ActiveBoss = null;
            GameManager.Instance?.OnEnemyKilled(ScoreValue, true);
            Destroy(gameObject);
        }
    }
}
