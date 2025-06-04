using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Anim_Organize : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private float movementDuration = 2f;
    [SerializeField] private float formationHoldTime = 3f;
    [SerializeField] private float dispersalHoldTime = 2f;
    [SerializeField] private float formationSpacing = 2f;
    [SerializeField] private float dispersalRadius = 8f;
    
    [Header("Animation Curve")]
    [SerializeField] private AnimationCurve movementCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    
    [Header("Area Constraints")]
    [SerializeField] private bool useAreaConstraints = true;
    [SerializeField] private AreaType areaType = AreaType.Rectangle;
    [SerializeField] private Vector3 areaCenter = Vector3.zero;
    [SerializeField] private Vector3 areaSize = new Vector3(20f, 10f, 20f);
    [SerializeField] private float circularRadius = 10f;
    [SerializeField] private bool showAreaGizmos = true;
    [SerializeField] private Color gizmoColor = Color.yellow;
    
    [Header("Unit Separation")]
    [SerializeField] private float minimumUnitDistance = 1.5f;
    [SerializeField] private bool enforceSeparation = true;
    [SerializeField] private int separationIterations = 3;
    [SerializeField] private float separationStrength = 0.8f;
    
    private List<Transform> units = new List<Transform>();
    private Dictionary<Transform, Vector3> originalPositions = new Dictionary<Transform, Vector3>();
    private Dictionary<Transform, Vector3> targetPositions = new Dictionary<Transform, Vector3>();
    
    private Coroutine animationLoop;
    private bool isAnimating = false;

    private AnimationManager animationManager;
    public int triggeringSlideIndex = 1;
    
    // Formation types
    private enum FormationType
    {
        Line,
        Circle,
        Square,
        Wedge,
        DoubleColumn
    }
    
    // Area constraint types
    public enum AreaType
    {
        Rectangle,
        Circle,
        Custom
    }
    
    private void Awake()
    {
        CollectUnits();
        animationManager = FindFirstObjectByType<AnimationManager>();
        animationManager.OnSlideReached += (slideIndex) => {
            if(slideIndex == triggeringSlideIndex){
                StartFormationLoop();
            } else if (isAnimating){
                StopFormationLoop();
            }
        };
    }
    
    /// <summary>
    /// Starts the formation animation loop
    /// </summary>
    public void StartFormationLoop()
    {
        if (isAnimating) return;
        
        if (units.Count == 0)
        {
            CollectUnits();
        }
        
        if (units.Count > 0)
        {
            isAnimating = true;
            animationLoop = StartCoroutine(FormationAnimationLoop());
        }
    }
    
    /// <summary>
    /// Stops the formation animation loop
    /// </summary>
    public void StopFormationLoop()
    {
        if (animationLoop != null)
        {
            StopCoroutine(animationLoop);
            animationLoop = null;
        }
        isAnimating = false;
    }
    
    /// <summary>
    /// Collects all child game objects as units
    /// </summary>
    private void CollectUnits()
    {
        units.Clear();
        originalPositions.Clear();
        
        foreach (Transform child in transform)
        {
            if (child.gameObject.activeInHierarchy)
            {
                units.Add(child);
                Vector3 constrainedPos = ConstrainPositionToArea(child.position);
                originalPositions[child] = constrainedPos;
                child.position = constrainedPos;
            }
        }
    }
    
    /// <summary>
    /// Main animation loop coroutine
    /// </summary>
    private IEnumerator FormationAnimationLoop()
    {
        while (isAnimating)
        {
            // Choose random formation
            FormationType formation = GetRandomFormation();
            
            // Calculate formation positions
            CalculateFormationPositions(formation);
            
            // Move to formation
            yield return StartCoroutine(MoveUnitsToTargets());
            
            // Hold formation
            yield return new WaitForSeconds(formationHoldTime);
            
            // Break formation - move to random positions
            CalculateRandomPositions();
            
            // Move to random positions
            yield return StartCoroutine(MoveUnitsToTargets());
            
            // Hold dispersed state
            yield return new WaitForSeconds(dispersalHoldTime);
        }
    }
    
    /// <summary>
    /// Gets a random formation type
    /// </summary>
    private FormationType GetRandomFormation()
    {
        FormationType[] formations = System.Enum.GetValues(typeof(FormationType)) as FormationType[];
        return formations[Random.Range(0, formations.Length)];
    }
    
    /// <summary>
    /// Calculates positions for the specified formation
    /// </summary>
    private void CalculateFormationPositions(FormationType formation)
    {
        targetPositions.Clear();
        Vector3 centerPos = GetConstrainedCenter();
        
        switch (formation)
        {
            case FormationType.Line:
                CalculateLineFormation(centerPos);
                break;
            case FormationType.Circle:
                CalculateCircleFormation(centerPos);
                break;
            case FormationType.Square:
                CalculateSquareFormation(centerPos);
                break;
            case FormationType.Wedge:
                CalculateWedgeFormation(centerPos);
                break;
            case FormationType.DoubleColumn:
                CalculateDoubleColumnFormation(centerPos);
                break;
        }
        
        // Ensure all positions are within constraints
        if (useAreaConstraints)
        {
            List<Transform> unitsToReposition = new List<Transform>(targetPositions.Keys);
            foreach (Transform unit in unitsToReposition)
            {
                targetPositions[unit] = ConstrainPositionToArea(targetPositions[unit]);
            }
        }
        
        // Note: No separation applied to formations to maintain precision and orderly appearance
    }
    
    /// <summary>
    /// Gets a formation center that respects area constraints
    /// </summary>
    private Vector3 GetConstrainedCenter()
    {
        if (!useAreaConstraints)
            return transform.position;
            
        Vector3 desiredCenter = transform.position;
        
        // Adjust center based on formation size and area constraints
        switch (areaType)
        {
            case AreaType.Rectangle:
                return new Vector3(
                    Mathf.Clamp(desiredCenter.x, areaCenter.x - areaSize.x * 0.3f, areaCenter.x + areaSize.x * 0.3f),
                    desiredCenter.y,
                    Mathf.Clamp(desiredCenter.z, areaCenter.z - areaSize.z * 0.3f, areaCenter.z + areaSize.z * 0.3f)
                );
            case AreaType.Circle:
                Vector3 offset = desiredCenter - areaCenter;
                offset.y = 0;
                if (offset.magnitude > circularRadius * 0.7f)
                {
                    offset = offset.normalized * circularRadius * 0.7f;
                }
                return areaCenter + offset;
            default:
                return desiredCenter;
        }
    }
    
    /// <summary>
    /// Constrains a position to stay within the defined area
    /// </summary>
    private Vector3 ConstrainPositionToArea(Vector3 position)
    {
        if (!useAreaConstraints)
            return position;
            
        switch (areaType)
        {
            case AreaType.Rectangle:
                return ConstrainToRectangle(position);
            case AreaType.Circle:
                return ConstrainToCircle(position);
            default:
                return position;
        }
    }
    
    /// <summary>
    /// Constrains position to rectangular area
    /// </summary>
    private Vector3 ConstrainToRectangle(Vector3 position)
    {
        Vector3 constrained = position;
        constrained.x = Mathf.Clamp(position.x, areaCenter.x - areaSize.x * 0.5f, areaCenter.x + areaSize.x * 0.5f);
        constrained.z = Mathf.Clamp(position.z, areaCenter.z - areaSize.z * 0.5f, areaCenter.z + areaSize.z * 0.5f);
        return constrained;
    }
    
    /// <summary>
    /// Constrains position to circular area
    /// </summary>
    private Vector3 ConstrainToCircle(Vector3 position)
    {
        Vector3 offset = position - areaCenter;
        offset.y = 0; // Don't constrain Y axis
        
        if (offset.magnitude > circularRadius)
        {
            offset = offset.normalized * circularRadius;
        }
        
        return areaCenter + new Vector3(offset.x, position.y, offset.z);
    }
    
    /// <summary>
    /// Calculates line formation positions
    /// </summary>
    private void CalculateLineFormation(Vector3 center)
    {
        float maxWidth = GetMaxFormationWidth();
        // Ensure spacing accounts for unit size and minimum distance
        float effectiveSpacing = Mathf.Max(formationSpacing, minimumUnitDistance);
        float actualSpacing = Mathf.Min(effectiveSpacing, maxWidth / Mathf.Max(1, units.Count - 1));
        float totalWidth = (units.Count - 1) * actualSpacing;
        Vector3 startPos = center - Vector3.right * (totalWidth * 0.5f);
        
        for (int i = 0; i < units.Count; i++)
        {
            Vector3 position = startPos + Vector3.right * (i * actualSpacing);
            targetPositions[units[i]] = position;
        }
    }
    
    /// <summary>
    /// Calculates circle formation positions
    /// </summary>
    private void CalculateCircleFormation(Vector3 center)
    {
        float maxRadius = GetMaxFormationRadius();
        // Calculate minimum radius needed for proper spacing
        float circumference = units.Count * Mathf.Max(formationSpacing, minimumUnitDistance);
        float minRequiredRadius = circumference / (2f * Mathf.PI);
        float desiredRadius = Mathf.Max(minRequiredRadius, units.Count * formationSpacing * 0.3f);
        float radius = Mathf.Min(desiredRadius, maxRadius);
        float angleStep = 360f / units.Count;
        
        for (int i = 0; i < units.Count; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 position = center + new Vector3(
                Mathf.Cos(angle) * radius,
                0f,
                Mathf.Sin(angle) * radius
            );
            targetPositions[units[i]] = position;
        }
    }
    
    /// <summary>
    /// Calculates square formation positions
    /// </summary>
    private void CalculateSquareFormation(Vector3 center)
    {
        int side = Mathf.CeilToInt(Mathf.Sqrt(units.Count));
        float maxWidth = GetMaxFormationWidth();
        // Ensure spacing is adequate for unit separation
        float effectiveSpacing = Mathf.Max(formationSpacing, minimumUnitDistance);
        float actualSpacing = Mathf.Min(effectiveSpacing, maxWidth / Mathf.Max(1, side - 1));
        Vector3 startPos = center - new Vector3((side - 1) * actualSpacing * 0.5f, 0f, (side - 1) * actualSpacing * 0.5f);
        
        for (int i = 0; i < units.Count; i++)
        {
            int row = i / side;
            int col = i % side;
            Vector3 position = startPos + new Vector3(col * actualSpacing, 0f, row * actualSpacing);
            targetPositions[units[i]] = position;
        }
    }
    
    /// <summary>
    /// Calculates wedge formation positions
    /// </summary>
    private void CalculateWedgeFormation(Vector3 center)
    {
        float maxWidth = GetMaxFormationWidth();
        // Use proper spacing that considers unit separation
        float effectiveSpacing = Mathf.Max(formationSpacing, minimumUnitDistance);
        float actualSpacing = Mathf.Min(effectiveSpacing, maxWidth / Mathf.Max(1, units.Count));
        
        int currentRow = 0;
        int unitsInCurrentRow = 1;
        int unitIndex = 0;
        
        while (unitIndex < units.Count)
        {
            float rowWidth = (unitsInCurrentRow - 1) * actualSpacing;
            Vector3 rowStart = center + new Vector3(-rowWidth * 0.5f, 0f, -currentRow * actualSpacing);
            
            for (int i = 0; i < unitsInCurrentRow && unitIndex < units.Count; i++)
            {
                Vector3 position = rowStart + Vector3.right * (i * actualSpacing);
                targetPositions[units[unitIndex]] = position;
                unitIndex++;
            }
            
            currentRow++;
            unitsInCurrentRow = Mathf.Min(unitsInCurrentRow + 2, units.Count - unitIndex + unitsInCurrentRow);
        }
    }
    
    /// <summary>
    /// Calculates double column formation positions
    /// </summary>
    private void CalculateDoubleColumnFormation(Vector3 center)
    {
        int rows = Mathf.CeilToInt(units.Count / 2f);
        float maxWidth = GetMaxFormationWidth();
        // Ensure proper spacing between columns and rows
        float effectiveSpacing = Mathf.Max(formationSpacing, minimumUnitDistance);
        float actualSpacing = Mathf.Min(effectiveSpacing, maxWidth);
        Vector3 startPos = center - new Vector3(actualSpacing * 0.5f, 0f, (rows - 1) * actualSpacing * 0.5f);
        
        for (int i = 0; i < units.Count; i++)
        {
            int row = i / 2;
            int col = i % 2;
            Vector3 position = startPos + new Vector3(col * actualSpacing, 0f, row * actualSpacing);
            targetPositions[units[i]] = position;
        }
    }
    
    /// <summary>
    /// Gets the maximum width available for formations
    /// </summary>
    private float GetMaxFormationWidth()
    {
        if (!useAreaConstraints)
            return float.MaxValue;
            
        switch (areaType)
        {
            case AreaType.Rectangle:
                return areaSize.x * 0.8f;
            case AreaType.Circle:
                return circularRadius * 1.6f;
            default:
                return float.MaxValue;
        }
    }
    
    /// <summary>
    /// Gets the maximum radius available for formations
    /// </summary>
    private float GetMaxFormationRadius()
    {
        if (!useAreaConstraints)
            return float.MaxValue;
            
        switch (areaType)
        {
            case AreaType.Rectangle:
                return Mathf.Min(areaSize.x, areaSize.z) * 0.4f;
            case AreaType.Circle:
                return circularRadius * 0.8f;
            default:
                return float.MaxValue;
        }
    }
    
    /// <summary>
    /// Calculates random dispersal positions
    /// </summary>
    private void CalculateRandomPositions()
    {
        targetPositions.Clear();
        float maxRadius = useAreaConstraints ? GetMaxFormationRadius() : dispersalRadius;
        float actualRadius = Mathf.Min(dispersalRadius, maxRadius);
        Vector3 center = GetConstrainedCenter();
        
        if (enforceSeparation)
        {
            // Use improved dispersal with better spacing
            CalculateDispersedPositionsWithSeparation(center, actualRadius);
        }
        else
        {
            // Fallback to simple random positioning
            foreach (Transform unit in units)
            {
                Vector2 randomCircle = Random.insideUnitCircle * actualRadius;
                Vector3 randomPosition = center + new Vector3(randomCircle.x, 0f, randomCircle.y);
                targetPositions[unit] = ConstrainPositionToArea(randomPosition);
            }
        }
    }
    
    /// <summary>
    /// Calculates dispersed positions with better separation using modified Poisson disk sampling
    /// </summary>
    private void CalculateDispersedPositionsWithSeparation(Vector3 center, float radius)
    {
        List<Vector3> validPositions = new List<Vector3>();
        int maxAttempts = 30;
        float minDistance = minimumUnitDistance;
        
        // Generate positions for each unit
        for (int unitIndex = 0; unitIndex < units.Count; unitIndex++)
        {
            Vector3 bestPosition = center;
            bool positionFound = false;
            
            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                // Generate random position within radius
                Vector2 randomCircle = Random.insideUnitCircle;
                
                // Use varying radius ranges to distribute units more evenly
                float minRadiusRatio = unitIndex > 0 ? 0.2f : 0f;
                float maxRadiusRatio = Mathf.Lerp(0.5f, 1f, (float)unitIndex / Mathf.Max(1, units.Count - 1));
                float actualDistance = Mathf.Lerp(minRadiusRatio, maxRadiusRatio, randomCircle.magnitude) * radius;
                
                Vector3 candidatePosition = center + new Vector3(
                    randomCircle.normalized.x * actualDistance,
                    0f,
                    randomCircle.normalized.y * actualDistance
                );
                
                candidatePosition = ConstrainPositionToArea(candidatePosition);
                
                // Check if position is valid (far enough from existing positions)
                bool validPosition = true;
                foreach (Vector3 existingPos in validPositions)
                {
                    Vector3 diff = candidatePosition - existingPos;
                    diff.y = 0;
                    if (diff.magnitude < minDistance)
                    {
                        validPosition = false;
                        break;
                    }
                }
                
                if (validPosition)
                {
                    bestPosition = candidatePosition;
                    positionFound = true;
                    break;
                }
            }
            
            // If no valid position found, use fallback position with offset
            if (!positionFound)
            {
                Vector3 fallbackOffset = new Vector3(
                    Random.Range(-radius * 0.8f, radius * 0.8f),
                    0f,
                    Random.Range(-radius * 0.8f, radius * 0.8f)
                );
                bestPosition = ConstrainPositionToArea(center + fallbackOffset);
            }
            
            validPositions.Add(bestPosition);
            targetPositions[units[unitIndex]] = bestPosition;
        }
        
        // Apply additional separation refinement
        ApplySeparation();
    }
    
    /// <summary>
    /// Moves all units to their target positions smoothly
    /// </summary>
    private IEnumerator MoveUnitsToTargets()
    {
        Dictionary<Transform, Vector3> startPositions = new Dictionary<Transform, Vector3>();
        
        // Record starting positions
        foreach (Transform unit in units)
        {
            if (unit != null)
            {
                startPositions[unit] = unit.position;
            }
        }
        
        float elapsedTime = 0f;
        
        while (elapsedTime < movementDuration)
        {
            float progress = elapsedTime / movementDuration;
            float curveValue = movementCurve.Evaluate(progress);
            
            foreach (Transform unit in units)
            {
                if (unit != null && startPositions.ContainsKey(unit) && targetPositions.ContainsKey(unit))
                {
                    unit.position = Vector3.Lerp(startPositions[unit], targetPositions[unit], curveValue);
                }
            }
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // Ensure units reach exact target positions
        foreach (Transform unit in units)
        {
            if (unit != null && targetPositions.ContainsKey(unit))
            {
                unit.position = targetPositions[unit];
            }
        }
    }
    
    /// <summary>
    /// Refreshes the unit list (call if children change during runtime)
    /// </summary>
    public void RefreshUnits()
    {
        CollectUnits();
    }
    
    /// <summary>
    /// Returns units to their original positions
    /// </summary>
    public void ReturnToOriginalPositions()
    {
        StopFormationLoop();
        StartCoroutine(ReturnToOriginalPositionsCoroutine());
    }
    
    private IEnumerator ReturnToOriginalPositionsCoroutine()
    {
        targetPositions.Clear();
        
        foreach (Transform unit in units)
        {
            if (originalPositions.ContainsKey(unit))
            {
                targetPositions[unit] = originalPositions[unit];
            }
        }
        
        yield return StartCoroutine(MoveUnitsToTargets());
    }
    
    /// <summary>
    /// Sets the area constraints at runtime
    /// </summary>
    public void SetAreaConstraints(Vector3 center, Vector3 size, AreaType type = AreaType.Rectangle)
    {
        areaCenter = center;
        areaSize = size;
        areaType = type;
        if (type == AreaType.Circle)
        {
            circularRadius = Mathf.Min(size.x, size.z) * 0.5f;
        }
    }
    
    /// <summary>
    /// Enables or disables area constraints
    /// </summary>
    public void SetUseAreaConstraints(bool enabled)
    {
        useAreaConstraints = enabled;
    }
    
    /// <summary>
    /// Applies separation logic to prevent units from overlapping
    /// </summary>
    private void ApplySeparation()
    {
        for (int iteration = 0; iteration < separationIterations; iteration++)
        {
            Dictionary<Transform, Vector3> adjustments = new Dictionary<Transform, Vector3>();
            
            // Initialize adjustments
            foreach (Transform unit in units)
            {
                adjustments[unit] = Vector3.zero;
            }
            
            // Calculate separation forces
            for (int i = 0; i < units.Count; i++)
            {
                for (int j = i + 1; j < units.Count; j++)
                {
                    Transform unitA = units[i];
                    Transform unitB = units[j];
                    
                    if (!targetPositions.ContainsKey(unitA) || !targetPositions.ContainsKey(unitB))
                        continue;
                        
                    Vector3 posA = targetPositions[unitA];
                    Vector3 posB = targetPositions[unitB];
                    
                    Vector3 direction = posA - posB;
                    direction.y = 0; // Only separate on XZ plane
                    float distance = direction.magnitude;
                    
                    if (distance < minimumUnitDistance && distance > 0.01f)
                    {
                        // Calculate separation force
                        float separationForce = (minimumUnitDistance - distance) * separationStrength;
                        Vector3 separationVector = direction.normalized * separationForce * 0.5f;
                        
                        adjustments[unitA] += separationVector;
                        adjustments[unitB] -= separationVector;
                    }
                    else if (distance <= 0.01f)
                    {
                        // Units are at nearly identical positions, add random offset
                        Vector3 randomOffset = new Vector3(
                            Random.Range(-0.5f, 0.5f),
                            0f,
                            Random.Range(-0.5f, 0.5f)
                        ) * minimumUnitDistance;
                        
                        adjustments[unitA] += randomOffset;
                        adjustments[unitB] -= randomOffset;
                    }
                }
            }
            
            // Apply adjustments and constrain to area
            foreach (Transform unit in units)
            {
                if (targetPositions.ContainsKey(unit))
                {
                    Vector3 newPosition = targetPositions[unit] + adjustments[unit];
                    targetPositions[unit] = useAreaConstraints ? ConstrainPositionToArea(newPosition) : newPosition;
                }
            }
        }
    }
    
    private void OnValidate()
    {
        movementDuration = Mathf.Max(0.1f, movementDuration);
        formationHoldTime = Mathf.Max(0f, formationHoldTime);
        dispersalHoldTime = Mathf.Max(0f, dispersalHoldTime);
        formationSpacing = Mathf.Max(0.1f, formationSpacing);
        dispersalRadius = Mathf.Max(1f, dispersalRadius);
        areaSize = new Vector3(Mathf.Max(1f, areaSize.x), Mathf.Max(1f, areaSize.y), Mathf.Max(1f, areaSize.z));
        circularRadius = Mathf.Max(1f, circularRadius);
        minimumUnitDistance = Mathf.Max(0.1f, minimumUnitDistance);
        separationIterations = Mathf.Max(1, separationIterations);
        separationStrength = Mathf.Clamp01(separationStrength);
    }
    
    private void OnDrawGizmos()
    {
        if (!showAreaGizmos || !useAreaConstraints)
            return;
            
        Gizmos.color = gizmoColor;
        
        switch (areaType)
        {
            case AreaType.Rectangle:
                Gizmos.DrawWireCube(areaCenter, areaSize);
                break;
            case AreaType.Circle:
                DrawCircleGizmo(areaCenter, circularRadius);
                break;
        }
    }
    
    private void DrawCircleGizmo(Vector3 center, float radius)
    {
        int segments = 32;
        float angleStep = 360f / segments;
        Vector3 prevPoint = center + Vector3.forward * radius;
        
        for (int i = 1; i <= segments; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 newPoint = center + new Vector3(Mathf.Sin(angle) * radius, 0f, Mathf.Cos(angle) * radius);
            Gizmos.DrawLine(prevPoint, newPoint);
            prevPoint = newPoint;
        }
    }
}
