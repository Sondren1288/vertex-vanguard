using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;
using System;
using System.ComponentModel;
using UnityEngine.SceneManagement;

public class AnimationManager : MonoBehaviour
{
    [Header("Presentation Settings")]
    public string[] slideTexts;
    public Transform[] cameraWaypointsForSlides;
    public float cameraTransitionDuration = 2f;
    public float typewriterSpeed = 0.05f;
    public AnimationCurve cameraMovementCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("UI References")]
    public UIDocument uiDocument;
    
    [Header("Events")]
    public Action<int> OnSlideReached;
    
    private Camera mainCamera;
    private VisualElement textContainer;
    private Label textLabel;
    private Label slideIndicator;
    private Button prevButton;
    private Button nextButton;
    private VisualElement progressFill;
    private VisualElement rootElement;
    private int currentSlideIndex = 0;
    private bool isTransitioning = false;
    private Coroutine currentTypewriterCoroutine;
    private bool isInitialized = false;
    
    private void Awake()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
            mainCamera = FindFirstObjectByType<Camera>();
        
        InitializeUI();
    }
    
    private void Start()
    {
        // Validate setup before starting
        if (ValidateSetup())
        {
            StartPresentation();
        }
        else
        {
            Debug.LogError("AnimationManager setup is invalid. Please check the configuration.");
        }
    }
    
    private bool ValidateSetup()
    {
        if (cameraWaypointsForSlides == null || cameraWaypointsForSlides.Length == 0)
        {
            Debug.LogError("No camera waypoints assigned to AnimationManager!");
            return false;
        }
        
        if (slideTexts == null || slideTexts.Length == 0)
        {
            Debug.LogWarning("No slide texts assigned. Using empty text for slides.");
            // Create empty array matching waypoints length
            slideTexts = new string[cameraWaypointsForSlides.Length];
            for (int i = 0; i < slideTexts.Length; i++)
            {
                slideTexts[i] = $"Slide {i + 1}";
            }
        }
        
        // Ensure arrays match in length
        if (slideTexts.Length != cameraWaypointsForSlides.Length)
        {
            Debug.LogWarning($"Slide texts ({slideTexts.Length}) and camera waypoints ({cameraWaypointsForSlides.Length}) arrays have different lengths. Adjusting slide texts array.");
            
            string[] newSlideTexts = new string[cameraWaypointsForSlides.Length];
            for (int i = 0; i < newSlideTexts.Length; i++)
            {
                if (i < slideTexts.Length && !string.IsNullOrEmpty(slideTexts[i]))
                {
                    newSlideTexts[i] = slideTexts[i];
                }
                else
                {
                    newSlideTexts[i] = $"Slide {i + 1}";
                }
            }
            slideTexts = newSlideTexts;
        }
        
        if (mainCamera == null)
        {
            Debug.LogError("No camera found for AnimationManager!");
            return false;
        }
        
        return true;
    }
    
    private void InitializeUI()
    {
        try
        {
            if (uiDocument == null)
            {
                Debug.LogWarning("UI Document not assigned to AnimationManager! Trying to get from component...");
                uiDocument = GetComponent<UIDocument>();
                
                if (uiDocument == null)
                {
                    Debug.LogError("No UI Document found! Please assign one or add a UIDocument component.");
                    return;
                }
            }
            
            rootElement = uiDocument.rootVisualElement;
            
            if (rootElement == null)
            {
                Debug.LogError("Failed to get root visual element from UI Document!");
                return;
            }
            
            // Find UI elements with null checks
            textLabel = rootElement.Q<Label>("slide-text");
            slideIndicator = rootElement.Q<Label>("slide-indicator");
            prevButton = rootElement.Q<Button>("prev-button");
            nextButton = rootElement.Q<Button>("next-button");
            progressFill = rootElement.Q<VisualElement>("progress-fill");
            textContainer = rootElement.Q<VisualElement>("text-container");
            
           
            
            // Log warnings for missing UI elements
            if (textLabel == null)
            {
                Debug.LogWarning("Could not find 'slide-text' label in UXML! Text display will not work.");
            }
            
            if (slideIndicator == null)
            {
                Debug.LogWarning("Could not find 'slide-indicator' label in UXML! Slide indicator will not work.");
            }
            
            if (prevButton == null || nextButton == null)
            {
                Debug.LogWarning("Could not find navigation buttons in UXML! Button navigation will not work.");
            }
            
            if (progressFill == null)
            {
                Debug.LogWarning("Could not find 'progress-fill' element in UXML! Progress bar will not work.");
            }
            
            // Setup button events with null checks
            if (prevButton != null)
            {
                prevButton.clicked += PreviousSlide;
            }
            
            if (nextButton != null)
            {
                nextButton.clicked += NextSlide;
            }
            
            isInitialized = true;
            UpdateUI();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error initializing UI: {e.Message}");
            isInitialized = false;
        }
    }
    
    public void StartPresentation()
    {
        if (!ValidateSetup()) return;
        
        currentSlideIndex = 0;
        textContainer.AddToClassList("text-container-left");
        MoveToSlide(0);
    }
    
    public void NextSlide()
    {
        if (isTransitioning || !ValidateSlideIndex(currentSlideIndex)) return;
        
        if (currentSlideIndex < cameraWaypointsForSlides.Length - 1)
        {
            currentSlideIndex++;
            MoveToSlide(currentSlideIndex);
            textContainer.AddToClassList("text-container-right");
        } else {
            //Change scene to main menu
            SceneManager.LoadScene("MainMenu");
        }
    }
    
    public void PreviousSlide()
    {
        if (isTransitioning || !ValidateSlideIndex(currentSlideIndex)) return;
        
        if (currentSlideIndex > 0)
        {
            currentSlideIndex--;
            if(currentSlideIndex == 0){
                textContainer.AddToClassList("text-container-left");
            }
            MoveToSlide(currentSlideIndex);
            
        }
    }
    
    public void MoveToSlide(int slideIndex)
    {
        if (!ValidateSlideIndex(slideIndex)) return;
        
        StartCoroutine(TransitionToSlide(slideIndex));
    }
    
    private bool ValidateSlideIndex(int slideIndex)
    {
        if (cameraWaypointsForSlides == null || cameraWaypointsForSlides.Length == 0)
        {
            Debug.LogError("No camera waypoints available!");
            return false;
        }
        
        if (slideIndex < 0 || slideIndex >= cameraWaypointsForSlides.Length)
        {
            Debug.LogError($"Slide index {slideIndex} is out of range! Valid range: 0-{cameraWaypointsForSlides.Length - 1}");
            return false;
        }
        
        if (cameraWaypointsForSlides[slideIndex] == null)
        {
            Debug.LogError($"Camera waypoint at index {slideIndex} is null!");
            return false;
        }
        
        return true;
    }
    
    private IEnumerator TransitionToSlide(int slideIndex)
    {
        if (!ValidateSlideIndex(slideIndex)) yield break;
        
        isTransitioning = true;
        UpdateUI();
        
        // Stop any current typewriter effect
        if (currentTypewriterCoroutine != null)
        {
            StopCoroutine(currentTypewriterCoroutine);
        }
        
        // Clear text during transition
        if (textLabel != null)
        {
            textLabel.text = "";
        }
        
        // Smooth camera transition
        yield return StartCoroutine(MoveCameraToWaypoint(cameraWaypointsForSlides[slideIndex]));
        
        // Start typewriter effect for new slide text
        if (slideIndex < slideTexts.Length && !string.IsNullOrEmpty(slideTexts[slideIndex]))
        {
            currentTypewriterCoroutine = StartCoroutine(TypewriterEffect(slideTexts[slideIndex]));
        }
        else if (textLabel != null)
        {
            // Show placeholder text if no text is available
            textLabel.text = $"Slide {slideIndex + 1}";
        }
        
        // Invoke slide reached event
        try
        {
            OnSlideReached?.Invoke(slideIndex);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error in OnSlideReached event: {e.Message}");
        }
        
        isTransitioning = false;
        UpdateUI();
    }
    
    private IEnumerator MoveCameraToWaypoint(Transform targetWaypoint)
    {
        if (mainCamera == null || targetWaypoint == null) yield break;
        
        Vector3 startPosition = mainCamera.transform.position;
        Quaternion startRotation = mainCamera.transform.rotation;
        Vector3 targetPosition = targetWaypoint.position;
        Quaternion targetRotation = targetWaypoint.rotation;
        
        float elapsedTime = 0f;
        
        while (elapsedTime < cameraTransitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / cameraTransitionDuration;
            float curveValue = cameraMovementCurve.Evaluate(progress);
            
            if (mainCamera != null)
            {
                mainCamera.transform.position = Vector3.Lerp(startPosition, targetPosition, curveValue);
                mainCamera.transform.rotation = Quaternion.Lerp(startRotation, targetRotation, curveValue);
            }
            
            yield return null;
        }
        
        // Ensure final position and rotation are exact
        if (mainCamera != null)
        {
            mainCamera.transform.position = targetPosition;
            mainCamera.transform.rotation = targetRotation;
        }
    }
    
    private IEnumerator TypewriterEffect(string fullText)
    {
        if (textLabel == null || string.IsNullOrEmpty(fullText)) yield break;
        
        textLabel.text = "";
        
        for (int i = 0; i <= fullText.Length; i++)
        {
            if (textLabel != null)
            {
                textLabel.text = fullText.Substring(0, i);
            }
            yield return new WaitForSeconds(typewriterSpeed);
        }
        
        currentTypewriterCoroutine = null;
    }
    
    public void SkipTypewriter()
    {
        if (currentTypewriterCoroutine != null)
        {
            StopCoroutine(currentTypewriterCoroutine);
            currentTypewriterCoroutine = null;
            
            if (ValidateSlideIndex(currentSlideIndex) && textLabel != null)
            {
                if (currentSlideIndex < slideTexts.Length && !string.IsNullOrEmpty(slideTexts[currentSlideIndex]))
                {
                    textLabel.text = slideTexts[currentSlideIndex];
                }
                else
                {
                    textLabel.text = $"Slide {currentSlideIndex + 1}";
                }
            }
        }
    }
    
    private void UpdateUI()
    {
        if (!isInitialized || cameraWaypointsForSlides == null || cameraWaypointsForSlides.Length == 0) return;
        
        try
        {
            // Update slide indicator
            if (slideIndicator != null)
            {
                slideIndicator.text = $"{currentSlideIndex + 1} / {cameraWaypointsForSlides.Length}";
            }
            
            // Update button states
            if (prevButton != null)
            {
                prevButton.SetEnabled(currentSlideIndex > 0 && !isTransitioning);
            }
            
            if (nextButton != null)
            {
                nextButton.SetEnabled(currentSlideIndex < cameraWaypointsForSlides.Length && !isTransitioning);
            }
            
            // Update progress bar
            if (progressFill != null && cameraWaypointsForSlides.Length > 0)
            {
                float progress = (float)(currentSlideIndex + 1) / cameraWaypointsForSlides.Length;
                progressFill.style.width = Length.Percent(progress * 100);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error updating UI: {e.Message}");
        }
    }
    
    private void Update()
    {
        if (!isInitialized || !ValidateSetup()) return;
        
        // Allow manual navigation with arrow keys
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.Space))
        {
            NextSlide();
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            PreviousSlide();
        }
        else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            SkipTypewriter();
        }
    }
    
    // Method to trigger custom actions when reaching specific slides
    public void OnSlideReachedHandler(int slideIndex)
    {
        if (!ValidateSlideIndex(slideIndex)) return;
        
        switch (slideIndex)
        {
            case 0:
                // First slide actions
                break;
            case 1:
                // Second slide actions
                break;
            // Add more cases as needed for your presentation
            default:
                break;
        }
    }
    
    private void OnEnable()
    {
        OnSlideReached += OnSlideReachedHandler;
    }
    
    private void OnDisable()
    {
        OnSlideReached -= OnSlideReachedHandler;
        
        // Clean up button events
        if (prevButton != null)
        {
            prevButton.clicked -= PreviousSlide;
        }
        
        if (nextButton != null)
        {
            nextButton.clicked -= NextSlide;
        }
    }
    
    // Public properties for external access
    public int CurrentSlideIndex => currentSlideIndex;
    public bool IsTransitioning => isTransitioning;
    public int TotalSlides => cameraWaypointsForSlides != null ? cameraWaypointsForSlides.Length : 0;
}
