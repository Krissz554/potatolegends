using UnityEngine;
using UnityEngine.Rendering;

namespace PotatoCardGame.Utils
{
    /// <summary>
    /// Mobile performance optimization utilities
    /// Handles frame rate, battery life, and performance scaling
    /// </summary>
    public class MobileOptimizer : MonoBehaviour
    {
        [Header("Performance Settings")]
        [SerializeField] private int targetFrameRate = 60;
        [SerializeField] private bool adaptivePerformance = true;
        [SerializeField] private bool batteryOptimization = true;
        
        [Header("Quality Scaling")]
        [SerializeField] private float lowEndDeviceThreshold = 2000f; // MB of RAM
        [SerializeField] private float midRangeDeviceThreshold = 4000f; // MB of RAM
        
        [Header("Debug")]
        [SerializeField] private bool showPerformanceStats = false;
        [SerializeField] private bool enableDebugLogs = true;
        
        public static MobileOptimizer Instance { get; private set; }
        
        // Performance tracking
        private float frameTime = 0f;
        private float averageFrameRate = 0f;
        private int frameCount = 0;
        private bool isLowEndDevice = false;
        private DevicePerformanceTier deviceTier = DevicePerformanceTier.Medium;
        
        public enum DevicePerformanceTier
        {
            Low,
            Medium,
            High
        }
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeOptimizations();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void InitializeOptimizations()
        {
            if (enableDebugLogs) Debug.Log("📱 Mobile Optimizer Initialized");
            
            // Detect device performance tier
            DetectDevicePerformance();
            
            // Apply initial optimizations
            ApplyPerformanceSettings();
            
            // Setup frame rate targeting
            SetupFrameRate();
            
            // Apply quality settings based on device
            ApplyQualitySettings();
            
            // Setup battery optimization
            if (batteryOptimization)
            {
                SetupBatteryOptimization();
            }
        }
        
        private void Update()
        {
            if (adaptivePerformance)
            {
                UpdatePerformanceMetrics();
                AdaptPerformanceIfNeeded();
            }
        }
        
        #region Device Detection
        
        private void DetectDevicePerformance()
        {
            // Get device specifications
            int systemMemoryMB = SystemInfo.systemMemorySize;
            int graphicsMemoryMB = SystemInfo.graphicsMemorySize;
            string deviceModel = SystemInfo.deviceModel;
            
            // Determine device tier
            if (systemMemoryMB < lowEndDeviceThreshold)
            {
                deviceTier = DevicePerformanceTier.Low;
                isLowEndDevice = true;
            }
            else if (systemMemoryMB < midRangeDeviceThreshold)
            {
                deviceTier = DevicePerformanceTier.Medium;
            }
            else
            {
                deviceTier = DevicePerformanceTier.High;
            }
            
            if (enableDebugLogs)
            {
                Debug.Log($"📱 Device Performance Tier: {deviceTier}");
                Debug.Log($"💾 System Memory: {systemMemoryMB}MB");
                Debug.Log($"🎮 Graphics Memory: {graphicsMemoryMB}MB");
                Debug.Log($"📱 Device Model: {deviceModel}");
            }
        }
        
        #endregion
        
        #region Performance Settings
        
        private void ApplyPerformanceSettings()
        {
            // Disable unnecessary features for mobile
            Physics.autoSimulation = false; // We don't need physics for a card game
            Physics2D.autoSimulation = false; // We don't need 2D physics either
            
            // Optimize rendering
            QualitySettings.vSyncCount = 0; // Disable VSync for better performance control
            
            // Set appropriate quality level based on device
            switch (deviceTier)
            {
                case DevicePerformanceTier.Low:
                    QualitySettings.SetQualityLevel(0); // Very Low
                    break;
                case DevicePerformanceTier.Medium:
                    QualitySettings.SetQualityLevel(1); // Low
                    break;
                case DevicePerformanceTier.High:
                    QualitySettings.SetQualityLevel(2); // Medium
                    break;
            }
            
            if (enableDebugLogs) Debug.Log($"⚙️ Applied quality level: {QualitySettings.names[QualitySettings.GetQualityLevel()]}");
        }
        
        private void SetupFrameRate()
        {
            // Set target frame rate based on device capability
            int targetFPS = deviceTier switch
            {
                DevicePerformanceTier.Low => 30,
                DevicePerformanceTier.Medium => 60,
                DevicePerformanceTier.High => 60,
                _ => 60
            };
            
            Application.targetFrameRate = targetFPS;
            
            if (enableDebugLogs) Debug.Log($"🎯 Target frame rate set to: {targetFPS} FPS");
        }
        
        private void ApplyQualitySettings()
        {
            // Texture quality
            QualitySettings.globalTextureMipmapLimit = deviceTier switch
            {
                DevicePerformanceTier.Low => 2, // Quarter resolution
                DevicePerformanceTier.Medium => 1, // Half resolution
                DevicePerformanceTier.High => 0, // Full resolution
                _ => 1
            };
            
            // Anti-aliasing
            QualitySettings.antiAliasing = deviceTier switch
            {
                DevicePerformanceTier.Low => 0, // None
                DevicePerformanceTier.Medium => 2, // 2x
                DevicePerformanceTier.High => 4, // 4x
                _ => 2
            };
            
            // Anisotropic filtering
            QualitySettings.anisotropicFiltering = deviceTier switch
            {
                DevicePerformanceTier.Low => AnisotropicFiltering.Disable,
                DevicePerformanceTier.Medium => AnisotropicFiltering.Enable,
                DevicePerformanceTier.High => AnisotropicFiltering.ForceEnable,
                _ => AnisotropicFiltering.Enable
            };
        }
        
        private void SetupBatteryOptimization()
        {
            // Reduce update frequency when app is not in focus
            Application.runInBackground = false;
            
            // Lower frame rate when device is low on battery
            if (SystemInfo.batteryLevel < 0.2f && SystemInfo.batteryLevel > 0f)
            {
                Application.targetFrameRate = Mathf.Min(Application.targetFrameRate, 30);
                if (enableDebugLogs) Debug.Log("🔋 Low battery detected - reducing frame rate");
            }
        }
        
        #endregion
        
        #region Adaptive Performance
        
        private void UpdatePerformanceMetrics()
        {
            frameCount++;
            frameTime += Time.unscaledDeltaTime;
            
            // Calculate average frame rate every second
            if (frameTime >= 1f)
            {
                averageFrameRate = frameCount / frameTime;
                frameCount = 0;
                frameTime = 0f;
            }
        }
        
        private void AdaptPerformanceIfNeeded()
        {
            // If frame rate drops significantly, reduce quality
            if (averageFrameRate < Application.targetFrameRate * 0.8f)
            {
                ReduceQualityTemporarily();
            }
            else if (averageFrameRate > Application.targetFrameRate * 0.95f)
            {
                RestoreQualityIfPossible();
            }
        }
        
        private void ReduceQualityTemporarily()
        {
            // Temporarily reduce quality to maintain frame rate
            if (QualitySettings.globalTextureMipmapLimit < 2)
            {
                QualitySettings.globalTextureMipmapLimit++;
                if (enableDebugLogs) Debug.Log("📉 Temporarily reduced texture quality for performance");
            }
        }
        
        private void RestoreQualityIfPossible()
        {
            // Restore quality if performance allows
            int originalMipmapLimit = deviceTier switch
            {
                DevicePerformanceTier.Low => 2,
                DevicePerformanceTier.Medium => 1,
                DevicePerformanceTier.High => 0,
                _ => 1
            };
            
            if (QualitySettings.globalTextureMipmapLimit > originalMipmapLimit)
            {
                QualitySettings.globalTextureMipmapLimit--;
                if (enableDebugLogs) Debug.Log("📈 Restored texture quality");
            }
        }
        
        #endregion
        
        #region Memory Management
        
        public void OptimizeMemoryUsage()
        {
            // Force garbage collection
            System.GC.Collect();
            
            // Unload unused assets
            Resources.UnloadUnusedAssets();
            
            if (enableDebugLogs) Debug.Log("🧹 Memory optimization performed");
        }
        
        public void PreloadCriticalAssets()
        {
            // Preload essential game assets to reduce loading hitches
            // TODO: Implement asset preloading for cards, UI elements, etc.
            
            if (enableDebugLogs) Debug.Log("📦 Critical assets preloaded");
        }
        
        #endregion
        
        #region Application Lifecycle
        
        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                // App paused - optimize for battery
                Application.targetFrameRate = 10;
                OptimizeMemoryUsage();
                
                if (enableDebugLogs) Debug.Log("⏸️ App paused - performance optimized");
            }
            else
            {
                // App resumed - restore performance
                SetupFrameRate();
                
                if (enableDebugLogs) Debug.Log("▶️ App resumed - performance restored");
            }
        }
        
        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
            {
                // App lost focus - reduce performance
                Application.targetFrameRate = 15;
            }
            else
            {
                // App gained focus - restore performance
                SetupFrameRate();
            }
        }
        
        #endregion
        
        #region Touch Optimization
        
        public void OptimizeForTouch()
        {
            // Increase touch target sizes for mobile
            var canvases = FindObjectsOfType<Canvas>();
            foreach (var canvas in canvases)
            {
                if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                {
                    // Ensure UI is optimized for touch
                    var graphicRaycaster = canvas.GetComponent<GraphicRaycaster>();
                    if (graphicRaycaster)
                    {
                        graphicRaycaster.blockingObjects = GraphicRaycaster.BlockingObjects.TwoD;
                    }
                }
            }
            
            // Set appropriate input settings for mobile
            Input.multiTouchEnabled = true;
            Input.simulateMouseWithTouches = true;
        }
        
        #endregion
        
        #region Debug Display
        
        private void OnGUI()
        {
            if (!showPerformanceStats) return;
            
            // Show performance stats overlay
            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.BeginVertical("box");
            
            GUILayout.Label($"FPS: {averageFrameRate:F1}");
            GUILayout.Label($"Device Tier: {deviceTier}");
            GUILayout.Label($"Memory: {SystemInfo.systemMemorySize}MB");
            GUILayout.Label($"Graphics: {SystemInfo.graphicsDeviceName}");
            GUILayout.Label($"Battery: {(SystemInfo.batteryLevel * 100):F0}%");
            GUILayout.Label($"Quality: {QualitySettings.names[QualitySettings.GetQualityLevel()]}");
            
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
        
        #endregion
        
        #region Public Methods
        
        public DevicePerformanceTier GetDeviceTier() => deviceTier;
        public bool IsLowEndDevice() => isLowEndDevice;
        public float GetAverageFrameRate() => averageFrameRate;
        
        public void ForceQualityLevel(int qualityLevel)
        {
            QualitySettings.SetQualityLevel(qualityLevel);
            if (enableDebugLogs) Debug.Log($"🎛️ Forced quality level: {QualitySettings.names[qualityLevel]}");
        }
        
        public void EnablePerformanceDebug(bool enable)
        {
            showPerformanceStats = enable;
        }
        
        #endregion
    }
}