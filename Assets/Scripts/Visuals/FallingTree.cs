using UnityEngine;
using System.Collections;

public class FallingTree : MonoBehaviour
{
    [SerializeField] private float fallDuration = 2.0f;
    [SerializeField] private AnimationCurve fallCurve = new AnimationCurve(new Keyframe(0, 0, 0, 0), new Keyframe(1, 1, 2, 0));
    
    [Header("Preparation Animation")]
    [SerializeField] private float prepareSwayAmount = 2.0f;
    [SerializeField] private float prepareSwaySpeed = 3.0f;
    [SerializeField] private Transform destination;
    
    public bool isFalling {get; private set;} = false;
    public bool isPreparing {get; private set;} = false;
    private Quaternion originalRotation;
    private Quaternion targetRotation;
    private Coroutine prepareCoroutine;
    public string clearingAfterFallenTree;
    
    private void Start()
    {
        originalRotation = transform.rotation;
        // Target rotation: 90 degrees on X axis, -134 degrees on Y axis
        targetRotation = originalRotation * Quaternion.Euler(90f, -134f, 0f);
    }
    
    public void PrepareToFall()
    {
        if (!isFalling && !isPreparing)
        {
            isPreparing = true;
            prepareCoroutine = StartCoroutine(PrepareAnimation());
        }
    }
    
    public void Fall()
    {
        if (!isFalling)
        {
            // Stop preparation animation if it's running
            if (isPreparing)
            {
                StopPrepareAnimation();
            }
            StartCoroutine(FallAnimation());
        }
    }
    
    public void StopPrepareAnimation()
    {
        if (isPreparing && prepareCoroutine != null)
        {
            StopCoroutine(prepareCoroutine);
            isPreparing = false;
            // Return to original rotation
            transform.rotation = originalRotation;
        }
    }
    
    private IEnumerator PrepareAnimation()
    {
        while (isPreparing)
        {
            // Create a gentle swaying motion
            float swayX = Mathf.Sin(Time.time * prepareSwaySpeed) * prepareSwayAmount;
            float swayZ = Mathf.Sin(Time.time * prepareSwaySpeed * 0.7f) * (prepareSwayAmount * 0.5f);
            
            Quaternion swayRotation = originalRotation * Quaternion.Euler(swayX, 0f, swayZ);
            transform.rotation = swayRotation;
            
            yield return null;
        }
    }
    
    private IEnumerator FallAnimation()
    {
        isFalling = true;
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
    
    // Optional: Method to reset the tree to its original position
    public void ResetTree()
    {
        if (!isFalling && !isPreparing)
        {
            transform.rotation = originalRotation;
        }
    }
}
