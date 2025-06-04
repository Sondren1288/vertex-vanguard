using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using DiscoJockeyNamespace;
using VertexVanguard.Utility;

namespace VertexVanguard.UI
{
    public class UI_MainMenu : MonoBehaviour
    {
        [SerializeField] private UIDocument uiDocument;
        [SerializeField] private string gameSceneName = "GameScene";
        private VisualElement root;
        private VisualElement mainMenuContainer;
        private VisualElement settingsOverlay;  
        private Button startGameButton;
        private Button settingsButton;
        private Button exitButton;
        private Button creditsButton;
        private Button creditsCloseButton;

        // Settings things
        private Slider musicVolumeSlider;
        private Slider musicPitchSlider;
        private Slider sfxVolumeSlider;
        private DropdownField vsyncDropdown;
        private DropdownField targetFrameRateDropdown;
        private Button fullscreenButton;
        private Button borderedButton;
        private Button borderlessButton;
        private Button settingsCloseButton;
        private void Start()
        {
            // Delay initialization to ensure UI is fully loaded
            StartCoroutine(TryFindButtons());
        }

        private IEnumerator TryFindButtons()
        {
            // Try multiple times to find the buttons
            int maxAttempts = 5;
            for (int i = 0; i < maxAttempts; i++)
            {
                if (FindButtons())
                {
                    Debug.Log("Successfully found and initialized all buttons");
                    yield break;
                }
                
                Debug.Log($"Attempt {i+1}/{maxAttempts} to find buttons failed, waiting and trying again...");
                yield return new WaitForSeconds(0.2f);
            }
            
            Debug.LogError("Failed to find buttons after multiple attempts");
        }
        
        private bool FindButtons()
        {
            if (uiDocument == null)
                uiDocument = GetComponent<UIDocument>();

            if (uiDocument == null)
            {
                Debug.LogError("UIDocument is null on " + gameObject.name);
                return false;
            }

            root = uiDocument.rootVisualElement;
            if (root == null)
            {
                Debug.LogError("Root visual element is null on " + gameObject.name);
                return false;
            }
            
            // Try to find buttons by name
            Debug.Log("Attempting to find buttons by name...");
            startGameButton = root.Q<Button>("start-game-button");
            settingsButton = root.Q<Button>("settings-button");
            exitButton = root.Q<Button>("exit-button");
            creditsButton = root.Q<Button>("credits-button");
            creditsCloseButton = root.Q<Button>("credits-close-button");

            musicVolumeSlider = root.Q<Slider>("music-volume-slider");
            musicPitchSlider = root.Q<Slider>("music-pitch-slider");
            sfxVolumeSlider = root.Q<Slider>("sfx-volume-slider");
            vsyncDropdown = root.Q<DropdownField>("vsync-dropdown");
            targetFrameRateDropdown = root.Q<DropdownField>("framerate-dropdown");
            fullscreenButton = root.Q<Button>("fullscreen-button");
            borderedButton = root.Q<Button>("bordered-button");
            borderlessButton = root.Q<Button>("borderless-button");
            settingsCloseButton = root.Q<Button>("settings-close-button");
            settingsOverlay = root.Q<VisualElement>("settings-overlay");
            mainMenuContainer = root.Q<VisualElement>("main-menu-container");

            // If that didn't work, try finding them by class
            if (startGameButton == null || settingsButton == null || exitButton == null)
            {
                Debug.Log("Trying to find buttons by class...");
                var buttons = root.Query<Button>(className: "menu-button").ToList();
                
                if (buttons.Count >= 3)
                {
                    Debug.Log($"Found {buttons.Count} buttons by class");
                    startGameButton = buttons[0];
                    settingsButton = buttons[1];
                    exitButton = buttons[2];
                    creditsButton = buttons[3];
                }
            }
            
            // If still not found, try finding them by their content
            if (startGameButton == null || settingsButton == null || exitButton == null)
            {
                Debug.Log("Trying to find buttons by content...");
                var allButtons = root.Query<Button>().ToList();
                
                foreach (var button in allButtons)
                {
                    var label = button.Q<Label>();
                    if (label != null)
                    {
                        if (label.text.Contains("Start game"))
                            startGameButton = button;
                        else if (label.text.Contains("Settings"))
                            settingsButton = button;
                        else if (label.text.Contains("Leave game"))
                            exitButton = button;
                        else if (label.text.Contains("Credits"))
                            creditsButton = button;
                        else if (label.text.Contains("×"))
                            creditsCloseButton = button;
                    }
                }
            }
            
            // Register click events if the buttons were found
            bool allButtonsFound = true;
            
            if (startGameButton != null) 
            {
                startGameButton.clicked += OnStartGameClicked;
                Debug.Log("Start game button found and click event registered");
            }
            else
            {
                Debug.LogError("Start game button not found in UI document");
                allButtonsFound = false;
            }
            
            if (settingsButton != null) 
            {
                if(settingsButton.Q<Label>().text == "Settings"){
                    settingsButton.clicked += OnSettingsClicked;
                } else {
                    settingsButton.clicked += () => {
                        SceneManager.LoadScene("MainMenu");
                    };
                }
                Debug.Log("Settings button found and click event registered");
            }
            else
            {
                Debug.LogError("Settings button not found in UI document");
                allButtonsFound = false;
            }
            
            if (exitButton != null) 
            {
                exitButton.clicked += OnExitClicked;
                Debug.Log("Exit button found and click event registered");
            }
            else
            {
                Debug.LogError("Exit button not found in UI document");
                allButtonsFound = false;
            }

            if (creditsButton != null)
            {
                creditsButton.clicked += OnCreditsClicked;
                Debug.Log("Credits button found and click event registered");
            }
            else
            {
                Debug.LogError("Credits button not found in UI document");
                allButtonsFound = false;
            }

            if (creditsCloseButton != null)
            {
                creditsCloseButton.clicked += OnCreditsCloseClicked;
                Debug.Log("Credits close button found and click event registered");
            }
            else
            {
                Debug.LogError("Credits close button not found in UI document");
                allButtonsFound = false;
            }
            if (musicVolumeSlider != null)
            {
                musicVolumeSlider.RegisterCallback<ChangeEvent<float>>(OnMusicVolumeChanged);
            }
            else 
            {
                Debug.LogError("Music volume slider not found in UI document");
                allButtonsFound = false;
            }
            if (musicPitchSlider != null)
            {
                musicPitchSlider.RegisterCallback<ChangeEvent<float>>(OnMusicPitchChanged);
            }
            else 
            {
                Debug.LogError("Music pitch slider not found in UI document");
                allButtonsFound = false;
            }
            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.RegisterCallback<ChangeEvent<float>>(OnSFXVolumeChanged);
            }
            else 
            {
                Debug.LogError("SFX volume slider not found in UI document");
                allButtonsFound = false;
            }
            if (vsyncDropdown != null)
            {
                vsyncDropdown.RegisterCallback<ChangeEvent<int>>(OnVSyncChanged);
            }
            else 
            {
                Debug.LogError("VSync dropdown not found in UI document");
                allButtonsFound = false;
            }
            if (targetFrameRateDropdown != null)
            {
                targetFrameRateDropdown.RegisterCallback<ChangeEvent<int>>(OnTargetFrameRateChanged);
            }
            else 
            {
                Debug.LogError("Target frame rate slider not found in UI document");
                allButtonsFound = false;
            }
            if (fullscreenButton != null)
            {
                fullscreenButton.clicked += OnFullscreenButtonClicked;
            }
            else 
            {
                Debug.LogError("Fullscreen button not found in UI document");
                allButtonsFound = false;
            }
            if (borderedButton != null)
            {
                borderedButton.clicked += OnBorderedButtonClicked;
            }
            else 
            {
                Debug.LogError("Bordered button not found in UI document");
                allButtonsFound = false;
            }
            if (borderlessButton != null)
            {
                borderlessButton.clicked += OnBorderlessButtonClicked;
            }
            else 
            {
                Debug.LogError("Borderless button not found in UI document");
                allButtonsFound = false;
            }
            if (settingsCloseButton != null)
            {
                settingsCloseButton.clicked += OnSettingsCloseButtonClicked;
            }
            else 
            {
                Debug.LogError("Settings close button not found in UI document");
                allButtonsFound = false;
            }
            if (settingsOverlay != null)
            {
                settingsOverlay.AddToClassList("settings-hidden");
            }
            else 
            {
                Debug.LogError("Settings overlay not found in UI document");
                allButtonsFound = false;
            }
            if (mainMenuContainer != null){
                mainMenuContainer.AddToClassList("visible");
            }
            else 
            {
                Debug.LogError("Main menu container not found in UI document");
                allButtonsFound = false;
            }
            // Set initial settings values
            if(Settings.Instance != null){
                musicVolumeSlider.value = Settings.Instance.musicVolume;
                musicPitchSlider.value = Settings.Instance.musicPitch;
                sfxVolumeSlider.value = Settings.Instance.sfxVolume;
                vsyncDropdown.value = Settings.Instance.vSyncCount == 0 ? "Off" : "On";
                targetFrameRateDropdown.value = Settings.Instance.targetFrameRate == 0 ? "30" : Settings.Instance.targetFrameRate == 1 ? "60" : Settings.Instance.targetFrameRate == 2 ? "120" : Settings.Instance.targetFrameRate == 3 ? "144" : "Unlimited";
            }
            return allButtonsFound;
        }

        private void OnStartGameClicked()
        {
            Debug.Log("Start Game button clicked");
            SceneManager.LoadScene(gameSceneName);
        }

        private void OnSettingsClicked()
        {
            mainMenuContainer.RemoveFromClassList("visible");
            settingsOverlay.RemoveFromClassList("settings-hidden");
        }

        private void OnExitClicked()
        {
            Debug.Log("Exit button clicked");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void OnCreditsClicked()
        {
            VisualElement creditsContainer = root.Q<VisualElement>("credits-container");
            if (creditsContainer == null)
            {
                Debug.LogError("Credits container not found in UI document");
                return;
            }
            Debug.Log("Credits button clicked");
            creditsContainer.AddToClassList("visible");
        }

        private void OnCreditsCloseClicked()
        {
            VisualElement creditsContainer = root.Q<VisualElement>("credits-container");
            if (creditsContainer == null)
            {
                Debug.LogError("Credits container not found in UI document");
                return;
            }
            Debug.Log("Credits close button clicked");
            creditsContainer.RemoveFromClassList("visible");
        }

        private void OnMusicVolumeChanged(ChangeEvent<float> evt){
            if(DJ.Instance != null){
                DJ.Instance.SetMusicVolume(evt.newValue/100);
                root.Q<Label>("music-volume-value").text = evt.newValue.ToString("F0") + "%";
            }
            Logger.Info("Music volume changed to " + evt.newValue);
        }

        private void OnMusicPitchChanged(ChangeEvent<float> evt){
            if(DJ.Instance != null){
                DJ.Instance.SetMusicPitch(evt.newValue);
                root.Q<Label>("music-pitch-value").text = evt.newValue.ToString("F0");
            }
            Logger.Info("Music pitch changed to " + evt.newValue);
        }

        private void OnSFXVolumeChanged(ChangeEvent<float> evt){
            if(DJ.Instance != null){
                DJ.Instance.SetSFXVolume(evt.newValue/100);
                root.Q<Label>("sfx-volume-value").text = evt.newValue.ToString("F0") + "%";
            }
            Logger.Info("SFX volume changed to " + evt.newValue);
        }

        private void OnVSyncChanged(ChangeEvent<int> evt){
            Settings.Instance.SetVSyncCount(evt.newValue);
            vsyncDropdown.value = evt.newValue == 0 ? "Off" : "On";
            root.Q<Label>("vsync-value").text = evt.newValue == 0 ? "Off" : "On";
            Logger.Info("VSync changed to " + evt.newValue);
        }

        private void OnTargetFrameRateChanged(ChangeEvent<int> evt){
            Settings.Instance.SetTargetFrameRate(evt.newValue);
            targetFrameRateDropdown.value = evt.newValue == 0 ? "30" : evt.newValue == 1 ? "60" : evt.newValue == 2 ? "120" : evt.newValue == 3 ? "144" : "Unlimited";
            root.Q<Label>("target-frame-rate-value").text = evt.newValue == 0 ? "30" : evt.newValue == 1 ? "60" : evt.newValue == 2 ? "120" : evt.newValue == 3 ? "144" : "Unlimited";
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
} 