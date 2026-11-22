using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace NeonArena
{
    /// <summary>
    /// Cyberpunk pilot VEX-09 controller: smooth 2D movement, Cyber Dash with afterimages,
    /// auto-aim crit lasers, orbital blades, Chain Lightning, EMP Shockwave, and build tracking.
    /// </summary>
    public class PlayerController : MonoBehaviour
    {
        public static PlayerController Instance { get; private set; }

        [Header("Core Stats")]
        public float MaxHealth = 100f;
        public float CurrentHealth = 100f;
        public float MoveSpeed = 6.0f;
        public float PickupRadius = 2.8f;

        [Header("Cyber Dash")]
        public float DashSpeed = 19f;
        public float DashDuration = 0.19f;
        public float DashCooldown = 2.4f;
        public bool IsDashing => _dashTimer > 0f;
        public float DashCooldownRatio => Mathf.Clamp01(1f - (_dashCooldownTimer / Mathf.Max(0.1f, DashCooldown)));

        [Header("Laser Weapon")]
        public float FireRate = 2.8f;
        public float LaserDamage = 15f;
        public float LaserSpeed = 17f;
        public float TargetingRange = 13f;
        public int ProjectileCount = 1;
        public int PierceCount = 0;
        public float CritChance = 0.15f;
        public float CritMultiplier = 1.8f;

        [Header("Super Weapons")]
        public int OrbitalBladeCount = 0;
        public float OrbitalRadius = 2.2f;
        public float OrbitalSpeed = 220f;
        public float OrbitalDamage = 20f;

        public int LightningLevel = 0;
        public int EmpLevel = 0;

        [Header("Progression")]
        public int Level = 1;
        public int CurrentXp = 0;
        public int XpToNextLevel = 45;

        public Vector2 ArenaHalfBounds = new Vector2(24f, 16f);
        public bool IsAlive => CurrentHealth > 0f;

        public readonly Dictionary<UpgradeId, int> UpgradeLevels = new Dictionary<UpgradeId, int>();

        private Rigidbody2D _rb;
        private SpriteRenderer _sr;
        private float _fireCooldown;
        private float _lightningTimer = 1.5f;
        private float _empTimer = 2.5f;
        private float _invulnTimer;
        private float _dashTimer;
        private float _dashCooldownTimer;
        private Vector2 _dashDirection;
        private Vector2 _lastMoveDir = Vector2.right;
        private float _thrusterTimer;
        private float _afterimageTimer;
        private float _orbitalAngle;
        private readonly List<Transform> _blades = new List<Transform>();
        private readonly Dictionary<EnemyAI, float> _bladeHitCooldowns = new Dictionary<EnemyAI, float>();

        public static PlayerController Spawn(Vector3 position)
        {
            var go = new GameObject("Player_VEX09");
            go.transform.position = position;
            go.transform.localScale = Vector3.one * 0.98f;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = NeonSpriteFactory.Player;
            sr.color = Color.white;
            sr.sortingOrder = 10;

            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.36f;

            var player = go.AddComponent<PlayerController>();
            player._rb = rb;
            player._sr = sr;
            return player;
        }

        private void Awake()
        {
            Instance = this;
            CurrentHealth = MaxHealth;
        }

        /// <summary>
        /// Applies the currently selected Hangar ship archetype and all permanent Shop upgrades.
        /// </summary>
        public void ApplyShipAndMetaStats()
        {
            var ship = NeonMetaProgression.SelectedShip;
            var def = NeonMetaProgression.GetShipDef(ship);

            if (_sr != null)
            {
                _sr.sprite = NeonSpriteFactory.GetShipSprite(ship);
            }

            // Reset to base stats + permanent Hangar shop bonuses
            MaxHealth = 100f + NeonMetaProgression.BonusMaxHealth;
            MoveSpeed = 6.0f * NeonMetaProgression.SpeedMultiplier;
            LaserDamage = 15f * NeonMetaProgression.DamageMultiplier;
            PickupRadius = 2.8f * NeonMetaProgression.MagnetMultiplier;
            FireRate = 2.8f;
            CritChance = 0.15f;
            DashCooldown = 2.4f;
            ProjectileCount = 1;
            PierceCount = 0;

            // Ship-specific unique traits
            if (ship == ShipType.Phantom7)
            {
                MoveSpeed *= 1.20f;
                CritChance += 0.18f;
                DashCooldown *= 0.70f;
            }
            else if (ship == ShipType.TitanMk4)
            {
                MaxHealth += 55f;
                LaserDamage *= 1.22f;
                MoveSpeed *= 0.92f;
                if (OrbitalBladeCount == 0)
                {
                    AddOrbitalBlade();
                    RecordUpgrade(UpgradeId.OrbitalBlade);
                }
            }

            CurrentHealth = MaxHealth;
        }

        public int GetUpgradeLevel(UpgradeId id)
        {
            return UpgradeLevels.TryGetValue(id, out int lv) ? lv : 0;
        }

        public void RecordUpgrade(UpgradeId id)
        {
            UpgradeLevels[id] = GetUpgradeLevel(id) + 1;
        }

        private void Update()
        {
            if (!IsAlive || (GameManager.Instance != null && !GameManager.Instance.IsPlaying)) return;

            if (_dashCooldownTimer > 0f) _dashCooldownTimer -= Time.deltaTime;

            if (WasDashKeyPressed())
            {
                TriggerDash();
            }

            if (_dashTimer > 0f)
            {
                _dashTimer -= Time.deltaTime;
                _afterimageTimer -= Time.deltaTime;
                if (_afterimageTimer <= 0f)
                {
                    SpawnAfterimage();
                    _afterimageTimer = 0.032f;
                }
            }

            if (_invulnTimer > 0f)
            {
                _invulnTimer -= Time.deltaTime;
                _sr.color = (Mathf.FloorToInt(_invulnTimer * 20f) % 2 == 0) ? new Color(1f, 0.35f, 0.5f) : Color.white;
                if (_invulnTimer <= 0f) _sr.color = Color.white;
            }

            HandleAutoFire();
            HandleSuperWeapons();
            UpdateOrbitalBlades();
        }

        private void HandleSuperWeapons()
        {
            // Chain Lightning
            if (LightningLevel > 0)
            {
                _lightningTimer -= Time.deltaTime;
                if (_lightningTimer <= 0f && EnemyAI.ActiveEnemies.Count > 0)
                {
                    int jumps = 2 + LightningLevel;
                    float dmg = 22f + LightningLevel * 10f;
                    ChainLightningEffect.Cast(transform.position, jumps, dmg);
                    _lightningTimer = Mathf.Max(1.3f, 3.4f - LightningLevel * 0.35f);
                }
            }

            // EMP Shockwave
            if (EmpLevel > 0)
            {
                _empTimer -= Time.deltaTime;
                if (_empTimer <= 0f)
                {
                    float radius = 3.8f + EmpLevel * 0.7f;
                    float dmg = 20f + EmpLevel * 12f;
                    EmpShockwaveEffect.Cast(transform.position, radius, dmg);
                    _empTimer = Mathf.Max(2.8f, 6.2f - EmpLevel * 0.55f);
                }
            }
        }

        private void FixedUpdate()
        {
            if (!IsAlive || (GameManager.Instance != null && !GameManager.Instance.IsPlaying))
            {
                SetVelocity(Vector2.zero);
                return;
            }

            Vector2 move = VirtualJoystick.Instance != null ? VirtualJoystick.Instance.Direction : Vector2.zero;
            if (move.sqrMagnitude > 0.04f)
            {
                _lastMoveDir = move.normalized;
                _thrusterTimer -= Time.fixedDeltaTime;
                if (_thrusterTimer <= 0f && !IsDashing)
                {
                    _thrusterTimer = 0.06f;
                    Vector3 back = transform.position - transform.right * 0.45f;
                    NeonBurstEffect.Spawn(back, new Color(0f, 0.85f, 1f), 1, 2.2f, 0.12f);
                }
            }

            SetVelocity(IsDashing ? _dashDirection * DashSpeed : move * MoveSpeed);

            Vector3 p = transform.position;
            p.x = Mathf.Clamp(p.x, -ArenaHalfBounds.x + 0.6f, ArenaHalfBounds.x - 0.6f);
            p.y = Mathf.Clamp(p.y, -ArenaHalfBounds.y + 0.6f, ArenaHalfBounds.y - 0.6f);
            transform.position = p;

            EnemyAI nearest = FindNearestEnemy();
            Vector2 lookDir = Vector2.zero;
            if (nearest != null) lookDir = (nearest.transform.position - transform.position);
            else if (move.sqrMagnitude > 0.01f) lookDir = move;

            if (lookDir.sqrMagnitude > 0.001f)
            {
                float targetAngle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg;
                float currentAngle = transform.eulerAngles.z;
                float nextAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, 760f * Time.fixedDeltaTime);
                transform.rotation = Quaternion.Euler(0f, 0f, nextAngle);
            }
        }

        public void TriggerDash()
        {
            if (!IsAlive || IsDashing || _dashCooldownTimer > 0f) return;
            if (GameManager.Instance != null && !GameManager.Instance.IsPlaying) return;

            Vector2 move = VirtualJoystick.Instance != null ? VirtualJoystick.Instance.Direction : Vector2.zero;
            _dashDirection = move.sqrMagnitude > 0.05f ? move.normalized : _lastMoveDir;
            _dashTimer = DashDuration;
            _dashCooldownTimer = DashCooldown;
            _invulnTimer = Mathf.Max(_invulnTimer, DashDuration + 0.08f);

            NeonBurstEffect.Spawn(transform.position, new Color(0f, 1f, 1f), 8, 5.5f, 0.18f);
            NeonAudio.Instance?.PlayDash();
        }

        private bool WasDashKeyPressed()
        {
#if ENABLE_INPUT_SYSTEM
            var kb = Keyboard.current;
            return kb != null && (kb.spaceKey.wasPressedThisFrame || kb.leftShiftKey.wasPressedThisFrame);
#else
            return Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.LeftShift);
#endif
        }

        private void SpawnAfterimage()
        {
            var ghost = new GameObject("DashGhost");
            ghost.transform.position = transform.position;
            ghost.transform.rotation = transform.rotation;
            ghost.transform.localScale = transform.localScale;
            var sr = ghost.AddComponent<SpriteRenderer>();
            sr.sprite = _sr != null ? _sr.sprite : NeonSpriteFactory.Player;
            var shipCol = NeonMetaProgression.GetShipDef(NeonMetaProgression.SelectedShip).AccentColor;
            sr.color = new Color(shipCol.r, shipCol.g, shipCol.b, 0.55f);
            sr.sortingOrder = 9;
            ghost.AddComponent<DashAfterimageFade>();
        }

        private void SetVelocity(Vector2 v)
        {
#if UNITY_6000_0_OR_NEWER
            _rb.linearVelocity = v;
#else
            _rb.velocity = v;
#endif
        }

        private void HandleAutoFire()
        {
            _fireCooldown -= Time.deltaTime;
            if (_fireCooldown > 0f) return;

            EnemyAI target = FindNearestEnemy();
            if (target == null) return;

            Vector2 baseDir = ((Vector2)(target.transform.position - transform.position)).normalized;
            FireLaserVolley(baseDir);
            _fireCooldown = 1f / Mathf.Max(0.2f, FireRate);
        }

        private void FireLaserVolley(Vector2 baseDir)
        {
            float baseAngle = Mathf.Atan2(baseDir.y, baseDir.x) * Mathf.Rad2Deg;
            float spreadStep = 11f;
            float startAngle = baseAngle - spreadStep * (ProjectileCount - 1) * 0.5f;

            for (int i = 0; i < ProjectileCount; i++)
            {
                float rad = (startAngle + i * spreadStep) * Mathf.Deg2Rad;
                Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
                bool isCrit = Random.value < CritChance;
                float dmg = isCrit ? LaserDamage * CritMultiplier : LaserDamage;

                var bolt = new GameObject("LaserBolt");
                bolt.transform.position = transform.position + (Vector3)(dir * 0.58f);
                bolt.transform.localScale = isCrit ? new Vector3(1.18f, 0.68f, 1f) : new Vector3(0.95f, 0.55f, 1f);

                var sr = bolt.AddComponent<SpriteRenderer>();
                sr.sprite = NeonSpriteFactory.Laser;
                sr.color = isCrit ? new Color(1f, 0.92f, 0.2f) : new Color(0.2f, 0.95f, 1f);
                sr.sortingOrder = 8;

                var col = bolt.AddComponent<CircleCollider2D>();
                col.isTrigger = true;
                col.radius = 0.28f;

                var proj = bolt.AddComponent<LaserProjectile>();
                proj.Initialize(dir, LaserSpeed, dmg, PierceCount, isCrit);
            }

            NeonAudio.Instance?.PlayLaser();
        }

        private EnemyAI FindNearestEnemy()
        {
            EnemyAI best = null;
            float bestDistSq = TargetingRange * TargetingRange;
            Vector3 myPos = transform.position;

            var list = EnemyAI.ActiveEnemies;
            for (int i = 0; i < list.Count; i++)
            {
                var e = list[i];
                if (e == null) continue;
                float dSq = (e.transform.position - myPos).sqrMagnitude;
                if (dSq < bestDistSq)
                {
                    bestDistSq = dSq;
                    best = e;
                }
            }
            return best;
        }

        public void AddOrbitalBlade()
        {
            OrbitalBladeCount++;
            RebuildOrbitalBlades();
        }

        private void RebuildOrbitalBlades()
        {
            for (int i = 0; i < _blades.Count; i++)
            {
                if (_blades[i] != null) Destroy(_blades[i].gameObject);
            }
            _blades.Clear();

            for (int i = 0; i < OrbitalBladeCount; i++)
            {
                var bladeGo = new GameObject($"OrbitalBlade_{i}");
                bladeGo.transform.localScale = Vector3.one * 0.62f;
                var sr = bladeGo.AddComponent<SpriteRenderer>();
                sr.sprite = NeonSpriteFactory.Blade;
                sr.color = new Color(0f, 1f, 0.95f);
                sr.sortingOrder = 11;
                _blades.Add(bladeGo.transform);
            }
        }

        private void UpdateOrbitalBlades()
        {
            if (_blades.Count == 0) return;

            _orbitalAngle += OrbitalSpeed * Time.deltaTime;
            float step = 360f / _blades.Count;

            for (int i = 0; i < _blades.Count; i++)
            {
                float rad = (_orbitalAngle + i * step) * Mathf.Deg2Rad;
                Vector3 pos = transform.position + new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f) * OrbitalRadius;
                _blades[i].position = pos;
                _blades[i].rotation = Quaternion.Euler(0f, 0f, _orbitalAngle * 2f);

                var enemies = EnemyAI.ActiveEnemies;
                for (int eIdx = enemies.Count - 1; eIdx >= 0; eIdx--)
                {
                    var enemy = enemies[eIdx];
                    if (enemy == null) continue;
                    float hitRad = enemy.IsBoss ? 1.45f : 0.78f;
                    if ((enemy.transform.position - pos).sqrMagnitude < hitRad * hitRad)
                    {
                        if (!_bladeHitCooldowns.TryGetValue(enemy, out float nextTime) || Time.time >= nextTime)
                        {
                            _bladeHitCooldowns[enemy] = Time.time + 0.35f;
                            Vector2 push = (enemy.transform.position - transform.position).normalized * 4f;
                            enemy.TakeDamage(OrbitalDamage, push, false);
                            NeonBurstEffect.Spawn(pos, new Color(0f, 1f, 0.95f), 4, 4f, 0.14f);
                        }
                    }
                }
            }
        }

        public void AddXp(int amount)
        {
            if (!IsAlive) return;
            NeonAudio.Instance?.PlayXp();
            CurrentXp += amount;
            if (CurrentXp >= XpToNextLevel)
            {
                CurrentXp -= XpToNextLevel;
                Level++;
                XpToNextLevel = Mathf.RoundToInt(XpToNextLevel * 1.35f + 18);
                NeonAudio.Instance?.PlayLevelUp();
                GameManager.Instance?.TriggerLevelUp();
            }
        }

        public void TakeDamage(float amount)
        {
            if (!IsAlive || _invulnTimer > 0f || IsDashing) return;

            CurrentHealth = Mathf.Max(0f, CurrentHealth - amount);
            _invulnTimer = 0.45f;
            NeonBurstEffect.Spawn(transform.position, new Color(1f, 0.2f, 0.4f), 8, 5f, 0.2f);
            NeonAudio.Instance?.PlayExplosion(0.6f);
            NeonCameraFollow.Instance?.Shake(0.22f, 0.16f);

            if (CurrentHealth <= 0f)
            {
                NeonBurstEffect.Spawn(transform.position, new Color(0f, 0.95f, 1f), 26, 8f, 0.28f);
                _sr.enabled = false;
                GameManager.Instance?.TriggerGameOver();
            }
        }

        public void Heal(float amount)
        {
            CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + amount);
        }

        /// <summary>
        /// Revives the player at full HP with brief invulnerability and a safety EMP shockwave.
        /// </summary>
        public void Revive()
        {
            CurrentHealth = MaxHealth;
            _invulnTimer = 2.2f;
            if (_sr != null)
            {
                _sr.enabled = true;
                _sr.color = Color.white;
            }
            EmpShockwaveEffect.Cast(transform.position, 6.5f, 35f);
            NeonBurstEffect.Spawn(transform.position, new Color(0.15f, 1f, 0.65f), 20, 7f, 0.26f);
            NeonAudio.Instance?.PlayLevelUp();
        }
    }

    public class DashAfterimageFade : MonoBehaviour
    {
        private SpriteRenderer _sr;
        private float _life = 0.22f;

        private void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            _life -= Time.deltaTime;
            if (_life <= 0f)
            {
                Destroy(gameObject);
                return;
            }
            var c = _sr.color;
            c.a = (_life / 0.22f) * 0.55f;
            _sr.color = c;
        }
    }
}
