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
                PerformAuthentication(true);
            }
        }

        private void OnSignUpClicked()
        {
            Debug.Log("📝 Sign Up button clicked");
            
            if (ValidateInput())
            {
                ShowLoading(true);
                HideError();
                PerformAuthentication(false);
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

        private async void PerformAuthentication(bool isSignIn)
        {
            string email = emailInput.text.Trim();
            string password = passwordInput.text;

            try
            {
                string userId;
                string error;

                if (isSignIn)
                {
                    (userId, error) = await PotatoLegends.Network.SupabaseClient.Instance.SignIn(email, password);
                }
                else
                {
                    (userId, error) = await PotatoLegends.Network.SupabaseClient.Instance.SignUp(email, password);
                }

                if (error != null)
                {
                    Debug.LogError($"❌ {(isSignIn ? "Sign In" : "Sign Up")} failed: {error}");
                    ShowError(ParseErrorMessage(error));
                }
                else
                {
                    Debug.Log($"✅ {(isSignIn ? "Sign In" : "Sign Up")} successful!");
                    
                    // Save user data
                    PlayerPrefs.SetString("user_id", userId);
                    PlayerPrefs.SetString("user_email", email);
                    PlayerPrefs.Save();
                    
                    // Notify GameSceneManager of successful authentication
                    if (PotatoLegends.Core.GameSceneManager.Instance != null)
                    {
                        PotatoLegends.Core.GameSceneManager.Instance.OnAuthenticationSuccess(
                            PotatoLegends.Network.SupabaseClient.Instance.GetAccessToken()
                        );
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"❌ Authentication error: {e.Message}");
                ShowError("Network error. Please try again.");
            }
            finally
            {
                ShowLoading(false);
            }
        }

        private string ParseErrorMessage(string error)
        {
            // Parse Supabase error messages to user-friendly text
            if (error.Contains("Invalid login credentials"))
                return "Invalid email or password";
            else if (error.Contains("User already registered"))
                return "Email already exists";
            else if (error.Contains("Password should be at least"))
                return "Password must be at least 6 characters";
            else if (error.Contains("Invalid email"))
                return "Please enter a valid email address";
            else if (error.Contains("Network"))
                return "Network error. Please check your connection.";
            else
                return "Authentication failed. Please try again.";
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