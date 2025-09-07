using UnityEngine;
using UnityEngine.InputSystem;

namespace PotatoLegends.Core
{
    public class InputManager : MonoBehaviour
    {
        public static InputManager Instance { get; private set; }

        [Header("Input Actions")]
        public InputActionAsset inputActions;
        
        private InputActionMap gameplayActionMap;
        private InputActionMap uiActionMap;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
        }

        void Start()
        {
            InitializeInputActions();
        }

        private void InitializeInputActions()
        {
            if (inputActions == null)
            {
                Debug.LogWarning("InputManager: No InputActionAsset assigned!");
                return;
            }

            gameplayActionMap = inputActions.FindActionMap("Gameplay");
            uiActionMap = inputActions.FindActionMap("UI");

            EnableGameplayInput();
        }

        public void EnableGameplayInput()
        {
            if (gameplayActionMap != null)
            {
                gameplayActionMap.Enable();
            }
            if (uiActionMap != null)
            {
                uiActionMap.Disable();
            }
        }

        public void EnableUIInput()
        {
            if (uiActionMap != null)
            {
                uiActionMap.Enable();
            }
            if (gameplayActionMap != null)
            {
                gameplayActionMap.Disable();
            }
        }

        public void DisableAllInput()
        {
            if (gameplayActionMap != null)
            {
                gameplayActionMap.Disable();
            }
            if (uiActionMap != null)
            {
                uiActionMap.Disable();
            }
        }

        void OnDestroy()
        {
            DisableAllInput();
        }
    }
}