using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// GameUIController manages the in-game UI for the volleyball game.
/// Queries and updates UI elements from the GameUI.uxml document.
/// Easy to extend: add new UI elements to UXML, query them here, and add public methods to update them.
/// </summary>
public class GameUIController : MonoBehaviour
{
    [Header("Game References")]
    [SerializeField] private VolleyballGameManager gameManager;

    // Player panels and labels
    private VisualElement player1Panel;
    private VisualElement player2Panel;
    private Label player1Label;
    private Label player2Label;

    // Score panel (center top)
    private VisualElement scoreContainer;
    private Label player1ScoreLabel;
    private Label player2ScoreLabel;
    private Label scoreSeparatorLabel;

    // Countdown UI
    private VisualElement countdownPanel;
    private Label countdownLabel;

    // Victory banner
    private VisualElement victoryBanner;
    private Label victoryLabel;

    // Waiting for players
    private VisualElement waitingPanel;
    private Label waitingLabel;

    // Pause menu panel
    private VisualElement pauseMenuPanel;

    // Bottom panel
    private VisualElement bottomPanel;

    void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        // Query player panels and labels
        player1Panel = root.Q<VisualElement>("Player1");
        player2Panel = root.Q<VisualElement>("Player2");

        if (player1Panel != null)
            player1Label = player1Panel.Q<Label>("Label");
        else
            Debug.LogWarning("GameUIController: 'Player1' panel not found in UIDocument.");

        if (player2Panel != null)
            player2Label = player2Panel.Q<Label>("Label");
        else
            Debug.LogWarning("GameUIController: 'Player2' panel not found in UIDocument.");

        // Query score container and labels (center top panel)
        scoreContainer = root.Q<VisualElement>("Score");
        if (scoreContainer != null)
        {
            player1ScoreLabel = scoreContainer.Q<Label>("Player1Score");
            scoreSeparatorLabel = scoreContainer.Q<Label>("ScoreSeparator");
            player2ScoreLabel = scoreContainer.Q<Label>("Player2Score");
        }
        else
            Debug.LogWarning("GameUIController: 'Score' container not found in UIDocument.");

        // Query countdown UI (add to UXML: VisualElement name="CountdownPanel" with Label name="CountdownLabel")
        countdownPanel = root.Q<VisualElement>("CountdownPanel");
        if (countdownPanel != null)
            countdownLabel = countdownPanel.Q<Label>("CountdownLabel");
        // Note: If not found, add CountdownPanel to GameUI.uxml

        // Query victory banner (add to UXML: VisualElement name="VictoryBanner" with Label name="VictoryLabel")
        victoryBanner = root.Q<VisualElement>("VictoryBanner");
        if (victoryBanner != null)
            victoryLabel = victoryBanner.Q<Label>("VictoryLabel");
        // Note: If not found, add VictoryBanner to GameUI.uxml

        // Query waiting panel (add to UXML: VisualElement name="WaitingPanel" with Label name="WaitingLabel")
        waitingPanel = root.Q<VisualElement>("WaitingPanel");
        if (waitingPanel != null)
            waitingLabel = waitingPanel.Q<Label>("WaitingLabel");
        // Note: If not found, add WaitingPanel to GameUI.uxml

        // Query pause menu and bottom panels (for future use)
        pauseMenuPanel = root.Q<VisualElement>("PauseMenu");
        bottomPanel = root.Q<VisualElement>("BottonPannel");

        if (pauseMenuPanel == null)
            Debug.LogWarning("GameUIController: 'PauseMenu' panel not found in UIDocument.");
        if (bottomPanel == null)
            Debug.LogWarning("GameUIController: 'BottonPannel' panel not found in UIDocument.");

        // Initialize overlay panels as hidden
        HideWaitingForPlayers();
        HideCountdown();
        HideVictoryBanner();
    }

    void OnDisable()
    {
        // Clean up if needed
    }

    /// <summary>
    /// Update player 1 display name or status.
    /// </summary>
    public void SetPlayer1Name(string name)
    {
        if (player1Label != null)
            player1Label.text = name;
    }

    /// <summary>
    /// Update player 2 display name or status.
    /// </summary>
    public void SetPlayer2Name(string name)
    {
        if (player2Label != null)
            player2Label.text = name;
    }

    /// <summary>
    /// Update player 1 score display in center score panel.
    /// </summary>
    public void SetPlayer1Score(int score)
    {
        if (player1ScoreLabel != null)
            player1ScoreLabel.text = score.ToString();
    }

    /// <summary>
    /// Update player 2 score display in center score panel.
    /// </summary>
    public void SetPlayer2Score(int score)
    {
        if (player2ScoreLabel != null)
            player2ScoreLabel.text = score.ToString();
    }

    /// <summary>
    /// Update both scores at once.
    /// </summary>
    public void UpdateScores(int p1Score, int p2Score)
    {
        SetPlayer1Score(p1Score);
        SetPlayer2Score(p2Score);
    }

    /// <summary>
    /// Show pause menu (for future expansion).
    /// </summary>
    public void ShowPauseMenu()
    {
        if (pauseMenuPanel != null)
            pauseMenuPanel.style.display = DisplayStyle.Flex;
    }

    /// <summary>
    /// Hide pause menu (for future expansion).
    /// </summary>
    public void HidePauseMenu()
    {
        if (pauseMenuPanel != null)
            pauseMenuPanel.style.display = DisplayStyle.None;
    }

    /// <summary>
    /// Show bottom panel (for future expansion - could be for countdown, serving indicator, etc.).
    /// </summary>
    public void ShowBottomPanel()
    {
        if (bottomPanel != null)
            bottomPanel.style.display = DisplayStyle.Flex;
    }

    /// <summary>
    /// Hide bottom panel.
    /// </summary>
    public void HideBottomPanel()
    {
        if (bottomPanel != null)
            bottomPanel.style.display = DisplayStyle.None;
    }

    /// <summary>
    /// Set visibility of player panels (useful for loading screen, etc.).
    /// </summary>
    public void SetPlayerPanelsVisible(bool visible)
    {
        if (player1Panel != null)
            player1Panel.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        if (player2Panel != null)
            player2Panel.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
    }

    /// <summary>
    /// Highlight a player panel to indicate whose turn or who scored.
    /// </summary>
    public void HighlightPlayer(int playerIndex)
    {
        var targetPanel = playerIndex == 0 ? player1Panel : player2Panel;
        if (targetPanel != null)
        {
            // Add a class or style change (you can define highlight styles in GameCSS.uss)
            targetPanel.AddToClassList("highlight");
        }
    }

    /// <summary>
    /// Remove highlight from a player panel.
    /// </summary>
    public void RemoveHighlightPlayer(int playerIndex)
    {
        var targetPanel = playerIndex == 0 ? player1Panel : player2Panel;
        if (targetPanel != null)
        {
            targetPanel.RemoveFromClassList("highlight");
        }
    }

    // ========== COUNTDOWN UI ==========
    /// <summary>
    /// Show countdown timer. Pass 0 to show "Go!".
    /// </summary>
    public void ShowCountdown(int seconds)
    {
        if (countdownPanel != null)
        {
            countdownPanel.RemoveFromClassList("hidden");
            countdownPanel.style.display = DisplayStyle.Flex;
        }
        
        if (countdownLabel != null)
            countdownLabel.text = seconds > 0 ? seconds.ToString() : "Go!";
    }

    /// <summary>
    /// Hide countdown timer.
    /// </summary>
    public void HideCountdown()
    {
        if (countdownPanel != null)
        {
            countdownPanel.AddToClassList("hidden");
            countdownPanel.style.display = DisplayStyle.None;
        }
    }

    // ========== VICTORY BANNER UI ==========
    /// <summary>
    /// Show victory banner for winning player.
    /// </summary>
    public void ShowVictoryBanner(int winningPlayerIndex)
    {
        if (victoryBanner != null)
        {
            victoryBanner.RemoveFromClassList("hidden");
            victoryBanner.style.display = DisplayStyle.Flex;
        }
        
        if (victoryLabel != null)
        {
            string playerName = winningPlayerIndex == 0 ? "Player 1" : "Player 2";
            victoryLabel.text = $"{playerName} WINS!";
        }
    }

    /// <summary>
    /// Hide victory banner.
    /// </summary>
    public void HideVictoryBanner()
    {
        if (victoryBanner != null)
        {
            victoryBanner.AddToClassList("hidden");
            victoryBanner.style.display = DisplayStyle.None;
        }
    }

    // ========== WAITING FOR PLAYERS UI ==========
    /// <summary>
    /// Show "Waiting for players" message.
    /// </summary>
    public void ShowWaitingForPlayers()
    {
        if (waitingPanel != null)
        {
            waitingPanel.RemoveFromClassList("hidden");
            waitingPanel.style.display = DisplayStyle.Flex;
        }
        
        if (waitingLabel != null)
            waitingLabel.text = "Waiting for players…";
    }

    /// <summary>
    /// Hide waiting for players message.
    /// </summary>
    public void HideWaitingForPlayers()
    {
        if (waitingPanel != null)
        {
            waitingPanel.AddToClassList("hidden");
            waitingPanel.style.display = DisplayStyle.None;
        }
    }
}
