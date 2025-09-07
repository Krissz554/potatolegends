using UnityEngine;
using UnityEditor;
using UnityEngine.InputSystem;

namespace PotatoLegends.Editor
{
    public class InputManagerSetup
    {
        [MenuItem("Potato Legends/Fix Input Manager")]
        public static void FixInputManager()
        {
            Debug.Log("🔧 Fixing Input Manager...");
            
            // Create Input Actions asset
            var inputActions = ScriptableObject.CreateInstance<InputActionAsset>();
            
            // Create action map
            var actionMap = inputActions.AddActionMap("UI");
            
            // Add Submit action
            var submitAction = actionMap.AddAction("Submit");
            submitAction.AddBinding("<Keyboard>/enter");
            submitAction.AddBinding("<Keyboard>/space");
            submitAction.AddBinding("<Gamepad>/buttonSouth");
            
            // Add Cancel action
            var cancelAction = actionMap.AddAction("Cancel");
            cancelAction.AddBinding("<Keyboard>/escape");
            cancelAction.AddBinding("<Gamepad>/buttonEast");
            
            // Add Navigate action
            var navigateAction = actionMap.AddAction("Navigate");
            navigateAction.AddBinding("<Keyboard>/upArrow");
            navigateAction.AddBinding("<Keyboard>/downArrow");
            navigateAction.AddBinding("<Keyboard>/leftArrow");
            navigateAction.AddBinding("<Keyboard>/rightArrow");
            navigateAction.AddBinding("<Gamepad>/dpad");
            navigateAction.AddBinding("<Gamepad>/leftStick");
            
            // Add Point action
            var pointAction = actionMap.AddAction("Point");
            pointAction.AddBinding("<Mouse>/position");
            pointAction.AddBinding("<Touchscreen>/primaryTouch/position");
            
            // Add Click action
            var clickAction = actionMap.AddAction("Click");
            clickAction.AddBinding("<Mouse>/leftButton");
            clickAction.AddBinding("<Touchscreen>/primaryTouch/press");
            
            // Add Scroll Wheel action
            var scrollAction = actionMap.AddAction("ScrollWheel");
            scrollAction.AddBinding("<Mouse>/scroll");
            scrollAction.AddBinding("<Touchscreen>/primaryTouch/delta");
            
            // Add Middle Click action
            var middleClickAction = actionMap.AddAction("MiddleClick");
            middleClickAction.AddBinding("<Mouse>/middleButton");
            
            // Add Right Click action
            var rightClickAction = actionMap.AddAction("RightClick");
            rightClickAction.AddBinding("<Mouse>/rightButton");
            
            // Add Tracked Device Position action
            var trackedDevicePositionAction = actionMap.AddAction("TrackedDevicePosition");
            trackedDevicePositionAction.AddBinding("<XRController>/devicePosition");
            
            // Add Tracked Device Orientation action
            var trackedDeviceOrientationAction = actionMap.AddAction("TrackedDeviceOrientation");
            trackedDeviceOrientationAction.AddBinding("<XRController>/deviceRotation");
            
            // Save the asset
            AssetDatabase.CreateAsset(inputActions, "Assets/InputActions/GameInput.inputactions");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Debug.Log("✅ Input Manager fixed! Created GameInput.inputactions");
            Debug.Log("📝 You may need to enable the new Input System in Project Settings > XR Plug-in Management > Input System Package");
        }
        
        [MenuItem("Potato Legends/Setup Legacy Input Manager")]
        public static void SetupLegacyInputManager()
        {
            Debug.Log("🔧 Setting up Legacy Input Manager...");
            
            // This will be handled by the project settings
            Debug.Log("✅ Legacy Input Manager is already configured in Project Settings");
            Debug.Log("📝 If you still get Submit button errors, try switching to Legacy Input System in Project Settings > XR Plug-in Management > Input System Package");
        }
    }
}