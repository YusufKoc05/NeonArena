using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace NeonArena
{
    /// <summary>
    /// Mobile-first dynamic floating neon joystick + full PC WASD/Arrow key support.
    /// Touch or click anywhere on the left side of the screen to steer smoothly.
    /// </summary>
    public class VirtualJoystick : MonoBehaviour
    {
        public static VirtualJoystick Instance { get; private set; }

        public Vector2 Direction { get; private set; }
        public float MaxRadius = 95f;

        private RectTransform _bgRect;
        private RectTransform _handleRect;
        private CanvasGroup _canvasGroup;
        private Canvas _rootCanvas;

        private bool _isDragging;
        private Vector2 _startScreenPos;

        public static VirtualJoystick Create(Canvas parentCanvas)
        {
            var root = new GameObject("VirtualJoystick", typeof(RectTransform), typeof(CanvasGroup));
            root.transform.SetParent(parentCanvas.transform, false);

            var cg = root.GetComponent<CanvasGroup>();
            cg.alpha = 0f;
            cg.blocksRaycasts = false;
            cg.interactable = false;

            // Outer glowing ring
            var bgGo = new GameObject("Ring", typeof(RectTransform), typeof(Image));
            bgGo.transform.SetParent(root.transform, false);
            var bgImg = bgGo.GetComponent<Image>();
            bgImg.sprite = NeonSpriteFactory.Ring;
            bgImg.color = new Color(0f, 0.95f, 1f, 0.65f);
            bgImg.raycastTarget = false;
            var bgRect = bgGo.GetComponent<RectTransform>();
            bgRect.sizeDelta = new Vector2(190f, 190f);

            // Inner glowing knob
            var handleGo = new GameObject("Knob", typeof(RectTransform), typeof(Image));
            handleGo.transform.SetParent(bgGo.transform, false);
            var handleImg = handleGo.GetComponent<Image>();
            handleImg.sprite = NeonSpriteFactory.Circle;
            handleImg.color = new Color(0.2f, 1f, 1f, 0.9f);
            handleImg.raycastTarget = false;
            var handleRect = handleGo.GetComponent<RectTransform>();
            handleRect.sizeDelta = new Vector2(76f, 76f);

            var joy = root.AddComponent<VirtualJoystick>();
            joy._bgRect = bgRect;
            joy._handleRect = handleRect;
            joy._canvasGroup = cg;
            joy._rootCanvas = parentCanvas;
            return joy;
        }

        private void Awake()
        {
            Instance = this;
        }

        private void Update()
        {
            if (GameManager.Instance != null && !GameManager.Instance.IsPlaying)
            {
                ResetJoystick();
                return;
            }

            Vector2 keyboardInput = ReadKeyboardInput();
            Vector2 pointerInput = ReadPointerJoystick();

            Direction = pointerInput.sqrMagnitude > 0.001f ? pointerInput : keyboardInput;
        }

        private Vector2 ReadKeyboardInput()
        {
            Vector2 kb = Vector2.zero;
#if ENABLE_INPUT_SYSTEM
            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) kb.y += 1f;
                if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) kb.y -= 1f;
                if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) kb.x += 1f;
                if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) kb.x -= 1f;
            }
#else
            kb.x = Input.GetAxisRaw("Horizontal");
            kb.y = Input.GetAxisRaw("Vertical");
#endif
            if (kb.sqrMagnitude > 1f) kb.Normalize();
            return kb;
        }

        private Vector2 ReadPointerJoystick()
        {
            bool pressedThisFrame = false;
            bool isHeld = false;
            Vector2 screenPos = Vector2.zero;

#if ENABLE_INPUT_SYSTEM
            var ts = Touchscreen.current;
            if (ts != null && ts.primaryTouch.press.isPressed)
            {
                pressedThisFrame = ts.primaryTouch.press.wasPressedThisFrame;
                isHeld = true;
                screenPos = ts.primaryTouch.position.ReadValue();
            }
            else if (Pointer.current != null && Pointer.current.press.isPressed)
            {
                pressedThisFrame = Pointer.current.press.wasPressedThisFrame;
                isHeld = true;
                screenPos = Pointer.current.position.ReadValue();
            }
#else
            if (Input.touchCount > 0)
            {
                var t = Input.GetTouch(0);
                pressedThisFrame = t.phase == TouchPhase.Began;
                isHeld = t.phase != TouchPhase.Ended && t.phase != TouchPhase.Canceled;
                screenPos = t.position;
            }
            else if (Input.GetMouseButton(0))
            {
                pressedThisFrame = Input.GetMouseButtonDown(0);
                isHeld = true;
                screenPos = Input.mousePosition;
            }
#endif

            if (pressedThisFrame)
            {
                bool overUi = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
                if (!overUi)
                {
                    _isDragging = true;
                    _startScreenPos = screenPos;
                    PositionRingAtScreenPoint(screenPos);
                    _canvasGroup.alpha = 1f;
                }
            }

            if (_isDragging && isHeld)
            {
                float scale = _rootCanvas != null ? _rootCanvas.scaleFactor : 1f;
                Vector2 delta = (screenPos - _startScreenPos) / Mathf.Max(0.01f, scale);
                Vector2 clamped = Vector2.ClampMagnitude(delta, MaxRadius);
                _handleRect.anchoredPosition = clamped;
                return clamped / MaxRadius;
            }

            if (_isDragging && !isHeld)
            {
                ResetJoystick();
            }

            return Vector2.zero;
        }

        private void PositionRingAtScreenPoint(Vector2 screenPos)
        {
            if (_rootCanvas == null) return;
            var canvasRect = _rootCanvas.transform as RectTransform;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, null, out Vector2 localPos))
            {
                _bgRect.anchoredPosition = localPos;
                _handleRect.anchoredPosition = Vector2.zero;
            }
        }

        private void ResetJoystick()
        {
            _isDragging = false;
            Direction = Vector2.zero;
            if (_canvasGroup != null) _canvasGroup.alpha = 0f;
            if (_handleRect != null) _handleRect.anchoredPosition = Vector2.zero;
        }
    }
}
