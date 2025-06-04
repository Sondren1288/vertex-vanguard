using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PaletteNamespace;
using UINamespace;
using UnityEngine.UIElements;

public class InfoButton : MonoBehaviour
{
    public string[] infoTextParagraphs;
    public Palette.PaletteColours clickedMaterialType;
    
    private bool hasBeenClicked = false;
    private bool isScaling = false;
    private bool isPanelOpen = false;
    private Vector3 originalScale;
    private Quaternion originalRotation;
    private Coroutine wiggleCoroutine;
    private Dictionary<Transform, Color> baseColors = new Dictionary<Transform, Color>();
    private UIDocument document;
    private VisualElement rootVisualElement;
    private VisualElement infoButtonContainer;
    private Button closeButton;
    public StyleSheet infoButtonStyleSheet;

    private void Start()
    {
        originalScale = transform.localScale;
        originalRotation = transform.rotation;
        
        // Store the base colors for each child (these are the brighter/default colors)
        foreach(Transform child in transform){
            Color originalColor = child.GetComponent<Renderer>().material.color;
            baseColors[child] = originalColor;
        }
        
        // Start the wiggle behavior
        wiggleCoroutine = StartCoroutine(WiggleBehavior());

        document = FindFirstObjectByType<UIDocument>();
        if (document == null)
        {
            document = gameObject.AddComponent<UIDocument>();
        }
        SetupUI();
    }

    private void SetupUI(){
        rootVisualElement = document.rootVisualElement;
        infoButtonContainer = rootVisualElement.Q<VisualElement>("info-text-container");
        closeButton = rootVisualElement.Q<Button>("close-button");
        
        if(infoButtonStyleSheet != null){
            if (!rootVisualElement.styleSheets.Contains(infoButtonStyleSheet))
            {
                rootVisualElement.styleSheets.Add(infoButtonStyleSheet);
            }
        }
        
        // Ensure the container starts hidden
        if (infoButtonContainer != null)
        {
            infoButtonContainer.AddToClassList("hidden");
        }

        if (closeButton != null)
        {
            closeButton.clicked += () => HideInfoText();
        }
        
        // Add click outside to close functionality
        if (rootVisualElement != null)
        {
            rootVisualElement.RegisterCallback<ClickEvent>(OnRootElementClicked, TrickleDown.TrickleDown);
        }
    }

    private void OnRootElementClicked(ClickEvent evt)
    {
        if (!isPanelOpen) return;
        
        // Check if the click was outside the info panel
        if (infoButtonContainer != null && !infoButtonContainer.worldBound.Contains(evt.position))
        {
            HideInfoText();
            evt.StopPropagation();
        }
    }

    public void OnMouseEnter()
    {
        if (isScaling) return;
        
        // Darken the colours (relative to base colors)
        float darknessFactor = 0.7f;
        foreach(Transform child in transform){
            Color baseColor = baseColors[child];
            child.GetComponent<Renderer>().material.color = new Color(
                baseColor.r * darknessFactor,
                baseColor.g * darknessFactor, 
                baseColor.b * darknessFactor,
                baseColor.a
            );
        }
        
        // Smooth scale increase
        StartCoroutine(SmoothScale(new Vector3(originalScale.x, originalScale.y * 2f, originalScale.z), 0.2f));
    }

    public void OnMouseExit()
    {
        if (isScaling) return;
        
        // Restore the base colours
        foreach(Transform child in transform){
            child.GetComponent<Renderer>().material.color = baseColors[child];
        }
        
        // Smooth scale decrease
        StartCoroutine(SmoothScale(originalScale, 0.2f));
    }

    public void OnMouseDown()
    {
        // Show the info text
        ShowInfoText();
        if (hasBeenClicked) return;
        
        hasBeenClicked = true;
        
        // Stop wiggling
        if (wiggleCoroutine != null)
        {
            StopCoroutine(wiggleCoroutine);
            wiggleCoroutine = null;
        }
        
        // Reset rotation to original scene rotation
        transform.rotation = originalRotation;
        
        // Change children materials to clicked color and update base colors
        foreach(Transform child in transform){
            Color clickedColor = Palette.Instance.GetColor(clickedMaterialType);
            child.GetComponent<Renderer>().material.color = clickedColor;
            baseColors[child] = clickedColor; // Update base color for future hover effects
        }        
    }

    private void ShowInfoText(){
        if (infoButtonContainer != null)
        {
            infoButtonContainer.RemoveFromClassList("hidden");
            isPanelOpen = true;
            
            // Update the text content
            Label infoLabel = infoButtonContainer.Q<Label>("info-text-0");
            if (infoLabel != null)
            {
                infoLabel.text = string.Join("\n\n", infoTextParagraphs);
            }
        }
    }

    private void HideInfoText(){
        if (infoButtonContainer != null)
        {
            infoButtonContainer.AddToClassList("hidden");
            isPanelOpen = false;
        }
    }
    
    private IEnumerator SmoothScale(Vector3 targetScale, float duration)
    {
        isScaling = true;
        Vector3 startScale = transform.localScale;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            t = t * t * (3f - 2f * t); // Smoothstep for easing
            
            transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }
        
        transform.localScale = targetScale;
        isScaling = false;
    }
    
    private IEnumerator WiggleBehavior()
    {
        while (!hasBeenClicked)
        {
            // Wait for random interval (minimum 2 seconds)
            float waitTime = Random.Range(2f, 6f);
            yield return new WaitForSeconds(waitTime);
            
            if (hasBeenClicked) break;
            
            // Perform wiggle animation
            yield return StartCoroutine(WiggleAnimation());
        }
    }
    
    private IEnumerator WiggleAnimation()
    {
        float wiggleDuration = 0.8f;
        float wiggleAngle = 15f;
        float elapsed = 0f;
        
        while (elapsed < wiggleDuration)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = elapsed / wiggleDuration;
            
            // Create a wiggle pattern using sine wave
            float angle = Mathf.Sin(normalizedTime * Mathf.PI * 6f) * wiggleAngle * (1f - normalizedTime);
            
            // Apply wiggle rotation relative to the original rotation
            Quaternion wiggleRotation = Quaternion.Euler(0, 0, angle);
            transform.rotation = originalRotation * wiggleRotation;
            
            yield return null;
        }
        
        // Return to original scene rotation
        transform.rotation = originalRotation;
    }

    private void OnDestroy()
    {
        // Clean up event listeners
        if (rootVisualElement != null)
        {
            rootVisualElement.UnregisterCallback<ClickEvent>(OnRootElementClicked, TrickleDown.TrickleDown);
        }
    }
}
