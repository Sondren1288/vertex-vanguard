using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Anim_Ambush : MonoBehaviour
{
    [Header("Animation Settings")]
    public Transform[] hidingLocations;
    public Transform[] ambushingUnits;
    public int triggeringSlideIndex = 3;
    
    [Header("Animation Parameters")]
    [SerializeField] private float jumpDuration = 1.5f;
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float hiddenScale = 0.6f;
    [SerializeField] private AnimationCurve jumpCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    private AnimationManager animationManager;
    private Vector3[] originalPositions;
    private Vector3[] originalScales;
    private Transform[] assignedHidingSpots;
    private bool isAnimating = false;
    private bool isHidden = false;

    private void Start()
    {
        // Store original positions and scales
        StoreOriginalTransforms();
        
        // Assign hiding spots to units
        AssignHidingSpots();
        
        // Find and subscribe to AnimationManager
        animationManager = FindFirstObjectByType<AnimationManager>();
        if (animationManager != null)
        {
            animationManager.OnSlideReached += OnSlideChanged;
        }
        else
        {
            Debug.LogWarning("AnimationManager not found! Ambush animation will not trigger automatically.");
        }
        
        // Start hidden
        StartCoroutine(InitialHide());
    }
    
    private void OnDestroy()
    {
        if (animationManager != null)
        {
            animationManager.OnSlideReached -= OnSlideChanged;
        }
    }
    
    private void StoreOriginalTransforms()
    {
        if (ambushingUnits == null || ambushingUnits.Length == 0)
        {
            Debug.LogWarning("No ambushing units assigned!");
            return;
        }
        
        originalPositions = new Vector3[ambushingUnits.Length];
        originalScales = new Vector3[ambushingUnits.Length];
        
        for (int i = 0; i < ambushingUnits.Length; i++)
        {
            if (ambushingUnits[i] != null)
            {
                originalPositions[i] = ambushingUnits[i].position;
                originalScales[i] = ambushingUnits[i].localScale;
            }
        }
    }
    
    private void AssignHidingSpots()
    {
        if (hidingLocations == null || hidingLocations.Length == 0 || 
            ambushingUnits == null || ambushingUnits.Length == 0)
        {
            Debug.LogWarning("Missing hiding locations or ambushing units!");
            return;
        }
        
        assignedHidingSpots = new Transform[ambushingUnits.Length];
        
        for (int i = 0; i < ambushingUnits.Length; i++)
        {
            if (ambushingUnits[i] != null)
            {
                // Find closest hiding location
                assignedHidingSpots[i] = FindClosestHidingLocation(ambushingUnits[i].position);
            }
        }
    }
    
    private Transform FindClosestHidingLocation(Vector3 unitPosition)
    {
        Transform closest = null;
        float closestDistance = float.MaxValue;
        
        foreach (Transform hidingSpot in hidingLocations)
        {
            if (hidingSpot != null)
            {
                float distance = Vector3.Distance(unitPosition, hidingSpot.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closest = hidingSpot;
                }
            }
        }
        
        return closest;
    }
    
    private void OnSlideChanged(int slideIndex)
    {
        if (slideIndex == triggeringSlideIndex && isHidden)
        {
            Ambush();
        }
        else if (slideIndex != triggeringSlideIndex && !isHidden)
        {
            Hide();
        }
    }
    
    public void Ambush()
    {
        if (!isAnimating && isHidden)
        {
            StartCoroutine(AmbushAnimation());
        }
    }
    
    public void Hide()
    {
        if (!isAnimating && !isHidden)
        {
            StartCoroutine(HideAnimation());
        }
    }
    
    private IEnumerator InitialHide()
    {
        // Instant hide without animation for initial setup
        for (int i = 0; i < ambushingUnits.Length; i++)
        {
            if (ambushingUnits[i] != null && assignedHidingSpots[i] != null)
            {
                ambushingUnits[i].position = assignedHidingSpots[i].position;
                ambushingUnits[i].localScale = originalScales[i] * hiddenScale;
            }
        }
        isHidden = true;
        yield return null;
    }
    
    private IEnumerator AmbushAnimation()
    {
        isAnimating = true;
        
        // Start all units jumping out simultaneously
        List<Coroutine> jumpCoroutines = new List<Coroutine>();
        
        for (int i = 0; i < ambushingUnits.Length; i++)
        {
            if (ambushingUnits[i] != null && assignedHidingSpots[i] != null)
            {
                jumpCoroutines.Add(StartCoroutine(JumpToPosition(
                    ambushingUnits[i], 
                    assignedHidingSpots[i].position, 
                    originalPositions[i], 
                    originalScales[i] * hiddenScale, 
                    originalScales[i]
                )));
            }
        }
        
        // Wait for all jumps to complete
        foreach (Coroutine coroutine in jumpCoroutines)
        {
            yield return coroutine;
        }
        
        isHidden = false;
        isAnimating = false;
    }
    
    private IEnumerator HideAnimation()
    {
        isAnimating = true;
        
        // Start all units jumping to hiding spots simultaneously
        List<Coroutine> jumpCoroutines = new List<Coroutine>();
        
        for (int i = 0; i < ambushingUnits.Length; i++)
        {
            if (ambushingUnits[i] != null && assignedHidingSpots[i] != null)
            {
                jumpCoroutines.Add(StartCoroutine(JumpToPosition(
                    ambushingUnits[i], 
                    originalPositions[i], 
                    assignedHidingSpots[i].position, 
                    originalScales[i], 
                    originalScales[i] * hiddenScale
                )));
            }
        }
        
        // Wait for all jumps to complete
        foreach (Coroutine coroutine in jumpCoroutines)
        {
            yield return coroutine;
        }
        
        isHidden = true;
        isAnimating = false;
    }
    
    private IEnumerator JumpToPosition(Transform unit, Vector3 startPos, Vector3 endPos, Vector3 startScale, Vector3 endScale)
    {
        float elapsedTime = 0f;
        
        while (elapsedTime < jumpDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / jumpDuration;
            
            // Apply curves for smooth motion
            float jumpProgress = jumpCurve.Evaluate(progress);
            float scaleProgress = scaleCurve.Evaluate(progress);
            
            // Calculate arc position
            Vector3 currentPos = Vector3.Lerp(startPos, endPos, jumpProgress);
            
            // Add arc height (parabolic curve)
            float heightOffset = jumpHeight * 4f * jumpProgress * (1f - jumpProgress);
            currentPos.y += heightOffset;
            
            // Apply position and scale
            unit.position = currentPos;
            unit.localScale = Vector3.Lerp(startScale, endScale, scaleProgress);
            
            yield return null;
        }
        
        // Ensure exact final position and scale
        unit.position = endPos;
        unit.localScale = endScale;
    }
    
    // Public methods for manual control
    public void ManualAmbush()
    {
        Ambush();
    }
    
    public void ManualHide()
    {
        Hide();
    }
}
