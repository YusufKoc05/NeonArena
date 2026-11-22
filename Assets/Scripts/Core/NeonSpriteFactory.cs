using System.Collections.Generic;
using UnityEngine;

namespace NeonArena
{
    /// <summary>
    /// Generates high-detail glowing cyberpunk neon sprites procedurally at runtime:
    /// ships, 5 robot classes, Final Boss core, chamfered sci-fi UI card frames, and 11 custom ability icons.
    /// </summary>
    public static class NeonSpriteFactory
    {
        private static Sprite _playerSprite;
        private static Sprite _phantomSprite;
        private static Sprite _titanSprite;
        private static Sprite _droneSprite;
        private static Sprite _swarmerSprite;
        private static Sprite _tankSprite;
        private static Sprite _sniperSprite;
        private static Sprite _splitterSprite;
        private static Sprite _bossSprite;
        private static Sprite _bossAuraSprite;
        private static Sprite _laserSprite;
        private static Sprite _plasmaOrbSprite;
        private static Sprite _xpGemSprite;
        private static Sprite _circleSprite;
        private static Sprite _ringSprite;
        private static Sprite _gridTileSprite;
        private static Sprite _bladeSprite;
        private static Sprite _solidRectSprite;
        private static Sprite _cyberCardBg;
        private static Sprite _cyberCardBorder;
        private static Sprite _hexBadgeSprite;
        private static Sprite _hudBarTrack;
        private static Sprite _hudBarFill;
        private static Sprite _hudPill;
        private static Sprite _heartIcon;
        private static readonly Dictionary<UpgradeId, Sprite> _upgradeIcons = new Dictionary<UpgradeId, Sprite>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetCache()
        {
            _playerSprite = null;
            _phantomSprite = null;
            _titanSprite = null;
            _droneSprite = null;
            _swarmerSprite = null;
            _tankSprite = null;
            _sniperSprite = null;
            _splitterSprite = null;
            _bossSprite = null;
            _bossAuraSprite = null;
            _laserSprite = null;
            _plasmaOrbSprite = null;
            _xpGemSprite = null;
            _circleSprite = null;
            _ringSprite = null;
            _gridTileSprite = null;
            _bladeSprite = null;
            _solidRectSprite = null;
            _cyberCardBg = null;
            _cyberCardBorder = null;
            _hexBadgeSprite = null;
            _hudBarTrack = null;
            _hudBarFill = null;
            _hudPill = null;
            _heartIcon = null;
            _upgradeIcons.Clear();
        }

        private static Sprite GetOrCreate(ref Sprite slot, System.Func<Sprite> factory)
        {
            if (slot == null || slot.texture == null)
            {
                slot = factory();
            }
            return slot;
        }

        public static Sprite Player => GetOrCreate(ref _playerSprite, CreatePlayerSprite);
        public static Sprite PhantomShip => GetOrCreate(ref _phantomSprite, CreatePhantomShipSprite);
        public static Sprite TitanShip => GetOrCreate(ref _titanSprite, CreateTitanShipSprite);

        public static Sprite GetShipSprite(ShipType ship)
        {
            switch (ship)
            {
                case ShipType.Phantom7: return PhantomShip;
                case ShipType.TitanMk4: return TitanShip;
                default: return Player;
            }
        }
        public static Sprite Drone => GetOrCreate(ref _droneSprite, () => CreateCyberRobotSprite(96, new Color(1f, 0.16f, 0.48f), new Color(1f, 0.8f, 0.9f), 4));
        public static Sprite Swarmer => GetOrCreate(ref _swarmerSprite, () => CreateTriangleSprite(72, new Color(1f, 0.55f, 0.1f), new Color(1f, 0.95f, 0.65f)));
        public static Sprite Tank => GetOrCreate(ref _tankSprite, () => CreateCyberRobotSprite(112, new Color(0.75f, 0.22f, 1f), new Color(0.95f, 0.78f, 1f), 8));
        public static Sprite Sniper => GetOrCreate(ref _sniperSprite, () => CreateSniperSprite(96));
        public static Sprite Splitter => GetOrCreate(ref _splitterSprite, () => CreateCyberRobotSprite(96, new Color(0.82f, 1f, 0.12f), new Color(1f, 1f, 0.75f), 6));
        public static Sprite BossCore => GetOrCreate(ref _bossSprite, () => CreateBossCoreSprite(160));
        public static Sprite BossAura => GetOrCreate(ref _bossAuraSprite, () => CreateBossAuraSprite(192));
        public static Sprite Laser => GetOrCreate(ref _laserSprite, CreateLaserSprite);
        public static Sprite PlasmaOrb => GetOrCreate(ref _plasmaOrbSprite, () => CreatePlasmaOrbSprite(48));
        public static Sprite XpGem => GetOrCreate(ref _xpGemSprite, () => CreateDiamondSprite(44, new Color(0.1f, 1f, 0.68f), new Color(0.85f, 1f, 0.95f)));
        public static Sprite Circle => GetOrCreate(ref _circleSprite, () => CreateCircleSprite(64));
        public static Sprite Ring => GetOrCreate(ref _ringSprite, () => CreateRingSprite(128, 0.78f, 0.94f));
        public static Sprite GridTile => GetOrCreate(ref _gridTileSprite, () => CreateGridTileSprite(128));
        public static Sprite Blade => GetOrCreate(ref _bladeSprite, () => CreateDiamondSprite(48, new Color(0f, 0.95f, 1f), Color.white));
        public static Sprite SolidRect => GetOrCreate(ref _solidRectSprite, CreateSolidRectSprite);
        public static Sprite CyberCardBg => GetOrCreate(ref _cyberCardBg, () => CreateCyberCardSprite(256, 360, false));
        public static Sprite CyberCardBorder => GetOrCreate(ref _cyberCardBorder, () => CreateCyberCardSprite(256, 360, true));
        public static Sprite HexBadge => GetOrCreate(ref _hexBadgeSprite, () => CreateHexBadgeSprite(128));
        public static Sprite HudBarTrack => GetOrCreate(ref _hudBarTrack, () => CreateSlantedBarSprite(256, 36, false));
        public static Sprite HudBarFill => GetOrCreate(ref _hudBarFill, () => CreateSlantedBarSprite(256, 36, true));
        public static Sprite HudPill => GetOrCreate(ref _hudPill, () => CreateChamferedPillSprite(256, 64));
        public static Sprite HeartIcon => GetOrCreate(ref _heartIcon, () => CreateHeartIconSprite(64));

        public static Sprite GetUpgradeIcon(UpgradeId id)
        {
            if (_upgradeIcons.TryGetValue(id, out var existing) && existing != null && existing.texture != null)
                return existing;
            var icon = CreateUpgradeIconSprite(id);
            _upgradeIcons[id] = icon;
            return icon;
        }

        private static Texture2D MakeTex(int w, int h, FilterMode filter = FilterMode.Bilinear, TextureWrapMode wrap = TextureWrapMode.Clamp)
        {
            return new Texture2D(w, h, TextureFormat.RGBA32, false)
            {
                filterMode = filter,
                wrapMode = wrap
            };
        }

        private static Sprite ToSprite(Texture2D tex, float ppu = 64f, SpriteMeshType meshType = SpriteMeshType.FullRect)
        {
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), ppu, 0, meshType);
        }

        /// <summary>
        /// Creates a handcrafted chamfered-corner sci-fi card panel (with subtle cyber scanlines & corner brackets).
        /// </summary>
        private static Sprite CreateCyberCardSprite(int w, int h, bool borderOnly)
        {
            var tex = MakeTex(w, h);
            float cut = 26f;

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    // Chamfered top-left and bottom-right corners (asymmetric cyberpunk cut!)
                    bool cutTL = (x + (h - 1 - y)) < cut;
                    bool cutBR = ((w - 1 - x) + y) < cut;
                    // Small chamfer on TR and BL
                    bool cutTR = ((w - 1 - x) + (h - 1 - y)) < cut * 0.45f;
                    bool cutBL = (x + y) < cut * 0.45f;

                    if (cutTL || cutBR || cutTR || cutBL)
                    {
                        tex.SetPixel(x, y, Color.clear);
                        continue;
                    }

                    // Distance to edges for crisp neon frame
                    float dLeft = x;
                    float dRight = w - 1 - x;
                    float dBottom = y;
                    float dTop = h - 1 - y;
                    float dTL = (x + (h - 1 - y) - cut) * 0.707f;
                    float dBR = ((w - 1 - x) + y - cut) * 0.707f;
                    float dTR = ((w - 1 - x) + (h - 1 - y) - cut * 0.45f) * 0.707f;
                    float dBL = (x + y - cut * 0.45f) * 0.707f;

                    float edgeDist = Mathf.Min(
                        Mathf.Min(Mathf.Min(dLeft, dRight), Mathf.Min(dBottom, dTop)),
                        Mathf.Min(Mathf.Min(dTL, dBR), Mathf.Min(dTR, dBL))
                    );

                    if (borderOnly)
                    {
                        bool isOuterFrame = edgeDist < 3.5f;
                        bool isTopAccent = y > h - 14 && edgeDist < 14f;
                        bool isCornerTick = edgeDist < 8f && (y > h - 42 || y < 42);
                        if (isOuterFrame || isTopAccent || isCornerTick)
                        {
                            float alpha = isOuterFrame ? 1f : (isTopAccent ? 0.55f : 0.85f);
                            tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                        }
                        else if (edgeDist < 12f)
                        {
                            float innerGlow = (1f - (edgeDist - 3.5f) / 8.5f) * 0.22f;
                            tex.SetPixel(x, y, new Color(1f, 1f, 1f, innerGlow));
                        }
                        else
                        {
                            tex.SetPixel(x, y, Color.clear);
                        }
                    }
                    else
                    {
                        // Rich dark cyber-slate gradient + subtle horizontal scanlines
                        float vGrad = (float)y / h;
                        Color topCol = new Color(0.08f, 0.12f, 0.23f, 0.96f);
                        Color botCol = new Color(0.03f, 0.05f, 0.11f, 0.98f);
                        Color c = Color.Lerp(botCol, topCol, vGrad);

                        if (y % 6 == 0) c += new Color(0.015f, 0.025f, 0.04f, 0f);
                        if (Mathf.Abs((x + y) % 48) < 1 && vGrad > 0.55f) c += new Color(0.02f, 0.035f, 0.05f, 0f);

                        tex.SetPixel(x, y, c);
                    }
                }
            }
            return ToSprite(tex, 64f);
        }

        private static Sprite CreateHexBadgeSprite(int size)
        {
            var tex = MakeTex(size, size);
            float half = size * 0.5f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float nx = (x - half) / half;
                    float ny = (y - half) / half;
                    float r = Mathf.Sqrt(nx * nx + ny * ny);
                    float a = Mathf.Atan2(ny, nx);
                    float sector = Mathf.PI / 3f;
                    float hex = r * Mathf.Cos(Mathf.Repeat(a + sector * 0.5f, sector) - sector * 0.5f);

                    if (hex < 0.78f)
                    {
                        bool rim = hex > 0.69f;
                        bool innerRing = hex > 0.58f && hex < 0.62f;
                        if (rim) tex.SetPixel(x, y, new Color(1f, 1f, 1f, 0.95f));
                        else if (innerRing) tex.SetPixel(x, y, new Color(1f, 1f, 1f, 0.35f));
                        else tex.SetPixel(x, y, new Color(0.05f, 0.08f, 0.16f, 0.92f));
                    }
                    else
                    {
                        float glow = Mathf.Clamp01(1f - (hex - 0.78f) / 0.16f);
                        tex.SetPixel(x, y, new Color(1f, 1f, 1f, glow * glow * 0.45f));
                    }
                }
            }
            return ToSprite(tex, size);
        }

        private static Sprite CreateUpgradeIconSprite(UpgradeId id)
        {
            const int size = 96;
            var tex = MakeTex(size, size);
            float half = size * 0.5f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float nx = (x - half) / half;
                    float ny = (y - half) / half;
                    float r = Mathf.Sqrt(nx * nx + ny * ny);
                    float intensity = 0f;

                    switch (id)
                    {
                        case UpgradeId.LaserPower:
                            // Crosshair + bright laser core
                            bool ring = Mathf.Abs(r - 0.58f) < 0.065f;
                            bool cross = (Mathf.Abs(nx) < 0.055f && Mathf.Abs(ny) < 0.78f) || (Mathf.Abs(ny) < 0.055f && Mathf.Abs(nx) < 0.78f);
                            bool centerDot = r < 0.22f;
                            if (ring || cross || centerDot) intensity = 1f;
                            break;

                        case UpgradeId.FireRate:
                            // Double forward chevrons (>>)
                            float c1 = Mathf.Abs(nx + 0.18f + Mathf.Abs(ny) * 0.7f);
                            float c2 = Mathf.Abs(nx - 0.24f + Mathf.Abs(ny) * 0.7f);
                            if (Mathf.Abs(ny) < 0.56f && (c1 < 0.11f || c2 < 0.11f)) intensity = 1f;
                            break;

                        case UpgradeId.MultiLaser:
                            // Three diverging laser beams from bottom-left to top-right
                            float b1 = DistToSegment(nx, ny, -0.55f, 0f, 0.65f, 0f);
                            float b2 = DistToSegment(nx, ny, -0.55f, 0f, 0.58f, 0.52f);
                            float b3 = DistToSegment(nx, ny, -0.55f, 0f, 0.58f, -0.52f);
                            if (Mathf.Min(b1, Mathf.Min(b2, b3)) < 0.085f) intensity = 1f;
                            break;

                        case UpgradeId.PierceBeam:
                            // Long spear/railgun bolt piercing a shield ring
                            float spear = DistToSegment(nx, ny, -0.72f, -0.72f, 0.72f, 0.72f);
                            float tip1 = DistToSegment(nx, ny, 0.72f, 0.72f, 0.32f, 0.68f);
                            float tip2 = DistToSegment(nx, ny, 0.72f, 0.72f, 0.68f, 0.32f);
                            bool shieldArc = Mathf.Abs(r - 0.42f) < 0.055f && (nx * ny < 0.05f);
                            if (Mathf.Min(spear, Mathf.Min(tip1, tip2)) < 0.08f || shieldArc) intensity = 1f;
                            break;

                        case UpgradeId.OrbitalBlade:
                            // Orbit ring + 3 glowing diamonds around it
                            bool orbit = Mathf.Abs(r - 0.54f) < 0.045f;
                            bool b0 = (Mathf.Abs(nx - 0.54f) + Mathf.Abs(ny)) < 0.22f;
                            bool b1d = (Mathf.Abs(nx + 0.27f) + Mathf.Abs(ny - 0.47f)) < 0.22f;
                            bool b2d = (Mathf.Abs(nx + 0.27f) + Mathf.Abs(ny + 0.47f)) < 0.22f;
                            if (orbit || b0 || b1d || b2d) intensity = 1f;
                            break;

                        case UpgradeId.ChainLightning:
                            // Crisp jagged lightning bolt
                            float s1 = DistToSegment(nx, ny, 0.22f, 0.76f, -0.25f, 0.05f);
                            float s2 = DistToSegment(nx, ny, -0.25f, 0.05f, 0.20f, 0.05f);
                            float s3 = DistToSegment(nx, ny, 0.20f, 0.05f, -0.22f, -0.76f);
                            if (Mathf.Min(s1, Mathf.Min(s2, s3)) < 0.11f) intensity = 1f;
                            break;

                        case UpgradeId.EmpShockwave:
                            // Concentric expanding pulse rings + core
                            bool r1 = Mathf.Abs(r - 0.72f) < 0.06f;
                            bool r2 = Mathf.Abs(r - 0.44f) < 0.06f;
                            bool core = r < 0.18f;
                            if (r1 || r2 || core) intensity = 1f;
                            break;

                        case UpgradeId.CritSystem:
                            // 4-point starburst diamond
                            float star = Mathf.Min(
                                Mathf.Abs(nx) * 2.6f + Mathf.Abs(ny) * 0.75f,
                                Mathf.Abs(ny) * 2.6f + Mathf.Abs(nx) * 0.75f
                            );
                            if (star < 0.62f || r < 0.24f) intensity = 1f;
                            break;

                        case UpgradeId.CyberDash:
                            // High-speed wing arrow + speed streaks
                            float arrow = DistToSegment(nx, ny, -0.1f, 0f, 0.68f, 0f);
                            float h1 = DistToSegment(nx, ny, 0.68f, 0f, 0.25f, 0.42f);
                            float h2 = DistToSegment(nx, ny, 0.68f, 0f, 0.25f, -0.42f);
                            float t1 = DistToSegment(nx, ny, -0.72f, 0.26f, -0.18f, 0.26f);
                            float t2 = DistToSegment(nx, ny, -0.62f, 0f, -0.22f, 0f);
                            float t3 = DistToSegment(nx, ny, -0.72f, -0.26f, -0.18f, -0.26f);
                            if (Mathf.Min(Mathf.Min(arrow, Mathf.Min(h1, h2)), Mathf.Min(t1, Mathf.Min(t2, t3))) < 0.085f) intensity = 1f;
                            break;

                        case UpgradeId.MagnetField:
                            // U-shaped cyber magnet
                            bool arc = ny <= 0f && Mathf.Abs(r - 0.46f) < 0.14f;
                            bool leftArm = ny > 0f && ny < 0.56f && Mathf.Abs(nx + 0.46f) < 0.14f;
                            bool rightArm = ny > 0f && ny < 0.56f && Mathf.Abs(nx - 0.46f) < 0.14f;
                            if (arc || leftArm || rightArm) intensity = 1f;
                            break;

                        case UpgradeId.NanoRepair:
                            // Bold cyber medic plus
                            bool vBar = Mathf.Abs(nx) < 0.21f && Mathf.Abs(ny) < 0.64f;
                            bool hBar = Mathf.Abs(ny) < 0.21f && Mathf.Abs(nx) < 0.64f;
                            if (vBar || hBar) intensity = 1f;
                            break;
                    }

                    if (intensity > 0f)
                    {
                        tex.SetPixel(x, y, Color.white);
                    }
                    else
                    {
                        float softGlow = Mathf.Exp(-r * r * 3.8f) * 0.22f;
                        tex.SetPixel(x, y, new Color(1f, 1f, 1f, softGlow));
                    }
                }
            }
            return ToSprite(tex, size);
        }

        private static float DistToSegment(float px, float py, float ax, float ay, float bx, float by)
        {
            float dx = bx - ax;
            float dy = by - ay;
            float lenSq = dx * dx + dy * dy;
            float t = lenSq > 0.0001f ? Mathf.Clamp01(((px - ax) * dx + (py - ay) * dy) / lenSq) : 0f;
            float cx = ax + t * dx;
            float cy = ay + t * dy;
            return Mathf.Sqrt((px - cx) * (px - cx) + (py - cy) * (py - cy));
        }

        private static Sprite CreatePlayerSprite()
        {
            const int size = 128;
            var tex = MakeTex(size, size);
            var cyan = new Color(0f, 0.95f, 1f, 1f);
            var deepHull = new Color(0.06f, 0.22f, 0.36f, 1f);
            var core = new Color(0.92f, 1f, 1f, 1f);
            float half = size * 0.5f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float nx = (x - half) / half;
                    float ny = (y - half) / half;
                    float ay = Mathf.Abs(ny);

                    float mainBody = Mathf.Max(ay * 1.45f + nx * 0.55f - 0.58f, -nx - 0.52f);
                    float wing = Mathf.Max(ay - 0.74f, Mathf.Max(nx + 0.05f, -nx - 0.56f + ay * 0.45f));
                    float ship = Mathf.Min(mainBody, wing);

                    bool engineNotch = nx < -0.34f && ay < 0.16f;
                    float cockpit = Mathf.Sqrt((nx - 0.08f) * (nx - 0.08f) * 3.5f + ny * ny * 12f);

                    Color c = Color.clear;
                    if (ship < 0f && !engineNotch)
                    {
                        bool neonEdge = ship > -0.075f;
                        bool wingStripe = ay > 0.36f && ay < 0.44f && nx < 0.05f && nx > -0.42f;
                        if (cockpit < 0.36f)
                            c = Color.Lerp(cyan * 1.3f, core, 1f - cockpit / 0.36f);
                        else if (neonEdge || wingStripe)
                            c = cyan * 1.35f;
                        else
                            c = Color.Lerp(deepHull, cyan * 0.65f, Mathf.Clamp01((nx + 0.5f) * 0.6f));
                        c.a = 1f;
                    }
                    else
                    {
                        float dist = Mathf.Sqrt(nx * nx + ny * ny);
                        float glow = Mathf.Exp(-dist * dist * 4.2f) * 0.48f;
                        c = new Color(cyan.r, cyan.g, cyan.b, glow);
                    }
                    tex.SetPixel(x, y, c);
                }
            }
            return ToSprite(tex, 108f);
        }

        private static Sprite CreatePhantomShipSprite()
        {
            const int size = 128;
            var tex = MakeTex(size, size);
            var neon = new Color(0.9f, 0.22f, 1f, 1f);
            var hull = new Color(0.14f, 0.04f, 0.24f, 1f);
            var core = new Color(1f, 0.9f, 1f, 1f);
            float half = size * 0.5f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float nx = (x - half) / half;
                    float ny = (y - half) / half;
                    float ay = Mathf.Abs(ny);

                    // Razor-sharp swept delta assassin interceptor pointing +X
                    float needle = Mathf.Max(ay * 2.1f + nx * 0.85f - 0.65f, -nx - 0.62f);
                    float sweptWing = Mathf.Max(ay - 0.78f, Mathf.Max(nx + 0.12f - ay * 0.45f, -nx - 0.68f + ay * 0.6f));
                    float ship = Mathf.Min(needle, sweptWing);
                    bool cutout = nx < -0.35f && ay < 0.26f;

                    Color c = Color.clear;
                    if (ship < 0f && !cutout)
                    {
                        bool rim = ship > -0.07f;
                        float visor = Mathf.Sqrt((nx - 0.15f) * (nx - 0.15f) * 4.5f + ny * ny * 16f);
                        if (visor < 0.32f) c = Color.Lerp(neon * 1.3f, core, 1f - visor / 0.32f);
                        else if (rim) c = neon * 1.35f;
                        else c = Color.Lerp(hull, neon * 0.55f, Mathf.Clamp01((nx + 0.6f) * 0.5f));
                        c.a = 1f;
                    }
                    else
                    {
                        float dist = Mathf.Sqrt(nx * nx + ny * ny);
                        c = new Color(neon.r, neon.g, neon.b, Mathf.Exp(-dist * dist * 4.5f) * 0.45f);
                    }
                    tex.SetPixel(x, y, c);
                }
            }
            return ToSprite(tex, 108f);
        }

        private static Sprite CreateTitanShipSprite()
        {
            const int size = 128;
            var tex = MakeTex(size, size);
            var gold = new Color(1f, 0.74f, 0.15f, 1f);
            var armor = new Color(0.22f, 0.12f, 0.05f, 1f);
            var core = new Color(1f, 0.96f, 0.82f, 1f);
            float half = size * 0.5f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float nx = (x - half) / half;
                    float ny = (y - half) / half;
                    float ay = Mathf.Abs(ny);

                    // Heavy armored dual-cannon dreadnought pointing +X
                    float hullBox = Mathf.Max(Mathf.Abs(nx) - 0.58f, ay - 0.54f);
                    float nose = Mathf.Max(ay * 1.15f + nx * 0.55f - 0.72f, -nx - 0.58f);
                    float cannons = Mathf.Max(Mathf.Abs(ay - 0.46f) - 0.11f, Mathf.Max(-nx - 0.1f, nx - 0.72f));
                    float ship = Mathf.Min(Mathf.Max(hullBox, nose), cannons);

                    Color c = Color.clear;
                    if (ship < 0f)
                    {
                        bool rim = ship > -0.08f;
                        bool reactor = Mathf.Sqrt(nx * nx + ny * ny) < 0.22f;
                        if (reactor) c = core;
                        else if (rim || cannons < 0f) c = gold * 1.3f;
                        else c = Color.Lerp(armor, gold * 0.45f, 0.35f);
                        c.a = 1f;
                    }
                    else
                    {
                        float dist = Mathf.Sqrt(nx * nx + ny * ny);
                        c = new Color(gold.r, gold.g, gold.b, Mathf.Exp(-dist * dist * 3.8f) * 0.48f);
                    }
                    tex.SetPixel(x, y, c);
                }
            }
            return ToSprite(tex, 102f);
        }

        private static Sprite CreateCyberRobotSprite(int size, Color neon, Color core, int sides)
        {
            var tex = MakeTex(size, size);
            var armor = new Color(0.08f, 0.06f, 0.14f, 1f);
            float half = size * 0.5f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float nx = (x - half) / half;
                    float ny = (y - half) / half;
                    float r = Mathf.Sqrt(nx * nx + ny * ny);
                    float a = Mathf.Atan2(ny, nx);

                    float sector = Mathf.PI * 2f / sides;
                    float polyDist = r * Mathf.Cos(Mathf.Repeat(a + sector * 0.5f, sector) - sector * 0.5f);

                    Color c;
                    if (polyDist < 0.72f)
                    {
                        bool outerRim = polyDist > 0.58f;
                        bool eyeCore = r < 0.24f;
                        bool sensorRing = r > 0.32f && r < 0.38f;
                        if (eyeCore) c = Color.Lerp(neon * 1.3f, core, 1f - r / 0.24f);
                        else if (outerRim || sensorRing) c = neon * 1.25f;
                        else c = Color.Lerp(armor, neon * 0.32f, 0.35f);
                        c.a = 1f;
                    }
                    else
                    {
                        float glow = Mathf.Clamp01(1f - (polyDist - 0.72f) / 0.34f);
                        c = new Color(neon.r, neon.g, neon.b, glow * glow * 0.55f);
                    }
                    tex.SetPixel(x, y, c);
                }
            }
            return ToSprite(tex, size * 0.85f);
        }

        private static Sprite CreateSniperSprite(int size)
        {
            var tex = MakeTex(size, size);
            var neon = new Color(1f, 0.18f, 0.22f, 1f);
            var accent = new Color(1f, 0.85f, 0.25f, 1f);
            var armor = new Color(0.12f, 0.05f, 0.08f, 1f);
            float half = size * 0.5f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float nx = (x - half) / half;
                    float ny = (y - half) / half;
                    float ay = Mathf.Abs(ny);

                    float body = Mathf.Max(ay * 1.9f + Mathf.Abs(nx) * 0.55f - 0.68f, -nx - 0.62f);
                    float barrel = Mathf.Max(ay - 0.11f, Mathf.Max(-nx, nx - 0.86f));
                    float d = Mathf.Min(body, barrel);

                    Color c;
                    if (d < 0f)
                    {
                        bool rim = d > -0.07f;
                        float lens = Mathf.Sqrt((nx - 0.1f) * (nx - 0.1f) + ny * ny);
                        if (lens < 0.18f) c = accent;
                        else if (rim || barrel < 0f) c = neon * 1.3f;
                        else c = armor;
                        c.a = 1f;
                    }
                    else
                    {
                        float glow = Mathf.Clamp01(1f - d / 0.28f);
                        c = new Color(neon.r, neon.g, neon.b, glow * glow * 0.5f);
                    }
                    tex.SetPixel(x, y, c);
                }
            }
            return ToSprite(tex, size * 0.88f);
        }

        private static Sprite CreateBossCoreSprite(int size)
        {
            var tex = MakeTex(size, size);
            var armor = new Color(0.08f, 0.04f, 0.12f, 1f);
            float half = size * 0.5f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float nx = (x - half) / half;
                    float ny = (y - half) / half;
                    float r = Mathf.Sqrt(nx * nx + ny * ny);
                    float a = Mathf.Atan2(ny, nx);

                    float spikes = 0.72f + 0.12f * Mathf.Cos(a * 8f);
                    Color c;
                    if (r < spikes)
                    {
                        bool outerEdge = r > spikes - 0.08f;
                        bool innerRing = r > 0.36f && r < 0.43f;
                        bool spokes = Mathf.Abs(Mathf.Sin(a * 4f)) < 0.12f && r > 0.28f && r < spikes - 0.08f;
                        if (r < 0.24f) c = Color.Lerp(new Color(1f, 0.95f, 0.9f), Color.white, 1f - r / 0.24f);
                        else if (outerEdge || innerRing || spokes) c = Color.white;
                        else c = armor;
                        c.a = 1f;
                    }
                    else
                    {
                        float glow = Mathf.Clamp01(1f - (r - spikes) / 0.22f);
                        c = new Color(1f, 1f, 1f, glow * glow * 0.55f);
                    }
                    tex.SetPixel(x, y, c);
                }
            }
            return ToSprite(tex, size * 0.65f);
        }

        private static Sprite CreateBossAuraSprite(int size)
        {
            var tex = MakeTex(size, size);
            float half = size * 0.5f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float nx = (x - half) / half;
                    float ny = (y - half) / half;
                    float r = Mathf.Sqrt(nx * nx + ny * ny);
                    float a = Mathf.Atan2(ny, nx);

                    bool dashedRing = r > 0.78f && r < 0.90f && Mathf.Sin(a * 6f) > -0.25f;
                    bool thinRing = r > 0.66f && r < 0.70f;
                    float alpha = dashedRing ? 0.85f : (thinRing ? 0.45f : 0f);
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }
            return ToSprite(tex, size * 0.55f);
        }

        private static Sprite CreatePlasmaOrbSprite(int size)
        {
            var tex = MakeTex(size, size);
            float half = size * 0.5f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float nx = (x - half) / half;
                    float ny = (y - half) / half;
                    float d = Mathf.Sqrt(nx * nx + ny * ny);
                    if (d < 0.5f)
                    {
                        Color c = Color.Lerp(Color.white, new Color(1f, 0.9f, 0.9f), d / 0.5f);
                        c.a = 1f;
                        tex.SetPixel(x, y, c);
                    }
                    else
                    {
                        float g = Mathf.Clamp01(1f - (d - 0.5f) / 0.5f);
                        tex.SetPixel(x, y, new Color(1f, 1f, 1f, g * g * 0.8f));
                    }
                }
            }
            return ToSprite(tex, size * 0.9f);
        }

        private static Sprite CreateDiamondSprite(int size, Color outer, Color inner)
        {
            var tex = MakeTex(size, size);
            float half = size * 0.5f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float nx = Mathf.Abs(x - half) / half;
                    float ny = Mathf.Abs(y - half) / half;
                    float d = nx + ny;

                    Color c;
                    if (d < 0.72f)
                    {
                        float t = Mathf.Clamp01((0.72f - d) / 0.35f);
                        c = Color.Lerp(outer, inner, t * 0.75f);
                        c.a = 1f;
                    }
                    else
                    {
                        float glow = Mathf.Clamp01(1f - (d - 0.72f) / 0.45f);
                        c = new Color(outer.r, outer.g, outer.b, glow * glow * 0.55f);
                    }
                    tex.SetPixel(x, y, c);
                }
            }
            return ToSprite(tex, size * 0.85f);
        }

        private static Sprite CreateTriangleSprite(int size, Color outer, Color inner)
        {
            var tex = MakeTex(size, size);
            var armor = new Color(0.12f, 0.06f, 0.04f, 1f);
            float half = size * 0.5f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float nx = (x - half) / half;
                    float ny = (y - half) / half;
                    float d = Mathf.Max(Mathf.Abs(ny) * 1.3f + nx * 0.6f - 0.48f, -nx - 0.55f);

                    Color c;
                    if (d < 0f)
                    {
                        bool rim = d > -0.09f;
                        float coreDist = Mathf.Sqrt(nx * nx + ny * ny);
                        c = coreDist < 0.22f ? inner : (rim ? outer * 1.3f : armor);
                        c.a = 1f;
                    }
                    else
                    {
                        float glow = Mathf.Clamp01(1f - d / 0.38f);
                        c = new Color(outer.r, outer.g, outer.b, glow * glow * 0.55f);
                    }
                    tex.SetPixel(x, y, c);
                }
            }
            return ToSprite(tex, size * 0.9f);
        }

        private static Sprite CreateLaserSprite()
        {
            const int w = 64;
            const int h = 24;
            var tex = MakeTex(w, h);
            var cyan = new Color(0.15f, 0.95f, 1f, 1f);

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float nx = (x - w * 0.5f) / (w * 0.5f);
                    float ny = (y - h * 0.5f) / (h * 0.5f);
                    float capsule = Mathf.Sqrt(Mathf.Max(0f, Mathf.Abs(nx) - 0.5f) * Mathf.Max(0f, Mathf.Abs(nx) - 0.5f) * 3f + ny * ny);
                    float alpha = Mathf.Clamp01(1f - capsule);
                    Color c = Color.Lerp(cyan, Color.white, Mathf.Pow(alpha, 2.2f));
                    c.a = Mathf.Pow(alpha, 1.3f);
                    tex.SetPixel(x, y, c);
                }
            }
            return ToSprite(tex, 64f);
        }

        private static Sprite CreateCircleSprite(int size)
        {
            var tex = MakeTex(size, size);
            float half = size * 0.5f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float nx = (x - half) / half;
                    float ny = (y - half) / half;
                    float d = Mathf.Sqrt(nx * nx + ny * ny);
                    float a = Mathf.Clamp01((1f - d) * 10f);
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
                }
            }
            return ToSprite(tex, size);
        }

        private static Sprite CreateRingSprite(int size, float innerRadius, float outerRadius)
        {
            var tex = MakeTex(size, size);
            float half = size * 0.5f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float nx = (x - half) / half;
                    float ny = (y - half) / half;
                    float d = Mathf.Sqrt(nx * nx + ny * ny);
                    float a = 0f;
                    if (d >= innerRadius && d <= outerRadius)
                    {
                        float mid = (innerRadius + outerRadius) * 0.5f;
                        float halfW = (outerRadius - innerRadius) * 0.5f;
                        a = Mathf.Clamp01(1f - Mathf.Abs(d - mid) / halfW);
                        a = Mathf.Pow(a, 0.5f);
                    }
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
                }
            }
            return ToSprite(tex, size);
        }

        private static Sprite CreateGridTileSprite(int size)
        {
            var tex = MakeTex(size, size, FilterMode.Bilinear, TextureWrapMode.Repeat);
            var bg = new Color(0.025f, 0.032f, 0.07f, 1f);
            var gridLine = new Color(0.18f, 0.24f, 0.34f, 1f);
            var subLine = new Color(0.06f, 0.09f, 0.16f, 1f);
            var nodeDot = new Color(0.35f, 0.45f, 0.6f, 1f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool corner = x < 4 && y < 4;
                    bool mainBorder = x < 2 || y < 2;
                    bool midLine = (x == size / 2) || (y == size / 2);
                    Color c = corner ? nodeDot : (mainBorder ? gridLine : (midLine ? subLine : bg));
                    tex.SetPixel(x, y, c);
                }
            }
            return ToSprite(tex, 64f, SpriteMeshType.FullRect);
        }

        private static Sprite CreateSolidRectSprite()
        {
            var tex = MakeTex(4, 4, FilterMode.Point);
            for (int y = 0; y < 4; y++)
                for (int x = 0; x < 4; x++)
                    tex.SetPixel(x, y, Color.white);
            return ToSprite(tex, 4f, SpriteMeshType.FullRect);
        }

        /// <summary>
        /// Sleek Cyberpunk angled/parallelogram energy bar (track or segmented glowing fill).
        /// </summary>
        private static Sprite CreateSlantedBarSprite(int w, int h, bool isFill)
        {
            var tex = MakeTex(w, h);
            float slant = h * 0.48f;

            for (int y = 0; y < h; y++)
            {
                float tY = (float)y / h;
                float leftEdge = (1f - tY) * slant;
                float rightEdge = w - tY * slant;

                for (int x = 0; x < w; x++)
                {
                    if (x < leftEdge || x > rightEdge)
                    {
                        tex.SetPixel(x, y, Color.clear);
                        continue;
                    }

                    if (isFill)
                    {
                        // Subtle vertical energy cell segments + glossy top highlight
                        float cellPos = Mathf.Repeat(x + y * 0.48f, 22f);
                        bool gap = cellPos < 2.2f;
                        float gloss = tY > 0.62f ? 1.18f : (tY < 0.25f ? 0.78f : 1f);
                        float alpha = gap ? 0.28f : 1f;
                        tex.SetPixel(x, y, new Color(gloss, gloss, gloss, alpha));
                    }
                    else
                    {
                        bool border = (y <= 1 || y >= h - 2 || x - leftEdge < 2f || rightEdge - x < 2f);
                        if (border)
                            tex.SetPixel(x, y, new Color(0.28f, 0.55f, 0.72f, 0.72f));
                        else
                            tex.SetPixel(x, y, new Color(0.03f, 0.055f, 0.11f, 0.84f));
                    }
                }
            }
            return ToSprite(tex, 64f, SpriteMeshType.FullRect);
        }

        /// <summary>
        /// Clean chamfered translucent HUD badge for score/telemetry (no harsh thick outlines).
        /// </summary>
        private static Sprite CreateChamferedPillSprite(int w, int h)
        {
            var tex = MakeTex(w, h);
            float cut = 14f;

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    bool cutTL = (x + (h - 1 - y)) < cut;
                    bool cutBR = ((w - 1 - x) + y) < cut;
                    if (cutTL || cutBR)
                    {
                        tex.SetPixel(x, y, Color.clear);
                        continue;
                    }

                    bool topLine = y >= h - 2;
                    bool botLine = y <= 1;
                    bool leftAccent = x < 4;
                    if (leftAccent)
                        tex.SetPixel(x, y, new Color(1f, 1f, 1f, 0.9f));
                    else if (topLine || botLine)
                        tex.SetPixel(x, y, new Color(1f, 1f, 1f, 0.22f));
                    else
                    {
                        float grad = 1f - ((float)x / w) * 0.35f;
                        tex.SetPixel(x, y, new Color(0.04f, 0.07f, 0.14f, 0.78f * grad));
                    }
                }
            }
            return ToSprite(tex, 64f, SpriteMeshType.FullRect);
        }

        /// <summary>
        /// Crisp geometric cyber-heart/shield icon for the HP bar.
        /// </summary>
        private static Sprite CreateHeartIconSprite(int size)
        {
            var tex = MakeTex(size, size);
            float half = size * 0.5f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float nx = (x - half) / (half * 0.75f);
                    float ny = (y - half) / (half * 0.75f) + 0.12f;
                    float ax = Mathf.Abs(nx);
                    // Clean geometric cyber heart
                    float d = Mathf.Pow(nx * nx + ny * ny - 0.65f, 3f) - nx * nx * ny * ny * ny * 1.4f;
                    if (d < 0f)
                    {
                        float core = Mathf.Clamp01(1f - Mathf.Sqrt(ax * ax + ny * ny) * 0.6f);
                        tex.SetPixel(x, y, Color.Lerp(new Color(1f, 0.2f, 0.48f, 1f), new Color(1f, 0.8f, 0.9f, 1f), core * 0.5f));
                    }
                    else
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                }
            }
            return ToSprite(tex, size);
        }
    }
}
