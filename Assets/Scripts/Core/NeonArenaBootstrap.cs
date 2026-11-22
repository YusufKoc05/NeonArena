using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

namespace NeonArena
{
    /// <summary>
    /// Bootstraps the entire playable Cyberpunk Neon Arena at runtime and supports
    /// dynamic sector neon color themes.
    /// </summary>
    public class NeonArenaBootstrap : MonoBehaviour
    {
        public static NeonArenaBootstrap Instance { get; private set; }

        public Vector2 ArenaHalfBounds = new Vector2(24f, 16f);

        private SpriteRenderer _floorSr;
        private readonly List<SpriteRenderer> _wallRenderers = new List<SpriteRenderer>();

        private void Awake()
        {
            Instance = this;
            EnsureEventSystem();
            NeonAudio.Create();
            NeonAdsManager.Create();
            BuildArenaFloorAndWalls();

            var gmGo = new GameObject("GameManager");
            gmGo.AddComponent<GameManager>();

            var player = PlayerController.Spawn(Vector3.zero);
            player.ArenaHalfBounds = ArenaHalfBounds;

            var spawnerGo = new GameObject("WaveSpawner");
            var spawner = spawnerGo.AddComponent<WaveSpawner>();
            spawner.ArenaHalfBounds = ArenaHalfBounds;

            var hud = NeonHUD.Create();
            VirtualJoystick.Create(hud.RootCanvas);

            SetupCamera(player.transform);
        }

        public void ApplySectorTheme(Color neonAccent)
        {
            if (_floorSr != null)
            {
                // Subtle tint on the dark grid so each sector has a distinct cyberpunk mood
                _floorSr.color = Color.Lerp(Color.white, neonAccent, 0.35f);
            }

            for (int i = 0; i < _wallRenderers.Count; i++)
            {
                if (_wallRenderers[i] != null)
                {
                    _wallRenderers[i].color = neonAccent;
                }
            }
        }

        private void SetupCamera(Transform target)
        {
            var cam = Camera.main;
            if (cam == null)
            {
                var camGo = new GameObject("Main Camera");
                camGo.tag = "MainCamera";
                cam = camGo.AddComponent<Camera>();
                cam.orthographic = true;
            }

            cam.orthographic = true;
            cam.orthographicSize = 7.4f;
            cam.backgroundColor = new Color(0.02f, 0.025f, 0.06f, 1f);
            cam.transform.position = new Vector3(0f, 0f, -10f);

            var follow = cam.GetComponent<NeonCameraFollow>();
            if (follow == null) follow = cam.gameObject.AddComponent<NeonCameraFollow>();
            follow.Target = target;
            follow.ArenaHalfBounds = new Vector2(ArenaHalfBounds.x - 4f, ArenaHalfBounds.y - 2.5f);
        }

        private void BuildArenaFloorAndWalls()
        {
            var arenaRoot = new GameObject("NeonArena_Environment");

            // Tiled dark cyberpunk grid floor
            var floor = new GameObject("GridFloor");
            floor.transform.SetParent(arenaRoot.transform, false);
            _floorSr = floor.AddComponent<SpriteRenderer>();
            _floorSr.sprite = NeonSpriteFactory.GridTile;
            _floorSr.drawMode = SpriteDrawMode.Tiled;
            _floorSr.size = new Vector2(ArenaHalfBounds.x * 2f + 2f, ArenaHalfBounds.y * 2f + 2f);
            _floorSr.sortingOrder = -20;

            // 4 glowing neon boundary walls
            Color borderColor = new Color(0f, 0.92f, 1f, 0.95f);
            float w = ArenaHalfBounds.x * 2f;
            float h = ArenaHalfBounds.y * 2f;
            float t = 0.26f;

            _wallRenderers.Clear();
            _wallRenderers.Add(CreateWall(arenaRoot.transform, "Wall_Top", new Vector3(0f, ArenaHalfBounds.y, 0f), new Vector2(w + t, t), borderColor));
            _wallRenderers.Add(CreateWall(arenaRoot.transform, "Wall_Bottom", new Vector3(0f, -ArenaHalfBounds.y, 0f), new Vector2(w + t, t), borderColor));
            _wallRenderers.Add(CreateWall(arenaRoot.transform, "Wall_Left", new Vector3(-ArenaHalfBounds.x, 0f, 0f), new Vector2(t, h + t), borderColor));
            _wallRenderers.Add(CreateWall(arenaRoot.transform, "Wall_Right", new Vector3(ArenaHalfBounds.x, 0f, 0f), new Vector2(t, h + t), borderColor));
        }

        private static SpriteRenderer CreateWall(Transform parent, string name, Vector3 pos, Vector2 size, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            go.transform.localScale = new Vector3(size.x, size.y, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = NeonSpriteFactory.SolidRect;
            sr.color = color;
            sr.sortingOrder = -5;
            return sr;
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindFirstObjectByType<EventSystem>() != null) return;

            var esGo = new GameObject("EventSystem");
            esGo.AddComponent<EventSystem>();
#if ENABLE_INPUT_SYSTEM
            esGo.AddComponent<InputSystemUIInputModule>();
#else
            esGo.AddComponent<StandaloneInputModule>();
#endif
        }
    }
}
