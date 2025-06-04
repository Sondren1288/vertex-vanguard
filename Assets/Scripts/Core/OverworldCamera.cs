using System;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class OverworldCamera : MonoBehaviour
{ 
    private List<GameObject> nodeObjects = new();
    
    // Controls
    [SerializeField] private Camera mainCamera;
    private InputAction moveCameraAction;
    private InputAction cursorMovementAction;
    private InputAction rightClickAction;
    private InputAction middleClickAction;
    private InputAction scrollWheelAction;
    private Vector3 targetRotation;
    private Vector3 defaultCameraPosition;
    private Vector3 defaultCameraRotation;
 
    // Boundary
    private Vector3 minBoundary;
    private Vector3 maxBoundary;
    private float maxHeight = 20f;
    private float minHeight = 3f;
    private float padding = 15f;

    public bool IsNodesEmpty()
    {
        return nodeObjects.Count == 0;
    }

    public void SetNodeObjects(List<GameObject> nodeObjects)
    {
        this.nodeObjects = nodeObjects;
    }

    public int GetNodeCount()
    {
        return nodeObjects.Count;
    }

    private void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            throw new NullReferenceException("Game has no MainCamera");
        }
        InputActionMap UIKeybinds = InputSystem.actions.FindActionMap("UI");
        if (UIKeybinds != null)
        {
            UIKeybinds.Disable();
        }
        InputActionMap overworldActionMap = InputSystem.actions.FindActionMap("Overworld");
        if (overworldActionMap != null)
        {
            overworldActionMap.Enable();
            Logger.Error("Forced activation of overworld action map");
            Logger.Warning("Now, is it indeed active? " + overworldActionMap.enabled);
        }
        else
        {
            throw new NullReferenceException("Game has no Overworld keybinds");
        }
        
            
        targetRotation = new Vector3(mainCamera.transform.rotation.eulerAngles.x, mainCamera.transform.rotation.eulerAngles.y, mainCamera.transform.position.z);
        defaultCameraRotation = new Vector3(mainCamera.transform.rotation.eulerAngles.x, mainCamera.transform.rotation.eulerAngles.y, mainCamera.transform.position.z);
        defaultCameraPosition = new Vector3(mainCamera.transform.position.x, mainCamera.transform.position.y, mainCamera.transform.position.z);
        moveCameraAction = InputSystem.actions.FindActionMap("Overworld").FindAction("MoveCamera"); 
        cursorMovementAction = InputSystem.actions.FindActionMap("Overworld").FindAction("CursorMovement");
        middleClickAction = InputSystem.actions.FindActionMap("Overworld").FindAction("MoveCameraButton");
        scrollWheelAction = InputSystem.actions.FindActionMap("Overworld").FindAction("Zoom");
        
        maxBoundary = Vector3.zero;
        minBoundary = Vector3.zero;
    }

    private void Update()
    {
        float td = Time.deltaTime * 60;
        
        if (maxBoundary == Vector3.zero || minBoundary == Vector3.zero)
        {
            CalculateBoundaries();
        }

        Vector2 moveCameraInput = new Vector2();
        if (Input.GetKey(KeyCode.W))
        {
            moveCameraInput.y += 1;
        }
        if (Input.GetKey(KeyCode.S))
        {
            moveCameraInput.y -= 1;
        }
        if (Input.GetKey(KeyCode.A))
        {
            moveCameraInput.x -= 1;
        }
        if (Input.GetKey(KeyCode.D))
        {
            moveCameraInput.x += 1;
        }
                
        Vector2 zoom = scrollWheelAction.ReadValue<Vector2>();
        if (zoom.magnitude > 0.1f)
        {
            Vector3 forward = mainCamera.transform.forward;
            forward.Normalize();
                    
            // Use the y component of zoom vector for camera movement
            float zoomSpeed = 1f * td; // Adjust speed as needed
            Vector3 potentialNewPosition = mainCamera.transform.position + forward * (zoom.y * zoomSpeed);
                    
            // Apply the clamped position
            Vector3 targetPosition = ClampVector(potentialNewPosition);
            // Apply smooth movement using Lerp
            mainCamera.transform.position = Vector3.Lerp(
                mainCamera.transform.position, 
                targetPosition, 
                0.3f); // Adjust lerp factor for smoothness
        }
        
        //Vector2 moveCameraInput = moveCameraAction.ReadValue<Vector2>();
        if (moveCameraInput.x != 0 || moveCameraInput.y != 0 || middleClickAction.IsPressed())
        {
            if (middleClickAction.IsPressed())
            {
                moveCameraInput -= cursorMovementAction.ReadValue<Vector2>(); 
            }
        
            // Transform movement based on camera orientation
            Vector3 forward = mainCamera.transform.forward;
            forward.y = 0; // Project onto horizontal plane
            forward.Normalize();
        
            Vector3 right = mainCamera.transform.right;
            right.y = 0; // Project onto horizontal plane
            right.Normalize();
        
            // Combine the directions based on input
            Vector3 moveDirection = right * moveCameraInput.x + forward * moveCameraInput.y;
        
            // Apply movement
            float moveSpeed = 0.1f * td * Mathf.Log(mainCamera.transform.position.y); // Adjust speed as needed
            Vector3 potentialNewPosition = mainCamera.transform.position + moveDirection * moveSpeed;
        
            // Clamp vector
            potentialNewPosition = ClampVector(potentialNewPosition);
        
            // Apply the clamped position
            mainCamera.transform.position = potentialNewPosition;
        }
        
        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            mainCamera.transform.position = defaultCameraPosition;
            mainCamera.transform.eulerAngles = defaultCameraRotation;
            targetRotation = defaultCameraRotation;
        }
    }
    
    private Vector3 ClampVector(Vector3 vector)
    {
        // Clamp the position within boundaries
        float clampedX = Mathf.Clamp(vector.x, minBoundary.x, maxBoundary.x);
        float clampedZ = Mathf.Clamp(vector.z, minBoundary.y, maxBoundary.y);
        float clampedY = Mathf.Clamp(vector.y, minHeight, maxHeight);
            
        return new Vector3(clampedX, clampedY, clampedZ);
    }
    public void CalculateBoundaries()
    {
        // Find the min/max positions of all nodes
        if (nodeObjects.Count == 0) return;
     
        float minX = float.MaxValue, minZ = float.MaxValue;
        float maxX = float.MinValue, maxZ = float.MinValue;
     
        foreach (GameObject node in nodeObjects)
        {
            Vector3 pos = node.transform.position;
            minX = Mathf.Min(minX, pos.x);
            minZ = Mathf.Min(minZ, pos.z);
            maxX = Mathf.Max(maxX, pos.x);
            maxZ = Mathf.Max(maxZ, pos.z);
        }
     
        // Add padding
        minBoundary = new Vector2(minX - padding, minZ - padding);
        maxBoundary = new Vector2(maxX + padding, maxZ + padding);
    }

}
