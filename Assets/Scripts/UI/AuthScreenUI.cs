using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

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
            SetupEventListeners();
        }

        private void SetupEventListeners()
        {
            // Subscribe to SupabaseClient events
            if (PotatoLegends.Network.SupabaseClient.Instance != null)
            {
                PotatoLegends.Network.SupabaseClient.Instance.OnAuthenticationSuccess += OnAuthenticationSuccess;
                PotatoLegends.Network.SupabaseClient.Instance.OnAuthenticationError += OnAuthenticationError;
            }
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (PotatoLegends.Network.SupabaseClient.Instance != null)
            {
                PotatoLegends.Network.SupabaseClient.Instance.OnAuthenticationSuccess -= OnAuthenticationSuccess;
                PotatoLegends.Network.SupabaseClient.Instance.OnAuthenticationError -= OnAuthenticationError;
            }
        }

        private void OnAuthenticationSuccess(string userEmail)
        {
            Debug.Log($"✅ Authentication successful for: {userEmail}");
            ShowLoading(false);
            
            // Notify GameSceneManager of successful authentication
            if (PotatoLegends.Core.GameSceneManager.Instance != null)
            {
                PotatoLegends.Core.GameSceneManager.Instance.OnAuthenticationSuccess(
                    PotatoLegends.Network.SupabaseClient.Instance.GetAccessToken()
                );
            }
        }

        private void OnAuthenticationError(string error)
        {
            Debug.LogError($"❌ Authentication error: {error}");
            ShowError(ParseErrorMessage(error));
            ShowLoading(false);
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

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ShowError("Please enter both email and password.");
                ShowLoading(false);
                return;
            }

            try
            {
                bool success;

                if (isSignIn)
                {
                    success = await PotatoLegends.Network.SupabaseClient.Instance.SignIn(email, password);
                }
                else
                {
                    success = await PotatoLegends.Network.SupabaseClient.Instance.SignUp(email, password);
                }

                if (success)
                {
                    Debug.Log($"✅ {(isSignIn ? "Sign In" : "Sign Up")} successful!");
                    // Authentication success is handled by SupabaseClient events
                }
                else
                {
                    Debug.LogError($"❌ {(isSignIn ? "Sign In" : "Sign Up")} failed");
                    ShowError("Authentication failed. Please check your credentials.");
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