using UnityEngine;

namespace PotatoLegends.Core
{
    public class InputManager : MonoBehaviour
    {
        public static InputManager Instance { get; private set; }

        [Header("Input Settings")]
        public float touchSensitivity = 1.0f;
        public float swipeThreshold = 50f;

        // Input state
        private bool isTouching = false;
        private Vector2 touchStartPosition;
        private Vector2 lastTouchPosition;

        // Events
        public System.Action<Vector2> OnTouchStart;
        public System.Action<Vector2> OnTouchEnd;
        public System.Action<Vector2> OnTouchMove;
        public System.Action<Vector2> OnSwipeLeft;
        public System.Action<Vector2> OnSwipeRight;
        public System.Action<Vector2> OnSwipeUp;
        public System.Action<Vector2> OnSwipeDown;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        void Update()
        {
            HandleInput();
        }

        private void HandleInput()
        {
            // Handle mouse input (for editor testing)
            if (Input.GetMouseButtonDown(0))
            {
                Vector2 mousePos = Input.mousePosition;
                OnTouchStart?.Invoke(mousePos);
                isTouching = true;
                touchStartPosition = mousePos;
                lastTouchPosition = mousePos;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                Vector2 mousePos = Input.mousePosition;
                OnTouchEnd?.Invoke(mousePos);
                isTouching = false;
                CheckForSwipe(mousePos);
            }
            else if (Input.GetMouseButton(0))
            {
                Vector2 mousePos = Input.mousePosition;
                OnTouchMove?.Invoke(mousePos);
                lastTouchPosition = mousePos;
            }

            // Handle touch input (for mobile)
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                Vector2 touchPos = touch.position;

                switch (touch.phase)
                {
                    case TouchPhase.Began:
                        OnTouchStart?.Invoke(touchPos);
                        isTouching = true;
                        touchStartPosition = touchPos;
                        lastTouchPosition = touchPos;
                        break;

                    case TouchPhase.Ended:
                    case TouchPhase.Canceled:
                        OnTouchEnd?.Invoke(touchPos);
                        isTouching = false;
                        CheckForSwipe(touchPos);
                        break;

                    case TouchPhase.Moved:
                        OnTouchMove?.Invoke(touchPos);
                        lastTouchPosition = touchPos;
                        break;
                }
            }
        }

        private void CheckForSwipe(Vector2 endPosition)
        {
            Vector2 swipeVector = endPosition - touchStartPosition;
            float swipeDistance = swipeVector.magnitude;

            if (swipeDistance > swipeThreshold)
            {
                Vector2 normalizedSwipe = swipeVector.normalized;

                if (Mathf.Abs(normalizedSwipe.x) > Mathf.Abs(normalizedSwipe.y))
                {
                    // Horizontal swipe
                    if (normalizedSwipe.x > 0)
                        OnSwipeRight?.Invoke(swipeVector);
                    else
                        OnSwipeLeft?.Invoke(swipeVector);
                }
                else
                {
                    // Vertical swipe
                    if (normalizedSwipe.y > 0)
                        OnSwipeUp?.Invoke(swipeVector);
                    else
                        OnSwipeDown?.Invoke(swipeVector);
                }
            }
        }

        public bool IsTouching()
        {
            return isTouching;
        }

        public Vector2 GetTouchPosition()
        {
            if (Input.touchCount > 0)
                return Input.GetTouch(0).position;
            else
                return Input.mousePosition;
        }

        public Vector2 GetTouchDelta()
        {
            if (Input.touchCount > 0)
                return Input.GetTouch(0).deltaPosition;
            else
                return Vector2.zero;
        }
    }
}