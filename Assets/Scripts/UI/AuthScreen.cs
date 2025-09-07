using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Threading.Tasks;
using PotatoLegends.Network;
using PotatoLegends.Core;

namespace PotatoLegends.UI
{
    public class AuthScreen : MonoBehaviour
    {
        [Header("UI Elements")]
        public TMP_InputField emailInputField;
        public TMP_InputField passwordInputField;
        public Button signInButton;
        public Button signUpButton;
        public TextMeshProUGUI statusText;

        [Header("References")]
        public UIManager uiManager;
        public SupabaseClient supabaseClient;

        void OnEnable()
        {
            signInButton.onClick.AddListener(OnSignInButtonClicked);
            signUpButton.onClick.AddListener(OnSignUpButtonClicked);
            statusText.text = "";
        }

        void OnDisable()
        {
            signInButton.onClick.RemoveListener(OnSignInButtonClicked);
            signUpButton.onClick.RemoveListener(OnSignUpButtonClicked);
        }

        private async void OnSignInButtonClicked()
        {
            string email = emailInputField.text;
            string password = passwordInputField.text;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                statusText.text = "Please enter email and password.";
                return;
            }

            statusText.text = "Signing in...";
            signInButton.interactable = false;
            signUpButton.interactable = false;

            bool success = await supabaseClient.SignIn(email, password);

            if (!success)
            {
                statusText.text = "Sign-in failed";
            }
            else
            {
                statusText.text = "Signed in successfully";
                Debug.Log("User signed in successfully.");
                GameManager.Instance.ChangeGameState(GameManager.GameState.MainMenu);
            }

            signInButton.interactable = true;
            signUpButton.interactable = true;
        }

        private async void OnSignUpButtonClicked()
        {
            string email = emailInputField.text;
            string password = passwordInputField.text;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                statusText.text = "Please enter email and password.";
                return;
            }

            statusText.text = "Signing up...";
            signInButton.interactable = false;
            signUpButton.interactable = false;

            bool success = await supabaseClient.SignUp(email, password);

            if (!success)
            {
                statusText.text = "Sign-up failed";
            }
            else
            {
                statusText.text = "Signed up successfully";
                Debug.Log("User signed up successfully.");
                GameManager.Instance.ChangeGameState(GameManager.GameState.MainMenu);
            }

            signInButton.interactable = true;
            signUpButton.interactable = true;
        }
    }
}