using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Menu Panels")]
    public GameObject mainMenuPanel;
    public GameObject settingsPanel;
    public GameObject difficultyPanel;
    
    [Header("Difficulty Buttons")]
    public GameObject easyButton;
    public GameObject mediumButton;
    public GameObject hardButton;
    
    private BirdAI birdAI;
    
    void Start()
    {
        ShowMainMenu();
        
        // Find AI bird to set difficulty
        GameObject aiBird = GameObject.FindGameObjectWithTag("AI");
        if (aiBird != null)
        {
            birdAI = aiBird.GetComponent<BirdAI>();
        }
    }
    
    public void ShowMainMenu()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (difficultyPanel != null) difficultyPanel.SetActive(false);
    }
    
    public void ShowSettings()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(true);
        if (difficultyPanel != null) difficultyPanel.SetActive(false);
    }
    
    public void ShowDifficulty()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (difficultyPanel != null) difficultyPanel.SetActive(true);
    }
    
    public void SetDifficulty(string difficulty)
    {
        if (birdAI != null)
        {
            switch (difficulty.ToLower())
            {
                case "easy":
                    birdAI.difficulty = BirdAI.Difficulty.Easy;
                    break;
                case "medium":
                    birdAI.difficulty = BirdAI.Difficulty.Medium;
                    break;
                case "hard":
                    birdAI.difficulty = BirdAI.Difficulty.Hard;
                    break;
            }
            
            PlayerPrefs.SetString("AIDifficulty", difficulty);
            PlayerPrefs.Save();
        }
        
        ShowMainMenu();
    }
    
    public void StartGame()
    {
        // Load game scene
        SceneManager.LoadScene(1); // Assuming game scene is at index 1
    }
    
    public void QuitGame()
    {
        Application.Quit();
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
    
    void LoadSavedDifficulty()
    {
        string savedDifficulty = PlayerPrefs.GetString("AIDifficulty", "Medium");
        SetDifficulty(savedDifficulty);
    }
}
