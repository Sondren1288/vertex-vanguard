using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class UI_EndScreen : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject endScreenPanel;
    [SerializeField] private TextMeshProUGUI resultText;
    [SerializeField] private Button playAgainButton;
    [SerializeField] private Button mainMenuButton;

    private void Start()
    {
        // Add button listeners
        playAgainButton.onClick.AddListener(OnPlayAgainClicked);
        mainMenuButton.onClick.AddListener(OnMainMenuClicked);
    }

    public void ShowEndScreen(bool isVictory)
    {
        endScreenPanel.SetActive(true);
        resultText.text = isVictory ? "Victory!" : "Defeat!";
        resultText.color = isVictory ? Color.green : Color.red;
    }

    public void OnPlayAgainClicked()
    {
        // Reset all GameEvents to ensure a clean state
        GameEvents.ResetAllEvents();
        
        // Reload the current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void OnMainMenuClicked()
    {
        // Reset all GameEvents to ensure a clean state
        GameEvents.ResetAllEvents();
        
        // Load the main menu scene
        SceneManager.LoadScene("MainMenu");
    }
}
