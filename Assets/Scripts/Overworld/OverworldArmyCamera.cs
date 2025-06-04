using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))] // Ensure this GameObject always has a Camera component
public class OverworldArmyCamera : MonoBehaviour
{
    public Camera cameraComponent;
    private const float hoverHeight = 8.0f; // Height above the target position
    private const float distanceFromCenter = 8.0f;

    public void Awake()
    {
        // Get the Camera component attached to this GameObject
        cameraComponent = GetComponent<Camera>();
        // Start disabled, assuming the main camera is active initially
        if (cameraComponent != null)
        {
            cameraComponent.enabled = false;
        }
        else
        {
            Debug.LogError("OverworldArmyCamera requires a Camera component.", this);
        }
    }

    /// <summary>
    /// Moves the camera to hover above the target position, points it down, and enables it.
    /// </summary>
    /// <param name="targetPosition">The world position the camera should look at from above.</param>
    public void MoveAndEnable(Vector3 targetPosition)
    {
        if (cameraComponent == null) return;

        // Calculate the position 10 units directly above the target
        transform.position = targetPosition + Vector3.up * hoverHeight + new Vector3(0, 0, 1) * distanceFromCenter;

        // Set rotation to look straight down (90 degrees on the X-axis)
        transform.rotation = Quaternion.Euler(35.264f, 180f, 0f);

        // Enable this camera
        cameraComponent.enabled = true;
    }

    /// <summary>
    /// Disables this camera, allowing another camera (e.g., the main camera) to become active.
    /// </summary>
    public void DisableCamera()
    {
        if (cameraComponent != null)
        {
            cameraComponent.enabled = false;
        }
    }

    // void Update()
    // {
    //     // Check if the 'Q' key was pressed down this frame
    //     if (Input.GetKeyDown(KeyCode.Q))
    //     {
    //         Logger.Info("Q was actually pressed, frfr");
    //         // Only invoke if this camera is currently active, otherwise pressing Q
    //         // when the main camera is active would also trigger it.
    //         if (cameraComponent != null && cameraComponent.enabled)
    //         {
    //             Logger.Info("Q key pressed, invoking StopLookingAtArmy event.");
    //             GameEvents.StopLookingAtArmy.Invoke(new EmptyEventArgs());
    //         }
    //     }
    // }
    
    // Ensure all points are visible
    public void EnsureAllPointsVisible(List<Vector3> points)
    {
        if (points == null || points.Count == 0) return;
        // Calculate bounds
        Bounds bounds = new Bounds(points[0], Vector3.zero);
        foreach (Vector3 point in points)
        {
            bounds.Encapsulate(point);
        }
        
        // Determine required distance
        // --- Calculate Distance Accounting for Aspect Ratio ---
        Vector3 targetCenter = bounds.center;
        float distance;

        // Calculate distance needed to fit the bounds vertically
        float halfFovVerticalRad = cameraComponent.fieldOfView * 0.5f * Mathf.Deg2Rad;
        float distVert = (bounds.size.y * 0.5f) / Mathf.Tan(halfFovVerticalRad);

        // Calculate distance needed to fit the bounds horizontally
        // Horizontal FOV depends on vertical FOV and aspect ratio
        float halfFovHorizontalRad = Mathf.Atan(Mathf.Tan(halfFovVerticalRad) * cameraComponent.aspect);
        float distHor = (bounds.size.x * 0.5f) / Mathf.Tan(halfFovHorizontalRad);

        // Use the larger distance to ensure both dimensions fit
        distance = Mathf.Max(distVert, distHor);

        // Add a buffer (e.g., 10%) so points aren't right at the edge
        distance *= 1.1f;
        // --- End Distance Calculation ---

        // Use the calculated distance to adjust camera position
        // Note: How 'distance' translates to position depends on your camera setup (angle, etc.)
        // This example assumes 'distance' primarily affects the Z offset from the target center.
        // You might need to adjust how hoverHeight and distanceFromCenter are modified
        // based on this new 'distance' value depending on the desired framing.


        // Option 2: Adjusting along camera's backward direction (might be better for angled views)
        transform.position = targetCenter - transform.forward * distance; // Position 'distance' units away looking at center

        // Keep fixed rotation (if desired)
        transform.rotation = Quaternion.Euler(35.264f, 180f, 0f);

        Logger.Info($"Framing points. Bounds: {bounds.size}, Aspect: {cameraComponent.aspect}, Final Distance: {distance}");
    }
}
