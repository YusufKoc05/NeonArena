#if UNITY_EDITOR
using System.IO;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace NeonArena
{
    /// <summary>
    /// Press F12 during Unity Play Mode to capture crisp, full-resolution, borderless PNG screenshots
    /// straight into docs/screenshots/ for the GitHub README gallery.
    /// </summary>
    public class NeonScreenshotTool : MonoBehaviour
    {
        private static int _counter = 1;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Init()
        {
            if (Object.FindFirstObjectByType<NeonScreenshotTool>() != null) return;
            var go = new GameObject("NeonScreenshotTool");
            DontDestroyOnLoad(go);
            go.AddComponent<NeonScreenshotTool>();
        }

        private void Update()
        {
            bool pressed = false;
#if ENABLE_INPUT_SYSTEM
            var kb = Keyboard.current;
            if (kb != null && kb.f12Key.wasPressedThisFrame) pressed = true;
#else
            if (Input.GetKeyDown(KeyCode.F12)) pressed = true;
#endif
            if (!pressed) return;

            string dir = Path.Combine(Directory.GetParent(Application.dataPath).FullName, "docs", "screenshots");
            Directory.CreateDirectory(dir);

            string filePath;
            do
            {
                filePath = Path.Combine(dir, $"screenshot_{_counter:00}.png");
                _counter++;
            } while (File.Exists(filePath));

            ScreenCapture.CaptureScreenshot(filePath, 1);
            Debug.Log($"[NeonArena] Ekran goruntusu kaydedildi: {filePath}");
        }
    }
}
#endif
