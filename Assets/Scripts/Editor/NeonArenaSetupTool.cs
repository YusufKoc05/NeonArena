#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace NeonArena.Editor
{
    /// <summary>
    /// One-click Unity Editor setup tool for NeonArena.
    /// Menu: NeonArena -> Sahneyi Tek Tikla Kur (Setup Scene)
    /// </summary>
    public static class NeonArenaSetupTool
    {
        [MenuItem("NeonArena/Sahneyi Tek Tikla Kur (Setup Scene)")]
        public static void SetupScene()
        {
            // 1. Configure Landscape orientation for mobile
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;

            // 2. Configure Main Camera for dark cyberpunk neon look + URP Post-Processing
            var cam = Camera.main;
            if (cam == null)
            {
                var camGo = new GameObject("Main Camera");
                camGo.tag = "MainCamera";
                cam = camGo.AddComponent<Camera>();
                camGo.AddComponent<AudioListener>();
            }
            cam.orthographic = true;
            cam.orthographicSize = 7.2f;
            cam.backgroundColor = new Color(0.02f, 0.03f, 0.07f, 1f);
            cam.transform.position = new Vector3(0f, 0f, -10f);

            var urpCam = cam.GetComponent<UniversalAdditionalCameraData>();
            if (urpCam == null) urpCam = cam.gameObject.AddComponent<UniversalAdditionalCameraData>();
            urpCam.renderPostProcessing = true;

            // 3. Create or update Global Volume with URP Bloom for neon glow
            var existingVol = Object.FindFirstObjectByType<Volume>();
            if (existingVol == null)
            {
                var volGo = new GameObject("NeonGlow_GlobalVolume");
                existingVol = volGo.AddComponent<Volume>();
                existingVol.isGlobal = true;
            }

            if (existingVol.sharedProfile == null)
            {
                const string profilePath = "Assets/NeonBloomProfile.asset";
                var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(profilePath);
                if (profile == null)
                {
                    profile = ScriptableObject.CreateInstance<VolumeProfile>();
                    AssetDatabase.CreateAsset(profile, profilePath);
                }

                if (!profile.TryGet<Bloom>(out var bloom))
                {
                    bloom = profile.Add<Bloom>(true);
                }
                bloom.active = true;
                bloom.threshold.overrideState = true;
                bloom.threshold.value = 0.85f;
                bloom.intensity.overrideState = true;
                bloom.intensity.value = 1.15f;
                bloom.scatter.overrideState = true;
                bloom.scatter.value = 0.65f;

                EditorUtility.SetDirty(profile);
                AssetDatabase.SaveAssets();
                existingVol.sharedProfile = profile;
            }

            // 4. Add NeonArenaBootstrap to the scene
            var bootstrap = Object.FindFirstObjectByType<NeonArenaBootstrap>();
            if (bootstrap == null)
            {
                var go = new GameObject("NeonArena_Bootstrap");
                go.AddComponent<NeonArenaBootstrap>();
            }

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();

            EditorUtility.DisplayDialog(
                "NeonArena Hazir!",
                "Sahne, Yatay (Landscape) mobil ayarlar, URP Neon Bloom ve oyun sistemleri kuruldu.\n\nSimdi yukaridaki PLAY tusuna basip oynayabilirsin!\n(PC'de WASD veya fareyle surukleyerek, mobilde dokunmatik joystick ile kontrol edebilirsin.)",
                "Harika!"
            );
        }

        [MenuItem("NeonArena/Kayitlari Sifirla (Reset Save)")]
        public static void ResetSaveData()
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            EditorUtility.DisplayDialog("Kayitlar Sifirlandi", "Tum kredi, gemi ve magaza kayitlari sifirlandi.", "Tamam");
        }

        [MenuItem("NeonArena/Android ve Google Play Ayarlarini Uygula")]
        public static void ConfigureAndroidAndGooglePlay()
        {
            // 1. Product & Identity
            PlayerSettings.productName = "Neon Arena";
            PlayerSettings.companyName = "NeonStudio";
            PlayerSettings.bundleVersion = "1.0.0";
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.neonstudio.neonarena");
            PlayerSettings.Android.bundleVersionCode = 1;

            // 2. Landscape Mobile Orientation
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;

            // 3. Google Play 64-bit (IL2CPP + ARM64) Requirements
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64 | AndroidArchitecture.ARMv7;
            if ((int)PlayerSettings.Android.minSdkVersion < 26)
            {
                PlayerSettings.Android.minSdkVersion = (AndroidSdkVersions)26;
            }
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;

            // 4. Apply App Icon (Assets/Art/AppIcon.jpg)
            const string iconPath = "Assets/Art/AppIcon.jpg";
            var importer = AssetImporter.GetAtPath(iconPath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Default;
                importer.npotScale = TextureImporterNPOTScale.None;
                importer.mipmapEnabled = false;
                importer.SaveAndReimport();
            }

            var iconTex = AssetDatabase.LoadAssetAtPath<Texture2D>(iconPath);
            if (iconTex != null)
            {
                PlayerSettings.SetIcons(NamedBuildTarget.Unknown, new[] { iconTex }, IconKind.Any);
                PlayerSettings.SetIcons(NamedBuildTarget.Android, new[] { iconTex }, IconKind.Any);
            }

            // 5. Ensure active scene is included in Build Settings
            var activeScenePath = EditorSceneManager.GetActiveScene().path;
            if (!string.IsNullOrEmpty(activeScenePath))
            {
                EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(activeScenePath, true) };
            }

            AssetDatabase.SaveAssets();

            EditorUtility.DisplayDialog(
                "Android & Google Play Hazir!",
                "Uygulama ikonu, Landscape kilit, Paket Adi (com.neonstudio.neonarena), IL2CPP ve ARM64 (Google Play zorunlu) ayarlari uygulandi!\n\n" +
                "Simdi:\n" +
                "1) Unity Hub -> Installs -> Unity 6.6 -> Add Modules -> 'Android Build Support' (SDK/NDK + OpenJDK ile) kurulu oldugundan emin ol.\n" +
                "2) Unity'de File -> Build Profiles -> Android -> Switch Platform de.\n" +
                "3) Telefonda test icin direkt Build (APK), Google Play'e yuklemek icin 'Build App Bundle (Google Play)' secip Build (AAB) al!",
                "Super!"
            );
        }

        [MenuItem("NeonArena/Google AdMob SDK Aktif Et (ADMOB_ENABLED)")]
        public static void EnableAdMobSdk()
        {
            if (!AssetDatabase.IsValidFolder("Assets/GoogleMobileAds"))
            {
                EditorUtility.DisplayDialog(
                    "Google Mobile Ads SDK Bulunamadi",
                    "Once resmi Google Mobile Ads Unity eklentisini (GoogleMobileAds-v*.unitypackage) indirip Unity'ye Assets -> Import Package -> Custom Package ile ekle.\n\nSonra tekrar bu menuye tikladiginda gercek AdMob reklamlarin otomatik aktif olacak!",
                    "Anladim"
                );
                return;
            }

            string defines = PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.Android);
            if (!defines.Contains("ADMOB_ENABLED"))
            {
                defines = string.IsNullOrEmpty(defines) ? "ADMOB_ENABLED" : defines + ";ADMOB_ENABLED";
                PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.Android, defines);
            }

            EditorUtility.DisplayDialog(
                "Google AdMob Aktif!",
                "ADMOB_ENABLED aktif edildi! Simdi:\n" +
                "1) Assets -> Google Mobile Ads -> Settings menusune kendi AdMob App ID'ni gir.\n" +
                "2) NeonAdsManager.cs icindeki Rewarded ve Interstitial Ad Unit ID'lerini yayinlamadan once kendi AdMob ID'lerinle degistir.",
                "Harika!"
            );
        }
    }
}
#endif
