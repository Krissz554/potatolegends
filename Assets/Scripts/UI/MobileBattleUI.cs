using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PotatoCardGame.Data;
using PotatoCardGame.Battle;
using PotatoCardGame.Network;

namespace PotatoCardGame.UI
{
    /// <summary>
    /// Simple mobile battle UI that works with the current BattleManager
    /// Displays battle information and handles basic battle interactions
    /// </summary>
    public class MobileBattleUI : MonoBehaviour
    {
        [Header("Battle Info")]
        [SerializeField] private TextMeshProUGUI turnIndicatorText;
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private TextMeshProUGUI playerManaText;
        [SerializeField] private TextMeshProUGUI opponentManaText;
        
        [Header("Battle Actions")]
        [SerializeField] private Button endTurnButton;
        [SerializeField] private Button surrenderButton;
        [SerializeField] private Button battleMenuButton;
        
        [Header("Battle Status")]
        [SerializeField] private GameObject battlePanel;
        [SerializeField] private TextMeshProUGUI battleStatusText;
        
        private void Start()
        {
            SetupBattleUI();
        }
        
        private void SetupBattleUI()
        {
            if (endTurnButton) endTurnButton.onClick.AddListener(OnEndTurnPressed);
            if (surrenderButton) surrenderButton.onClick.AddListener(OnSurrenderPressed);
            if (battleMenuButton) battleMenuButton.onClick.AddListener(OnBattleMenuPressed);
            
            // Subscribe to battle events
            if (BattleManager.Instance != null)
            {
                BattleManager.Instance.OnBattleStateChanged += OnBattleStateChanged;
                BattleManager.Instance.OnTurnChanged += OnTurnChanged;
                BattleManager.Instance.OnManaChanged += OnManaChanged;
                BattleManager.Instance.OnTurnTimerChanged += OnTimerChanged;
            }
        }
        
        private void OnBattleStateChanged(BattleManager.BattleState newState)
        {
            switch (newState)
            {
                case BattleManager.BattleState.NotInBattle:
                    if (battlePanel) battlePanel.SetActive(false);
                    break;
                    
                case BattleManager.BattleState.Searching:
                    if (battleStatusText) battleStatusText.text = "Searching for opponent...";
                    break;
                    
                case BattleManager.BattleState.Initializing:
                    if (battleStatusText) battleStatusText.text = "Initializing battle...";
                    break;
                    
                case BattleManager.BattleState.Mulligan:
                    if (battleStatusText) battleStatusText.text = "Choose cards to mulligan";
                    if (battlePanel) battlePanel.SetActive(true);
                    break;
                    
                case BattleManager.BattleState.InProgress:
                    if (battleStatusText) battleStatusText.text = "Battle in progress";
                    if (battlePanel) battlePanel.SetActive(true);
                    break;
                    
                case BattleManager.BattleState.Ended:
                    if (battleStatusText) battleStatusText.text = "Battle ended";
                    break;
            }
        }
        
        private void OnTurnChanged(bool isPlayerTurn)
        {
            if (turnIndicatorText)
            {
                turnIndicatorText.text = isPlayerTurn ? "Your Turn" : "Opponent's Turn";
                turnIndicatorText.color = isPlayerTurn ? Color.green : Color.red;
            }
            
            if (endTurnButton)
            {
                endTurnButton.interactable = isPlayerTurn;
            }
        }
        
        private void OnManaChanged(int currentMana)
        {
            if (playerManaText)
            {
                playerManaText.text = $"Mana: {currentMana}";
            }
        }
        
        private void OnTimerChanged(float timeRemaining)
        {
            if (timerText)
            {
                int seconds = Mathf.CeilToInt(timeRemaining);
                timerText.text = $"Time: {seconds}s";
                
                // Change color when time is running out
                if (seconds <= 10)
                {
                    timerText.color = Color.red;
                }
                else if (seconds <= 30)
                {
                    timerText.color = Color.yellow;
                }
                else
                {
                    timerText.color = Color.white;
                }
            }
        }
        
        private void OnEndTurnPressed()
        {
            if (BattleManager.Instance != null && BattleManager.Instance.IsPlayerTurn())
            {
                BattleManager.Instance.EndTurn();
                Debug.Log("🔄 End turn button pressed");
            }
        }
        
        private void OnSurrenderPressed()
        {
            // TODO: Implement surrender functionality
            Debug.Log("🏳️ Surrender button pressed");
        }
        
        private void OnBattleMenuPressed()
        {
            // TODO: Implement battle menu
            Debug.Log("📋 Battle menu button pressed");
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from events
            if (BattleManager.Instance != null)
            {
                BattleManager.Instance.OnBattleStateChanged -= OnBattleStateChanged;
                BattleManager.Instance.OnTurnChanged -= OnTurnChanged;
                BattleManager.Instance.OnManaChanged -= OnManaChanged;
                BattleManager.Instance.OnTurnTimerChanged -= OnTimerChanged;
            }
        }
    }
}