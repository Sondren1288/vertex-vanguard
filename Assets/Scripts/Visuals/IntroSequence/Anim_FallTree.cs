using UnityEngine;
using System.Collections;

public class Anim_FallTree : MonoBehaviour
{
    [SerializeField] private float fallDuration = 2.0f;
    [SerializeField] private AnimationCurve fallCurve = new AnimationCurve(new Keyframe(0, 0, 0, 0), new Keyframe(1, 1, 2, 0));
    
    [Header("Slide Trigger Settings")]
    [SerializeField] private int triggerSlideIndex = 3;
    
    public bool isFalling { get; private set; } = false;
    public bool isReversing { get; private set; } = false;
    private Quaternion originalRotation;
    private Quaternion targetRotation;
    private AnimationManager animationManager;
    private int previousSlideIndex = -1;
    
    private void Start()
    {
        // Store the original rotation
        originalRotation = transform.rotation;
        // Target rotation: 90 degrees on X axis, -134 degrees on Y axis (same as FallingTree)
        targetRotation = originalRotation * Quaternion.Euler(90f, -134f, 0f);
        
        // Find and subscribe to the AnimationManager
        animationManager = FindFirstObjectByType<AnimationManager>();
        if (animationManager != null)
        {
            animationManager.OnSlideReached += OnSlideChanged;
        }
        else
        {
            Debug.LogWarning("AnimationManager not found! Tree animation will not trigger automatically.");
        }
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from events to prevent memory leaks
        if (animationManager != null)
        {
            animationManager.OnSlideReached -= OnSlideChanged;
        }
    }
    
    private void OnSlideChanged(int newSlideIndex)
    {
        // Check if we're entering slide 3
        if (newSlideIndex == triggerSlideIndex && previousSlideIndex != triggerSlideIndex)
        {
            Fall();
        }
        // Check if we're leaving slide 3
        else if (previousSlideIndex == triggerSlideIndex && newSlideIndex != triggerSlideIndex)
        {
            ResetTree();
        }
        
        previousSlideIndex = newSlideIndex;
    }
    
    public void Fall()
    {
        if (!isFalling && !isReversing)
        {
            StopAllCoroutines();
            StartCoroutine(FallAnimation());
        }
    }
    
    public void ResetTree()
    {
        if (!isReversing && !isFalling)
        {
            StopAllCoroutines();
            StartCoroutine(ReverseAnimation());
        }
        else if (isFalling)
        {
            // If currently falling, stop and start reverse from current position
            StopAllCoroutines();
            StartCoroutine(ReverseAnimation());
        }
    }
    
    private IEnumerator FallAnimation()
    {
        isFalling = true;
        isReversing = false;
        float elapsedTime = 0f;
        
        while (elapsedTime < fallDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / fallDuration;
            
            // Apply the animation curve for realistic falling motion
            float curveValue = fallCurve.Evaluate(progress);
            
            // Smoothly interpolate between original and target rotation
            transform.rotation = Quaternion.Slerp(originalRotation, targetRotation, curveValue);
            
            yield return null;
        }
        
        // Ensure we end at the exact target rotation
        transform.rotation = targetRotation;
        isFalling = false;
    }
    
    private IEnumerator ReverseAnimation()
    {
        isReversing = true;
        isFalling = false;
        
        // Get the current rotation as starting point for reverse
        Quaternion startRotation = transform.rotation;
        float elapsedTime = 0f;
        
        while (elapsedTime < fallDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / fallDuration;
            
            // Use the same curve but in reverse for smooth motion
            float curveValue = fallCurve.Evaluate(1f - progress);
            
            // Interpolate from current position back to original
            transform.rotation = Quaternion.Slerp(startRotation, originalRotation, progress);
            
            yield return null;
        }
        
        // Ensure we end at the exact original rotation
        transform.rotation = originalRotation;
        isReversing = false;
    }
    
    // Public methods for manual control (useful for testing)
    public void ManualFall()
    {
        Fall();
    }
    
    public void ManualReset()
    {
        ResetTree();
    }
}

