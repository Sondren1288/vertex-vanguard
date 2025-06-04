using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using System.Linq;
using System;
using TMPro;
using Unity.VisualScripting;
using Random = UnityEngine.Random;
using PaletteNamespace;
using UINamespace;
using System.Collections;

namespace OverworldNamespace {

public class OverworldManager : MonoBehaviour
{
    public GameObject overworldClearingPrefab;
    public GameObject overworldArmyPrefab;
    public GameObject individualUnitPrefab;
    public GameObject planePrefab;
    public GameObject pointerPrefab;
    
    // Display
    public GameObject edgeVisual;
    private ScreenTextDisplayManager screenTextDisplayManager;
    private readonly Dictionary<string, GameObject> connections = new();
    private List<GameObject> displayedArmies = new();
    private readonly List<GameObject> nodeObjects = new();

    private GameObject overworldSpawnGroup;
    private List<GameObject> displayedUnits = new();
    public Army globalSelectedArmy = null;
    public List<Unit> individuallySelectedUnits = new();
    
    private List<GameObject> arrows = new();
    private GameObject selectedArmyCube = null;
    
    // Camera
     private OverworldCamera overworldCamera;
    [SerializeField] private OverworldArmyCamera armyCamera;
    [SerializeField] private GameObject armyCameraPrefab;

    // Further animations
    [Header("Further animations")]
    public FallingTree fallenTree;
    public Ambush ambush;
    private Vector3 originalPosition = Vector3.zero;
    private Vector3 originalScale = Vector3.zero;
    private Color originalColor;
    public ParticleSystem cubeExplosion; 
    private float shrinkScale = 0.7f;


    private void FixedUpdate()
    {
        foreach (GameObject displayedUnit in displayedUnits)
        {
            Vector3 displayedUnitPos = displayedUnit.transform.position;
            if (displayedUnitPos.y < 0)
            {
                displayedUnit.transform.position = new Vector3(displayedUnitPos.x, 0.01f, displayedUnitPos.z);
                
                Rigidbody rb = displayedUnit.GetComponent<Rigidbody>();
                rb.linearVelocity *= -0.6f;
            }

            TextMeshPro tmp = displayedUnit.GetComponentInChildren<TextMeshPro>();
            if (!tmp) continue;
            tmp.AlignTextMeshCamera(armyCamera.cameraComponent);
        }
    }

    private void Awake()
    {
        if (overworldCamera == null)
        {
            this.AddComponent<OverworldCamera>();
            overworldCamera = this.GetComponent<OverworldCamera>();
        }

        overworldSpawnGroup = GameObject.FindGameObjectWithTag("Overworld");
        Logger.Warning("Overworld camera awake: " + overworldCamera.didAwake);
    }

    public void DrawOverworld(Dictionary<string, Clearing> clearingRegistry){
        // String represent the name of the clearing; it is still in the clearing
        foreach(Clearing clearing in clearingRegistry.Values){
            Vector3 clearingPosition = new Vector3(clearing.position.x, 0, clearing.position.y);
            GameObject clearingObject = Instantiate(overworldClearingPrefab, clearingPosition, Quaternion.identity, overworldSpawnGroup.transform);
            clearingObject.GetComponent<OverworldClearing>().SetClearingName(clearing.clearingName);
            nodeObjects.Add(clearingObject);
        }
        ConnectNodes(clearingRegistry.Values);
        if (overworldCamera  != null && overworldCamera.GetNodeCount() != nodeObjects.Count)
        {
            if (nodeObjects.Count > 0)
            {
                overworldCamera.SetNodeObjects(nodeObjects);
                overworldCamera.CalculateBoundaries();
            }
        }
    }

    public void RedrawLinks(Dictionary<string, Clearing> clearingRegistry){
        foreach(GameObject connection in connections.Values){
            Destroy(connection);
        }
        connections.Clear();
        ConnectNodes(clearingRegistry.Values);
    }
   
    private void ConnectNodes(Dictionary<string, Clearing>.ValueCollection tiles)
    {
        foreach (Clearing tile in tiles)
        {
            // Skip tiles without connections
            if (tile.inputLinks == null || tile.inputLinks.Length == 0)
                continue;

            foreach (Clearing connectedTile in tile.inputLinks)
            {
                // Find the target tile
                if (connectedTile == null) continue;

                // Check if the connection already exists in either order
                string connection = tile.clearingName + "->" + connectedTile.clearingName;
                string reverseConnection = connectedTile.clearingName + "->" + tile.clearingName;
                if(connections.TryGetValue(connection, out GameObject _)){
                    Logger.Info("Connection " + connection + " already exists");
                    continue;
                } else if(connections.TryGetValue(reverseConnection, out GameObject _)){
                    Logger.Info("Connection " + reverseConnection + " already exists");
                    continue;
                }

                // Draw a line between the tiles
                DrawConnectionLine(tile, connectedTile);
            }
        }
    }

    private void DrawConnectionLine(Clearing fromTile, Clearing toTile)
    {
        // Create a line renderer between the tiles
        GameObject connectionObj = Instantiate(edgeVisual, overworldSpawnGroup.transform);
        connections.Add(fromTile.clearingName + "->" + toTile.clearingName, connectionObj);

        LineRenderer lineRenderer = connectionObj.GetComponentInChildren<LineRenderer>();

        lineRenderer.positionCount = 2;
        Vector3 start = new Vector3(fromTile.position.x, 0, fromTile.position.y);
        Vector3 end = new Vector3(toTile.position.x, 0, toTile.position.y);
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);
    }

    public void RedrawArmies(Dictionary<string, Clearing> clearingRegistry, Dictionary<string, Army> armyRegistry)
    {
        GameObject ambushingArmy = ambush.ambushingArmy;
        foreach(GameObject army in displayedArmies){
            if(ambushingArmy != null && army == ambushingArmy) continue;
            
            Destroy(army);
        }
        displayedArmies.Clear();
        if(ambushingArmy != null){
            displayedArmies.Add(ambushingArmy);
        }
        int playerArmyCount = 0;
        int enemyArmyCount = 0;
        foreach(Army army in armyRegistry.Values){
            if(clearingRegistry.TryGetValue(army.currentClearing.clearingName, out Clearing clearing)){
                Logger.Warning($"Ambushing army exists: {ambushingArmy != null}");
                if (ambushingArmy != null && army.regimentName == ambush.ambushingArmyName)
                {
                    continue;
                }
                
                Logger.Info("Painting army " + army.regimentName + " at clearing " + clearing.clearingName + " with " + army.units.Length + " units");
                ShowClearingArmy(clearing.position, army.ownership, army.units.Length, clearing.clearingName);
                
                if(army.ownership == Ownership.Player){
                    playerArmyCount++;
                } else {
                    enemyArmyCount++;
                }
                
                if (ambush.ambushingArmyName != null && army.regimentName == ambush.ambushingArmyName)
                {
                    
                    // Should always be the most recently added army
                    GameObject theAmbusher = displayedArmies[^1];
                    Logger.Error($"Original position is: {originalPosition}");
                    if (originalPosition == Vector3.zero)
                    {
                        StartCoroutine(AmbushAnimation(theAmbusher));
                        continue;
                    }
                    
                    if (theAmbusher == null)
                    {
                        Logger.Error("Ambusher not founds :(");
                        continue;
                    }
                    theAmbusher.transform.position = ambush.hidingPosition.transform.position;
                    theAmbusher.transform.localScale *= shrinkScale;
                    ambush.ambushingArmy = theAmbusher;
                    if (ambushingArmy == null)
                    {
                        // Disable gravity for the cube, so that it stays
                        Rigidbody rb = theAmbusher.GetComponentInChildren<Rigidbody>();
                        if (rb == null)
                        {
                            Logger.Info("No rigidbody found");
                            continue;
                        }
                        rb.isKinematic = true;
                    }
                    else
                    {
                        displayedArmies.Remove(theAmbusher);
                        Destroy(theAmbusher);
                    }
                }
            } else {
                Logger.Error("Army " + army.regimentName + " not found in clearing registry, clearing name: " + army.currentClearing.clearingName);
            }
        }
        if(ambushingArmy == null && (playerArmyCount == 0 || enemyArmyCount == 0)){
            GameEvents.AllArmiesDefeated.Invoke(playerArmyCount == 0 ? Ownership.Player : Ownership.Enemy);
        }
    }

    // Displays a single large cube representing an army at a clearing.
    private void ShowClearingArmy(Vector3 clearingPosition, Ownership ownership, int unitCount, string clearingName)
    {
        // Don't display anything if there are no units
        if (unitCount <= 0) return;

        // Create an empty game object to hold the army representation
        GameObject armyContainer = new GameObject("ArmyContainer_" + clearingPosition.ToString()); // Unique name helps debugging
        armyContainer.transform.position = new Vector3(clearingPosition.x, 0, clearingPosition.y);
        armyContainer.transform.parent = overworldSpawnGroup.transform;
        displayedArmies.Add(armyContainer); // Keep track for cleanup

        // Instantiate the single army representation prefab
        GameObject armyRepresentation = Instantiate(overworldArmyPrefab, armyContainer.transform);

        // Position it slightly above the ground plane (Y=0.5f) within the container
        armyRepresentation.transform.localPosition = new Vector3(0, 0.5f, 0);
        OverworldArmyClickable clickable = armyRepresentation.GetComponent<OverworldArmyClickable>();
        if (!clickable) { Logger.Error("No clickable on army" + armyContainer.name); return;}
        clickable.SetOwner(ownership);
        clickable.clearingName = clearingName;

        // Set a fixed, larger scale for the single cube
        // You might want to adjust this scale value (e.g., 0.6f) based on your visual preference
        const float armyScale = 0.6f;
        armyRepresentation.transform.localScale = new Vector3(armyScale, armyScale, armyScale);
    }

    public IEnumerator EnemyBaseVictory(string clearingName, Ownership ownership = Ownership.Player)
    {
        GameObject armyContainer = null;
        foreach (GameObject armyGO in displayedArmies)
        {
            OverworldArmyClickable armyTmp = armyGO.GetComponentInChildren<OverworldArmyClickable>();
            if (armyTmp == null)
            {
                continue;

            }
            if (armyTmp.clearingName == clearingName)
            {
                armyContainer = armyGO;
            }
        }
        if (armyContainer == null) {
            // To prevent NullReferenceException if armyContainer is null and we don't return early:
            GameEvents.AllArmiesDefeated.Invoke(Ownership.Player); // Or handle as an error
            yield break; // Exit the coroutine
        }

        // Move camera to focus on cube
        // Add camera follow in the update thingy
        // Do the animation
        Camera mainCamera = Camera.main;
        mainCamera.transform.LookAt(armyContainer.transform);
        ParticleSystem cubeParticle = Instantiate(cubeExplosion);
        cubeParticle.transform.position = armyContainer.transform.position;
        
        ParticleSystem.MainModule cubeParamters = cubeParticle.main;
        cubeParamters.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.5f);
        cubeParticle.Play();
        Destroy(armyContainer);
        
        float elapsedTime = 0f;
        float duration = 4f;
        Vector3 cachePos = cubeParticle.transform.position;
        while (elapsedTime < duration)
        {
            mainCamera.transform.LookAt(cachePos);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        GameEvents.AllArmiesDefeated.Invoke(Ownership.Player);
    }

    private IEnumerator BaseVictoryAnimation(GameObject army)
    {
        yield return null;
    }
    public void DeselectAll()
    {
        if (selectedArmyCube != null)
        {
            selectedArmyCube.HideText();
            selectedArmyCube = null;
        }
        individuallySelectedUnits.Clear();
        globalSelectedArmy = null;
    }

    public void HideAllText()
    {
        foreach (GameObject go in displayedArmies)
        {
            go.HideText();
        }
    }
    
    public void PaintEnemyArmyPath(string[] path){
        // Attempt painting connections between the clearings in the path
        // The clearings are in a linked order, so we can just paint the connections between them
        // But they may be saved in the opposite order in connections so both need to be checked
        Logger.Info("Checking for these connections:" + string.Join(", ", connections.Keys));
        for(int i = 0; i < path.Length - 1; i++){
            string connection = path[i] + "->" + path[i + 1];
            string reverseConnection = path[i + 1] + "->" + path[i];
            if(connections.TryGetValue(connection, out GameObject connectionObj)) 
            {
                connectionObj.GetComponent<Renderer>().material.color = Palette.Instance.enemyColor;
            } 
            else if (connections.TryGetValue(reverseConnection, out GameObject connectionObj2))
            {
                connectionObj2.GetComponent<Renderer>().material.color = Palette.Instance.enemyColor;
            }
            else {
                Logger.Error("Connection " + connection + " not found");
            }
        }
    }
    
    public void RemoveEntireOverworld(){
        foreach(GameObject army in displayedArmies){
            Destroy(army);
        }
        displayedArmies.Clear();
        foreach(GameObject connection in connections.Values){
            Destroy(connection);
        }
        connections.Clear();
        foreach(GameObject node in nodeObjects){
            Destroy(node);
        }
        nodeObjects.Clear();
        foreach (Unit u in individuallySelectedUnits)
        {
            Destroy(u);
        }
        individuallySelectedUnits.Clear();
        DisableArmyCamera();
        RemovePlayerPaths(true);
        DeselectAll();
        Destroy(ambush.ambushingArmy);
        ambush.ambushingArmy = null;
    }
    
    public void SpawnArmyInCircle(Army selectedArmy)
    {
        globalSelectedArmy = selectedArmy;
        if (selectedArmy == null || selectedArmy.units == null || selectedArmy.units.Length == 0)
        {
            Logger.Warning("Attempted to spawn an empty or null army.");
            return;
        }
        if (individualUnitPrefab == null)
        {
            Logger.Error("Individual Unit Prefab is not assigned in OverworldState.");
            return;
        }
        if (planePrefab == null)
        {
            Logger.Error("Plane Prefab is not assigned in OverworldState.");
            return;
        }
    
        // Clear previously displayed units if any
        foreach (GameObject displayedUnit in displayedUnits)
        {
            Destroy(displayedUnit);
        }
        displayedUnits.Clear();
        
        // Initialize list to store spawn points for camera framing
        List<Vector3> points = new List<Vector3>();
    
        // --- Circle Parameters ---
        Vector3 center = planePrefab.transform.position; // Use the plane's center
        float angleStep = 180.0f / (selectedArmy.units.Length > 1 ? selectedArmy.units.Length -1 : 1); // Angle between units in degrees for a semi-circle
        float radius = 4.5f; // Adjust radius as needed
        const float minimumDistance = 1.2f; // Minimum desired distance between units

        // Dynamically adjust radius if units are too close (only if more than one unit)
        if (selectedArmy.units.Length > 1)
        {
            float angleStepRadians = angleStep * Mathf.Deg2Rad;
            // Calculate chord length (distance between adjacent points on the circle)
            float distance = 2.0f * radius * Mathf.Sin(angleStepRadians / 2.0f);

            int safetyBreak = 0; // Prevent infinite loops in edge cases
            while (distance < minimumDistance && safetyBreak < 10)
            {
                Logger.Info($"Units too close (Dist: {distance:F2} < {minimumDistance}). Increasing radius from {radius:F2}.");
                radius *= 1.1f; // Increase radius by 10%
                distance = 2.0f * radius * Mathf.Sin(angleStepRadians / 2.0f);
                safetyBreak++;
            }
            if (safetyBreak >= 10)
            {
                Logger.Warning("Could not reach minimum unit distance within reasonable radius increases.");
            }
            Logger.Info($"Final radius set to {radius:F2} for minimum distance {minimumDistance}.");
        }
        float startAngle = 180.0f; // Start angle in degrees (e.g., -90 for bottom start of semi-circle)
        float unitYOffset = 0.2f; // How high above the plane to spawn units
        
        armyCamera.EnsureAllPointsVisible(points);
    
        // --- Spawn Units ---
        for (int i = 0; i < selectedArmy.units.Length; i++)
        {
            // Calculate angle for this unit
            // Handle single unit case to avoid division by zero in angleStep calculation if length is 1
            float currentAngleDeg = startAngle + (selectedArmy.units.Length > 1 ? i * angleStep : 0);
            float currentAngleRad = currentAngleDeg * Mathf.Deg2Rad; // Convert to radians
    
            // Calculate position offset from center
            float xOffset = radius * Mathf.Cos(currentAngleRad);
            float zOffset = radius * Mathf.Sin(currentAngleRad);
    
            // Calculate final world position
            float randomHeight = Random.Range(0.1f, 0.3f);
            Vector3 spawnPosition = new Vector3(center.x + xOffset, center.y + unitYOffset + randomHeight, center.z + zOffset);
            
            // Add the calculated position to the list for camera framing
            points.Add(spawnPosition);
    
            // Instantiate the unit prefab with a random rotation around the Y axis
            float randomYRotation = Random.Range(0f, 360f);
            Quaternion randomRotation = Quaternion.Euler(0f, randomYRotation, 0f);
            GameObject unitGO = Instantiate(individualUnitPrefab, spawnPosition, randomRotation);
            Renderer unitRenderer = unitGO.GetComponent<Renderer>();
            
            if (selectedArmy.ownership == Ownership.Enemy)
            {
                unitRenderer.material.color = Palette.Instance.enemyColor;
            }
            else
            {
                if (individuallySelectedUnits.Select(u => u.characterName).Contains(selectedArmy.units[i].characterName))
                {
                    unitRenderer.material.color = Palette.Instance.selectedColor;
                }
                else
                {
                    unitRenderer.material.color = Palette.Instance.playerColor;
                }
            }

            UnitDataHolder dataHolder = unitGO.GetComponent<UnitDataHolder>();
            dataHolder.unitData = selectedArmy.units[i];

            unitGO.DisplayUnitHealth(armyCamera.cameraComponent);
            if (selectedArmy.ownership == Ownership.Player)
            {
                unitGO.DisplayMovementRemainingForObject(armyCamera.cameraComponent);
            }

            //unitGO.DisplayTextAboveObject(armyCamera.cameraComponent, "Some long ass text just to show that it works because why not");
            displayedUnits.Add(unitGO);
    
            Logger.Info($"Spawned unit {selectedArmy.units[i].characterName} at {spawnPosition}");
            foreach (Unit u in individuallySelectedUnits)
            {
                if (u.characterName == selectedArmy.units[i].characterName)
                {
                    unitGO.GetComponent<Renderer>();
                }
                
            }
        }
    }

    public void EnableArmyCamera()
    {
        if (armyCamera == null)
        {
            OverworldArmyCamera[] armyCameras = FindObjectsByType<OverworldArmyCamera>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (armyCameras.Length > 0)
            {
                armyCamera = armyCameras[0];
            }
            else
            {
                GameObject g = Instantiate(armyCameraPrefab);
                armyCamera = g.GetComponent<OverworldArmyCamera>();
            }
        }

        if (planePrefab == null)
        {
            GameObject[] planeGOs = FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < planeGOs.Length; i++)
            {
                if (planeGOs[i].name == "SpawnSurface")
                {
                    planePrefab = planeGOs[i];
                    break;
                }
            }
        }
        armyCamera.MoveAndEnable(planePrefab.transform.position);
        if (displayedUnits.Count > 0)
        {
            foreach (GameObject unit in displayedUnits)
            {
                unit.DisplayUnitHealth(armyCamera.cameraComponent);
                
            }
        }
    }

    public void DisableArmyCamera()
    {
        armyCamera.DisableCamera();
        // Clear displayed units if any
        foreach (GameObject displayedUnit in displayedUnits)
        {
            Destroy(displayedUnit);
        }
        displayedUnits.Clear();
        globalSelectedArmy = null;
        if (screenTextDisplayManager != null)
        {
            screenTextDisplayManager.UpdateDisplayText(""); // Clear text when disabling army camera
        }
    }

    public void AddToSelectedUnits(Unit unit)
    {
        // Do not select enemy units; you cannot use them anyways
        if (globalSelectedArmy == null) return;
        if (globalSelectedArmy.ownership == Ownership.Enemy) return;
        if (individuallySelectedUnits.Contains(unit))
        {
            individuallySelectedUnits.Remove(unit);
            foreach (GameObject unitGO in displayedUnits)
            {
                // Ensure color correction on update
                if (unitGO.GetComponent<UnitDataHolder>().unitData.characterName == unit.characterName)
                {
                    unitGO.GetComponent<Renderer>().material.color = Palette.Instance.playerColor;
                }
            }
        }
        else
        {
            individuallySelectedUnits.Add(unit);
            foreach (GameObject unitGO in displayedUnits)
            {
                // Ensure color correction on update
                if (unitGO.GetComponent<UnitDataHolder>().unitData.characterName == unit.characterName)
                {
                    unitGO.GetComponent<Renderer>().material.color = Palette.Instance.selectedColor;
                }
            }
        }
    }

    public void RemovePlayerPaths(bool keepButtonVisible = false)
    {
        foreach(GameObject arrow in arrows)
        {
            Destroy(arrow);
        }

        if (!keepButtonVisible) 
            UIGuy.SetBottomRightButtonType(UIGuy.BottomRightButtonType.Invisible, 1);
        arrows.Clear(); 
    }

    public void RemoveTextForCubes()
    {
        foreach (GameObject go in displayedArmies)
        {
            go.GetComponentInChildren<OverworldArmyClickable>().gameObject.HideText();
            
        }
    }

    public bool ArmyIsAmbushingFromClearing(Clearing clear)
    {
        return clear.specialFeature == SpecialFeature.Ambush && ambush.isPreparing;
    }

    // Draw valid paths you can take
    public void DrawPlayerPaths(Clearing clearing, Dictionary<string, Army> armyRegistry)
    {
        // Also check if the globalSelectedArmy exists, as it holds the action points
        if (individuallySelectedUnits.Count == 0 || globalSelectedArmy == null) return;
        if(clearing.specialFeature == SpecialFeature.FallenTree && !fallenTree.isFalling){
            UIGuy.SetBottomRightButtonType(UIGuy.BottomRightButtonType.Overworld_FallenTree, 1);
        }
        if(clearing.specialFeature == SpecialFeature.Ambush && !ambush.isPreparing){
            UIGuy.SetBottomRightButtonType(UIGuy.BottomRightButtonType.Overworld_Ambush, 1);
        }

        if (ArmyIsAmbushingFromClearing(clearing))
        {
            UIGuy.SetBottomRightButtonType(UIGuy.BottomRightButtonType.Overworld_Leave_Ambush, 1);
            return;
        }
        foreach (Clearing clear in clearing.inputLinks)
        {
            Vector2 remote = clear.position;
            Vector2 local = clearing.position;
            Vector2 direction = remote - local;
            
            // Set magnitude of vector to 2
            direction.Normalize();
            direction *= 1.5f;
            Vector2 combined = local + direction;

            Vector3 positionForArrow = new Vector3(combined.x, 0, combined.y);

            Quaternion rotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.y));
            // Uses positive z-direction of object as direction
            GameObject arrow = Instantiate(pointerPrefab, positionForArrow, rotation);

            // Determine arrow color
            Color arrowColor;
            bool hasEnoughStamina = globalSelectedArmy.actionPoints > 0;
            foreach (Unit u in individuallySelectedUnits)
            {
                if (u.movePoints <= 0) hasEnoughStamina = false;
            }
            bool enemyPresent = false;
            bool playerPresent = false;
            foreach (Army army in armyRegistry.Values)
            {
                if (army.currentClearing.clearingName == clear.clearingName)
                {
                    if (army.ownership == Ownership.Enemy)
                    {
                        enemyPresent = true;
                        break;
                    }
                    else if (army.ownership == Ownership.Player)
                    {
                        playerPresent = true;
                    }
                }

            }
            ArrowState arrowState = ArrowState.Ok;

            if (!hasEnoughStamina)
            {
                Color c = new Color(0.5f, 0.5f, 0.5f);
                arrowColor = Color.gray; // Not enough action points (stamina)
                arrowState = ArrowState.NoStamina;
            }
            else if (enemyPresent)
            {
                Color c = new Color(0.6f, 0f, 0f);
                arrowColor = c; // Enemy in target clearing
                arrowState = ArrowState.Enemy;
            }
            else if (playerPresent)
            {
                Color c = new Color(0.2971698f, 0.4450342f, 1f);
                arrowColor = c;
                arrowState = ArrowState.Ally;
            }
            else
            {
                Color c = new Color(0.2f, 0.6f, 0.2f);
                arrowColor = c; // Path is clear and affordable
            }

            // Apply the color to the arrow's material
            Renderer arrowRenderer = arrow.GetComponentInChildren<Renderer>();
            if (arrowRenderer != null)
            {
                arrowRenderer.material.color = arrowColor;
            }
            else
            {
                Logger.Warning($"Arrow prefab is missing a Renderer component.");
            }

            ClickableArrow arrowData = arrow.GetComponentInChildren<ClickableArrow>();
            // I honestly don't know how this would happen
            if (arrowData == null) {throw new Exception("The prefab for arrow is missing its script");}

            // Remove the one that is not necessary
            arrowData.targetClearing = clear.clearingName;
            arrowData.clearing = clear;
            arrowData.state = arrowState;
            
            arrows.Add(arrow);
        }
        DrawNumSelectedUnits(armyRegistry);
    }

    public void VisualizeAmbush(){
        if(globalSelectedArmy == null) return;
        Logger.Info("Global selected army: " + globalSelectedArmy.currentClearing.clearingName);
        GameObject ambushingArmy = null;
        foreach(GameObject army in displayedArmies){
            OverworldArmyClickable armyClickable = army.GetComponentInChildren<OverworldArmyClickable>();
            if(armyClickable == null){
                Logger.Error("Army clickable is missing a OverworldArmyClickable");
                continue;
            }
            if(armyClickable.clearingName == globalSelectedArmy.currentClearing.clearingName){
                Logger.Info("Ambushing army found");
                ambushingArmy = army;
                break;
            }
        }
        if(ambushingArmy == null){
            Logger.Error("Ambushing army not found");
            return;
        }

        else{
            ambush.SetAmbush(ambushingArmy, globalSelectedArmy.regimentName);
            StartCoroutine(AmbushAnimation(ambushingArmy));
            DeselectAll();
            RemovePlayerPaths();
            RemoveTextForCubes();
        }
    }

    private IEnumerator AmbushAnimation(GameObject army)
    {
        // Store original properties
        originalPosition = army.transform.position;
        originalScale = army.transform.localScale;
        Renderer armyRenderer = army.GetComponentInChildren<Renderer>();
        originalColor = armyRenderer.material.color;
        // Turn off rigid body
        army.GetComponentInChildren<Rigidbody>().isKinematic = true;

        // Animation parameters
        float duration = 0.8f;
        float jumpHeight = 1.2f;
        float elapsedTime = 0f;
        
        // Use the ambush hiding position as the final destination
        Vector3 destination = this.ambush.hidingPosition.transform.position;
        
        // Phase 1: Jump up and toward hiding position (first 60% of animation)
        float jumpPhase = duration * 0.6f;
        while (elapsedTime < jumpPhase)
        {
            float t = elapsedTime / jumpPhase;
            
            // Parabolic jump trajectory
            float parabolicOffset = jumpHeight * (4f * t * (1f - t));
            
            // Linear movement toward hiding position
            Vector3 currentPos = Vector3.Lerp(originalPosition, destination, t);
            currentPos.y += parabolicOffset;
            
            army.transform.position = currentPos;
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // Phase 2: Drop down and hide (remaining 40% of animation)
        float hidePhase = duration * 0.4f;
        float hideStartTime = elapsedTime;
        
        while (elapsedTime < duration)
        {
            float t = (elapsedTime - hideStartTime) / hidePhase;
            
            // Move to final hiding position
            Vector3 currentPos = destination;
            currentPos.y = Mathf.Lerp(destination.y, destination.y - 0.1f, t); // Slightly lower to show hiding
            army.transform.position = currentPos;
            
            // Shrink the army to show it's hiding
            Vector3 currentScale = Vector3.Lerp(originalScale, originalScale * shrinkScale, t);
            army.transform.localScale = currentScale;
            
            // Make the army more transparent/darker to show it's hidden
            Color hiddenColor = Color.Lerp(originalColor, originalColor * 0.6f, t);
            hiddenColor.a = Mathf.Lerp(1f, 0.7f, t);
            armyRenderer.material.color = hiddenColor;
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    public void LeaveAmbush(Army ambushingArmy){
        globalSelectedArmy = ambushingArmy;
        StartCoroutine(AmbushAnimationUndo(ambush.ambushingArmy));
        GameEvents.LeaveAmbush.Invoke(ambushingArmy);
        ambush.LeaveAmbush();
    }

    private IEnumerator AmbushAnimationUndo(GameObject army)
    {
        Renderer armyRenderer = army.GetComponentInChildren<Renderer>();
        if (armyRenderer == null)
        {
            Logger.Error("Army renderer not found for undo animation");
            yield break;
        }

        // Get current hidden state as starting point
        Vector3 hiddenPosition = army.transform.position;
        Vector3 hiddenScale = army.transform.localScale;
        Color hiddenColor = armyRenderer.material.color;

        // Animation parameters
        float duration = 0.8f;
        float jumpHeight = 1.0f;
        float elapsedTime = 0f;

        // Phase 1: Rise up from hiding position (first 40% of animation)
        float risePhase = duration * 0.4f;
        while (elapsedTime < risePhase)
        {
            float t = elapsedTime / risePhase;
            
            // Restore scale and color during rise
            Vector3 currentScale = Vector3.Lerp(hiddenScale, originalScale, t);
            army.transform.localScale = currentScale;
            
            // Restore color and transparency
            Color currentColor = Color.Lerp(hiddenColor, originalColor, t);
            armyRenderer.material.color = currentColor;
            
            // Rise up slightly
            Vector3 currentPos = hiddenPosition;
            currentPos.y = Mathf.Lerp(hiddenPosition.y, originalPosition.y, t);
            army.transform.position = currentPos;
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Phase 2: Jump back to original position (remaining 60% of animation)
        float jumpPhase = duration * 0.6f;
        float jumpStartTime = elapsedTime;
        Vector3 jumpStartPosition = army.transform.position;
        
        while (elapsedTime < duration)
        {
            float t = (elapsedTime - jumpStartTime) / jumpPhase;
            
            // Parabolic jump trajectory back to original position
            float parabolicOffset = jumpHeight * (4f * t * (1f - t));
            
            // Linear movement back to original position
            Vector3 currentPos = Vector3.Lerp(jumpStartPosition, originalPosition, t);
            currentPos.y += parabolicOffset;
            
            army.transform.position = currentPos;
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // Ensure final state is exactly the original state
        army.transform.position = originalPosition;
        army.transform.localScale = originalScale;
        armyRenderer.material.color = originalColor;
        
        // Re-enable rigid body physics
        Rigidbody armyRigidbody = army.GetComponentInChildren<Rigidbody>();
        if (armyRigidbody != null)
        {
            armyRigidbody.isKinematic = false;
        }
        
        Logger.Info("Ambush undo animation completed for army: " + army.name);
    }

    // Draw the number of units selected (above the cuboid army)
    public void DrawNumSelectedUnits(Dictionary<String, Army> armyRegistry)
    {
        if (globalSelectedArmy == null) return;
        if (individuallySelectedUnits.Count == 0) return;
        foreach (GameObject armyGO in displayedArmies)
        {
            OverworldArmyClickable armyClickable = armyGO.GetComponent<OverworldArmyClickable>();
            if (armyClickable == null)
            {
                armyClickable = armyGO.GetComponentInChildren<OverworldArmyClickable>();
                if (armyClickable == null)
                {
                    Logger.Error("Army clickable is missing a OverworldArmyClickable");
                    continue;
                }
            }

            String clearingNameFromClickable = armyClickable.clearingName;
            if (globalSelectedArmy != null 
                && globalSelectedArmy.currentClearing != null 
                && clearingNameFromClickable == globalSelectedArmy.currentClearing.clearingName)
            {
                int maxUnits = 0;
                // Get the total number of units on this tile
                foreach (Army army in armyRegistry.Values)
                {
                    if (army.currentClearing.clearingName == clearingNameFromClickable && army.ownership == Ownership.Player)
                    {
                        maxUnits += army.units.Length;
                    }
                }

                string cotainerName = "NumSelectedContainer";
                armyClickable.gameObject.DisplayTextAboveObject(Camera.main, $"{individuallySelectedUnits.Count} / {maxUnits}", containerName: cotainerName);
                if (globalSelectedArmy != null)
                {
                    Logger.Warning($"There are {globalSelectedArmy.units.Length} units in selected army");
                    armyClickable.gameObject.DisplayMovementRemainingForObject(Camera.main, globalSelectedArmy);
                }

                Transform createdContainer = armyClickable.gameObject.transform.Find(cotainerName);

                if (createdContainer == null)
                {
                    Logger.Warning("Created container could not be found");
                    break;
                }

                // Move slightly to the left of the scene
                createdContainer.position += new Vector3(-0.5f, 0f, 0f);

                break;
            }
        }
    }
}
}
