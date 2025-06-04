using UnityEngine;
using TMPro;

public class ScreenTextDisplayManager : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Assign the TextMeshProUGUI element from your Canvas here.")]
    public TextMeshProUGUI displayTextElement;

    private void Awake()
    {
        if (displayTextElement == null)
        {
            // Try to find it if not assigned - assumes a specific name or tag
            // For robust setup, manual assignment in Inspector is preferred.
            GameObject textObject = GameObject.FindGameObjectWithTag("ScreenDisplayText");
            if (textObject != null)
            {
                displayTextElement = textObject.GetComponent<TextMeshProUGUI>();
            }

            if (displayTextElement == null)
            {
                Logger.Warning("ScreenTextDisplayManager: TextMeshProUGUI element is not assigned and could not be found by tag 'ScreenDisplayText'. UI text will not be displayed.");
                // Optionally, disable this component if the text element is crucial
                // this.enabled = false;
            }
        }
        // Ensure it's initially empty or hidden
        UpdateDisplayText("");
    }

    /// <summary>
    /// Updates the text displayed on the screen.
    /// Pass an empty string or null to hide the text.
    /// </summary>
    /// <param name="newText">The text to display.</param>
    public void UpdateDisplayText(string newText)
    {
        if (displayTextElement != null)
        {
            if (string.IsNullOrEmpty(newText))
            {
                displayTextElement.gameObject.SetActive(false);
                displayTextElement.text = "";
            }
            else
            {
                displayTextElement.gameObject.SetActive(true);
                displayTextElement.text = newText;
            }
        }
        else
        {
            // Logger.Warning("ScreenTextDisplayManager: Attempted to update text, but TextMeshProUGUIelement is not set.");
        }
    }

    /// <summary>
    /// Hides the display text element.
    /// </summary>
    public void HideText()
    {
        if (displayTextElement != null)
        {
            displayTextElement.gameObject.SetActive(false);
            displayTextElement.text = "";
        }
    }

    /// <summary>
    /// Shows the display text element (if it has content).
    /// </summary>
    public void ShowText()
    {
        if (displayTextElement != null && !string.IsNullOrEmpty(displayTextElement.text))
        {
            displayTextElement.gameObject.SetActive(true);
        }
    }
}