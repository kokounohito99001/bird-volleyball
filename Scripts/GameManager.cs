using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("Game Settings")]
    public int winningScore = 7;
    public int minWinDifference = 2;
    
    [Header("References")]
    public BallController ball;
    public BirdController playerOne;
    public BirdController playerTwo;
    
    [Header("UI References")]
    public TextMeshProUGUI playerOneScoreText;
    public TextMeshProUGUI playerTwoScoreText;
    public TextMeshProUGUI countdownText;
    public GameObject gameOverPanel;
    public TextMeshProUGUI winnerText;
    
    private int playerOneScore = 0;
    private int playerTwoScore = 0;
    private bool isCountingDown = false;
    private float countdownTimer = 3f;
    private int servingPlayer = 1;
    private bool isGameActive = true;
    
    public static GameManager Instance { get; private set; }
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        ResetScores();
        UpdateScoreUI();
    }
    
    void Update()
    {
        if (!isGameActive)
        {
            if (ControlsManager.Instance != null && ControlsManager.Instance.IsResetPressed())
            {
                RestartGame();
            }
            return;
        }
        
        if (isCountingDown)
        {
            countdownTimer -= Time.deltaTime;
            
            if (countdownText != null)
            {
                countdownText.text = Mathf.CeilToInt(countdownTimer).ToString();
            }
            
            if (countdownTimer <= 0)
            {
                isCountingDown = false;
                if (countdownText != null)
                {
                    countdownText.text = "";
                }
            }
        }
        
        // Reset bird touches when ball changes sides significantly
        if (ball != null)
        {
            if (playerOne != null && playerOne.HasTouchedBall() && ball.transform.position.x > 2f)
            {
                playerOne.ResetTouch();
            }
            if (playerTwo != null && playerTwo.HasTouchedBall() && ball.transform.position.x < -2f)
            {
                playerTwo.ResetTouch();
            }
        }
    }
    
    public void StartCountdown(int servingPlayer)
    {
        this.servingPlayer = servingPlayer;
        isCountingDown = true;
        countdownTimer = 3f;
    }
    
    public void ScorePoint(string scoringSide)
    {
        if (!isGameActive) return;
        
        if (scoringSide == "PlayerOne")
        {
            playerOneScore++;
            servingPlayer = 2; // Serve to loser
        }
        else
        {
            playerTwoScore++;
            servingPlayer = 1; // Serve to loser
        }
        
        UpdateScoreUI();
        CheckForWinner();
        
        if (isGameActive)
        {
            // Reset ball after short delay
            Invoke(nameof(ResetRound), 1.5f);
        }
    }
    
    void CheckForWinner()
    {
        int scoreDiff = Mathf.Abs(playerOneScore - playerTwoScore);
        
        if ((playerOneScore >= winningScore || playerTwoScore >= winningScore) && 
            scoreDiff >= minWinDifference)
        {
            EndGame();
        }
    }
    
    void EndGame()
    {
        isGameActive = false;
        
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
        
        string winner = playerOneScore > playerTwoScore ? "Player 1" : "Player 2";
        
        if (winnerText != null)
        {
            winnerText.text = $"{winner} Wins!";
        }
        
        AudioManager.Instance?.PlaySound("GameOver");
    }
    
    void ResetRound()
    {
        if (ball != null)
        {
            ball.ResetBall(servingPlayer);
        }
        
        if (playerOne != null)
        {
            playerOne.ResetTouch();
        }
        
        if (playerTwo != null)
        {
            playerTwo.ResetTouch();
        }
    }
    
    void UpdateScoreUI()
    {
        if (playerOneScoreText != null)
        {
            playerOneScoreText.text = playerOneScore.ToString();
        }
        
        if (playerTwoScoreText != null)
        {
            playerTwoScoreText.text = playerTwoScore.ToString();
        }
    }
    
    void ResetScores()
    {
        playerOneScore = 0;
        playerTwoScore = 0;
        isGameActive = true;
        
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }
    
    void RestartGame()
    {
        ResetScores();
        UpdateScoreUI();
        ResetRound();
    }
}
