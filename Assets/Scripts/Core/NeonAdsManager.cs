using System;
using UnityEngine;
#if ADMOB_ENABLED
using GoogleMobileAds.Api;
#endif

namespace NeonArena
{
    /// <summary>
    /// Google AdMob manager for Neon Arena.
    /// Runs in safe Test Simulator mode until the official Google Mobile Ads Unity SDK
    /// is imported and ADMOB_ENABLED is activated via NeonArena -> AdMob SDK Aktif Et.
    /// </summary>
    public class NeonAdsManager : MonoBehaviour
    {
        public static NeonAdsManager Instance { get; private set; }

        [Header("Google AdMob Android Ad Unit IDs (Varsayilan: Google Test ID'leri)")]
        // Yayinlamadan once kendi Google AdMob panelinden aldigin ID'lerle degistir!
        public string AndroidRewardedAdUnitId = "ca-app-pub-3940256099942544/5224354917";
        public string AndroidInterstitialAdUnitId = "ca-app-pub-3940256099942544/1033173712";

        private int _runsCompleted;

#if ADMOB_ENABLED
        private RewardedAd _rewardedAd;
        private InterstitialAd _interstitialAd;
#endif

        public static NeonAdsManager Create()
        {
            if (Instance != null) return Instance;
            var go = new GameObject("NeonAdsManager");
            DontDestroyOnLoad(go);
            return go.AddComponent<NeonAdsManager>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAdMob();
        }

        private void InitializeAdMob()
        {
#if ADMOB_ENABLED
            MobileAds.Initialize(_ =>
            {
                LoadRewardedAd();
                LoadInterstitialAd();
            });
#else
            Debug.Log("[NeonAdsManager] AdMob Test Simulator aktif. Gercek reklamlar icin Google Mobile Ads SDK yukleyip 'NeonArena -> AdMob SDK Aktif Et' tikla.");
#endif
        }

        public void ShowRewardedAd(Action onRewardEarned, Action onFailed = null)
        {
#if ADMOB_ENABLED
            if (_rewardedAd != null && _rewardedAd.CanShowAd())
            {
                bool rewarded = false;
                _rewardedAd.OnAdFullScreenContentClosed += () =>
                {
                    LoadRewardedAd();
                };
                _rewardedAd.Show(_ =>
                {
                    rewarded = true;
                    onRewardEarned?.Invoke();
                });
                if (!rewarded)
                {
                    // Callback fires asynchronously on reward
                }
            }
            else
            {
                LoadRewardedAd();
                onFailed?.Invoke();
            }
#else
            // Editor / Pre-SDK simulator: grants reward immediately for testing flow
            Debug.Log("[NeonAdsManager] Test Odullu Reklam izlendi -> Odul verildi!");
            onRewardEarned?.Invoke();
#endif
        }

        public void NotifyRunEndedAndMaybeShowInterstitial()
        {
            _runsCompleted++;
            if (_runsCompleted % 3 != 0) return; // Only every 3rd run so players aren't annoyed

#if ADMOB_ENABLED
            if (_interstitialAd != null && _interstitialAd.CanShowAd())
            {
                _interstitialAd.OnAdFullScreenContentClosed += LoadInterstitialAd;
                _interstitialAd.Show();
            }
            else
            {
                LoadInterstitialAd();
            }
#else
            Debug.Log("[NeonAdsManager] Test Gecis Reklami (3. oyun sonu) tetiklendi.");
#endif
        }

#if ADMOB_ENABLED
        private void LoadRewardedAd()
        {
            if (_rewardedAd != null)
            {
                _rewardedAd.Destroy();
                _rewardedAd = null;
            }
            var request = new AdRequest();
            RewardedAd.Load(AndroidRewardedAdUnitId, request, (ad, error) =>
            {
                if (error != null || ad == null) return;
                _rewardedAd = ad;
            });
        }

        private void LoadInterstitialAd()
        {
            if (_interstitialAd != null)
            {
                _interstitialAd.Destroy();
                _interstitialAd = null;
            }
            var request = new AdRequest();
            InterstitialAd.Load(AndroidInterstitialAdUnitId, request, (ad, error) =>
            {
                if (error != null || ad == null) return;
                _interstitialAd = ad;
            });
        }
#endif
    }
}
