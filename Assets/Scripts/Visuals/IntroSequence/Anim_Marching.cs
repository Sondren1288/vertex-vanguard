using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class Anim_Marching : MonoBehaviour
{
    [Header("Formation Settings")]
    [SerializeField] private GameObject unitPrefab;
    [SerializeField] private int unitCount = 16;
    [SerializeField] private float unitSpacing = 1.5f;
    [SerializeField] private Vector2 boundingBoxSize = new Vector2(10f, 10f);
    
    [Header("Animation Settings")]
    [SerializeField] private float splitAnimationDuration = 2f;
    [SerializeField] private float rejoinAnimationDuration = 2f;
    [SerializeField] private float splitDistance = 3f;
    
    [Header("Gizmos")]
    [SerializeField] private Color boundingBoxColor = Color.yellow;
    [SerializeField] private bool showBoundingBox = true;
    
    private List<GameObject> allUnits = new List<GameObject>();
    private List<Vector3> originalFormationPositions = new List<Vector3>();
    private List<GameObject> leftGroup = new List<GameObject>();
    private List<GameObject> rightGroup = new List<GameObject>();
    
    private bool isFormationSplit = false;
    private bool isAnimating = false;

    private AnimationManager animationManager;
    public int triggeringSlideIndex = 2;

    private void Start()
    {
        CreateSquareFormation();
        animationManager = FindFirstObjectByType<AnimationManager>();
        animationManager.OnSlideReached += (slideIndex) => {
            if(slideIndex == triggeringSlideIndex){
                SplitFormation();
            } else {
                RejoinFormation();
            }
        };

    }

    /// <summary>
    /// Creates a square formation of units within the bounding box
    /// </summary>
    public void CreateSquareFormation()
    {
        ClearExistingUnits();
        
        if (unitPrefab == null)
        {
            Debug.LogWarning("Unit prefab is not assigned!");
            return;
        }

        // Calculate grid dimensions for square formation
        int unitsPerSide = Mathf.CeilToInt(Mathf.Sqrt(unitCount));
        
        // Calculate actual spacing to fit within bounding box
        float actualSpacingX = boundingBoxSize.x / (unitsPerSide + 1);
        float actualSpacingZ = boundingBoxSize.y / (unitsPerSide + 1);
        float actualSpacing = Mathf.Min(actualSpacingX, actualSpacingZ, unitSpacing);
        
        // Calculate starting position (centered)
        Vector3 startPos = transform.position;
        startPos.x -= (unitsPerSide - 1) * actualSpacing * 0.5f;
        startPos.z -= (unitsPerSide - 1) * actualSpacing * 0.5f;
        
        // Create units in grid formation
        int unitsCreated = 0;
        for (int row = 0; row < unitsPerSide && unitsCreated < unitCount; row++)
        {
            for (int col = 0; col < unitsPerSide && unitsCreated < unitCount; col++)
            {
                Vector3 position = startPos + new Vector3(col * actualSpacing, 0, row * actualSpacing);
                
                // Check if position is within bounding box
                if (IsWithinBoundingBox(position))
                {
                    GameObject unit = Instantiate(unitPrefab, position, Quaternion.identity, transform);
                    unit.name = $"Unit_{unitsCreated}";
                    
                    allUnits.Add(unit);
                    originalFormationPositions.Add(position);
                    unitsCreated++;
                }
            }
        }
        
        isFormationSplit = false;
        Debug.Log($"Created {allUnits.Count} units in square formation");
    }

    /// <summary>
    /// Splits the army formation as if something fell in the middle
    /// </summary>
    public void SplitFormation()
    {
        if (isAnimating || isFormationSplit || allUnits.Count == 0)
            return;
            
        StartCoroutine(SplitFormationCoroutine());
    }

    /// <summary>
    /// Rejoins the split groups back to original formation
    /// </summary>
    public void RejoinFormation()
    {
        if (isAnimating || !isFormationSplit || allUnits.Count == 0)
            return;
            
        StartCoroutine(RejoinFormationCoroutine());
    }

    private IEnumerator SplitFormationCoroutine()
    {
        isAnimating = true;
        
        // Divide units into front and back groups based on their position relative to center
        Vector3 center = transform.position;
        leftGroup.Clear();
        rightGroup.Clear();
        
        foreach (GameObject unit in allUnits)
        {
            if (unit.transform.position.z < center.z)
                leftGroup.Add(unit); // Back group
            else
                rightGroup.Add(unit); // Front group
        }
        
        // Animate splitting
        float elapsedTime = 0f;
        List<Vector3> leftStartPositions = new List<Vector3>();
        List<Vector3> rightStartPositions = new List<Vector3>();
        
        // Record starting positions
        foreach (GameObject unit in leftGroup)
            leftStartPositions.Add(unit.transform.position);
        foreach (GameObject unit in rightGroup)
            rightStartPositions.Add(unit.transform.position);
        
        while (elapsedTime < splitAnimationDuration)
        {
            float t = elapsedTime / splitAnimationDuration;
            float easedT = EaseInOutQuad(t);
            
            // Move back group backward
            for (int i = 0; i < leftGroup.Count; i++)
            {
                Vector3 startPos = leftStartPositions[i];
                Vector3 targetPos = startPos + Vector3.back * splitDistance;
                leftGroup[i].transform.position = Vector3.Lerp(startPos, targetPos, easedT);
            }
            
            // Move front group forward
            for (int i = 0; i < rightGroup.Count; i++)
            {
                Vector3 startPos = rightStartPositions[i];
                Vector3 targetPos = startPos + Vector3.forward * splitDistance;
                rightGroup[i].transform.position = Vector3.Lerp(startPos, targetPos, easedT);
            }
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        isFormationSplit = true;
        isAnimating = false;
    }

    private IEnumerator RejoinFormationCoroutine()
    {
        isAnimating = true;
        
        // Record current positions
        List<Vector3> currentPositions = new List<Vector3>();
        foreach (GameObject unit in allUnits)
            currentPositions.Add(unit.transform.position);
        
        float elapsedTime = 0f;
        
        while (elapsedTime < rejoinAnimationDuration)
        {
            float t = elapsedTime / rejoinAnimationDuration;
            float easedT = EaseInOutQuad(t);
            
            // Move each unit back to original position
            for (int i = 0; i < allUnits.Count; i++)
            {
                Vector3 currentPos = currentPositions[i];
                Vector3 targetPos = originalFormationPositions[i];
                allUnits[i].transform.position = Vector3.Lerp(currentPos, targetPos, easedT);
            }
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // Ensure exact positioning
        for (int i = 0; i < allUnits.Count; i++)
        {
            allUnits[i].transform.position = originalFormationPositions[i];
        }
        
        leftGroup.Clear();
        rightGroup.Clear();
        isFormationSplit = false;
        isAnimating = false;
    }

    private bool IsWithinBoundingBox(Vector3 position)
    {
        Vector3 localPos = position - transform.position;
        return Mathf.Abs(localPos.x) <= boundingBoxSize.x * 0.5f && 
               Mathf.Abs(localPos.z) <= boundingBoxSize.y * 0.5f;
    }

    private void ClearExistingUnits()
    {
        foreach (GameObject unit in allUnits)
        {
            if (unit != null)
            {
                if (Application.isPlaying)
                    Destroy(unit);
                else
                    DestroyImmediate(unit);
            }
        }
        
        allUnits.Clear();
        originalFormationPositions.Clear();
        leftGroup.Clear();
        rightGroup.Clear();
    }

    private float EaseInOutQuad(float t)
    {
        return t < 0.5f ? 2f * t * t : -1f + (4f - 2f * t) * t;
    }

    private void OnDrawGizmos()
    {
        if (!showBoundingBox) return;
        
        Gizmos.color = boundingBoxColor;
        Gizmos.DrawWireCube(transform.position, new Vector3(boundingBoxSize.x, 0.1f, boundingBoxSize.y));
        
        // Draw a slightly transparent filled rectangle
        Color fillColor = boundingBoxColor;
        fillColor.a = 0.1f;
        Gizmos.color = fillColor;
        Gizmos.DrawCube(transform.position, new Vector3(boundingBoxSize.x, 0.05f, boundingBoxSize.y));
    }

    private void OnValidate()
    {
        // Clamp values to reasonable ranges
        unitCount = Mathf.Max(1, unitCount);
        unitSpacing = Mathf.Max(0.1f, unitSpacing);
        boundingBoxSize.x = Mathf.Max(1f, boundingBoxSize.x);
        boundingBoxSize.y = Mathf.Max(1f, boundingBoxSize.y);
        splitAnimationDuration = Mathf.Max(0.1f, splitAnimationDuration);
        rejoinAnimationDuration = Mathf.Max(0.1f, rejoinAnimationDuration);
        splitDistance = Mathf.Max(0.1f, splitDistance);
    }

    // Public properties for external access
    public bool IsFormationSplit => isFormationSplit;
    public bool IsAnimating => isAnimating;
    public int UnitCount => allUnits.Count;
    public List<GameObject> AllUnits => new List<GameObject>(allUnits);
}
