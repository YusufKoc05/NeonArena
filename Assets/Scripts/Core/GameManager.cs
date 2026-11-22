using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NeonArena
{
    public struct UpgradeOption
    {
        public UpgradeId Id;
        public UpgradeRarity Rarity;
        public string CategoryTag;
        public string Title;
        public string Description;
        public string StatDelta;
        public Color AccentColor;
        public Action<PlayerController> Apply;
    }

    /// <summary>
    /// Coordinates story sectors, high-level Final Bosses, rarity-tiered upgrades,
    /// combo multiplier, rerolls, pause menu, and score persistence.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public bool IsInMenu { get; private set; } = true;
        public bool IsPaused { get; private set; } = false;
        public bool IsPlaying { get; private set; } = false;
        public float ElapsedTime { get; private set; }

        public int CurrentSector { get; private set; } = 1;
        public int SectorKills { get; private set; }
        public int SectorKillGoal => 18 + (CurrentSector - 1) * 10;
        public float SectorProgressRatio => Mathf.Clamp01((float)SectorKills / Mathf.Max(1, SectorKillGoal));

        public bool IsBossActive { get; private set; }
        public bool ShouldSpawnBoss => IsPlaying && !IsBossActive && SectorKills >= SectorKillGoal;

        public int Score { get; private set; }
        public int Kills { get; private set; }
        public int BossesDefeated { get; private set; }
        public int BestScore { get; private set; }
        public int SessionCredits { get; private set; }
        public bool HasRevivedThisRun { get; private set; }
        public bool HasDoubledCreditsThisRun { get; private set; }

        // Combo system
        public int ComboCount { get; private set; }
        public int ComboMultiplier => ComboCount >= 25 ? 3 : (ComboCount >= 10 ? 2 : 1);
        private float _comboTimer;

        // Reroll charges for Level-Up cards
        public int RerollsRemaining { get; private set; } = 3;

        private const string BestScoreKey = "NeonArena_BestScore";
        private readonly List<UpgradeOption> _upgradePool = new List<UpgradeOption>();

        private void Awake()
        {
            Instance = this;
            Time.timeScale = 1f;
            Application.targetFrameRate = 60;

            Screen.autorotateToPortrait = false;
            Screen.autorotateToPortraitUpsideDown = false;
            Screen.autorotateToLandscapeLeft = true;
            Screen.autorotateToLandscapeRight = true;
            Screen.orientation = ScreenOrientation.AutoRotation;

            BestScore = PlayerPrefs.GetInt(BestScoreKey, 0);
            BuildUpgradePool();
        }

        private void Start()
        {
            ApplyCurrentSectorVisuals();
            NeonHUD.Instance?.ShowMainMenu(BestScore);
        }

        private void Update()
        {
            if (!IsPlaying) return;
            ElapsedTime += Time.deltaTime;

            if (_comboTimer > 0f)
            {
                _comboTimer -= Time.deltaTime;
                if (_comboTimer <= 0f) ComboCount = 0;
            }
        }

        public void StartMission()
        {
            PlayerController.Instance?.ApplyShipAndMetaStats();
            IsInMenu = false;
            IsPaused = false;
            IsPlaying = true;
            Time.timeScale = 1f;
            NeonHUD.Instance?.HideMainMenu();
            NeonHUD.Instance?.RefreshBuildStrip();
            ApplyCurrentSectorVisuals();

            string shipName = NeonMetaProgression.GetShipDef(NeonMetaProgression.SelectedShip).Name;
            NeonHUD.Instance?.ShowRadioMessage(
                "NOVA // İSTİHBARAT",
                $"Pilot {shipName}, yıl 2147. OMEGA-CORE Neo-İstanbul'u kilitledi. Sektör 1'i temizle ve Muhafız SENTINEL-X'i indir!",
                new Color(0f, 0.95f, 1f),
                6.5f
            );
        }

        public void TogglePause()
        {
            if (IsInMenu || (PlayerController.Instance != null && !PlayerController.Instance.IsAlive)) return;
            if (!IsPlaying && !IsPaused) return; // Don't toggle during Level-Up modal

            IsPaused = !IsPaused;
            IsPlaying = !IsPaused;
            Time.timeScale = IsPaused ? 0f : 1f;
            NeonHUD.Instance?.SetPauseVisible(IsPaused);
        }

        public string GetSectorTitle(int sector)
        {
            switch (sector)
            {
                case 1: return "SEKTÖR 1: DIŞ ÇEMBER";
                case 2: return "SEKTÖR 2: PLAZMA REAKTÖRÜ";
                case 3: return "SEKTÖR 3: OMEGA ÇEKİRDEĞİ";
                default: return $"SEKTÖR {sector}: SONSUZ PROTOKOL";
            }
        }

        public Color GetSectorThemeColor(int sector)
        {
            switch (sector)
            {
                case 1: return new Color(0f, 0.92f, 1f);
                case 2: return new Color(0.82f, 0.25f, 1f);
                case 3: return new Color(1f, 0.28f, 0.18f);
                default: return new Color(1f, 0.78f, 0.15f);
            }
        }

        private void ApplyCurrentSectorVisuals()
        {
            NeonArenaBootstrap.Instance?.ApplySectorTheme(GetSectorThemeColor(CurrentSector));
        }

        public void OnEnemyKilled(int scoreValue, bool wasBoss)
        {
            Kills++;
            ComboCount++;
            _comboTimer = 3.2f;

            Score += scoreValue * ComboMultiplier;
            int crEarned = Mathf.Max(1, Mathf.RoundToInt((wasBoss ? 75 : 2) * NeonMetaProgression.CreditMultiplier));
            SessionCredits += crEarned;
            NeonMetaProgression.AddCredits(crEarned);

            if (Score > BestScore)
            {
                BestScore = Score;
                PlayerPrefs.SetInt(BestScoreKey, BestScore);
            }

            if (wasBoss) OnBossDefeated();
            else if (!IsBossActive) SectorKills++;
        }

        public void OnBossSpawned(BossAI boss)
        {
            IsBossActive = true;
            NeonHUD.Instance?.ShowBossWarning($"FİNAL BOSS: {boss.BossName} [LV.{boss.EnemyLevel}]");

            string taunt = CurrentSector == 1
                ? "İzinsiz siber birim tespit edildi. SENTINEL-X imha protokolünü başlatıyor!"
                : CurrentSector == 2
                    ? "Plazma reaktörüne kadar gelebildin... HYDRA-V9 seni burada küle çevirecek!"
                    : "Ben OMEGA-OVERLORD! İnsanlığın son pilotu burada yok olacak!";

            NeonHUD.Instance?.ShowRadioMessage($"{boss.BossName} [LV.{boss.EnemyLevel}]", taunt, new Color(1f, 0.22f, 0.42f), 5.5f);
        }

        private void OnBossDefeated()
        {
            IsBossActive = false;
            BossesDefeated++;
            RerollsRemaining++; // Bonus reroll for defeating a Boss!

            PlayerController.Instance?.Heal(40f);

            int clearedSector = CurrentSector;
            CurrentSector++;
            SectorKills = 0;
            ApplyCurrentSectorVisuals();

            string msg = clearedSector == 1
                ? "SENTINEL-X yok edildi! Plazma Reaktörü'ne geçiyorsun — keskin nişancı robotlar aktif! (+1 Kart Yenileme)"
                : clearedSector == 2
                    ? "HYDRA-V9 düştü! OMEGA Çekirdeği'ne girdin. Bölünen robotlara ve OMEGA-OVERLORD'a hazır ol!"
                    : $"TEBRİKLER VEX-09! OMEGA Çekirdeği kırıldı! Şimdi Sonsuz Protokol (Sektör {CurrentSector}) başlıyor!";

            NeonHUD.Instance?.ShowRadioMessage("NOVA // İSTİHBARAT", msg, new Color(0.15f, 1f, 0.68f), 6.5f);
        }

        public void TriggerLevelUp()
        {
            if (!IsPlaying) return;
            IsPlaying = false;
            Time.timeScale = 0f;

            var choices = PickRandomUpgrades(3);
            NeonHUD.Instance?.ShowLevelUpChoices(choices, OnUpgradeSelected, RerollsRemaining);
        }

        public void RerollUpgrades()
        {
            if (RerollsRemaining <= 0) return;
            RerollsRemaining--;
            NeonAudio.Instance?.PlayDash();
            var choices = PickRandomUpgrades(3);
            NeonHUD.Instance?.ShowLevelUpChoices(choices, OnUpgradeSelected, RerollsRemaining);
        }

        private void OnUpgradeSelected(UpgradeOption chosen)
        {
            if (PlayerController.Instance != null)
            {
                chosen.Apply?.Invoke(PlayerController.Instance);
                PlayerController.Instance.RecordUpgrade(chosen.Id);
            }
            NeonHUD.Instance?.HideLevelUp();
            NeonHUD.Instance?.RefreshBuildStrip();
            Time.timeScale = 1f;
            IsPlaying = true;
        }

        public void TriggerGameOver()
        {
            IsPlaying = false;
            IsPaused = false;
            Time.timeScale = 0f;
            PlayerPrefs.Save();
            NeonHUD.Instance?.ShowGameOver(
                ElapsedTime, CurrentSector, BossesDefeated, Kills, Score, BestScore,
                SessionCredits, !HasRevivedThisRun, !HasDoubledCreditsThisRun && SessionCredits > 0
            );
        }

        public void RequestReviveWithAd()
        {
            if (HasRevivedThisRun) return;
            NeonAdsManager.Instance?.ShowRewardedAd(() =>
            {
                HasRevivedThisRun = true;
                PlayerController.Instance?.Revive();
                NeonHUD.Instance?.HideGameOver();
                Time.timeScale = 1f;
                IsPlaying = true;
            });
        }

        public void RequestDoubleCreditsWithAd()
        {
            if (HasDoubledCreditsThisRun || SessionCredits <= 0) return;
            NeonAdsManager.Instance?.ShowRewardedAd(() =>
            {
                HasDoubledCreditsThisRun = true;
                NeonMetaProgression.AddCredits(SessionCredits);
                SessionCredits *= 2;
                NeonAudio.Instance?.PlayLevelUp();
                NeonHUD.Instance?.ShowGameOver(
                    ElapsedTime, CurrentSector, BossesDefeated, Kills, Score, BestScore,
                    SessionCredits, !HasRevivedThisRun, false
                );
            });
        }

        public void RestartGame()
        {
            NeonAdsManager.Instance?.NotifyRunEndedAndMaybeShowInterstitial();
            Time.timeScale = 1f;
            EnemyAI.ActiveEnemies.Clear();
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private List<UpgradeOption> PickRandomUpgrades(int count)
        {
            var copy = new List<UpgradeOption>(_upgradePool);
            var picked = new List<UpgradeOption>();
            for (int i = 0; i < count && copy.Count > 0; i++)
            {
                int idx = UnityEngine.Random.Range(0, copy.Count);
                picked.Add(copy[idx]);
                copy.RemoveAt(idx);
            }
            return picked;
        }

        private void BuildUpgradePool()
        {
            _upgradePool.Clear();

            Color rareCol = new Color(0.1f, 0.92f, 1f);      // Cyan
            Color epicCol = new Color(0.82f, 0.35f, 1f);     // Neon Purple
            Color legCol = new Color(1f, 0.8f, 0.18f);       // Cyber Gold

            _upgradePool.Add(new UpgradeOption
            {
                Id = UpgradeId.LaserPower,
                Rarity = UpgradeRarity.Rare,
                CategoryTag = "ANA SİLAH",
                Title = "LAZER GÜCÜ",
                Description = "Plazma çekirdeğini aşırı yükleyerek lazer hasarını artırır.",
                StatDelta = "+%28 Lazer Hasarı",
                AccentColor = rareCol,
                Apply = p => p.LaserDamage *= 1.28f
            });

            _upgradePool.Add(new UpgradeOption
            {
                Id = UpgradeId.FireRate,
                Rarity = UpgradeRarity.Rare,
                CategoryTag = "ANA SİLAH",
                Title = "HIZLI ATIŞ",
                Description = "Soğutma bobinlerini güçlendirip namlu döngüsünü hızlandırır.",
                StatDelta = "+%22 Atış Hızı",
                AccentColor = rareCol,
                Apply = p => p.FireRate *= 1.22f
            });

            _upgradePool.Add(new UpgradeOption
            {
                Id = UpgradeId.MultiLaser,
                Rarity = UpgradeRarity.Legendary,
                CategoryTag = "EFSANEVİ SİLAH",
                Title = "ÇOKLU LAZER",
                Description = "Her tetikte ek bir yüksek enerjili lazer ışını fırlatır.",
                StatDelta = "+1 Ekstra Lazer",
                AccentColor = legCol,
                Apply = p => p.ProjectileCount = Mathf.Min(7, p.ProjectileCount + 1)
            });

            _upgradePool.Add(new UpgradeOption
            {
                Id = UpgradeId.PierceBeam,
                Rarity = UpgradeRarity.Epic,
                CategoryTag = "SİLAH MODÜLÜ",
                Title = "DELİCİ IŞIN",
                Description = "Lazerlerin robot zırhlarını delip arkadaki hedeflere ulaşmasını sağlar.",
                StatDelta = "+1 Hedef Delme",
                AccentColor = epicCol,
                Apply = p => p.PierceCount++
            });

            _upgradePool.Add(new UpgradeOption
            {
                Id = UpgradeId.ChainLightning,
                Rarity = UpgradeRarity.Legendary,
                CategoryTag = "SÜPER YETENEK",
                Title = "ZİNCİR YILDIRIM",
                Description = "Düşmanlar arasında sıçrayan yüksek voltajlı neon yıldırım çakar.",
                StatDelta = "+1 Sıçrama & Şok Hasarı",
                AccentColor = legCol,
                Apply = p => p.LightningLevel++
            });

            _upgradePool.Add(new UpgradeOption
            {
                Id = UpgradeId.EmpShockwave,
                Rarity = UpgradeRarity.Epic,
                CategoryTag = "SÜPER YETENEK",
                Title = "EMP ŞOK DALGASI",
                Description = "Periyodik şok halkası yayar; robotları iter ve düşman mermilerini siler.",
                StatDelta = "+Alan & Mermi Silme",
                AccentColor = epicCol,
                Apply = p => p.EmpLevel++
            });

            _upgradePool.Add(new UpgradeOption
            {
                Id = UpgradeId.OrbitalBlade,
                Rarity = UpgradeRarity.Epic,
                CategoryTag = "SÜPER YETENEK",
                Title = "YÖRÜNGE BIÇAĞI",
                Description = "Geminin etrafında dönen keskin neon enerji bıçağı ekler.",
                StatDelta = "+1 Dönen Bıçak",
                AccentColor = epicCol,
                Apply = p => p.AddOrbitalBlade()
            });

            _upgradePool.Add(new UpgradeOption
            {
                Id = UpgradeId.CritSystem,
                Rarity = UpgradeRarity.Epic,
                CategoryTag = "NİŞAN MODÜLÜ",
                Title = "KRİTİK SİSTEM",
                Description = "Zayıf noktaları tespit ederek sarı kritik vuruşları güçlendirir.",
                StatDelta = "+%12 Kritik | +%35 Hasar",
                AccentColor = epicCol,
                Apply = p => { p.CritChance = Mathf.Min(0.75f, p.CritChance + 0.12f); p.CritMultiplier += 0.35f; }
            });

            _upgradePool.Add(new UpgradeOption
            {
                Id = UpgradeId.CyberDash,
                Rarity = UpgradeRarity.Rare,
                CategoryTag = "HAREKET",
                Title = "SİBER DASH",
                Description = "İtici motorları ve kaçış reflekslerini hızlandırır.",
                StatDelta = "-%25 Dash Süresi | +%12 Hız",
                AccentColor = rareCol,
                Apply = p => { p.DashCooldown = Mathf.Max(0.8f, p.DashCooldown * 0.75f); p.MoveSpeed *= 1.12f; }
            });

            _upgradePool.Add(new UpgradeOption
            {
                Id = UpgradeId.MagnetField,
                Rarity = UpgradeRarity.Rare,
                CategoryTag = "DESTEK",
                Title = "MIKNATIS ALANI",
                Description = "XP kristallerini çok daha uzak mesafeden kendine çeker.",
                StatDelta = "+%45 Çekim Menzili",
                AccentColor = rareCol,
                Apply = p => p.PickupRadius *= 1.45f
            });

            _upgradePool.Add(new UpgradeOption
            {
                Id = UpgradeId.NanoRepair,
                Rarity = UpgradeRarity.Rare,
                CategoryTag = "SAVUNMA",
                Title = "NANO ONARIM",
                Description = "Gövde zırhını güçlendirir ve anında nano-onarım uygular.",
                StatDelta = "+25 Maks Can | +50 İyileşme",
                AccentColor = new Color(0.18f, 1f, 0.58f),
                Apply = p => { p.MaxHealth += 25f; p.Heal(50f); }
            });
        }
    }
}
