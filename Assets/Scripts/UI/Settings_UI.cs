using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;
using System.Collections.Generic;

namespace VertexVanguard.UI 
{
  [RequireComponent(typeof(UIDocument))]
  public class Settings_UI : MonoBehaviour
  {
    [SerializeField] private StyleSheet settingsUSS;

    private UIDocument uiDocument;
    private VisualElement root;
    private Slider musicVolumeSlider;
    private Slider musicPitchSlider;
    private Slider sfxVolumeSlider;
    private DropdownField vsyncDropdown;
    private Slider targetFrameRateSlider;
    private Button fullscreenButton;
    private Button borderedButton;
    private Button borderlessButton;
    private Button settingsCloseButton;
    
    

    private void Awake(){
      uiDocument = GetComponent<UIDocument>();

      ValidateUIAssets();

      SetupUIDocument();
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
    }

    private void ValidateUIAssets(){
      if(uiDocument == null){
        Logger.Error("UIDocument is null on " + gameObject.name);
        return;
      }

      root = uiDocument.rootVisualElement;
      if(root == null){
        Logger.Error("Root visual element is null on " + gameObject.name);
        return;
      }

      musicVolumeSlider = root.Q<Slider>("music-volume-slider");
      if(musicVolumeSlider == null){
        Logger.Error("Music volume slider not found on " + gameObject.name);
        return;
      }

      musicPitchSlider = root.Q<Slider>("music-pitch-slider");
      if(musicPitchSlider == null){
        Logger.Error("Music pitch slider not found on " + gameObject.name);
        return;
      }

      sfxVolumeSlider = root.Q<Slider>("sfx-volume-slider");
      if(sfxVolumeSlider == null){
        Logger.Error("SFX volume slider not found on " + gameObject.name);
        return;
      }

      vsyncDropdown = root.Q<DropdownField>("vsync-dropdown");
      if(vsyncDropdown == null){
        Logger.Error("VSync dropdown not found on " + gameObject.name);
        return;
      }

      targetFrameRateSlider = root.Q<Slider>("target-frame-rate-slider");
      if(targetFrameRateSlider == null){
        Logger.Error("Target frame rate slider not found on " + gameObject.name);
        return;
      }

      fullscreenButton = root.Q<Button>("fullscreen-button");
      if(fullscreenButton == null){
        Logger.Error("Fullscreen button not found on " + gameObject.name);
        return;
      }      

      borderedButton = root.Q<Button>("bordered-button");
      if(borderedButton == null){
        Logger.Error("Bordered button not found on " + gameObject.name);
        return;
      }

      borderlessButton = root.Q<Button>("borderless-button");
      if(borderlessButton == null){
        Logger.Error("Borderless button not found on " + gameObject.name);
        return;
      }

      settingsCloseButton = root.Q<Button>("settings-close-button");
      if(settingsCloseButton == null){
        Logger.Error("Settings close button not found on " + gameObject.name);
        return;
      }
    }
    
    private void SetupUIDocument(){
      if (settingsUSS != null)
      {
          if (!root.styleSheets.Contains(settingsUSS))
          {
              root.styleSheets.Add(settingsUSS);
              Debug.Log("Added stylesheet to UI root");
          }
      }

      musicVolumeSlider.RegisterCallback<ChangeEvent<float>>(OnMusicVolumeChanged);
      musicPitchSlider.RegisterCallback<ChangeEvent<float>>(OnMusicPitchChanged);
      sfxVolumeSlider.RegisterCallback<ChangeEvent<float>>(OnSFXVolumeChanged);
      vsyncDropdown.RegisterCallback<ChangeEvent<int>>(OnVSyncChanged);
      targetFrameRateSlider.RegisterCallback<ChangeEvent<int>>(OnTargetFrameRateChanged);
      fullscreenButton.clicked += OnFullscreenButtonClicked;
      borderedButton.clicked += OnBorderedButtonClicked;
      
    }

    private void OnMusicVolumeChanged(ChangeEvent<float> evt){
      Logger.Info("Music volume changed to " + evt.newValue);
    }

    private void OnMusicPitchChanged(ChangeEvent<float> evt){
      Logger.Info("Music pitch changed to " + evt.newValue);
    }
    
    private void OnSFXVolumeChanged(ChangeEvent<float> evt){
      Logger.Info("SFX volume changed to " + evt.newValue);
    }

    private void OnVSyncChanged(ChangeEvent<int> evt){
      Logger.Info("VSync changed to " + evt.newValue);
    }
    
    private void OnTargetFrameRateChanged(ChangeEvent<int> evt){
      Logger.Info("Target frame rate changed to " + evt.newValue);
    }

    private void OnFullscreenButtonClicked(){
      Logger.Info("Fullscreen button clicked");
    }
    
    private void OnBorderedButtonClicked(){
      Logger.Info("Bordered button clicked");
    }

    private void OnBorderlessButtonClicked(){
      Logger.Info("Borderless button clicked");
    }
    
    private void OnSettingsCloseButtonClicked(){
      root.AddToClassList("settings-hidden");
    }
  }
}