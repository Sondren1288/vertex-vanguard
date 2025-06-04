using UnityEngine;
using UnityEngine.UIElements;
using System.IO;
using System.Collections;

namespace VertexVanguard.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class MainMenuSetup : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private PanelSettings panelSettings;
        [SerializeField] private VisualTreeAsset mainMenuUXML;
        [SerializeField] private StyleSheet mainMenuUSS;
        [SerializeField] private StyleSheet settingsUSS;
        
        [Header("UI Configuration")]
        [SerializeField] private Color backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.5f);
        [SerializeField] private string titleText = "Vertex Vanguard";
        [SerializeField] private string startButtonText = "Start Game";
        [SerializeField] private string settingsButtonText = "Settings";
        [SerializeField] private string exitButtonText = "Exit";
        
        
        private UIDocument uiDocument;
        private UI_MainMenu menuController;
        
        private void Awake()
        {
            // Get or add required components
            uiDocument = GetComponent<UIDocument>();
            
            // Validate UI assets
            ValidateUIAssets();
            
            // Set up the UIDocument
            SetupUIDocument();
            
            // Prevent other components from initializing in Awake
            // We'll initialize them in Start after the UI is loaded
            DisableOtherComponents();
        }

        private void Start()
        {
            // Wait a frame to ensure UIDocument is fully loaded
            StartCoroutine(InitializeAfterUILoaded());
        }
        
        private IEnumerator InitializeAfterUILoaded()
        {
            // Wait for end of frame to ensure UI is fully loaded
            yield return new WaitForEndOfFrame();
            
            // Add and initialize controllers after UI is loaded
            AddControllers();
            
            // Now enable the controllers which will find the UI elements
            EnableOtherComponents();
        }
        
        private void DisableOtherComponents()
        {
            // Disable other scripts temporarily to prevent premature initialization
            var mainMenu = GetComponent<UI_MainMenu>();
            if (mainMenu != null) mainMenu.enabled = false;
        }
        
        private void EnableOtherComponents()
        {
            // Enable the controllers now that UI is loaded
            if (menuController != null) menuController.enabled = true;
        }
        
        private void ValidateUIAssets()
        {
            if (panelSettings == null)
            {
                Debug.LogError("PanelSettings is not assigned to " + gameObject.name);
            }
            
            if (mainMenuUXML == null)
            {
                Debug.LogError("MainMenuUXML is not assigned to " + gameObject.name);
            }
            
            if (mainMenuUSS == null)
            {
                Debug.LogError("MainMenuUSS is not assigned to " + gameObject.name);
            }
        }
        
        private void SetupUIDocument()
        {
            if (uiDocument == null)
            {
                Debug.LogError("UIDocument component not found on " + gameObject.name);
                return;
            }
            
            // Set up UIDocument
            if (mainMenuUXML != null)
            {
                uiDocument.visualTreeAsset = mainMenuUXML;
            }
            
            if (panelSettings != null) 
            {
                uiDocument.panelSettings = panelSettings;
            }
        }
        
        private void AddControllers()
        {
            // Add UI_MainMenu if not already present
            menuController = GetComponent<UI_MainMenu>();
            if (menuController == null)
            {
                menuController = gameObject.AddComponent<UI_MainMenu>();
                Debug.Log("Added UI_MainMenu component to " + gameObject.name);
            }
        }
        
        private void OnEnable()
        {
            if (uiDocument == null) 
            {
                Debug.LogError("UIDocument is null on " + gameObject.name);
                return;
            }
            
            var root = uiDocument.rootVisualElement;
            if (root == null)
            {
                Debug.LogError("Root visual element is null on " + gameObject.name);
                return;
            }
            
            // Apply the stylesheet
            if (mainMenuUSS != null)
            {
                if (!root.styleSheets.Contains(mainMenuUSS))
                {
                    root.styleSheets.Add(mainMenuUSS);
                    Debug.Log("Added stylesheet to UI root");
                }
            }

            if (settingsUSS != null)
            {
                if (!root.styleSheets.Contains(settingsUSS))
                {
                    root.styleSheets.Add(settingsUSS);
                    Debug.Log("Added stylesheet to UI root");
                }
            }
            
            // Set title text if it exists
            var titleLabel = root.Q<Label>("game-title");
            if (titleLabel != null)
            {
                titleLabel.text = titleText;
            }
            else
            {
                Debug.LogWarning("Game title label not found in UI");
            }

             var startGameButton = root.Q<Label>("start-game-button-text");
            if (startGameButton != null)
            {
                startGameButton.text = startButtonText;
            }
            else
            {
                Debug.LogWarning("Game title label not found in UI");
            }

            var settingsButtonText = root.Q<Label>("settings-button-text");
            if (settingsButtonText != null)
            {
                settingsButtonText.text = this.settingsButtonText;
            }
            else
            {
                Debug.LogWarning("Game title label not found in UI");
            }

            var exitButtonText = root.Q<Label>("exit-button-text");
            if (exitButtonText != null)
            {
                exitButtonText.text = this.exitButtonText;
            }
            else
            {
                Debug.LogWarning("Game title label not found in UI");
            }

            // Set background color
            var container = root.Q("main-menu-container");
            if (container != null)
            {
                container.style.backgroundColor = backgroundColor;
            }
            else
            {
                Debug.LogWarning("Main menu container not found in UI");
            }
            
            // Make sure the menu is centered
            root.style.width = new StyleLength(new Length(100, LengthUnit.Percent));
            root.style.height = new StyleLength(new Length(100, LengthUnit.Percent));
            root.style.alignItems = Align.Center;
            root.style.justifyContent = Justify.Center;
            
            // Print debug info about the UI elements
            Debug.Log($"MainMenuSetup: UI has been configured successfully");
            
            // Debug button elements
            var startButton = root.Q("start-game-button");
            var settingsButton = root.Q("settings-button");
            var exitButton = root.Q("exit-button");
            
            Debug.Log($"MainMenuSetup found start-game-button: {startButton != null}");
            Debug.Log($"MainMenuSetup found settings-button: {settingsButton != null}");
            Debug.Log($"MainMenuSetup found exit-button: {exitButton != null}");
        }
    }
} 