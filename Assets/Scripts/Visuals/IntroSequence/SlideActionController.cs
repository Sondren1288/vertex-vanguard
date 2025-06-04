using UnityEngine;
using System.Collections;

/// <summary>
/// Controls specific actions that should happen when reaching different slides.
/// This class follows the Single Responsibility Principle by separating slide-specific logic
/// from the main animation management.
/// </summary>
public class SlideActionController : MonoBehaviour
{
    [Header("Slide Action Settings")]
    public GameObject[] slideSpecificObjects; // Objects to activate/deactivate on specific slides
    public ParticleSystem[] particleEffects; // Particle effects to trigger
    public AudioClip[] slideAudioClips; // Audio clips to play on specific slides
    public Light[] sceneLights; // Lights to control
    
    [Header("Audio")]
    public AudioSource audioSource;
    
    private AnimationManager animationManager;
    
    private void Awake()
    {
        animationManager = GetComponent<AnimationManager>();
        
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }
    
    private void OnEnable()
    {
        if (animationManager != null)
        {
            animationManager.OnSlideReached += HandleSlideReached;
        }
        else
        {
            Debug.LogWarning("SlideActionController: No AnimationManager found on this GameObject!");
        }
    }
    
    private void OnDisable()
    {
        if (animationManager != null)
        {
            animationManager.OnSlideReached -= HandleSlideReached;
        }
    }
    
    /// <summary>
    /// Main handler for slide-specific actions
    /// </summary>
    /// <param name="slideIndex">The index of the slide that was reached</param>
    private void HandleSlideReached(int slideIndex)
    {
        if (slideIndex < 0)
        {
            Debug.LogWarning($"SlideActionController: Invalid slide index {slideIndex}");
            return;
        }
        
        Debug.Log($"Slide {slideIndex} reached - executing specific actions");
        
        try
        {
            // Execute slide-specific actions
            switch (slideIndex)
            {
                case 0:
                    ExecuteSlide0Actions();
                    break;
                case 1:
                    ExecuteSlide1Actions();
                    break;
                case 2:
                    ExecuteSlide2Actions();
                    break;
                case 3:
                    ExecuteSlide3Actions();
                    break;
                case 4:
                    ExecuteSlide4Actions();
                    break;
                default:
                    ExecuteDefaultSlideActions(slideIndex);
                    break;
            }
            
            // Play slide-specific audio if available
            PlaySlideAudio(slideIndex);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"SlideActionController: Error handling slide {slideIndex}: {e.Message}");
        }
    }
    
    #region Slide-Specific Action Methods
    
    private void ExecuteSlide0Actions()
    {
        // Example: Introduction slide - fade in scene lights
        if (sceneLights != null && sceneLights.Length > 0)
        {
            StartCoroutine(FadeLights(0.5f, 2f));
        }
        
        // Activate any intro-specific objects
        ActivateSlideObjects(0);
    }
    
    private void ExecuteSlide1Actions()
    {
        // Example: Second slide - start particle effects
        TriggerParticleEffect(0);
        
        // Change lighting to create atmosphere
        if (sceneLights != null && sceneLights.Length > 0)
        {
            StartCoroutine(ChangeLightColor(Color.blue, 1f));
        }
        
        ActivateSlideObjects(1);
    }
    
    private void ExecuteSlide2Actions()
    {
        // Example: Third slide - dramatic lighting change
        if (sceneLights != null && sceneLights.Length > 0)
        {
            StartCoroutine(ChangeLightColor(Color.red, 1.5f));
        }
        
        // Stop previous particle effects and start new ones
        StopAllParticleEffects();
        TriggerParticleEffect(1);
        
        ActivateSlideObjects(2);
    }
    
    private void ExecuteSlide3Actions()
    {
        // Example: Fourth slide - environmental changes
        if (sceneLights != null && sceneLights.Length > 0)
        {
            StartCoroutine(ChangeLightColor(Color.green, 1f));
        }
        
        TriggerParticleEffect(2);
        ActivateSlideObjects(3);
    }
    
    private void ExecuteSlide4Actions()
    {
        // Example: Final slide - reset to normal lighting
        if (sceneLights != null && sceneLights.Length > 0)
        {
            StartCoroutine(ChangeLightColor(Color.white, 2f));
            StartCoroutine(FadeLights(1f, 2f));
        }
        
        // Stop all particle effects for clean ending
        StopAllParticleEffects();
        
        ActivateSlideObjects(4);
    }
    
    private void ExecuteDefaultSlideActions(int slideIndex)
    {
        // Default actions for slides not specifically handled
        Debug.Log($"Default actions for slide {slideIndex}");
        
        // Basic object activation if available
        ActivateSlideObjects(slideIndex);
    }
    
    #endregion
    
    #region Helper Methods
    
    private void ActivateSlideObjects(int slideIndex)
    {
        if (slideSpecificObjects == null) return;
        
        // Deactivate all slide-specific objects first
        foreach (var obj in slideSpecificObjects)
        {
            if (obj != null)
                obj.SetActive(false);
        }
        
        // Activate the specific object for this slide
        if (slideIndex >= 0 && slideIndex < slideSpecificObjects.Length && slideSpecificObjects[slideIndex] != null)
        {
            slideSpecificObjects[slideIndex].SetActive(true);
        }
    }
    
    private void TriggerParticleEffect(int effectIndex)
    {
        if (particleEffects == null || effectIndex < 0 || effectIndex >= particleEffects.Length)
        {
            return;
        }
        
        if (particleEffects[effectIndex] != null)
        {
            particleEffects[effectIndex].Play();
        }
    }
    
    private void StopAllParticleEffects()
    {
        if (particleEffects == null) return;
        
        foreach (var effect in particleEffects)
        {
            if (effect != null)
                effect.Stop();
        }
    }
    
    private void PlaySlideAudio(int slideIndex)
    {
        if (audioSource == null || slideAudioClips == null) return;
        
        if (slideIndex >= 0 && slideIndex < slideAudioClips.Length && slideAudioClips[slideIndex] != null)
        {
            audioSource.clip = slideAudioClips[slideIndex];
            audioSource.Play();
        }
    }
    
    private IEnumerator FadeLights(float targetIntensity, float duration)
    {
        if (sceneLights == null || sceneLights.Length == 0 || duration <= 0) yield break;
        
        float[] startIntensities = new float[sceneLights.Length];
        for (int i = 0; i < sceneLights.Length; i++)
        {
            if (sceneLights[i] != null)
                startIntensities[i] = sceneLights[i].intensity;
        }
        
        float elapsedTime = 0f;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / duration;
            
            for (int i = 0; i < sceneLights.Length; i++)
            {
                if (sceneLights[i] != null)
                {
                    sceneLights[i].intensity = Mathf.Lerp(startIntensities[i], targetIntensity, progress);
                }
            }
            
            yield return null;
        }
        
        // Ensure final values
        for (int i = 0; i < sceneLights.Length; i++)
        {
            if (sceneLights[i] != null)
            {
                sceneLights[i].intensity = targetIntensity;
            }
        }
    }
    
    private IEnumerator ChangeLightColor(Color targetColor, float duration)
    {
        if (sceneLights == null || sceneLights.Length == 0 || duration <= 0) yield break;
        
        Color[] startColors = new Color[sceneLights.Length];
        for (int i = 0; i < sceneLights.Length; i++)
        {
            if (sceneLights[i] != null)
                startColors[i] = sceneLights[i].color;
        }
        
        float elapsedTime = 0f;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / duration;
            
            for (int i = 0; i < sceneLights.Length; i++)
            {
                if (sceneLights[i] != null)
                {
                    sceneLights[i].color = Color.Lerp(startColors[i], targetColor, progress);
                }
            }
            
            yield return null;
        }
        
        // Ensure final color
        for (int i = 0; i < sceneLights.Length; i++)
        {
            if (sceneLights[i] != null)
            {
                sceneLights[i].color = targetColor;
            }
        }
    }
    
    #endregion
} 