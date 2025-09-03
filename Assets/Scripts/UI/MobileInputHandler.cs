using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using PotatoCardGame.Core;

namespace PotatoCardGame.UI
{
    /// <summary>
    /// Handles mobile-specific input including touch gestures, swipes, and multi-touch
    /// Optimized for card game interactions on mobile devices
    /// </summary>
    public class MobileInputHandler : MonoBehaviour
    {
        [Header("Touch Settings")]
        [SerializeField] private float tapThreshold = 0.2f;
        [SerializeField] private float dragThreshold = 50f;
        [SerializeField] private float swipeThreshold = 100f;
        [SerializeField] private float pinchThreshold = 0.1f;
        
        [Header("Haptic Feedback")]
        [SerializeField] private bool enableHaptics = true;
        // Haptic feedback durations (removed unused fields)
        
        // Singleton
        public static MobileInputHandler Instance { get; private set; }
        
        // Touch tracking
        private Dictionary<int, TouchInfo> activeTouches = new Dictionary<int, TouchInfo>();
        private Vector2 lastPinchDistance;
        private bool isPinching = false;
        
        // Events
        public System.Action<Vector2> OnTap;
        public System.Action<Vector2, Vector2> OnDrag;
        public System.Action<Vector2, Vector2> OnSwipe;
        public System.Action<float> OnPinch;
        public System.Action<Vector2> OnLongPress;
        
        private struct TouchInfo
        {
            public Vector2 startPosition;
            public Vector2 currentPosition;
            public float startTime;
            public bool hasMoved;
        }
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void Update()
        {
            HandleInput();
        }
        
        private void HandleInput()
        {
            // Handle mobile touch input
            if (Input.touchCount > 0)
            {
                HandleTouchInput();
            }
            // Handle mouse input for editor testing
            else if (Application.isEditor)
            {
                HandleMouseInput();
            }
        }
        
        #region Touch Input
        
        private void HandleTouchInput()
        {
            // Handle multi-touch
            if (Input.touchCount == 2)
            {
                HandlePinchGesture();
            }
            else if (Input.touchCount == 1)
            {
                HandleSingleTouch();
            }
            
            // Clean up ended touches
            List<int> touchesToRemove = new List<int>();
            foreach (var kvp in activeTouches)
            {
                bool touchExists = false;
                for (int i = 0; i < Input.touchCount; i++)
                {
                    if (Input.GetTouch(i).fingerId == kvp.Key)
                    {
                        touchExists = true;
                        break;
                    }
                }
                
                if (!touchExists)
                {
                    touchesToRemove.Add(kvp.Key);
                }
            }
            
            foreach (int touchId in touchesToRemove)
            {
                activeTouches.Remove(touchId);
            }
        }
        
        private void HandleSingleTouch()
        {
            Touch touch = Input.GetTouch(0);
            int touchId = touch.fingerId;
            
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    OnTouchBegan(touchId, touch.position);
                    break;
                    
                case TouchPhase.Moved:
                    OnTouchMoved(touchId, touch.position);
                    break;
                    
                case TouchPhase.Ended:
                    OnTouchEnded(touchId, touch.position);
                    break;
                    
                case TouchPhase.Canceled:
                    OnTouchCanceled(touchId);
                    break;
            }
        }
        
        private void OnTouchBegan(int touchId, Vector2 position)
        {
            TouchInfo touchInfo = new TouchInfo
            {
                startPosition = position,
                currentPosition = position,
                startTime = Time.time,
                hasMoved = false
            };
            
            activeTouches[touchId] = touchInfo;
            
            // Light haptic feedback
            if (enableHaptics)
            {
                Handheld.Vibrate();
            }
        }
        
        private void OnTouchMoved(int touchId, Vector2 position)
        {
            if (!activeTouches.ContainsKey(touchId)) return;
            
            TouchInfo touchInfo = activeTouches[touchId];
            Vector2 deltaPosition = position - touchInfo.currentPosition;
            
            // Check if moved beyond threshold
            if (!touchInfo.hasMoved)
            {
                float distance = Vector2.Distance(position, touchInfo.startPosition);
                if (distance > dragThreshold)
                {
                    touchInfo.hasMoved = true;
                    OnDrag?.Invoke(touchInfo.startPosition, position);
                }
            }
            else
            {
                OnDrag?.Invoke(touchInfo.currentPosition, position);
            }
            
            touchInfo.currentPosition = position;
            activeTouches[touchId] = touchInfo;
        }
        
        private void OnTouchEnded(int touchId, Vector2 position)
        {
            if (!activeTouches.ContainsKey(touchId)) return;
            
            TouchInfo touchInfo = activeTouches[touchId];
            float touchDuration = Time.time - touchInfo.startTime;
            Vector2 deltaPosition = position - touchInfo.startPosition;
            
            if (!touchInfo.hasMoved && touchDuration < tapThreshold)
            {
                // Tap
                OnTap?.Invoke(position);
                TriggerHaptic(HapticType.Light);
            }
            else if (touchInfo.hasMoved)
            {
                // Check for swipe
                float swipeDistance = deltaPosition.magnitude;
                if (swipeDistance > swipeThreshold)
                {
                    OnSwipe?.Invoke(touchInfo.startPosition, position);
                    TriggerHaptic(HapticType.Medium);
                }
            }
            else if (touchDuration > 0.5f)
            {
                // Long press
                OnLongPress?.Invoke(position);
                TriggerHaptic(HapticType.Heavy);
            }
            
            activeTouches.Remove(touchId);
        }
        
        private void OnTouchCanceled(int touchId)
        {
            activeTouches.Remove(touchId);
        }
        
        #endregion
        
        #region Pinch Gesture
        
        private void HandlePinchGesture()
        {
            Touch touch1 = Input.GetTouch(0);
            Touch touch2 = Input.GetTouch(1);
            
            Vector2 currentDistance = touch1.position - touch2.position;
            
            if (!isPinching)
            {
                isPinching = true;
                lastPinchDistance = currentDistance;
            }
            else
            {
                float deltaDistance = currentDistance.magnitude - lastPinchDistance.magnitude;
                
                if (Mathf.Abs(deltaDistance) > pinchThreshold)
                {
                    float pinchScale = deltaDistance > 0 ? 1.1f : 0.9f;
                    OnPinch?.Invoke(pinchScale);
                    lastPinchDistance = currentDistance;
                }
            }
            
            // End pinch when one finger is lifted
            if (Input.touchCount < 2)
            {
                isPinching = false;
            }
        }
        
        #endregion
        
        #region Mouse Input (Editor Testing)
        
        private void HandleMouseInput()
        {
            if (Input.GetMouseButtonDown(0))
            {
                OnTap?.Invoke(Input.mousePosition);
            }
            
            if (Input.GetMouseButton(0))
            {
                // Simulate drag
                Vector2 mouseDelta = Input.mousePosition - (Vector3)Input.mousePosition;
                if (mouseDelta.magnitude > dragThreshold)
                {
                    OnDrag?.Invoke(Input.mousePosition - (Vector3)mouseDelta, Input.mousePosition);
                }
            }
            
            // Scroll wheel for pinch simulation
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.01f)
            {
                OnPinch?.Invoke(1f + scroll);
            }
        }
        
        #endregion
        
        #region Haptic Feedback
        
        public enum HapticType
        {
            Light,
            Medium,
            Heavy
        }
        
        public void TriggerHaptic(HapticType type)
        {
            if (!enableHaptics) return;
            
            switch (type)
            {
                case HapticType.Light:
                    // Light tap feedback
                    Handheld.Vibrate();
                    break;
                    
                case HapticType.Medium:
                    // Medium feedback for card play
                    StartCoroutine(VibratePattern(new float[] { 0.1f, 0.05f, 0.1f }));
                    break;
                    
                case HapticType.Heavy:
                    // Heavy feedback for important actions
                    StartCoroutine(VibratePattern(new float[] { 0.2f, 0.1f, 0.2f }));
                    break;
            }
        }
        
        private IEnumerator VibratePattern(float[] pattern)
        {
            foreach (float duration in pattern)
            {
                Handheld.Vibrate();
                yield return new WaitForSeconds(duration);
            }
        }
        
        #endregion
        
        #region Utility Methods
        
        public bool IsPointerOverUI()
        {
            return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        }
        
        public bool IsPointerOverUI(int touchId)
        {
            return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(touchId);
        }
        
        public Vector2 ScreenToWorldPoint(Vector2 screenPoint)
        {
            return Camera.main.ScreenToWorldPoint(screenPoint);
        }
        
        public Vector2 WorldToScreenPoint(Vector3 worldPoint)
        {
            return Camera.main.WorldToScreenPoint(worldPoint);
        }
        
        #endregion
        
        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStateChanged -= OnGameStateChanged;
            }
        }
        
        private void OnGameStateChanged(GameManager.GameState newState)
        {
            // Handle game state changes from GameManager
            Debug.Log($"📱 UI responding to game state: {newState}");
        }
    }
}