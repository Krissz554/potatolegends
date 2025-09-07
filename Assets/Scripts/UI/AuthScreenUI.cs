using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace PotatoLegends.UI
{
    public class AuthScreenUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TMP_InputField emailInput;
        [SerializeField] private TMP_InputField passwordInput;
        [SerializeField] private Button signInButton;
        [SerializeField] private Button signUpButton;
        [SerializeField] private TextMeshProUGUI errorMessage;
        [SerializeField] private GameObject loadingIndicator;

        [Header("Animation")]
        [SerializeField] private float buttonAnimationDuration = 0.2f;

        private void Start()
        {
            SetupUI();
        }

        private void SetupUI()
        {
            // Setup button listeners
            if (signInButton != null)
                signInButton.onClick.AddListener(OnSignInClicked);
            
            if (signUpButton != null)
                signUpButton.onClick.AddListener(OnSignUpClicked);

            // Setup input field listeners
            if (emailInput != null)
            {
                emailInput.onValueChanged.AddListener(OnEmailChanged);
                emailInput.placeholder.GetComponent<TextMeshProUGUI>().text = "Enter your email";
            }

            if (passwordInput != null)
            {
                passwordInput.onValueChanged.AddListener(OnPasswordChanged);
                passwordInput.placeholder.GetComponent<TextMeshProUGUI>().text = "Enter your password";
            }

            // Hide error message initially
            if (errorMessage != null)
                errorMessage.gameObject.SetActive(false);

            // Hide loading indicator initially
            if (loadingIndicator != null)
                loadingIndicator.SetActive(false);
        }

        private void OnSignInClicked()
        {
            Debug.Log("🔐 Sign In button clicked");
            
            if (ValidateInput())
            {
                ShowLoading(true);
                HideError();
                
                // TODO: Implement actual authentication
                // For now, simulate authentication
                StartCoroutine(SimulateAuthentication(true));
            }
        }

        private void OnSignUpClicked()
        {
            Debug.Log("📝 Sign Up button clicked");
            
            if (ValidateInput())
            {
                ShowLoading(true);
                HideError();
                
                // TODO: Implement actual registration
                // For now, simulate registration
                StartCoroutine(SimulateAuthentication(false));
            }
        }

        private bool ValidateInput()
        {
            if (emailInput == null || passwordInput == null)
                return false;

            string email = emailInput.text.Trim();
            string password = passwordInput.text;

            if (string.IsNullOrEmpty(email))
            {
                ShowError("Please enter your email");
                return false;
            }

            if (string.IsNullOrEmpty(password))
            {
                ShowError("Please enter your password");
                return false;
            }

            if (!IsValidEmail(email))
            {
                ShowError("Please enter a valid email address");
                return false;
            }

            if (password.Length < 6)
            {
                ShowError("Password must be at least 6 characters");
                return false;
            }

            return true;
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private void OnEmailChanged(string value)
        {
            // Real-time validation feedback
            if (emailInput != null)
            {
                bool isValid = IsValidEmail(value);
                // TODO: Change input field color based on validity
            }
        }

        private void OnPasswordChanged(string value)
        {
            // Real-time validation feedback
            if (passwordInput != null)
            {
                bool isValid = value.Length >= 6;
                // TODO: Change input field color based on validity
            }
        }

        private System.Collections.IEnumerator SimulateAuthentication(bool isSignIn)
        {
            // Simulate network delay
            yield return new WaitForSeconds(1.5f);

            // Simulate random success/failure for demo
            bool success = Random.Range(0f, 1f) > 0.3f; // 70% success rate

            if (success)
            {
                Debug.Log($"✅ {(isSignIn ? "Sign In" : "Sign Up")} successful!");
                
                // Notify SceneManager of successful authentication
                if (PotatoLegends.Core.SceneManager.Instance != null)
                {
                    PotatoLegends.Core.SceneManager.Instance.OnAuthenticationSuccess("demo_token_123");
                }
            }
            else
            {
                Debug.Log($"❌ {(isSignIn ? "Sign In" : "Sign Up")} failed!");
                ShowError(isSignIn ? "Invalid email or password" : "Email already exists");
            }

            ShowLoading(false);
        }

        private void ShowError(string message)
        {
            if (errorMessage != null)
            {
                errorMessage.text = message;
                errorMessage.gameObject.SetActive(true);
            }
        }

        private void HideError()
        {
            if (errorMessage != null)
            {
                errorMessage.gameObject.SetActive(false);
            }
        }

        private void ShowLoading(bool show)
        {
            if (loadingIndicator != null)
            {
                loadingIndicator.SetActive(show);
            }

            // Disable buttons during loading
            if (signInButton != null)
                signInButton.interactable = !show;
            
            if (signUpButton != null)
                signUpButton.interactable = !show;
        }

        private void OnDestroy()
        {
            // Clean up listeners
            if (signInButton != null)
                signInButton.onClick.RemoveAllListeners();
            
            if (signUpButton != null)
                signUpButton.onClick.RemoveAllListeners();

            if (emailInput != null)
                emailInput.onValueChanged.RemoveAllListeners();

            if (passwordInput != null)
                passwordInput.onValueChanged.RemoveAllListeners();
        }
    }
}