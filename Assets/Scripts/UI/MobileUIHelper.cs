using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace PotatoLegends.UI
{
    public class MobileUIHelper : MonoBehaviour
    {
        [Header("Mobile Settings")]
        public float touchSensitivity = 1.0f;
        public float dragThreshold = 10f;
        public bool enableHapticFeedback = true;

        [Header("UI References")]
        public EventSystem eventSystem;
        public GraphicRaycaster graphicRaycaster;

        void Start()
        {
            SetupMobileUI();
        }

        private void SetupMobileUI()
        {
            // Ensure we have an EventSystem
            if (eventSystem == null)
            {
                eventSystem = FindFirstObjectByType<EventSystem>();
                if (eventSystem == null)
                {
                    GameObject eventSystemGO = new GameObject("EventSystem");
                    eventSystem = eventSystemGO.AddComponent<EventSystem>();
                    eventSystemGO.AddComponent<StandaloneInputModule>();
                }
            }

            // Ensure we have a GraphicRaycaster on the Canvas
            if (graphicRaycaster == null)
            {
                Canvas canvas = FindFirstObjectByType<Canvas>();
                if (canvas != null)
                {
                    graphicRaycaster = canvas.GetComponent<GraphicRaycaster>();
                    if (graphicRaycaster == null)
                    {
                        graphicRaycaster = canvas.gameObject.AddComponent<GraphicRaycaster>();
                    }
                }
            }

            // Configure for mobile
            ConfigureForMobile();
        }

        private void ConfigureForMobile()
        {
            // Set up mobile-specific UI settings
            if (Application.isMobilePlatform)
            {
                // Configure Canvas for mobile
                Canvas canvas = FindFirstObjectByType<Canvas>();
                if (canvas != null)
                {
                    CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
                    if (scaler == null)
                    {
                        scaler = canvas.gameObject.AddComponent<CanvasScaler>();
                    }
                    
                    scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                    scaler.referenceResolution = new Vector2(1920, 1080);
                    scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                    scaler.matchWidthOrHeight = 0.5f;
                }
            }
        }

        public void TriggerHapticFeedback()
        {
            if (enableHapticFeedback && Application.isMobilePlatform)
            {
                // Trigger haptic feedback for mobile devices
                #if UNITY_ANDROID || UNITY_IOS
                Handheld.Vibrate();
                #endif
            }
        }

        public bool IsTouchInput()
        {
            return Application.isMobilePlatform && Input.touchCount > 0;
        }

        public Vector2 GetTouchPosition()
        {
            if (IsTouchInput())
            {
                return Input.GetTouch(0).position;
            }
            return Input.mousePosition;
        }
    }
}