using CameraNamespace;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using UINamespace;
using UnityEngine.SceneManagement;
using VertexVanguard.Utility;
using DiscoJockeyNamespace;

public class CameraRotationButtons : MonoBehaviour
{
    [SerializeField] private StyleSheet cameraRotationStyleSheet;
    [SerializeField] private StyleSheet bottomRightButtonStyleSheet;
    [SerializeField] private StyleSheet hideableMainMenuStyleSheet;
    [SerializeField] private StyleSheet settingsStyleSheet;
    [SerializeField] private CameraManager cameraManager;

    private UIDocument document;
    private VisualElement rootVisualElement;
    private VisualElement cameraRotationContainer;
    private VisualElement bottomRightContainer;
    private Button leftRotateButton;
    private Button rightRotateButton;
    // Main Menu
    private VisualElement mainMenuContainer;
    private Button mainMenuButton;
    private Button creditsCloseButton;
    private Button backToGameButton;
    private Button settingsButton;
    private Button restartButton;
    private Button exitButton;
    // Settings
    private VisualElement settingsOverlay;
    private Slider musicVolumeSlider;
    private Slider musicPitchSlider;
    private Slider sfxVolumeSlider;
    private DropdownField vsyncDropdown;
    private DropdownField targetFrameRateDropdown;
    private Button fullscreenButton;
    private Button borderedButton;
    private Button borderlessButton;
    private Button settingsCloseButton;
    
    // Bottom Right Button System
    private List<Button> bottomRightButtons;
    public List<UIGuy.BottomRightButtonType> buttonTypes;
    
    private readonly Dictionary<UIGuy.BottomRightButtonType, string> buttonTypeToText = new Dictionary<UIGuy.BottomRightButtonType, string>{
        {UIGuy.BottomRightButtonType.EndTurn, "End Turn"},
        {UIGuy.BottomRightButtonType.ExitUnitSelection, "Exit Unit Selection"},
        {UIGuy.BottomRightButtonType.ExitBattlePrep, "Exit Battle Prep"},
        {UIGuy.BottomRightButtonType.SkipUnitTurn, "Skip Unit Turn"},
        {UIGuy.BottomRightButtonType.Disabled, "Disabled"},
        {UIGuy.BottomRightButtonType.Invisible, "Invisible"},
        {UIGuy.BottomRightButtonType.StartBattle, "Start Battle"},
        {UIGuy.BottomRightButtonType.Overworld_Ambush, "Set ambush"},
        {UIGuy.BottomRightButtonType.Overworld_Leave_Ambush, "Leave ambush"},
        {UIGuy.BottomRightButtonType.Overworld_FallenTree, "Fell tree"},
        {UIGuy.BottomRightButtonType.Select_All_Units, "Select all units"},
        {UIGuy.BottomRightButtonType.Deselect_All_Units, "Deselect all units"},
    };

    private void Awake()
    {
        document = GetComponent<UIDocument>();
        if (document == null)
        {
            document = gameObject.AddComponent<UIDocument>();
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SetupUI();
        Hide();
    }

    private void SetupUI()
    {
        // Get the root visual element
        rootVisualElement = GetComponent<UIDocument>().rootVisualElement;
        
        // Apply both stylesheets
        if (cameraRotationStyleSheet != null)
        {
            rootVisualElement.styleSheets.Add(cameraRotationStyleSheet);
        }
        
        if (bottomRightButtonStyleSheet != null)
        {
            if (!rootVisualElement.styleSheets.Contains(bottomRightButtonStyleSheet))
            {
                rootVisualElement.styleSheets.Add(bottomRightButtonStyleSheet);
            }
        }

        if (hideableMainMenuStyleSheet != null)
        {
            rootVisualElement.styleSheets.Add(hideableMainMenuStyleSheet);
        }

        if (settingsStyleSheet != null)
        {
            rootVisualElement.styleSheets.Add(settingsStyleSheet);
        }

        // Find and assign camera rotation references
        cameraRotationContainer = rootVisualElement.Q<VisualElement>("camera-rotation-container");
        leftRotateButton = rootVisualElement.Q<Button>("camera-rotate-left");
        rightRotateButton = rootVisualElement.Q<Button>("camera-rotate-right");

        settingsOverlay = rootVisualElement.Q<VisualElement>("settings-overlay");

        // Register camera rotation button click events
        if (leftRotateButton != null)
        {
            leftRotateButton.clicked += OnLeftRotateClicked;
        }

        if (rightRotateButton != null)
        {
            rightRotateButton.clicked += OnRightRotateClicked;
        }
        
        // Setup bottom right buttons
        SetupBottomRightButtons();

        // Setup main menu
        SetupMainMenu();

        // Setup settings
        SetupSettings();
    }
    
    private void SetupBottomRightButtons()
    {
        bottomRightContainer = rootVisualElement.Q<VisualElement>("bottom-right-buttons-container");
        bottomRightButtons = bottomRightContainer.Query<Button>().ToList();
        buttonTypes = new List<UIGuy.BottomRightButtonType>();

        foreach(Button button in bottomRightButtons)
        {
            button.clicked += () => OnBottomRightButtonClicked(button);
            SetBottomRightButtonType(bottomRightButtons.IndexOf(button), UIGuy.BottomRightButtonType.Invisible);
        }
    }

    private void SetupMainMenu()
    {
        mainMenuButton = rootVisualElement.Q<Button>("main-menu-button");
        mainMenuContainer = rootVisualElement.Q<VisualElement>("main-menu-container");
        creditsCloseButton = rootVisualElement.Q<Button>("credits-close-button");
        backToGameButton = rootVisualElement.Q<Button>("back-to-game-button");
        settingsButton = rootVisualElement.Q<Button>("settings-button");
        restartButton = rootVisualElement.Q<Button>("restart-button");
        exitButton = rootVisualElement.Q<Button>("exit-button");

        // Register main menu button click events
        if (mainMenuButton != null)
        {
            mainMenuButton.clicked += OnMainMenuButtonClicked;
        }

        if (creditsCloseButton != null)
        {
            creditsCloseButton.clicked += OnCreditsCloseClicked;
        }

        if (backToGameButton != null)
        {
            backToGameButton.clicked += OnBackToGameClicked;
        }

        if (settingsButton != null)
        {
            settingsButton.clicked += OnSettingsButtonClicked;
        }

        if (restartButton != null)
        {
            restartButton.clicked += OnRestartButtonClicked;
        }

        if (exitButton != null)
        {
            exitButton.clicked += OnExitButtonClicked;
        }
        
        
        
    }

    private void SetupSettings()
    {
        settingsCloseButton = rootVisualElement.Q<Button>("settings-close-button");
        musicVolumeSlider = rootVisualElement.Q<Slider>("music-volume-slider");
        musicPitchSlider = rootVisualElement.Q<Slider>("music-pitch-slider");
        sfxVolumeSlider = rootVisualElement.Q<Slider>("sfx-volume-slider");
        vsyncDropdown = rootVisualElement.Q<DropdownField>("vsync-dropdown");
        targetFrameRateDropdown = rootVisualElement.Q<DropdownField>("target-frame-rate-dropdown");
        fullscreenButton = rootVisualElement.Q<Button>("fullscreen-button");
        borderedButton = rootVisualElement.Q<Button>("bordered-button");
        borderlessButton = rootVisualElement.Q<Button>("borderless-button");

        if (settingsCloseButton != null)
        {
            settingsCloseButton.clicked += OnSettingsCloseButtonClicked;
        }

        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.RegisterCallback<ChangeEvent<float>>(OnMusicVolumeChanged);
        }

        if (musicPitchSlider != null)
        {
            musicPitchSlider.RegisterCallback<ChangeEvent<float>>(OnMusicPitchChanged);
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.RegisterCallback<ChangeEvent<float>>(OnSFXVolumeChanged);
        }

        if (vsyncDropdown != null)
        {
            vsyncDropdown.RegisterCallback<ChangeEvent<int>>(OnVSyncChanged);
        }

        if (targetFrameRateDropdown != null)
        {
            targetFrameRateDropdown.RegisterCallback<ChangeEvent<int>>(OnTargetFrameRateChanged);
        }

        if (fullscreenButton != null)
        {
            fullscreenButton.clicked += OnFullscreenButtonClicked;
        }

        if (borderedButton != null)
        {
            borderedButton.clicked += OnBorderedButtonClicked;
        }

        if (borderlessButton != null)
        {
            borderlessButton.clicked += OnBorderlessButtonClicked;
        }

        if (settingsCloseButton != null)
        {
            settingsCloseButton.clicked += OnSettingsCloseButtonClicked;
        }
    }

    // Expose method for left rotation - to be extended with functionality
    public virtual void OnLeftRotateClicked()
    {
        Debug.Log("Left rotate button clicked");
        cameraManager.RotateCamera(Direction.Left);
    }

    // Expose method for right rotation - to be extended with functionality
    public virtual void OnRightRotateClicked()
    {
        Debug.Log("Right rotate button clicked");
        cameraManager.RotateCamera(Direction.Right);
    }

    // Method to enable/disable buttons
    public void SetButtonsEnabled(bool enabled)
    {
        if (leftRotateButton != null)
        {
            leftRotateButton.SetEnabled(enabled);
            if (enabled)
                leftRotateButton.RemoveFromClassList("disabled");
            else
                leftRotateButton.AddToClassList("disabled");
        }

        if (rightRotateButton != null)
        {
            rightRotateButton.SetEnabled(enabled);
            if (enabled)
                rightRotateButton.RemoveFromClassList("disabled");
            else
                rightRotateButton.AddToClassList("disabled");
        }
    }

    // Method to show/hide the rotation buttons
    public void SetButtonsVisible(bool visible)
    {
        if (cameraRotationContainer != null)
        {
            if (visible)
                cameraRotationContainer.RemoveFromClassList("hidden");
            else
                cameraRotationContainer.AddToClassList("hidden");
        }
    }

    public void Hide()
    {
        SetButtonsVisible(false);
    }

    public void Show()
    {
        SetButtonsVisible(true);
    }

    public void SetBottomRightButtonType(int index, UIGuy.BottomRightButtonType buttonType)
    {
        Logger.Success("Setting button type to: " + buttonType);
        if(bottomRightButtons[index] != null)
        {
            if(buttonTypes.Count > index)
            {
                buttonTypes[index] = buttonType;
            }
            else
            {
                buttonTypes.Add(buttonType);
            }
            
            var label = bottomRightButtons[index].Q<Label>("bottom-right-text"+"-"+index);
            if(label != null)
            {
                label.text = buttonTypeToText[buttonType];
            }
            
            if(buttonType == UIGuy.BottomRightButtonType.Disabled)
            {
                bottomRightButtons[index].AddToClassList("disabled");
            }
            else if(buttonType == UIGuy.BottomRightButtonType.Invisible)
            {
                bottomRightButtons[index].AddToClassList("invisible");
            } 
            else 
            {
                bottomRightButtons[index].RemoveFromClassList("disabled");
                bottomRightButtons[index].RemoveFromClassList("invisible");
            }
        }
    }

    private void OnBottomRightButtonClicked(Button button)
    {
        if(bottomRightButtons.Contains(button))
        {
            GameEvents.BottomRightButtonClicked.Invoke(buttonTypes[bottomRightButtons.IndexOf(button)]);
        }
    }

    private void OnCreditsCloseClicked()
    {
        if (mainMenuContainer != null)
        {
            mainMenuContainer.RemoveFromClassList("visible");
        }
    }

    private void OnBackToGameClicked()
    {
        if (mainMenuContainer != null)
        {
            mainMenuContainer.RemoveFromClassList("visible");
        }
    }

    private void OnSettingsButtonClicked()
    {
        Logger.Info("Settings button clicked");
        if (mainMenuContainer != null)
        {
            mainMenuContainer.RemoveFromClassList("visible");
        }
        if (settingsOverlay != null)
        {
            settingsOverlay.RemoveFromClassList("settings-hidden");
        }
    }

    private static void OnRestartButtonClicked()
    {
        // Change scene to MainMenu
        GameEvents.GoToMainMenu.Invoke(new EmptyEventArgs());
    }

    private static void OnExitButtonClicked()
    {
        Debug.Log("Exit button clicked");
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    // Main menu
    private void OnMainMenuButtonClicked()
    {
        if (mainMenuContainer != null)
        {
            mainMenuContainer.AddToClassList("visible");
        }
    }
    

    // Main menu
    public VisualElement GetBottomRightContainer()
    {
        return bottomRightContainer;
    }

    void Update()
    {

        // Register ESC key
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Logger.Info("ESC key pressed");
            if (mainMenuContainer != null)
            {
                mainMenuContainer.AddToClassList("visible");
            }
        }
        
    }

    private void OnDisable()
    {
        // Unregister camera rotation events
        if (leftRotateButton != null)
            leftRotateButton.clicked -= OnLeftRotateClicked;
        if (rightRotateButton != null)
            rightRotateButton.clicked -= OnRightRotateClicked;
            
        // Unregister bottom right button events
        if (bottomRightButtons != null)
        {
            foreach(Button button in bottomRightButtons)
            {
                button.clicked -= () => OnBottomRightButtonClicked(button);
            }
        }
    }

    
        private void OnSettingsClicked()
        {
            mainMenuContainer.RemoveFromClassList("visible");
            settingsOverlay.RemoveFromClassList("settings-hidden");
        }


            private void OnMusicVolumeChanged(ChangeEvent<float> evt){
            if(DJ.Instance != null){
                DJ.Instance.SetMusicVolume(evt.newValue/100);
                musicVolumeSlider.value = evt.newValue;
            }
            Logger.Info("Music volume changed to " + evt.newValue);
        }

        private void OnMusicPitchChanged(ChangeEvent<float> evt){
            if(DJ.Instance != null){
                DJ.Instance.SetMusicPitch(evt.newValue);
                musicPitchSlider.value = evt.newValue;
            }
            Logger.Info("Music pitch changed to " + evt.newValue);
        }

        private void OnSFXVolumeChanged(ChangeEvent<float> evt){
            if(DJ.Instance != null){
                DJ.Instance.SetSFXVolume(evt.newValue/100);
                sfxVolumeSlider.value = evt.newValue;
            }
            Logger.Info("SFX volume changed to " + evt.newValue);
        }

        private void OnVSyncChanged(ChangeEvent<int> evt){
            Settings.Instance.SetVSyncCount(evt.newValue);
            vsyncDropdown.value = evt.newValue == 0 ? "Off" : "On";
            Logger.Info("VSync changed to " + evt.newValue);
        }

        private void OnTargetFrameRateChanged(ChangeEvent<int> evt){
            Settings.Instance.SetTargetFrameRate(evt.newValue);
            targetFrameRateDropdown.value = evt.newValue == 0 ? "30" : evt.newValue == 1 ? "60" : evt.newValue == 2 ? "120" : evt.newValue == 3 ? "144" : "Unlimited";
            Logger.Info("Target frame rate changed to " + evt.newValue);
        }

        private void OnFullscreenButtonClicked(){
            if(!fullscreenButton.ClassListContains("selected")){
                fullscreenButton.AddToClassList("selected");
                borderedButton.RemoveFromClassList("selected");
                borderlessButton.RemoveFromClassList("selected");
                
                // Set to exclusive fullscreen mode
                Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
                Logger.Info("Screen mode changed to Exclusive Fullscreen");
            }
        }

        private void OnBorderedButtonClicked(){
            if(!borderedButton.ClassListContains("selected")){
                borderedButton.AddToClassList("selected");
                fullscreenButton.RemoveFromClassList("selected");
                borderlessButton.RemoveFromClassList("selected");
                
                // Set to windowed mode (with borders)
                Screen.fullScreenMode = FullScreenMode.Windowed;
                Logger.Info("Screen mode changed to Windowed (Bordered)");
            }
        }
        private void OnBorderlessButtonClicked(){
            if(!borderlessButton.ClassListContains("selected")){
                borderlessButton.AddToClassList("selected");
                fullscreenButton.RemoveFromClassList("selected");
                borderedButton.RemoveFromClassList("selected");
                
                // Set to fullscreen window mode (borderless)
                Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
                Logger.Info("Screen mode changed to Borderless Windowed");
            }
        }

        private void OnSettingsCloseButtonClicked(){
            settingsOverlay.AddToClassList("settings-hidden");
            mainMenuContainer.AddToClassList("visible");
        }
        
}
