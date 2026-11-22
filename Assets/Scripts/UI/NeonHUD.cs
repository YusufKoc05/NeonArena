using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NeonArena
{
    /// <summary>
    /// Sleek Cyberpunk HUD: slanted segmented energy bars, frameless telemetry score/combo,
    /// compact hex build strip, chamfered rarity-tiered Upgrade Cards, and clean modal isolation.
    /// </summary>
    public class NeonHUD : MonoBehaviour
    {
        public static NeonHUD Instance { get; private set; }

        public Canvas RootCanvas { get; private set; }

        private Font _font;
        private GameObject _hudRoot;

        // Top-Left Tactical Cluster (Level + HP + XP)
        private Text _levelBadgeText;
        private Image _hpFill;
        private Text _hpText;
        private Image _xpFill;

        // Top-Right Telemetry (Score + Kills + Combo)
        private Text _scoreValueText;
        private Text _killsValueText;
        private Text _comboText;

        // Top-Center Sector / Boss
        private GameObject _sectorBarGroup;
        private Text _sectorTitleText;
        private Image _sectorProgressFill;
        private Text _sectorTimerText;

        private GameObject _bossBarGroup;
        private Text _bossNameText;
        private Image _bossHpFill;
        private Text _bossHpText;

        // Banners
        private GameObject _bossWarningBanner;
        private Text _bossWarningText;
        private float _bossWarningTimer;

        private GameObject _radioPanel;
        private Text _radioSpeakerText;
        private Text _radioBodyText;
        private Image _radioAccentBar;
        private float _radioTimer;

        // Bottom Controls & Build Strip
        private Image _dashFill;
        private Text _dashLabel;
        private Transform _buildStripContainer;

        // Modals
        private GameObject _mainMenuPanel;
        private Text _menuBestScoreText;
        private Text _menuCreditsText;
        private Text _menuActiveShipText;
        private GameObject _hangarPanel;
        private Transform _hangarCardsContainer;
        private Text _hangarCreditsText;
        private GameObject _shopPanel;
        private Transform _shopRowsContainer;
        private Text _shopCreditsText;
        private Text _shopTitleText;
        private GameObject _levelUpPanel;
        private Transform _cardContainer;
        private Button _rerollBtn;
        private Text _rerollBtnText;
        private GameObject _pausePanel;
        private GameObject _gameOverPanel;
        private Text _gameOverStatsText;
        private GameObject _reviveAdBtn;
        private GameObject _doubleCrAdBtn;
        private Text _doubleCrAdLabel;

        public static NeonHUD Create()
        {
            var canvasGo = new GameObject("NeonHUD_Canvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            canvasGo.AddComponent<GraphicRaycaster>();

            var hud = canvasGo.AddComponent<NeonHUD>();
            hud.RootCanvas = canvas;
            hud.BuildUI();
            return hud;
        }

        private void Awake()
        {
            Instance = this;
        }

        private void BuildUI()
        {
            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (_font == null) _font = Font.CreateDynamicFontFromOSFont("Arial", 32);

            // All gameplay HUD lives inside _hudRoot so modals never clash or overlap with it!
            _hudRoot = new GameObject("GameplayHUD", typeof(RectTransform));
            _hudRoot.transform.SetParent(RootCanvas.transform, false);
            StretchFull(_hudRoot.GetComponent<RectTransform>());

            BuildTopLeftStatus();
            BuildTopCenterSectorAndBoss();
            BuildTopRightTelemetry();
            BuildActiveBuildStrip();
            BuildDashButton();
            BuildRadioBanner();
            BuildBossWarningBanner();

            // Modals sit cleanly on top of _hudRoot
            BuildLevelUpPanel();
            BuildPausePanel();
            BuildGameOverPanel();
            BuildMainMenuPanel();
            BuildHangarPanel();
            BuildShopPanel();
        }

        private void BuildTopLeftStatus()
        {
            var cluster = new GameObject("StatusCluster", typeof(RectTransform));
            cluster.transform.SetParent(_hudRoot.transform, false);
            var cr = cluster.GetComponent<RectTransform>();
            cr.anchorMin = new Vector2(0f, 1f);
            cr.anchorMax = new Vector2(0f, 1f);
            cr.pivot = new Vector2(0f, 1f);
            cr.anchoredPosition = new Vector2(30f, -24f);
            cr.sizeDelta = new Vector2(460f, 102f);

            // Hex Level Emblem (Left)
            var lvHex = new GameObject("LevelHex", typeof(RectTransform), typeof(Image));
            lvHex.transform.SetParent(cluster.transform, false);
            var hr = lvHex.GetComponent<RectTransform>();
            hr.anchorMin = new Vector2(0f, 0.5f);
            hr.anchorMax = new Vector2(0f, 0.5f);
            hr.pivot = new Vector2(0f, 0.5f);
            hr.anchoredPosition = new Vector2(0f, 4f);
            hr.sizeDelta = new Vector2(88f, 88f);
            var hexImg = lvHex.GetComponent<Image>();
            hexImg.sprite = NeonSpriteFactory.HexBadge;
            hexImg.color = new Color(0f, 0.95f, 1f, 0.95f);
            hexImg.raycastTarget = false;

            var lvMicro = CreateText("LvMicro", lvHex.transform, "SVY", 15, new Color(0.55f, 0.95f, 1f), TextAnchor.MiddleCenter);
            lvMicro.fontStyle = FontStyle.Bold;
            var lmr = lvMicro.rectTransform;
            lmr.anchorMin = new Vector2(0f, 0.56f);
            lmr.anchorMax = new Vector2(1f, 0.86f);
            lmr.offsetMin = Vector2.zero;
            lmr.offsetMax = Vector2.zero;

            _levelBadgeText = CreateText("LvVal", lvHex.transform, "1", 30, Color.white, TextAnchor.MiddleCenter);
            _levelBadgeText.fontStyle = FontStyle.Bold;
            AddShadow(_levelBadgeText.gameObject);
            var lvr = _levelBadgeText.rectTransform;
            lvr.anchorMin = new Vector2(0f, 0.14f);
            lvr.anchorMax = new Vector2(1f, 0.62f);
            lvr.offsetMin = Vector2.zero;
            lvr.offsetMax = Vector2.zero;

            // Heart Icon + HP Text above HP Bar
            var heartGo = new GameObject("Heart", typeof(RectTransform), typeof(Image));
            heartGo.transform.SetParent(cluster.transform, false);
            var htr = heartGo.GetComponent<RectTransform>();
            htr.anchorMin = new Vector2(0f, 1f);
            htr.anchorMax = new Vector2(0f, 1f);
            htr.pivot = new Vector2(0f, 1f);
            htr.anchoredPosition = new Vector2(102f, -4f);
            htr.sizeDelta = new Vector2(24f, 24f);
            var hImg = heartGo.GetComponent<Image>();
            hImg.sprite = NeonSpriteFactory.HeartIcon;
            hImg.raycastTarget = false;

            _hpText = CreateText("HP_Text", cluster.transform, "100 / 100", 21, Color.white, TextAnchor.MiddleLeft);
            _hpText.fontStyle = FontStyle.Bold;
            AddShadow(_hpText.gameObject);
            var htx = _hpText.rectTransform;
            htx.anchorMin = new Vector2(0f, 1f);
            htx.anchorMax = new Vector2(0f, 1f);
            htx.pivot = new Vector2(0f, 1f);
            htx.anchoredPosition = new Vector2(132f, -2f);
            htx.sizeDelta = new Vector2(220f, 28f);

            // Slanted Segmented HP Energy Bar
            var hpTrack = new GameObject("HP_Track", typeof(RectTransform), typeof(Image));
            hpTrack.transform.SetParent(cluster.transform, false);
            var hpTr = hpTrack.GetComponent<RectTransform>();
            hpTr.anchorMin = new Vector2(0f, 1f);
            hpTr.anchorMax = new Vector2(0f, 1f);
            hpTr.pivot = new Vector2(0f, 1f);
            hpTr.anchoredPosition = new Vector2(96f, -32f);
            hpTr.sizeDelta = new Vector2(320f, 30f);
            hpTrack.GetComponent<Image>().sprite = NeonSpriteFactory.HudBarTrack;
            hpTrack.GetComponent<Image>().raycastTarget = false;

            var hpFillGo = new GameObject("HP_Fill", typeof(RectTransform), typeof(Image));
            hpFillGo.transform.SetParent(hpTrack.transform, false);
            var hfr = hpFillGo.GetComponent<RectTransform>();
            hfr.anchorMin = Vector2.zero;
            hfr.anchorMax = Vector2.one;
            hfr.offsetMin = new Vector2(4f, 4f);
            hfr.offsetMax = new Vector2(-4f, -4f);
            _hpFill = hpFillGo.GetComponent<Image>();
            _hpFill.sprite = NeonSpriteFactory.HudBarFill;
            _hpFill.color = new Color(1f, 0.2f, 0.48f, 1f);
            _hpFill.type = Image.Type.Filled;
            _hpFill.fillMethod = Image.FillMethod.Horizontal;
            _hpFill.raycastTarget = false;

            // Slim Slanted Segmented XP Bar right below HP
            var xpTrack = new GameObject("XP_Track", typeof(RectTransform), typeof(Image));
            xpTrack.transform.SetParent(cluster.transform, false);
            var xpTr = xpTrack.GetComponent<RectTransform>();
            xpTr.anchorMin = new Vector2(0f, 1f);
            xpTr.anchorMax = new Vector2(0f, 1f);
            xpTr.pivot = new Vector2(0f, 1f);
            xpTr.anchoredPosition = new Vector2(92f, -68f);
            xpTr.sizeDelta = new Vector2(280f, 16f);
            xpTrack.GetComponent<Image>().sprite = NeonSpriteFactory.HudBarTrack;
            xpTrack.GetComponent<Image>().raycastTarget = false;

            var xpFillGo = new GameObject("XP_Fill", typeof(RectTransform), typeof(Image));
            xpFillGo.transform.SetParent(xpTrack.transform, false);
            var xfr = xpFillGo.GetComponent<RectTransform>();
            xfr.anchorMin = Vector2.zero;
            xfr.anchorMax = Vector2.one;
            xfr.offsetMin = new Vector2(3f, 2f);
            xfr.offsetMax = new Vector2(-3f, -2f);
            _xpFill = xpFillGo.GetComponent<Image>();
            _xpFill.sprite = NeonSpriteFactory.HudBarFill;
            _xpFill.color = new Color(0.12f, 1f, 0.68f, 1f);
            _xpFill.type = Image.Type.Filled;
            _xpFill.fillMethod = Image.FillMethod.Horizontal;
            _xpFill.fillAmount = 0f;
            _xpFill.raycastTarget = false;
        }

        private void BuildTopCenterSectorAndBoss()
        {
            // Minimal Floating Sector Telemetry (no bulky rectangle!)
            _sectorBarGroup = new GameObject("SectorCluster", typeof(RectTransform));
            _sectorBarGroup.transform.SetParent(_hudRoot.transform, false);
            var sRect = _sectorBarGroup.GetComponent<RectTransform>();
            sRect.anchorMin = new Vector2(0.5f, 1f);
            sRect.anchorMax = new Vector2(0.5f, 1f);
            sRect.pivot = new Vector2(0.5f, 1f);
            sRect.anchoredPosition = new Vector2(0f, -22f);
            sRect.sizeDelta = new Vector2(540f, 62f);

            _sectorTitleText = CreateText("SectorTitle", _sectorBarGroup.transform, "SEKTÖR 1 • DIŞ ÇEMBER", 22, new Color(0.2f, 0.98f, 1f), TextAnchor.MiddleLeft);
            _sectorTitleText.fontStyle = FontStyle.Bold;
            AddShadow(_sectorTitleText.gameObject);
            var stRect = _sectorTitleText.rectTransform;
            stRect.anchorMin = new Vector2(0f, 0.45f);
            stRect.anchorMax = new Vector2(0.75f, 1f);
            stRect.offsetMin = new Vector2(8f, 0f);
            stRect.offsetMax = Vector2.zero;

            _sectorTimerText = CreateText("SectorTimer", _sectorBarGroup.transform, "00:00", 24, Color.white, TextAnchor.MiddleRight);
            _sectorTimerText.fontStyle = FontStyle.Bold;
            AddShadow(_sectorTimerText.gameObject);
            var tmRect = _sectorTimerText.rectTransform;
            tmRect.anchorMin = new Vector2(0.75f, 0.45f);
            tmRect.anchorMax = new Vector2(1f, 1f);
            tmRect.offsetMin = Vector2.zero;
            tmRect.offsetMax = new Vector2(-8f, 0f);

            var progTrack = new GameObject("ProgTrack", typeof(RectTransform), typeof(Image));
            progTrack.transform.SetParent(_sectorBarGroup.transform, false);
            var ptRect = progTrack.GetComponent<RectTransform>();
            ptRect.anchorMin = new Vector2(0f, 0f);
            ptRect.anchorMax = new Vector2(1f, 0f);
            ptRect.pivot = new Vector2(0.5f, 0f);
            ptRect.anchoredPosition = new Vector2(0f, 6f);
            ptRect.sizeDelta = new Vector2(0f, 16f);
            progTrack.GetComponent<Image>().sprite = NeonSpriteFactory.HudBarTrack;
            progTrack.GetComponent<Image>().raycastTarget = false;

            var progFillGo = new GameObject("ProgFill", typeof(RectTransform), typeof(Image));
            progFillGo.transform.SetParent(progTrack.transform, false);
            var pfRect = progFillGo.GetComponent<RectTransform>();
            pfRect.anchorMin = Vector2.zero;
            pfRect.anchorMax = Vector2.one;
            pfRect.offsetMin = new Vector2(3f, 2f);
            pfRect.offsetMax = new Vector2(-3f, -2f);
            _sectorProgressFill = progFillGo.GetComponent<Image>();
            _sectorProgressFill.sprite = NeonSpriteFactory.HudBarFill;
            _sectorProgressFill.color = new Color(0f, 0.92f, 1f, 1f);
            _sectorProgressFill.type = Image.Type.Filled;
            _sectorProgressFill.fillMethod = Image.FillMethod.Horizontal;
            _sectorProgressFill.raycastTarget = false;

            // Dramatic Slanted Boss Health Bar
            _bossBarGroup = new GameObject("BossCluster", typeof(RectTransform));
            _bossBarGroup.transform.SetParent(_hudRoot.transform, false);
            var bRect = _bossBarGroup.GetComponent<RectTransform>();
            bRect.anchorMin = new Vector2(0.5f, 1f);
            bRect.anchorMax = new Vector2(0.5f, 1f);
            bRect.pivot = new Vector2(0.5f, 1f);
            bRect.anchoredPosition = new Vector2(0f, -18f);
            bRect.sizeDelta = new Vector2(680f, 76f);

            _bossNameText = CreateText("BossName", _bossBarGroup.transform, "SENTINEL-X  [LV.10]", 26, new Color(1f, 0.86f, 0.25f), TextAnchor.MiddleLeft);
            _bossNameText.fontStyle = FontStyle.Bold;
            AddShadow(_bossNameText.gameObject);
            var bnRect = _bossNameText.rectTransform;
            bnRect.anchorMin = new Vector2(0f, 0.52f);
            bnRect.anchorMax = new Vector2(0.75f, 1f);
            bnRect.offsetMin = new Vector2(10f, 0f);
            bnRect.offsetMax = Vector2.zero;

            _bossHpText = CreateText("BossHpTxt", _bossBarGroup.transform, "100%", 22, new Color(1f, 0.65f, 0.72f), TextAnchor.MiddleRight);
            _bossHpText.fontStyle = FontStyle.Bold;
            AddShadow(_bossHpText.gameObject);
            var bhtRect = _bossHpText.rectTransform;
            bhtRect.anchorMin = new Vector2(0.75f, 0.52f);
            bhtRect.anchorMax = new Vector2(1f, 1f);
            bhtRect.offsetMin = Vector2.zero;
            bhtRect.offsetMax = new Vector2(-10f, 0f);

            var bossTrack = new GameObject("BossTrack", typeof(RectTransform), typeof(Image));
            bossTrack.transform.SetParent(_bossBarGroup.transform, false);
            var btRect = bossTrack.GetComponent<RectTransform>();
            btRect.anchorMin = new Vector2(0f, 0f);
            btRect.anchorMax = new Vector2(1f, 0f);
            btRect.pivot = new Vector2(0.5f, 0f);
            btRect.anchoredPosition = new Vector2(0f, 6f);
            btRect.sizeDelta = new Vector2(0f, 28f);
            var btImg = bossTrack.GetComponent<Image>();
            btImg.sprite = NeonSpriteFactory.HudBarTrack;
            btImg.color = new Color(1f, 0.35f, 0.45f, 1f);
            btImg.raycastTarget = false;

            var bossFillGo = new GameObject("BossFill", typeof(RectTransform), typeof(Image));
            bossFillGo.transform.SetParent(bossTrack.transform, false);
            var bfRect = bossFillGo.GetComponent<RectTransform>();
            bfRect.anchorMin = Vector2.zero;
            bfRect.anchorMax = Vector2.one;
            bfRect.offsetMin = new Vector2(4f, 4f);
            bfRect.offsetMax = new Vector2(-4f, -4f);
            _bossHpFill = bossFillGo.GetComponent<Image>();
            _bossHpFill.sprite = NeonSpriteFactory.HudBarFill;
            _bossHpFill.color = new Color(1f, 0.16f, 0.36f, 1f);
            _bossHpFill.type = Image.Type.Filled;
            _bossHpFill.fillMethod = Image.FillMethod.Horizontal;
            _bossHpFill.raycastTarget = false;

            _bossBarGroup.SetActive(false);
        }

        private void BuildTopRightTelemetry()
        {
            // Sleek Hex Pause Button (Far Top-Right)
            var pauseGo = new GameObject("PauseBtn", typeof(RectTransform), typeof(Image), typeof(Button));
            pauseGo.transform.SetParent(_hudRoot.transform, false);
            var pRect = pauseGo.GetComponent<RectTransform>();
            pRect.anchorMin = new Vector2(1f, 1f);
            pRect.anchorMax = new Vector2(1f, 1f);
            pRect.pivot = new Vector2(1f, 1f);
            pRect.anchoredPosition = new Vector2(-28f, -22f);
            pRect.sizeDelta = new Vector2(64f, 64f);
            var pImg = pauseGo.GetComponent<Image>();
            pImg.sprite = NeonSpriteFactory.HexBadge;
            pImg.color = new Color(0f, 0.92f, 1f, 0.9f);
            pauseGo.GetComponent<Button>().onClick.AddListener(() => GameManager.Instance?.TogglePause());
            var pTxt = CreateText("PauseIcon", pauseGo.transform, "II", 24, Color.white, TextAnchor.MiddleCenter);
            pTxt.fontStyle = FontStyle.Bold;
            StretchFull(pTxt.rectTransform);

            // Chamfered Translucent Telemetry Pill for Score & Kills
            var pill = new GameObject("ScorePill", typeof(RectTransform), typeof(Image));
            pill.transform.SetParent(_hudRoot.transform, false);
            var pr = pill.GetComponent<RectTransform>();
            pr.anchorMin = new Vector2(1f, 1f);
            pr.anchorMax = new Vector2(1f, 1f);
            pr.pivot = new Vector2(1f, 1f);
            pr.anchoredPosition = new Vector2(-104f, -24f);
            pr.sizeDelta = new Vector2(280f, 60f);
            var pillImg = pill.GetComponent<Image>();
            pillImg.sprite = NeonSpriteFactory.HudPill;
            pillImg.color = new Color(0.65f, 0.35f, 1f, 1f);
            pillImg.raycastTarget = false;

            _scoreValueText = CreateText("ScoreVal", pill.transform, "0", 30, Color.white, TextAnchor.MiddleRight);
            _scoreValueText.fontStyle = FontStyle.Bold;
            AddShadow(_scoreValueText.gameObject);
            var svRect = _scoreValueText.rectTransform;
            svRect.anchorMin = new Vector2(0.08f, 0.32f);
            svRect.anchorMax = new Vector2(0.92f, 0.96f);
            svRect.offsetMin = Vector2.zero;
            svRect.offsetMax = Vector2.zero;

            var scoreLabel = CreateText("ScoreLbl", pill.transform, "SKOR", 15, new Color(0.75f, 0.65f, 1f), TextAnchor.MiddleLeft);
            scoreLabel.fontStyle = FontStyle.Bold;
            var slRect = scoreLabel.rectTransform;
            slRect.anchorMin = new Vector2(0.08f, 0.45f);
            slRect.anchorMax = new Vector2(0.45f, 0.92f);
            slRect.offsetMin = Vector2.zero;
            slRect.offsetMax = Vector2.zero;

            _killsValueText = CreateText("KillsVal", pill.transform, "İMHA: 0", 17, new Color(0.65f, 0.95f, 1f), TextAnchor.MiddleLeft);
            _killsValueText.fontStyle = FontStyle.Bold;
            var kvRect = _killsValueText.rectTransform;
            kvRect.anchorMin = new Vector2(0.08f, 0.06f);
            kvRect.anchorMax = new Vector2(0.92f, 0.44f);
            kvRect.offsetMin = Vector2.zero;
            kvRect.offsetMax = Vector2.zero;

            // Floating glowing Combo label under telemetry
            _comboText = CreateText("ComboText", _hudRoot.transform, "", 28, new Color(1f, 0.88f, 0.2f), TextAnchor.MiddleRight);
            _comboText.fontStyle = FontStyle.Bold;
            AddShadow(_comboText.gameObject);
            var cbRect = _comboText.rectTransform;
            cbRect.anchorMin = new Vector2(1f, 1f);
            cbRect.anchorMax = new Vector2(1f, 1f);
            cbRect.pivot = new Vector2(1f, 1f);
            cbRect.anchoredPosition = new Vector2(-106f, -88f);
            cbRect.sizeDelta = new Vector2(360f, 40f);
        }

        private void BuildActiveBuildStrip()
        {
            var go = new GameObject("BuildStrip", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            go.transform.SetParent(_hudRoot.transform, false);
            var r = go.GetComponent<RectTransform>();
            r.anchorMin = new Vector2(0f, 0f);
            r.anchorMax = new Vector2(0f, 0f);
            r.pivot = new Vector2(0f, 0f);
            r.anchoredPosition = new Vector2(28f, 24f);
            r.sizeDelta = new Vector2(780f, 64f);

            var hlg = go.GetComponent<HorizontalLayoutGroup>();
            hlg.spacing = 12f;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
            _buildStripContainer = go.transform;
        }

        public void RefreshBuildStrip()
        {
            var player = PlayerController.Instance;
            if (player == null || _buildStripContainer == null) return;

            for (int i = _buildStripContainer.childCount - 1; i >= 0; i--)
                Destroy(_buildStripContainer.GetChild(i).gameObject);

            foreach (var kv in player.UpgradeLevels)
            {
                var slot = new GameObject($"Slot_{kv.Key}", typeof(RectTransform), typeof(Image));
                slot.transform.SetParent(_buildStripContainer, false);
                var sr = slot.GetComponent<RectTransform>();
                sr.sizeDelta = new Vector2(58f, 58f);

                var hexBg = slot.GetComponent<Image>();
                hexBg.sprite = NeonSpriteFactory.HexBadge;
                hexBg.color = new Color(0f, 0.92f, 1f, 0.88f);
                hexBg.raycastTarget = false;

                var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(Image));
                iconGo.transform.SetParent(slot.transform, false);
                var ir = iconGo.GetComponent<RectTransform>();
                ir.anchorMin = new Vector2(0.22f, 0.22f);
                ir.anchorMax = new Vector2(0.78f, 0.78f);
                ir.offsetMin = Vector2.zero;
                ir.offsetMax = Vector2.zero;
                var iconImg = iconGo.GetComponent<Image>();
                iconImg.sprite = NeonSpriteFactory.GetUpgradeIcon(kv.Key);
                iconImg.color = Color.white;
                iconImg.raycastTarget = false;

                var lvTxt = CreateText("Lv", slot.transform, kv.Value.ToString(), 18, new Color(1f, 0.92f, 0.25f), TextAnchor.LowerRight);
                lvTxt.fontStyle = FontStyle.Bold;
                AddShadow(lvTxt.gameObject);
                var lr = lvTxt.rectTransform;
                lr.anchorMin = Vector2.zero;
                lr.anchorMax = Vector2.one;
                lr.offsetMin = Vector2.zero;
                lr.offsetMax = new Vector2(-4f, 0f);
            }
        }

        private void BuildDashButton()
        {
            var dashGo = new GameObject("DashButton", typeof(RectTransform), typeof(Image), typeof(Button));
            dashGo.transform.SetParent(_hudRoot.transform, false);
            var dRect = dashGo.GetComponent<RectTransform>();
            dRect.anchorMin = new Vector2(1f, 0f);
            dRect.anchorMax = new Vector2(1f, 0f);
            dRect.pivot = new Vector2(1f, 0f);
            dRect.anchoredPosition = new Vector2(-44f, 34f);
            dRect.sizeDelta = new Vector2(142f, 142f);

            var bgImg = dashGo.GetComponent<Image>();
            bgImg.sprite = NeonSpriteFactory.HexBadge;
            bgImg.color = new Color(0f, 0.65f, 0.85f, 0.85f);

            var fillGo = new GameObject("DashFill", typeof(RectTransform), typeof(Image));
            fillGo.transform.SetParent(dashGo.transform, false);
            StretchFull(fillGo.GetComponent<RectTransform>());
            _dashFill = fillGo.GetComponent<Image>();
            _dashFill.sprite = NeonSpriteFactory.Ring;
            _dashFill.color = new Color(0.15f, 0.98f, 1f, 0.95f);
            _dashFill.type = Image.Type.Filled;
            _dashFill.fillMethod = Image.FillMethod.Radial360;
            _dashFill.raycastTarget = false;

            _dashLabel = CreateText("DashLabel", dashGo.transform, "DASH\n[SPACE]", 21, Color.white, TextAnchor.MiddleCenter);
            _dashLabel.fontStyle = FontStyle.Bold;
            AddShadow(_dashLabel.gameObject);
            StretchFull(_dashLabel.rectTransform);

            dashGo.GetComponent<Button>().onClick.AddListener(() => PlayerController.Instance?.TriggerDash());
        }

        private void BuildRadioBanner()
        {
            _radioPanel = new GameObject("RadioComms", typeof(RectTransform), typeof(Image));
            _radioPanel.transform.SetParent(_hudRoot.transform, false);
            var rRect = _radioPanel.GetComponent<RectTransform>();
            rRect.anchorMin = new Vector2(0.5f, 0f);
            rRect.anchorMax = new Vector2(0.5f, 0f);
            rRect.pivot = new Vector2(0.5f, 0f);
            rRect.anchoredPosition = new Vector2(0f, 26f);
            rRect.sizeDelta = new Vector2(880f, 92f);
            var rImg = _radioPanel.GetComponent<Image>();
            rImg.sprite = NeonSpriteFactory.HudPill;
            rImg.color = new Color(0f, 0.95f, 1f, 1f);
            rImg.raycastTarget = false;
            _radioAccentBar = rImg;

            _radioSpeakerText = CreateText("Speaker", _radioPanel.transform, "NOVA // İSTİHBARAT", 20, new Color(0f, 0.95f, 1f), TextAnchor.MiddleLeft);
            _radioSpeakerText.fontStyle = FontStyle.Bold;
            var spRect = _radioSpeakerText.rectTransform;
            spRect.anchorMin = new Vector2(0f, 0.6f);
            spRect.anchorMax = new Vector2(1f, 0.96f);
            spRect.offsetMin = new Vector2(26f, 0f);
            spRect.offsetMax = new Vector2(-22f, 0f);

            _radioBodyText = CreateText("Body", _radioPanel.transform, "", 22, Color.white, TextAnchor.MiddleLeft);
            var bdRect = _radioBodyText.rectTransform;
            bdRect.anchorMin = new Vector2(0f, 0.06f);
            bdRect.anchorMax = new Vector2(1f, 0.62f);
            bdRect.offsetMin = new Vector2(26f, 0f);
            bdRect.offsetMax = new Vector2(-22f, 0f);

            _radioPanel.SetActive(false);
        }

        private void BuildBossWarningBanner()
        {
            _bossWarningBanner = CreatePanel("BossWarning", _hudRoot.transform, new Color(0.55f, 0.03f, 0.12f, 0.92f));
            var wRect = _bossWarningBanner.GetComponent<RectTransform>();
            wRect.anchorMin = new Vector2(0f, 0.5f);
            wRect.anchorMax = new Vector2(1f, 0.5f);
            wRect.pivot = new Vector2(0.5f, 0.5f);
            wRect.anchoredPosition = new Vector2(0f, 140f);
            wRect.sizeDelta = new Vector2(0f, 90f);

            _bossWarningText = CreateText("WarnText", _bossWarningBanner.transform, "⚠ YÜKSEK SEVİYE BOSS TESPİT EDİLDİ ⚠", 40, new Color(1f, 0.9f, 0.25f), TextAnchor.MiddleCenter);
            _bossWarningText.fontStyle = FontStyle.Bold;
            AddShadow(_bossWarningText.gameObject);
            StretchFull(_bossWarningText.rectTransform);
            _bossWarningBanner.SetActive(false);
        }

        private void BuildLevelUpPanel()
        {
            _levelUpPanel = CreatePanel("LevelUpPanel", RootCanvas.transform, new Color(0.012f, 0.02f, 0.055f, 0.94f));
            StretchFull(_levelUpPanel.GetComponent<RectTransform>());

            var sub = CreateText("Sub", _levelUpPanel.transform, "// SİBER ÇEKİRDEK YÜKSELTME PROTOKOLÜ //", 22, new Color(0f, 0.95f, 1f, 0.85f), TextAnchor.MiddleCenter);
            sub.fontStyle = FontStyle.Bold;
            var subRect = sub.rectTransform;
            subRect.anchorMin = new Vector2(0.5f, 1f);
            subRect.anchorMax = new Vector2(0.5f, 1f);
            subRect.pivot = new Vector2(0.5f, 1f);
            subRect.anchoredPosition = new Vector2(0f, -44f);
            subRect.sizeDelta = new Vector2(1000f, 36f);

            var title = CreateText("Title", _levelUpPanel.transform, "SEVİYE ATLADIN — MODÜL SEÇ", 48, Color.white, TextAnchor.MiddleCenter);
            title.fontStyle = FontStyle.Bold;
            AddShadow(title.gameObject);
            var titleRect = title.rectTransform;
            titleRect.anchorMin = new Vector2(0.5f, 1f);
            titleRect.anchorMax = new Vector2(0.5f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0f, -84f);
            titleRect.sizeDelta = new Vector2(1200f, 68f);

            var containerGo = new GameObject("Cards", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            containerGo.transform.SetParent(_levelUpPanel.transform, false);
            var cRect = containerGo.GetComponent<RectTransform>();
            cRect.anchorMin = new Vector2(0.5f, 0.5f);
            cRect.anchorMax = new Vector2(0.5f, 0.5f);
            cRect.pivot = new Vector2(0.5f, 0.5f);
            cRect.anchoredPosition = new Vector2(0f, -8f);
            cRect.sizeDelta = new Vector2(1420f, 600f);

            var layout = containerGo.GetComponent<HorizontalLayoutGroup>();
            layout.spacing = 48f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;

            _cardContainer = containerGo.transform;

            var rerollGo = new GameObject("RerollBtn", typeof(RectTransform), typeof(Image), typeof(Button));
            rerollGo.transform.SetParent(_levelUpPanel.transform, false);
            var rRect = rerollGo.GetComponent<RectTransform>();
            rRect.anchorMin = new Vector2(0.5f, 0f);
            rRect.anchorMax = new Vector2(0.5f, 0f);
            rRect.pivot = new Vector2(0.5f, 0f);
            rRect.anchoredPosition = new Vector2(0f, 42f);
            rRect.sizeDelta = new Vector2(360f, 66f);
            var rImg = rerollGo.GetComponent<Image>();
            rImg.sprite = NeonSpriteFactory.HudPill;
            rImg.color = new Color(0f, 0.92f, 1f, 1f);

            _rerollBtn = rerollGo.GetComponent<Button>();
            _rerollBtn.onClick.AddListener(() => GameManager.Instance?.RerollUpgrades());
            _rerollBtnText = CreateText("RerollLabel", rerollGo.transform, "KARTLARI YENİLE (3)", 24, Color.white, TextAnchor.MiddleCenter);
            _rerollBtnText.fontStyle = FontStyle.Bold;
            StretchFull(_rerollBtnText.rectTransform);

            _levelUpPanel.SetActive(false);
        }

        public void ShowLevelUpChoices(List<UpgradeOption> options, Action<UpgradeOption> onSelect, int rerollsLeft)
        {
            _hudRoot.SetActive(false); // Clean modal isolation — zero overlap with Boss bar or Radio!

            for (int i = _cardContainer.childCount - 1; i >= 0; i--)
                Destroy(_cardContainer.GetChild(i).gameObject);

            if (_rerollBtn != null)
            {
                _rerollBtn.interactable = rerollsLeft > 0;
                _rerollBtnText.text = rerollsLeft > 0 ? $"KARTLARI YENİLE ({rerollsLeft})" : "YENİLEME HAKKI BİTTİ";
                _rerollBtnText.color = rerollsLeft > 0 ? new Color(0.2f, 0.98f, 1f) : new Color(0.45f, 0.5f, 0.58f);
            }

            var player = PlayerController.Instance;

            for (int i = 0; i < options.Count; i++)
            {
                var opt = options[i];
                int curLv = player != null ? player.GetUpgradeLevel(opt.Id) : 0;

                var cardGo = new GameObject($"CyberCard_{i}", typeof(RectTransform), typeof(Image), typeof(Button));
                cardGo.transform.SetParent(_cardContainer, false);

                var bgImg = cardGo.GetComponent<Image>();
                bgImg.sprite = NeonSpriteFactory.CyberCardBg;
                bgImg.color = Color.white;

                var btn = cardGo.GetComponent<Button>();
                btn.transition = Selectable.Transition.None;
                btn.onClick.AddListener(() => onSelect?.Invoke(opt));

                var anim = cardGo.AddComponent<CyberCardAnimator>();
                anim.Init(i * 0.06f);

                // Chamfered neon outer border
                var borderGo = new GameObject("NeonBorder", typeof(RectTransform), typeof(Image));
                borderGo.transform.SetParent(cardGo.transform, false);
                StretchFull(borderGo.GetComponent<RectTransform>());
                var borderImg = borderGo.GetComponent<Image>();
                borderImg.sprite = NeonSpriteFactory.CyberCardBorder;
                borderImg.color = opt.AccentColor;
                borderImg.raycastTarget = false;

                // High-contrast Rarity Pill (dark interior + crisp neon border + bright text)
                string rarityLabel = opt.Rarity == UpgradeRarity.Legendary ? "★ EFSANEVİ ★"
                    : (opt.Rarity == UpgradeRarity.Epic ? "◆ EPİK ◆" : "● NADİR ●");

                var rarityPill = CreatePanel("RarityPill", cardGo.transform, new Color(0.03f, 0.05f, 0.11f, 0.95f));
                var rpRect = rarityPill.GetComponent<RectTransform>();
                rpRect.anchorMin = new Vector2(0.18f, 0.87f);
                rpRect.anchorMax = new Vector2(0.82f, 0.945f);
                rpRect.offsetMin = Vector2.zero;
                rpRect.offsetMax = Vector2.zero;
                AddOutline(rarityPill, opt.AccentColor, 1.8f);

                var rarityTxt = CreateText("RarityTxt", rarityPill.transform, rarityLabel, 20, opt.AccentColor, TextAnchor.MiddleCenter);
                rarityTxt.fontStyle = FontStyle.Bold;
                StretchFull(rarityTxt.rectTransform);

                // Category tag
                var catTxt = CreateText("Category", cardGo.transform, opt.CategoryTag, 17, new Color(0.68f, 0.78f, 0.9f), TextAnchor.MiddleCenter);
                var catRect = catTxt.rectTransform;
                catRect.anchorMin = new Vector2(0.1f, 0.80f);
                catRect.anchorMax = new Vector2(0.9f, 0.86f);
                catRect.offsetMin = Vector2.zero;
                catRect.offsetMax = Vector2.zero;

                // Hexagonal glowing emblem + custom 96px icon
                var hexGo = new GameObject("HexEmblem", typeof(RectTransform), typeof(Image));
                hexGo.transform.SetParent(cardGo.transform, false);
                var hexRect = hexGo.GetComponent<RectTransform>();
                hexRect.anchorMin = new Vector2(0.5f, 0.62f);
                hexRect.anchorMax = new Vector2(0.5f, 0.62f);
                hexRect.sizeDelta = new Vector2(158f, 158f);
                var hexImg = hexGo.GetComponent<Image>();
                hexImg.sprite = NeonSpriteFactory.HexBadge;
                hexImg.color = opt.AccentColor;
                hexImg.raycastTarget = false;

                var iconGo = new GameObject("AbilityIcon", typeof(RectTransform), typeof(Image));
                iconGo.transform.SetParent(hexGo.transform, false);
                var iconRect = iconGo.GetComponent<RectTransform>();
                iconRect.anchorMin = new Vector2(0.22f, 0.22f);
                iconRect.anchorMax = new Vector2(0.78f, 0.78f);
                iconRect.offsetMin = Vector2.zero;
                iconRect.offsetMax = Vector2.zero;
                var iconImg = iconGo.GetComponent<Image>();
                iconImg.sprite = NeonSpriteFactory.GetUpgradeIcon(opt.Id);
                iconImg.color = Color.Lerp(opt.AccentColor, Color.white, 0.55f);
                iconImg.raycastTarget = false;

                // Card Title
                var titleTxt = CreateText("CardTitle", cardGo.transform, opt.Title, 32, Color.white, TextAnchor.MiddleCenter);
                titleTxt.fontStyle = FontStyle.Bold;
                var tr = titleTxt.rectTransform;
                tr.anchorMin = new Vector2(0.08f, 0.39f);
                tr.anchorMax = new Vector2(0.92f, 0.48f);
                tr.offsetMin = Vector2.zero;
                tr.offsetMax = Vector2.zero;

                // Level progression tag
                string lvString = curLv == 0 ? "YENİ MODÜL" : $"SVY {curLv}  →  SVY {curLv + 1}";
                var lvTxt = CreateText("LvTag", cardGo.transform, lvString, 20, opt.AccentColor, TextAnchor.MiddleCenter);
                lvTxt.fontStyle = FontStyle.Bold;
                var lvr = lvTxt.rectTransform;
                lvr.anchorMin = new Vector2(0.1f, 0.33f);
                lvr.anchorMax = new Vector2(0.9f, 0.39f);
                lvr.offsetMin = Vector2.zero;
                lvr.offsetMax = Vector2.zero;

                // Description
                var descTxt = CreateText("CardDesc", cardGo.transform, opt.Description, 22, new Color(0.8f, 0.88f, 0.96f), TextAnchor.MiddleCenter);
                var dr = descTxt.rectTransform;
                dr.anchorMin = new Vector2(0.1f, 0.18f);
                dr.anchorMax = new Vector2(0.9f, 0.33f);
                dr.offsetMin = Vector2.zero;
                dr.offsetMax = Vector2.zero;

                // High-contrast Stat Gain pill at bottom (dark interior + glowing border + crisp white text)
                var statBox = CreatePanel("StatDeltaBox", cardGo.transform, new Color(0.03f, 0.06f, 0.13f, 0.96f));
                var sbRect = statBox.GetComponent<RectTransform>();
                sbRect.anchorMin = new Vector2(0.1f, 0.065f);
                sbRect.anchorMax = new Vector2(0.9f, 0.155f);
                sbRect.offsetMin = Vector2.zero;
                sbRect.offsetMax = Vector2.zero;
                AddOutline(statBox, opt.AccentColor, 1.8f);

                var statTxt = CreateText("StatTxt", statBox.transform, opt.StatDelta, 22, Color.white, TextAnchor.MiddleCenter);
                statTxt.fontStyle = FontStyle.Bold;
                StretchFull(statTxt.rectTransform);
            }

            _levelUpPanel.SetActive(true);
        }

        public void HideLevelUp()
        {
            _levelUpPanel.SetActive(false);
            _hudRoot.SetActive(true);
        }

        private void BuildPausePanel()
        {
            _pausePanel = CreatePanel("PausePanel", RootCanvas.transform, new Color(0.015f, 0.025f, 0.06f, 0.9f));
            StretchFull(_pausePanel.GetComponent<RectTransform>());

            var box = new GameObject("PauseBox", typeof(RectTransform), typeof(Image));
            box.transform.SetParent(_pausePanel.transform, false);
            var bRect = box.GetComponent<RectTransform>();
            bRect.anchorMin = new Vector2(0.5f, 0.5f);
            bRect.anchorMax = new Vector2(0.5f, 0.5f);
            bRect.sizeDelta = new Vector2(600f, 470f);
            box.GetComponent<Image>().sprite = NeonSpriteFactory.CyberCardBg;

            var border = new GameObject("Border", typeof(RectTransform), typeof(Image));
            border.transform.SetParent(box.transform, false);
            StretchFull(border.GetComponent<RectTransform>());
            var bImg = border.GetComponent<Image>();
            bImg.sprite = NeonSpriteFactory.CyberCardBorder;
            bImg.color = new Color(0f, 0.92f, 1f);
            bImg.raycastTarget = false;

            var title = CreateText("PauseTitle", box.transform, "SİSTEM DURAKLATILDI", 36, new Color(0f, 0.95f, 1f), TextAnchor.MiddleCenter);
            title.fontStyle = FontStyle.Bold;
            var tRect = title.rectTransform;
            tRect.anchorMin = new Vector2(0f, 0.78f);
            tRect.anchorMax = new Vector2(1f, 0.95f);
            tRect.offsetMin = Vector2.zero;
            tRect.offsetMax = Vector2.zero;

            // Audio toggles (Music + SFX)
            var musGo = CreatePanel("MusicToggle", box.transform, new Color(0.08f, 0.14f, 0.25f, 1f));
            var mRect = musGo.GetComponent<RectTransform>();
            mRect.anchorMin = new Vector2(0.5f, 0.5f);
            mRect.anchorMax = new Vector2(0.5f, 0.5f);
            mRect.anchoredPosition = new Vector2(-108f, 76f);
            mRect.sizeDelta = new Vector2(198f, 58f);
            AddOutline(musGo, new Color(0f, 0.92f, 1f, 0.8f), 2f);
            var musTxt = CreateText("MusTxt", musGo.transform, "MÜZİK: AÇIK", 20, Color.white, TextAnchor.MiddleCenter);
            musTxt.fontStyle = FontStyle.Bold;
            StretchFull(musTxt.rectTransform);
            musGo.AddComponent<Button>().onClick.AddListener(() =>
            {
                if (NeonAudio.Instance != null)
                {
                    NeonAudio.Instance.IsMusicEnabled = !NeonAudio.Instance.IsMusicEnabled;
                    musTxt.text = NeonAudio.Instance.IsMusicEnabled ? "MÜZİK: AÇIK" : "MÜZİK: KAPALI";
                }
            });

            var sfxGo = CreatePanel("SfxToggle", box.transform, new Color(0.08f, 0.14f, 0.25f, 1f));
            var sxRect = sfxGo.GetComponent<RectTransform>();
            sxRect.anchorMin = new Vector2(0.5f, 0.5f);
            sxRect.anchorMax = new Vector2(0.5f, 0.5f);
            sxRect.anchoredPosition = new Vector2(108f, 76f);
            sxRect.sizeDelta = new Vector2(198f, 58f);
            AddOutline(sfxGo, new Color(0.85f, 0.3f, 1f, 0.8f), 2f);
            var sfxTxt = CreateText("SfxTxt", sfxGo.transform, "SES: AÇIK", 20, Color.white, TextAnchor.MiddleCenter);
            sfxTxt.fontStyle = FontStyle.Bold;
            StretchFull(sfxTxt.rectTransform);
            sfxGo.AddComponent<Button>().onClick.AddListener(() =>
            {
                if (NeonAudio.Instance != null)
                {
                    NeonAudio.Instance.IsSfxEnabled = !NeonAudio.Instance.IsSfxEnabled;
                    sfxTxt.text = NeonAudio.Instance.IsSfxEnabled ? "SES: AÇIK" : "SES: KAPALI";
                }
            });

            var resGo = CreatePanel("ResumeBtn", box.transform, new Color(0f, 0.92f, 1f, 1f));
            var rRect = resGo.GetComponent<RectTransform>();
            rRect.anchorMin = new Vector2(0.5f, 0.5f);
            rRect.anchorMax = new Vector2(0.5f, 0.5f);
            rRect.anchoredPosition = new Vector2(0f, -14f);
            rRect.sizeDelta = new Vector2(414f, 72f);
            resGo.AddComponent<Button>().onClick.AddListener(() => GameManager.Instance?.TogglePause());
            var rTxt = CreateText("ResTxt", resGo.transform, "DEVAM ET", 30, new Color(0.02f, 0.05f, 0.1f), TextAnchor.MiddleCenter);
            rTxt.fontStyle = FontStyle.Bold;
            StretchFull(rTxt.rectTransform);

            var rstGo = CreatePanel("RestartPauseBtn", box.transform, new Color(0.08f, 0.12f, 0.22f, 1f));
            var rsRect = rstGo.GetComponent<RectTransform>();
            rsRect.anchorMin = new Vector2(0.5f, 0.5f);
            rsRect.anchorMax = new Vector2(0.5f, 0.5f);
            rsRect.anchoredPosition = new Vector2(0f, -108f);
            rsRect.sizeDelta = new Vector2(414f, 66f);
            AddOutline(rstGo, new Color(1f, 0.25f, 0.5f, 0.85f), 2f);
            rstGo.AddComponent<Button>().onClick.AddListener(() => GameManager.Instance?.RestartGame());
            var rsTxt = CreateText("RstTxt", rstGo.transform, "ANA MENÜ / YENİDEN BAŞLAT", 23, new Color(1f, 0.45f, 0.65f), TextAnchor.MiddleCenter);
            rsTxt.fontStyle = FontStyle.Bold;
            StretchFull(rsTxt.rectTransform);

            _pausePanel.SetActive(false);
        }

        public void SetPauseVisible(bool visible)
        {
            _pausePanel.SetActive(visible);
        }

        private void BuildMainMenuPanel()
        {
            _mainMenuPanel = CreatePanel("MainMenuPanel", RootCanvas.transform, new Color(0.015f, 0.022f, 0.06f, 0.94f));
            StretchFull(_mainMenuPanel.GetComponent<RectTransform>());

            var card = new GameObject("MenuCard", typeof(RectTransform), typeof(Image));
            card.transform.SetParent(_mainMenuPanel.transform, false);
            var cRect = card.GetComponent<RectTransform>();
            cRect.anchorMin = new Vector2(0.5f, 0.5f);
            cRect.anchorMax = new Vector2(0.5f, 0.5f);
            cRect.sizeDelta = new Vector2(1060f, 640f);
            card.GetComponent<Image>().sprite = NeonSpriteFactory.CyberCardBg;

            var border = new GameObject("MenuBorder", typeof(RectTransform), typeof(Image));
            border.transform.SetParent(card.transform, false);
            StretchFull(border.GetComponent<RectTransform>());
            var bImg = border.GetComponent<Image>();
            bImg.sprite = NeonSpriteFactory.CyberCardBorder;
            bImg.color = new Color(0f, 0.95f, 1f);
            bImg.raycastTarget = false;

            var title = CreateText("GameTitle", card.transform, "NEON ARENA", 74, new Color(0f, 0.98f, 1f), TextAnchor.MiddleCenter);
            title.fontStyle = FontStyle.Bold;
            AddShadow(title.gameObject);
            var tRect = title.rectTransform;
            tRect.anchorMin = new Vector2(0f, 0.76f);
            tRect.anchorMax = new Vector2(1f, 0.95f);
            tRect.offsetMin = Vector2.zero;
            tRect.offsetMax = Vector2.zero;

            var subtitle = CreateText("Subtitle", card.transform, "// PROTOKOL ZERO : NEO-İSTANBUL 2147 //", 26, new Color(1f, 0.25f, 0.62f), TextAnchor.MiddleCenter);
            subtitle.fontStyle = FontStyle.Bold;
            var subRect = subtitle.rectTransform;
            subRect.anchorMin = new Vector2(0f, 0.66f);
            subRect.anchorMax = new Vector2(1f, 0.76f);
            subRect.offsetMin = Vector2.zero;
            subRect.offsetMax = Vector2.zero;

            var story = CreateText(
                "StoryText",
                card.transform,
                "Dev yapay zeka OMEGA-CORE tüm yeraltı enerji sektörlerini ele geçirdi.\n" +
                "Son bağımsız siber pilot VEX-09 olarak 3 sektörü temizle,\n" +
                "her sektörün yüksek seviyeli FİNAL BOSS'unu yok et ve çekirdeği kır!\n\n" +
                "Kontrol: Dokunmatik Joystick / WASD   •   Kaçış: DASH / SPACE",
                26,
                new Color(0.86f, 0.94f, 1f),
                TextAnchor.MiddleCenter
            );
            var stRect = story.rectTransform;
            stRect.anchorMin = new Vector2(0.06f, 0.30f);
            stRect.anchorMax = new Vector2(0.94f, 0.64f);
            stRect.offsetMin = Vector2.zero;
            stRect.offsetMax = Vector2.zero;

            _menuBestScoreText = CreateText("BestScore", card.transform, "EN İYİ SKOR: 0", 23, new Color(0.15f, 1f, 0.7f), TextAnchor.MiddleLeft);
            _menuBestScoreText.fontStyle = FontStyle.Bold;
            var bRect = _menuBestScoreText.rectTransform;
            bRect.anchorMin = new Vector2(0.08f, 0.21f);
            bRect.anchorMax = new Vector2(0.45f, 0.29f);
            bRect.offsetMin = Vector2.zero;
            bRect.offsetMax = Vector2.zero;

            _menuActiveShipText = CreateText("ActiveShip", card.transform, "GEMİ: VEX-09", 23, new Color(0f, 0.95f, 1f), TextAnchor.MiddleCenter);
            _menuActiveShipText.fontStyle = FontStyle.Bold;
            var asRect = _menuActiveShipText.rectTransform;
            asRect.anchorMin = new Vector2(0.35f, 0.21f);
            asRect.anchorMax = new Vector2(0.65f, 0.29f);
            asRect.offsetMin = Vector2.zero;
            asRect.offsetMax = Vector2.zero;

            _menuCreditsText = CreateText("MenuCredits", card.transform, "0 CR", 25, new Color(1f, 0.85f, 0.2f), TextAnchor.MiddleRight);
            _menuCreditsText.fontStyle = FontStyle.Bold;
            var mcRect = _menuCreditsText.rectTransform;
            mcRect.anchorMin = new Vector2(0.55f, 0.21f);
            mcRect.anchorMax = new Vector2(0.92f, 0.29f);
            mcRect.offsetMin = Vector2.zero;
            mcRect.offsetMax = Vector2.zero;

            // 3 Action Buttons: HANGAR | GÖREVE BAŞLA | MAĞAZA
            var hangarBtnGo = CreatePanel("HangarBtn", card.transform, new Color(0.09f, 0.14f, 0.26f, 1f));
            var hbRect = hangarBtnGo.GetComponent<RectTransform>();
            hbRect.anchorMin = new Vector2(0.5f, 0f);
            hbRect.anchorMax = new Vector2(0.5f, 0f);
            hbRect.pivot = new Vector2(0.5f, 0f);
            hbRect.anchoredPosition = new Vector2(-320f, 42f);
            hbRect.sizeDelta = new Vector2(260f, 78f);
            AddOutline(hangarBtnGo, new Color(0.88f, 0.25f, 1f, 0.9f), 2.5f);
            hangarBtnGo.AddComponent<Button>().onClick.AddListener(OpenHangar);
            var hbTxt = CreateText("HangarLbl", hangarBtnGo.transform, "HANGAR\n(GEMİLER)", 22, new Color(0.92f, 0.55f, 1f), TextAnchor.MiddleCenter);
            hbTxt.fontStyle = FontStyle.Bold;
            StretchFull(hbTxt.rectTransform);

            var playBtnGo = CreatePanel("StartBtn", card.transform, new Color(0f, 0.92f, 1f, 1f));
            var pbRect = playBtnGo.GetComponent<RectTransform>();
            pbRect.anchorMin = new Vector2(0.5f, 0f);
            pbRect.anchorMax = new Vector2(0.5f, 0f);
            pbRect.pivot = new Vector2(0.5f, 0f);
            pbRect.anchoredPosition = new Vector2(0f, 38f);
            pbRect.sizeDelta = new Vector2(330f, 86f);

            playBtnGo.AddComponent<Button>().onClick.AddListener(() => GameManager.Instance?.StartMission());
            var playLabel = CreateText("PlayLabel", playBtnGo.transform, "GÖREVE BAŞLA", 32, new Color(0.02f, 0.05f, 0.1f), TextAnchor.MiddleCenter);
            playLabel.fontStyle = FontStyle.Bold;
            StretchFull(playLabel.rectTransform);

            var shopBtnGo = CreatePanel("ShopBtn", card.transform, new Color(0.09f, 0.14f, 0.26f, 1f));
            var sbRect = shopBtnGo.GetComponent<RectTransform>();
            sbRect.anchorMin = new Vector2(0.5f, 0f);
            sbRect.anchorMax = new Vector2(0.5f, 0f);
            sbRect.pivot = new Vector2(0.5f, 0f);
            sbRect.anchoredPosition = new Vector2(320f, 42f);
            sbRect.sizeDelta = new Vector2(260f, 78f);
            AddOutline(shopBtnGo, new Color(1f, 0.8f, 0.18f, 0.9f), 2.5f);
            shopBtnGo.AddComponent<Button>().onClick.AddListener(OpenShop);
            var sbTxt = CreateText("ShopLbl", shopBtnGo.transform, "MAĞAZA\n(KALICI)", 22, new Color(1f, 0.86f, 0.25f), TextAnchor.MiddleCenter);
            sbTxt.fontStyle = FontStyle.Bold;
            StretchFull(sbTxt.rectTransform);
        }

        private void BuildHangarPanel()
        {
            _hangarPanel = CreatePanel("HangarPanel", RootCanvas.transform, new Color(0.012f, 0.018f, 0.05f, 0.97f));
            StretchFull(_hangarPanel.GetComponent<RectTransform>());

            var sub = CreateText("HangarSub", _hangarPanel.transform, "// SİBER FİLO HANGARI • GEMİ SEÇİMİ //", 20, new Color(0.9f, 0.45f, 1f, 0.85f), TextAnchor.MiddleCenter);
            sub.fontStyle = FontStyle.Bold;
            var subR = sub.rectTransform;
            subR.anchorMin = new Vector2(0.5f, 1f);
            subR.anchorMax = new Vector2(0.5f, 1f);
            subR.pivot = new Vector2(0.5f, 1f);
            subR.anchoredPosition = new Vector2(0f, -30f);
            subR.sizeDelta = new Vector2(900f, 32f);

            var title = CreateText("HangarTitle", _hangarPanel.transform, "SİBER HANGAR", 48, Color.white, TextAnchor.MiddleCenter);
            title.fontStyle = FontStyle.Bold;
            AddShadow(title.gameObject);
            var tRect = title.rectTransform;
            tRect.anchorMin = new Vector2(0.5f, 1f);
            tRect.anchorMax = new Vector2(0.5f, 1f);
            tRect.pivot = new Vector2(0.5f, 1f);
            tRect.anchoredPosition = new Vector2(0f, -64f);
            tRect.sizeDelta = new Vector2(1100f, 64f);

            // Chamfered Gold Credit Pill (Top-Right)
            var crPill = new GameObject("HangarCrPill", typeof(RectTransform), typeof(Image));
            crPill.transform.SetParent(_hangarPanel.transform, false);
            var cpR = crPill.GetComponent<RectTransform>();
            cpR.anchorMin = new Vector2(1f, 1f);
            cpR.anchorMax = new Vector2(1f, 1f);
            cpR.pivot = new Vector2(1f, 1f);
            cpR.anchoredPosition = new Vector2(-44f, -44f);
            cpR.sizeDelta = new Vector2(230f, 56f);
            var cpImg = crPill.GetComponent<Image>();
            cpImg.sprite = NeonSpriteFactory.HudPill;
            cpImg.color = new Color(1f, 0.82f, 0.18f, 1f);
            cpImg.raycastTarget = false;

            _hangarCreditsText = CreateText("HangarCR", crPill.transform, "0 CR", 26, new Color(1f, 0.9f, 0.25f), TextAnchor.MiddleCenter);
            _hangarCreditsText.fontStyle = FontStyle.Bold;
            AddShadow(_hangarCreditsText.gameObject);
            StretchFull(_hangarCreditsText.rectTransform);

            var containerGo = new GameObject("ShipCards", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            containerGo.transform.SetParent(_hangarPanel.transform, false);
            var cRect = containerGo.GetComponent<RectTransform>();
            cRect.anchorMin = new Vector2(0.5f, 0.5f);
            cRect.anchorMax = new Vector2(0.5f, 0.5f);
            cRect.anchoredPosition = new Vector2(0f, -8f);
            cRect.sizeDelta = new Vector2(1500f, 650f);

            var hlg = containerGo.GetComponent<HorizontalLayoutGroup>();
            hlg.spacing = 42f;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;
            _hangarCardsContainer = containerGo.transform;

            var backGo = new GameObject("HangarBack", typeof(RectTransform), typeof(Image), typeof(Button));
            backGo.transform.SetParent(_hangarPanel.transform, false);
            var bkRect = backGo.GetComponent<RectTransform>();
            bkRect.anchorMin = new Vector2(0.5f, 0f);
            bkRect.anchorMax = new Vector2(0.5f, 0f);
            bkRect.pivot = new Vector2(0.5f, 0f);
            bkRect.anchoredPosition = new Vector2(0f, 34f);
            bkRect.sizeDelta = new Vector2(320f, 64f);
            var bkImg = backGo.GetComponent<Image>();
            bkImg.sprite = NeonSpriteFactory.HudPill;
            bkImg.color = new Color(0f, 0.92f, 1f, 1f);
            backGo.GetComponent<Button>().onClick.AddListener(CloseSubMenus);
            var bkTxt = CreateText("BackLbl", backGo.transform, "←  ANA MENÜYE DÖN", 23, Color.white, TextAnchor.MiddleCenter);
            bkTxt.fontStyle = FontStyle.Bold;
            StretchFull(bkTxt.rectTransform);

            _hangarPanel.SetActive(false);
        }

        private void OpenHangar()
        {
            _mainMenuPanel.SetActive(false);
            RefreshHangarUI();
            _hangarPanel.SetActive(true);
        }

        private void RefreshHangarUI()
        {
            _hangarCreditsText.text = $"{NeonMetaProgression.Credits} CR";
            for (int i = _hangarCardsContainer.childCount - 1; i >= 0; i--)
                Destroy(_hangarCardsContainer.GetChild(i).gameObject);

            var ships = NeonMetaProgression.Ships;
            for (int i = 0; i < ships.Length; i++)
            {
                var def = ships[i];
                bool unlocked = NeonMetaProgression.IsShipUnlocked(def.Type);
                bool selected = NeonMetaProgression.SelectedShip == def.Type;

                var cardGo = new GameObject($"ShipCard_{def.Type}", typeof(RectTransform), typeof(Image));
                cardGo.transform.SetParent(_hangarCardsContainer, false);
                cardGo.GetComponent<Image>().sprite = NeonSpriteFactory.CyberCardBg;

                var anim = cardGo.AddComponent<CyberCardAnimator>();
                anim.Init(i * 0.05f);

                var borderGo = new GameObject("Border", typeof(RectTransform), typeof(Image));
                borderGo.transform.SetParent(cardGo.transform, false);
                StretchFull(borderGo.GetComponent<RectTransform>());
                var bImg = borderGo.GetComponent<Image>();
                bImg.sprite = NeonSpriteFactory.CyberCardBorder;
                bImg.color = selected ? new Color(0.15f, 1f, 0.65f) : def.AccentColor;
                bImg.raycastTarget = false;

                // Top Role Pill (high contrast)
                var rolePill = CreatePanel("RolePill", cardGo.transform, new Color(0.03f, 0.05f, 0.11f, 0.96f));
                var rpR = rolePill.GetComponent<RectTransform>();
                rpR.anchorMin = new Vector2(0.16f, 0.875f);
                rpR.anchorMax = new Vector2(0.84f, 0.945f);
                rpR.offsetMin = Vector2.zero;
                rpR.offsetMax = Vector2.zero;
                AddOutline(rolePill, def.AccentColor, 1.6f);

                var roleTxt = CreateText("Role", rolePill.transform, def.Role, 18, def.AccentColor, TextAnchor.MiddleCenter);
                roleTxt.fontStyle = FontStyle.Bold;
                StretchFull(roleTxt.rectTransform);

                // Animated Rotating Holographic Ship Pad
                var hexGo = new GameObject("ShipHex", typeof(RectTransform), typeof(Image));
                hexGo.transform.SetParent(cardGo.transform, false);
                var hr = hexGo.GetComponent<RectTransform>();
                hr.anchorMin = new Vector2(0.5f, 0.70f);
                hr.anchorMax = new Vector2(0.5f, 0.70f);
                hr.sizeDelta = new Vector2(174f, 174f);
                var hImg = hexGo.GetComponent<Image>();
                hImg.sprite = NeonSpriteFactory.HexBadge;
                hImg.color = def.AccentColor;
                hImg.raycastTarget = false;

                var shipImgGo = new GameObject("ShipPreview", typeof(RectTransform), typeof(Image));
                shipImgGo.transform.SetParent(hexGo.transform, false);
                var sr = shipImgGo.GetComponent<RectTransform>();
                sr.anchorMin = new Vector2(0.15f, 0.15f);
                sr.anchorMax = new Vector2(0.85f, 0.85f);
                sr.offsetMin = Vector2.zero;
                sr.offsetMax = Vector2.zero;
                var sImg = shipImgGo.GetComponent<Image>();
                sImg.sprite = NeonSpriteFactory.GetShipSprite(def.Type);
                sImg.preserveAspect = true;
                sImg.raycastTarget = false;
                shipImgGo.AddComponent<ShipHologramSpinner>();

                // Ship Name
                var nameTxt = CreateText("Name", cardGo.transform, def.Name, 34, Color.white, TextAnchor.MiddleCenter);
                nameTxt.fontStyle = FontStyle.Bold;
                AddShadow(nameTxt.gameObject);
                var nr = nameTxt.rectTransform;
                nr.anchorMin = new Vector2(0.08f, 0.49f);
                nr.anchorMax = new Vector2(0.92f, 0.56f);
                nr.offsetMin = Vector2.zero;
                nr.offsetMax = Vector2.zero;

                // Visual Slanted Stat Bars (ZIRH, ATEŞ GÜCÜ, HIZ)
                float armorVal = def.Type == ShipType.TitanMk4 ? 1f : (def.Type == ShipType.Vex09 ? 0.65f : 0.52f);
                float fireVal = def.Type == ShipType.TitanMk4 ? 0.92f : (def.Type == ShipType.Phantom7 ? 0.85f : 0.68f);
                float speedVal = def.Type == ShipType.Phantom7 ? 1f : (def.Type == ShipType.Vex09 ? 0.72f : 0.55f);

                BuildShipStatBar(cardGo.transform, "ZIRH", armorVal, new Color(1f, 0.25f, 0.5f), 0.42f);
                BuildShipStatBar(cardGo.transform, "ATEŞ", fireVal, new Color(0f, 0.95f, 1f), 0.355f);
                BuildShipStatBar(cardGo.transform, "HIZ", speedVal, new Color(1f, 0.84f, 0.2f), 0.29f);

                // Special Passive Box
                var perkBox = CreatePanel("PerkBox", cardGo.transform, new Color(0.03f, 0.06f, 0.13f, 0.92f));
                var pbr = perkBox.GetComponent<RectTransform>();
                pbr.anchorMin = new Vector2(0.09f, 0.175f);
                pbr.anchorMax = new Vector2(0.91f, 0.27f);
                pbr.offsetMin = Vector2.zero;
                pbr.offsetMax = Vector2.zero;
                AddOutline(perkBox, new Color(def.AccentColor.r, def.AccentColor.g, def.AccentColor.b, 0.55f), 1.4f);

                var perkTxt = CreateText("Perk", perkBox.transform, def.Perk, 18, new Color(0.88f, 0.94f, 1f), TextAnchor.MiddleCenter);
                var pr = perkTxt.rectTransform;
                pr.anchorMin = new Vector2(0.05f, 0.05f);
                pr.anchorMax = new Vector2(0.95f, 0.95f);
                pr.offsetMin = Vector2.zero;
                pr.offsetMax = Vector2.zero;

                // Action Button
                Color btnCol = selected ? new Color(0.14f, 0.88f, 0.56f)
                    : (unlocked ? new Color(0f, 0.88f, 1f) : new Color(1f, 0.8f, 0.18f));
                string btnLabel = selected ? "✓  AKTİF GEMİ" : (unlocked ? "GEMİYİ SEÇ" : $"KİLİDİ AÇ  ({def.Cost} CR)");

                var actGo = CreatePanel("ActionBtn", cardGo.transform, btnCol);
                var ar = actGo.GetComponent<RectTransform>();
                ar.anchorMin = new Vector2(0.12f, 0.055f);
                ar.anchorMax = new Vector2(0.88f, 0.145f);
                ar.offsetMin = Vector2.zero;
                ar.offsetMax = Vector2.zero;

                var btn = actGo.AddComponent<Button>();
                bool canAfford = unlocked || NeonMetaProgression.Credits >= def.Cost;
                btn.interactable = !selected && canAfford;
                if (!canAfford) actGo.GetComponent<Image>().color = new Color(0.18f, 0.22f, 0.30f);

                ShipType capturedType = def.Type;
                btn.onClick.AddListener(() =>
                {
                    if (NeonMetaProgression.TrySelectOrBuyShip(capturedType))
                    {
                        NeonAudio.Instance?.PlayLevelUp();
                        RefreshHangarUI();
                    }
                });

                var actTxt = CreateText("ActTxt", actGo.transform, btnLabel, 22, (canAfford || selected) ? new Color(0.02f, 0.04f, 0.09f) : new Color(0.58f, 0.65f, 0.75f), TextAnchor.MiddleCenter);
                actTxt.fontStyle = FontStyle.Bold;
                StretchFull(actTxt.rectTransform);
            }
        }

        private void BuildShipStatBar(Transform parent, string label, float fillRatio, Color fillColor, float anchorY)
        {
            var lbl = CreateText($"Lbl_{label}", parent, label, 17, new Color(0.75f, 0.85f, 0.95f), TextAnchor.MiddleLeft);
            lbl.fontStyle = FontStyle.Bold;
            var lr = lbl.rectTransform;
            lr.anchorMin = new Vector2(0.1f, anchorY);
            lr.anchorMax = new Vector2(0.30f, anchorY + 0.05f);
            lr.offsetMin = Vector2.zero;
            lr.offsetMax = Vector2.zero;

            var track = new GameObject($"Track_{label}", typeof(RectTransform), typeof(Image));
            track.transform.SetParent(parent, false);
            var tr = track.GetComponent<RectTransform>();
            tr.anchorMin = new Vector2(0.31f, anchorY + 0.008f);
            tr.anchorMax = new Vector2(0.90f, anchorY + 0.044f);
            tr.offsetMin = Vector2.zero;
            tr.offsetMax = Vector2.zero;
            var tImg = track.GetComponent<Image>();
            tImg.sprite = NeonSpriteFactory.HudBarTrack;
            tImg.raycastTarget = false;

            var fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fill.transform.SetParent(track.transform, false);
            var fr = fill.GetComponent<RectTransform>();
            fr.anchorMin = Vector2.zero;
            fr.anchorMax = Vector2.one;
            fr.offsetMin = new Vector2(3f, 2f);
            fr.offsetMax = new Vector2(-3f, -2f);
            var fImg = fill.GetComponent<Image>();
            fImg.sprite = NeonSpriteFactory.HudBarFill;
            fImg.color = fillColor;
            fImg.type = Image.Type.Filled;
            fImg.fillMethod = Image.FillMethod.Horizontal;
            fImg.fillAmount = fillRatio;
            fImg.raycastTarget = false;
        }

        private void BuildShopPanel()
        {
            _shopPanel = CreatePanel("ShopPanel", RootCanvas.transform, new Color(0.012f, 0.018f, 0.05f, 0.97f));
            StretchFull(_shopPanel.GetComponent<RectTransform>());

            var sub = CreateText("ShopSub", _shopPanel.transform, "// KALICI ÇEKİRDEK GELİŞTİRME MERKEZİ //", 20, new Color(1f, 0.84f, 0.22f, 0.85f), TextAnchor.MiddleCenter);
            sub.fontStyle = FontStyle.Bold;
            var subR = sub.rectTransform;
            subR.anchorMin = new Vector2(0.5f, 1f);
            subR.anchorMax = new Vector2(0.5f, 1f);
            subR.pivot = new Vector2(0.5f, 1f);
            subR.anchoredPosition = new Vector2(0f, -28f);
            subR.sizeDelta = new Vector2(960f, 32f);

            var title = CreateText("ShopTitle", _shopPanel.transform, "KALICI MAĞAZA — VEX-09", 44, Color.white, TextAnchor.MiddleCenter);
            title.fontStyle = FontStyle.Bold;
            AddShadow(title.gameObject);
            _shopTitleText = title;
            var tRect = title.rectTransform;
            tRect.anchorMin = new Vector2(0.5f, 1f);
            tRect.anchorMax = new Vector2(0.5f, 1f);
            tRect.pivot = new Vector2(0.5f, 1f);
            tRect.anchoredPosition = new Vector2(0f, -60f);
            tRect.sizeDelta = new Vector2(1100f, 60f);

            // Chamfered Gold Credit Pill (Top-Right)
            var crPill = new GameObject("ShopCrPill", typeof(RectTransform), typeof(Image));
            crPill.transform.SetParent(_shopPanel.transform, false);
            var cpR = crPill.GetComponent<RectTransform>();
            cpR.anchorMin = new Vector2(1f, 1f);
            cpR.anchorMax = new Vector2(1f, 1f);
            cpR.pivot = new Vector2(1f, 1f);
            cpR.anchoredPosition = new Vector2(-44f, -44f);
            cpR.sizeDelta = new Vector2(230f, 56f);
            var cpImg = crPill.GetComponent<Image>();
            cpImg.sprite = NeonSpriteFactory.HudPill;
            cpImg.color = new Color(1f, 0.82f, 0.18f, 1f);
            cpImg.raycastTarget = false;

            _shopCreditsText = CreateText("ShopCR", crPill.transform, "0 CR", 26, new Color(1f, 0.9f, 0.25f), TextAnchor.MiddleCenter);
            _shopCreditsText.fontStyle = FontStyle.Bold;
            AddShadow(_shopCreditsText.gameObject);
            StretchFull(_shopCreditsText.rectTransform);

            var rowsGo = new GameObject("ShopRows", typeof(RectTransform), typeof(VerticalLayoutGroup));
            rowsGo.transform.SetParent(_shopPanel.transform, false);
            var rRect = rowsGo.GetComponent<RectTransform>();
            rRect.anchorMin = new Vector2(0.5f, 0.5f);
            rRect.anchorMax = new Vector2(0.5f, 0.5f);
            rRect.anchoredPosition = new Vector2(0f, -10f);
            rRect.sizeDelta = new Vector2(1420f, 660f);

            var vlg = rowsGo.GetComponent<VerticalLayoutGroup>();
            vlg.spacing = 16f;
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = true;
            _shopRowsContainer = rowsGo.transform;

            var backGo = new GameObject("ShopBack", typeof(RectTransform), typeof(Image), typeof(Button));
            backGo.transform.SetParent(_shopPanel.transform, false);
            var bkRect = backGo.GetComponent<RectTransform>();
            bkRect.anchorMin = new Vector2(0.5f, 0f);
            bkRect.anchorMax = new Vector2(0.5f, 0f);
            bkRect.pivot = new Vector2(0.5f, 0f);
            bkRect.anchoredPosition = new Vector2(0f, 34f);
            bkRect.sizeDelta = new Vector2(320f, 64f);
            var bkImg = backGo.GetComponent<Image>();
            bkImg.sprite = NeonSpriteFactory.HudPill;
            bkImg.color = new Color(0f, 0.92f, 1f, 1f);
            backGo.GetComponent<Button>().onClick.AddListener(CloseSubMenus);
            var bkTxt = CreateText("BackLbl", backGo.transform, "←  ANA MENÜYE DÖN", 23, Color.white, TextAnchor.MiddleCenter);
            bkTxt.fontStyle = FontStyle.Bold;
            StretchFull(bkTxt.rectTransform);

            _shopPanel.SetActive(false);
        }

        private void OpenShop()
        {
            _mainMenuPanel.SetActive(false);
            RefreshShopUI();
            _shopPanel.SetActive(true);
        }

        private void RefreshShopUI()
        {
            var activeDef = NeonMetaProgression.GetShipDef(NeonMetaProgression.SelectedShip);
            if (_shopTitleText != null)
            {
                _shopTitleText.text = $"KALICI MAĞAZA  —  {activeDef.Name}";
                _shopTitleText.color = Color.Lerp(Color.white, activeDef.AccentColor, 0.35f);
            }
            _shopCreditsText.text = $"{NeonMetaProgression.Credits} CR";
            for (int i = _shopRowsContainer.childCount - 1; i >= 0; i--)
                Destroy(_shopRowsContainer.GetChild(i).gameObject);

            AddShopRow(ShopUpgradeType.Hull, UpgradeId.NanoRepair, "GÖVDE ZIRHI", "Başlangıç Maksimum Can", "+18 Can", new Color(1f, 0.25f, 0.5f));
            AddShopRow(ShopUpgradeType.Damage, UpgradeId.LaserPower, "PLAZMA ÇEKİRDEĞİ", "Başlangıç Lazer Hasarı", "+%12 Hasar", new Color(0f, 0.95f, 1f));
            AddShopRow(ShopUpgradeType.Speed, UpgradeId.CyberDash, "İTİCİ MOTORLAR", "Kalıcı Hareket Hızı", "+%6 Hız", new Color(0.28f, 0.85f, 1f));
            AddShopRow(ShopUpgradeType.Magnet, UpgradeId.MagnetField, "MIKNATIS BOBİNİ", "Kalıcı XP Çekim Alanı", "+%16 Menzil", new Color(0.15f, 1f, 0.65f));
            AddShopRow(ShopUpgradeType.Greed, UpgradeId.CritSystem, "KREDİ ÇARPANI", "Tüm Kaynaklardan Neon Kredi", "+%18 Kredi", new Color(1f, 0.82f, 0.18f));
        }

        private void AddShopRow(ShopUpgradeType type, UpgradeId iconId, string title, string subtitle, string perLevelBonus, Color accent)
        {
            int lv = NeonMetaProgression.GetUpgradeLevel(type);
            int maxLv = NeonMetaProgression.MaxUpgradeLevel;
            int cost = NeonMetaProgression.GetUpgradeCost(type);
            bool isMax = lv >= maxLv;
            bool canAfford = !isMax && NeonMetaProgression.Credits >= cost;

            var row = new GameObject($"ShopRow_{type}", typeof(RectTransform), typeof(Image));
            row.transform.SetParent(_shopRowsContainer, false);
            var rowImg = row.GetComponent<Image>();
            rowImg.sprite = NeonSpriteFactory.HudPill;
            rowImg.color = accent;

            // Hexagonal Icon on the left!
            var hexGo = new GameObject("IconHex", typeof(RectTransform), typeof(Image));
            hexGo.transform.SetParent(row.transform, false);
            var hr = hexGo.GetComponent<RectTransform>();
            hr.anchorMin = new Vector2(0.02f, 0.5f);
            hr.anchorMax = new Vector2(0.02f, 0.5f);
            hr.pivot = new Vector2(0f, 0.5f);
            hr.sizeDelta = new Vector2(86f, 86f);
            var hexImg = hexGo.GetComponent<Image>();
            hexImg.sprite = NeonSpriteFactory.HexBadge;
            hexImg.color = accent;
            hexImg.raycastTarget = false;

            var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            iconGo.transform.SetParent(hexGo.transform, false);
            var ir = iconGo.GetComponent<RectTransform>();
            ir.anchorMin = new Vector2(0.22f, 0.22f);
            ir.anchorMax = new Vector2(0.78f, 0.78f);
            ir.offsetMin = Vector2.zero;
            ir.offsetMax = Vector2.zero;
            var iImg = iconGo.GetComponent<Image>();
            iImg.sprite = NeonSpriteFactory.GetUpgradeIcon(iconId);
            iImg.color = Color.Lerp(accent, Color.white, 0.55f);
            iImg.raycastTarget = false;

            // Title & Bonus text
            var tTxt = CreateText("Title", row.transform, title, 28, Color.white, TextAnchor.MiddleLeft);
            tTxt.fontStyle = FontStyle.Bold;
            AddShadow(tTxt.gameObject);
            var tr = tTxt.rectTransform;
            tr.anchorMin = new Vector2(0.10f, 0.50f);
            tr.anchorMax = new Vector2(0.46f, 0.92f);
            tr.offsetMin = Vector2.zero;
            tr.offsetMax = Vector2.zero;

            var dTxt = CreateText("Desc", row.transform, $"{subtitle}  •  Seviye başına {perLevelBonus}", 19, new Color(0.8f, 0.88f, 0.96f), TextAnchor.MiddleLeft);
            var dr = dTxt.rectTransform;
            dr.anchorMin = new Vector2(0.10f, 0.10f);
            dr.anchorMax = new Vector2(0.50f, 0.50f);
            dr.offsetMin = Vector2.zero;
            dr.offsetMax = Vector2.zero;

            // Real Slanted Segmented Progress Bar (Level 0..5)
            var lvLbl = CreateText("LvLbl", row.transform, isMax ? "MAKSİMUM SEVİYE" : $"SEVİYE {lv} / {maxLv}", 20, accent, TextAnchor.MiddleLeft);
            lvLbl.fontStyle = FontStyle.Bold;
            var llr = lvLbl.rectTransform;
            llr.anchorMin = new Vector2(0.51f, 0.54f);
            llr.anchorMax = new Vector2(0.76f, 0.88f);
            llr.offsetMin = Vector2.zero;
            llr.offsetMax = Vector2.zero;

            var barTrack = new GameObject("LvTrack", typeof(RectTransform), typeof(Image));
            barTrack.transform.SetParent(row.transform, false);
            var btr = barTrack.GetComponent<RectTransform>();
            btr.anchorMin = new Vector2(0.51f, 0.18f);
            btr.anchorMax = new Vector2(0.76f, 0.48f);
            btr.offsetMin = Vector2.zero;
            btr.offsetMax = Vector2.zero;
            barTrack.GetComponent<Image>().sprite = NeonSpriteFactory.HudBarTrack;
            barTrack.GetComponent<Image>().raycastTarget = false;

            var barFill = new GameObject("LvFill", typeof(RectTransform), typeof(Image));
            barFill.transform.SetParent(barTrack.transform, false);
            var bfr = barFill.GetComponent<RectTransform>();
            bfr.anchorMin = Vector2.zero;
            bfr.anchorMax = Vector2.one;
            bfr.offsetMin = new Vector2(4f, 3f);
            bfr.offsetMax = new Vector2(-4f, -3f);
            var bfImg = barFill.GetComponent<Image>();
            bfImg.sprite = NeonSpriteFactory.HudBarFill;
            bfImg.color = accent;
            bfImg.type = Image.Type.Filled;
            bfImg.fillMethod = Image.FillMethod.Horizontal;
            bfImg.fillAmount = (float)lv / maxLv;
            bfImg.raycastTarget = false;

            // Buy Button
            Color btnCol = isMax ? new Color(0.14f, 0.78f, 0.52f)
                : (canAfford ? accent : new Color(0.16f, 0.2f, 0.28f));

            var buyGo = CreatePanel("BuyBtn", row.transform, btnCol);
            var br = buyGo.GetComponent<RectTransform>();
            br.anchorMin = new Vector2(0.79f, 0.18f);
            br.anchorMax = new Vector2(0.97f, 0.82f);
            br.offsetMin = Vector2.zero;
            br.offsetMax = Vector2.zero;
            if (!canAfford && !isMax) AddOutline(buyGo, new Color(0.35f, 0.42f, 0.55f, 0.7f), 1.5f);

            var btn = buyGo.AddComponent<Button>();
            btn.interactable = canAfford;
            btn.onClick.AddListener(() =>
            {
                if (NeonMetaProgression.TryBuyUpgrade(type))
                {
                    NeonAudio.Instance?.PlayLevelUp();
                    RefreshShopUI();
                }
            });

            string label = isMax ? "✓ MAX" : $"YÜKSELT\n{cost} CR";
            var bTxt = CreateText("BuyTxt", buyGo.transform, label, 21, (canAfford || isMax) ? new Color(0.02f, 0.04f, 0.09f) : new Color(0.58f, 0.65f, 0.75f), TextAnchor.MiddleCenter);
            bTxt.fontStyle = FontStyle.Bold;
            StretchFull(bTxt.rectTransform);
        }

        private void CloseSubMenus()
        {
            _hangarPanel.SetActive(false);
            _shopPanel.SetActive(false);
            RefreshMainMenuStats();
            _mainMenuPanel.SetActive(true);
        }

        private void RefreshMainMenuStats()
        {
            if (_menuCreditsText != null) _menuCreditsText.text = $"{NeonMetaProgression.Credits} CR";
            if (_menuActiveShipText != null)
            {
                var def = NeonMetaProgression.GetShipDef(NeonMetaProgression.SelectedShip);
                _menuActiveShipText.text = $"GEMİ: {def.Name}";
                _menuActiveShipText.color = def.AccentColor;
            }
        }

        private void BuildGameOverPanel()
        {
            _gameOverPanel = CreatePanel("GameOverPanel", RootCanvas.transform, new Color(0.015f, 0.02f, 0.055f, 0.94f));
            StretchFull(_gameOverPanel.GetComponent<RectTransform>());

            var box = new GameObject("Box", typeof(RectTransform), typeof(Image));
            box.transform.SetParent(_gameOverPanel.transform, false);
            var boxRect = box.GetComponent<RectTransform>();
            boxRect.anchorMin = new Vector2(0.5f, 0.5f);
            boxRect.anchorMax = new Vector2(0.5f, 0.5f);
            boxRect.sizeDelta = new Vector2(920f, 620f);
            box.GetComponent<Image>().sprite = NeonSpriteFactory.CyberCardBg;

            var border = new GameObject("GO_Border", typeof(RectTransform), typeof(Image));
            border.transform.SetParent(box.transform, false);
            StretchFull(border.GetComponent<RectTransform>());
            var bImg = border.GetComponent<Image>();
            bImg.sprite = NeonSpriteFactory.CyberCardBorder;
            bImg.color = new Color(1f, 0.18f, 0.48f);
            bImg.raycastTarget = false;

            var title = CreateText("GO_Title", box.transform, "SİSTEM ÇÖKTÜ", 52, new Color(1f, 0.2f, 0.5f), TextAnchor.MiddleCenter);
            title.fontStyle = FontStyle.Bold;
            var tRect = title.rectTransform;
            tRect.anchorMin = new Vector2(0.5f, 1f);
            tRect.anchorMax = new Vector2(0.5f, 1f);
            tRect.pivot = new Vector2(0.5f, 1f);
            tRect.anchoredPosition = new Vector2(0f, -38f);
            tRect.sizeDelta = new Vector2(760f, 72f);

            _gameOverStatsText = CreateText("GO_Stats", box.transform, "", 28, Color.white, TextAnchor.MiddleCenter);
            var sRect = _gameOverStatsText.rectTransform;
            sRect.anchorMin = new Vector2(0.5f, 0.5f);
            sRect.anchorMax = new Vector2(0.5f, 0.5f);
            sRect.anchoredPosition = new Vector2(0f, 65f);
            sRect.sizeDelta = new Vector2(820f, 220f);

            // Rewarded Ad Button 1: Revive (Emerald)
            _reviveAdBtn = CreatePanel("ReviveAdBtn", box.transform, new Color(0.12f, 0.88f, 0.55f, 1f));
            var rvRect = _reviveAdBtn.GetComponent<RectTransform>();
            rvRect.anchorMin = new Vector2(0.5f, 0f);
            rvRect.anchorMax = new Vector2(0.5f, 0f);
            rvRect.pivot = new Vector2(0.5f, 0f);
            rvRect.anchoredPosition = new Vector2(-210f, 140f);
            rvRect.sizeDelta = new Vector2(390f, 74f);
            _reviveAdBtn.AddComponent<Button>().onClick.AddListener(() => GameManager.Instance?.RequestReviveWithAd());
            var rvTxt = CreateText("ReviveLbl", _reviveAdBtn.transform, "▶  REKLAM: CANLAN", 24, new Color(0.02f, 0.05f, 0.1f), TextAnchor.MiddleCenter);
            rvTxt.fontStyle = FontStyle.Bold;
            StretchFull(rvTxt.rectTransform);

            // Rewarded Ad Button 2: 2x Credits (Gold)
            _doubleCrAdBtn = CreatePanel("DoubleCrAdBtn", box.transform, new Color(1f, 0.82f, 0.18f, 1f));
            var dcRect = _doubleCrAdBtn.GetComponent<RectTransform>();
            dcRect.anchorMin = new Vector2(0.5f, 0f);
            dcRect.anchorMax = new Vector2(0.5f, 0f);
            dcRect.pivot = new Vector2(0.5f, 0f);
            dcRect.anchoredPosition = new Vector2(210f, 140f);
            dcRect.sizeDelta = new Vector2(390f, 74f);
            _doubleCrAdBtn.AddComponent<Button>().onClick.AddListener(() => GameManager.Instance?.RequestDoubleCreditsWithAd());
            _doubleCrAdLabel = CreateText("DoubleCrLbl", _doubleCrAdBtn.transform, "▶  REKLAM: 2x KREDİ", 24, new Color(0.02f, 0.05f, 0.1f), TextAnchor.MiddleCenter);
            _doubleCrAdLabel.fontStyle = FontStyle.Bold;
            StretchFull(_doubleCrAdLabel.rectTransform);

            // Main Restart / Menu Button
            var btnGo = CreatePanel("RestartBtn", box.transform, new Color(0f, 0.88f, 0.98f, 1f));
            var btnRect = btnGo.GetComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0.5f, 0f);
            btnRect.anchorMax = new Vector2(0.5f, 0f);
            btnRect.pivot = new Vector2(0.5f, 0f);
            btnRect.anchoredPosition = new Vector2(0f, 42f);
            btnRect.sizeDelta = new Vector2(420f, 76f);

            btnGo.AddComponent<Button>().onClick.AddListener(() => GameManager.Instance?.RestartGame());
            var btnText = CreateText("BtnLabel", btnGo.transform, "ANA MENÜ / TEKRAR OYNA", 27, new Color(0.03f, 0.05f, 0.1f), TextAnchor.MiddleCenter);
            btnText.fontStyle = FontStyle.Bold;
            StretchFull(btnText.rectTransform);

            _gameOverPanel.SetActive(false);
        }

        public void ShowMainMenu(int bestScore)
        {
            _hudRoot.SetActive(false);
            _menuBestScoreText.text = $"EN İYİ SKOR: {bestScore}";
            RefreshMainMenuStats();
            _mainMenuPanel.SetActive(true);
        }

        public void HideMainMenu()
        {
            _mainMenuPanel.SetActive(false);
            _hudRoot.SetActive(true);
        }

        public void ShowRadioMessage(string speaker, string message, Color accent, float duration = 5f)
        {
            _radioSpeakerText.text = speaker;
            _radioSpeakerText.color = accent;
            _radioBodyText.text = message;
            if (_radioAccentBar != null) _radioAccentBar.color = accent;
            _radioTimer = duration;
            _radioPanel.SetActive(true);
        }

        public void ShowBossWarning(string text)
        {
            _bossWarningText.text = $"⚠  {text}  ⚠";
            _bossWarningTimer = 2.6f;
            _bossWarningBanner.SetActive(true);
        }

        public void ShowGameOver(float elapsed, int sector, int bossesDefeated, int kills, int score, int bestScore, int creditsEarned, bool canRevive = false, bool canDoubleCredits = false)
        {
            _hudRoot.SetActive(false);
            int mins = Mathf.FloorToInt(elapsed / 60f);
            int secs = Mathf.FloorToInt(elapsed % 60f);
            _gameOverStatsText.text =
                $"Ulaşılan Sektör: {sector}   |   Süre: {mins:00}:{secs:00}\n" +
                $"Yenilen Final Boss: {bossesDefeated}   |   İmha: {kills}\n" +
                $"Skor: {score}   |   En İyi Skor: {bestScore}\n" +
                $"Kazanılan Kredi: +{creditsEarned} CR   (Toplam: {NeonMetaProgression.Credits} CR)";

            if (_reviveAdBtn != null) _reviveAdBtn.SetActive(canRevive);
            if (_doubleCrAdBtn != null)
            {
                _doubleCrAdBtn.SetActive(canDoubleCredits);
                if (_doubleCrAdLabel != null) _doubleCrAdLabel.text = $"▶  REKLAM: 2x (+{creditsEarned} CR)";
            }

            _gameOverPanel.SetActive(true);
        }

        public void HideGameOver()
        {
            _gameOverPanel.SetActive(false);
            _hudRoot.SetActive(true);
        }

        private void Update()
        {
            if (_radioTimer > 0f)
            {
                _radioTimer -= Time.unscaledDeltaTime;
                if (_radioTimer <= 0f) _radioPanel.SetActive(false);
            }

            if (_bossWarningTimer > 0f)
            {
                _bossWarningTimer -= Time.unscaledDeltaTime;
                if (_bossWarningTimer <= 0f) _bossWarningBanner.SetActive(false);
            }

            var player = PlayerController.Instance;
            var gm = GameManager.Instance;
            if (player == null || gm == null) return;

            _levelBadgeText.text = player.Level.ToString();
            _hpFill.fillAmount = Mathf.Clamp01(player.CurrentHealth / Mathf.Max(1f, player.MaxHealth));
            _hpText.text = $"{Mathf.CeilToInt(player.CurrentHealth)} / {Mathf.CeilToInt(player.MaxHealth)}";
            _xpFill.fillAmount = Mathf.Clamp01((float)player.CurrentXp / Mathf.Max(1, player.XpToNextLevel));

            _scoreValueText.text = gm.Score.ToString("N0");
            _killsValueText.text = $"İMHA: {gm.Kills}";

            if (gm.ComboCount >= 3)
            {
                string mult = gm.ComboMultiplier > 1 ? $"  •  x{gm.ComboMultiplier}" : "";
                _comboText.text = $"{gm.ComboCount} COMBO{mult}";
                _comboText.color = gm.ComboMultiplier >= 3 ? new Color(1f, 0.35f, 0.2f)
                    : (gm.ComboMultiplier == 2 ? new Color(1f, 0.88f, 0.18f) : new Color(0.2f, 1f, 0.95f));
            }
            else
            {
                _comboText.text = "";
            }

            _dashFill.fillAmount = player.DashCooldownRatio;
            _dashLabel.color = player.DashCooldownRatio >= 0.99f ? Color.white : new Color(0.5f, 0.6f, 0.7f);

            var boss = BossAI.ActiveBoss;
            if (boss != null)
            {
                _sectorBarGroup.SetActive(false);
                _bossBarGroup.SetActive(true);
                string phase = boss.IsEnraged ? " [FAZ 2]" : "";
                _bossNameText.text = $"{boss.BossName}  [LV.{boss.EnemyLevel}]{phase}";
                float pct = Mathf.Clamp01(boss.CurrentHealth / Mathf.Max(1f, boss.MaxHealth));
                _bossHpFill.fillAmount = pct;
                _bossHpText.text = $"{Mathf.CeilToInt(pct * 100f)}%";
            }
            else
            {
                _bossBarGroup.SetActive(false);
                _sectorBarGroup.SetActive(true);
                int mins = Mathf.FloorToInt(gm.ElapsedTime / 60f);
                int secs = Mathf.FloorToInt(gm.ElapsedTime % 60f);
                _sectorTitleText.text = gm.GetSectorTitle(gm.CurrentSector);
                _sectorTitleText.color = gm.GetSectorThemeColor(gm.CurrentSector);
                _sectorTimerText.text = $"{mins:00}:{secs:00}";
                _sectorProgressFill.fillAmount = gm.SectorProgressRatio;
                _sectorProgressFill.color = gm.GetSectorThemeColor(gm.CurrentSector);
            }
        }

        private GameObject CreatePanel(string name, Transform parent, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.sprite = NeonSpriteFactory.SolidRect;
            img.color = color;
            return go;
        }

        private Text CreateText(string name, Transform parent, string content, int fontSize, Color color, TextAnchor alignment)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var txt = go.GetComponent<Text>();
            txt.font = _font;
            txt.text = content;
            txt.fontSize = fontSize;
            txt.color = color;
            txt.alignment = alignment;
            txt.horizontalOverflow = HorizontalWrapMode.Wrap;
            txt.verticalOverflow = VerticalWrapMode.Overflow;
            txt.raycastTarget = false;
            return txt;
        }

        private static Outline AddOutline(GameObject go, Color color, float thickness)
        {
            var outline = go.AddComponent<Outline>();
            outline.effectColor = color;
            outline.effectDistance = new Vector2(thickness, -thickness);
            return outline;
        }

        private static void AddShadow(GameObject go)
        {
            var sh = go.AddComponent<Shadow>();
            sh.effectColor = new Color(0f, 0f, 0f, 0.78f);
            sh.effectDistance = new Vector2(2f, -2f);
        }

        private static void StretchFull(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }

    public class CyberCardAnimator : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        private float _delay;
        private float _elapsed;
        private float _targetScale = 1f;
        private bool _hovered;

        public void Init(float delay)
        {
            _delay = delay;
            _elapsed = 0f;
            transform.localScale = Vector3.one * 0.78f;
        }

        private void Update()
        {
            _elapsed += Time.unscaledDeltaTime;
            if (_elapsed < _delay) return;

            float dt = Time.unscaledDeltaTime;
            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one * _targetScale, Mathf.Clamp01(dt * 14f));
        }

        public void OnPointerEnter(PointerEventData eventData) { _hovered = true; _targetScale = 1.05f; }
        public void OnPointerExit(PointerEventData eventData) { _hovered = false; _targetScale = 1f; }
        public void OnPointerDown(PointerEventData eventData) { _targetScale = 0.96f; }
        public void OnPointerUp(PointerEventData eventData) { _targetScale = _hovered ? 1.05f : 1f; }
    }

    /// <summary>
    /// Gently rotates and floats ship previews inside the Cyber Hangar hologram pad.
    /// </summary>
    public class ShipHologramSpinner : MonoBehaviour
    {
        private void Update()
        {
            transform.Rotate(0f, 0f, 22f * Time.unscaledDeltaTime);
            float pulse = 1f + Mathf.Sin(Time.unscaledTime * 3.2f) * 0.05f;
            transform.localScale = Vector3.one * pulse;
        }
    }
}
