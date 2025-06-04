using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Unity.VisualScripting;
using System.Linq;
using System;
using GetPositionInfo;
using UnityEngine.SceneManagement;
using UINamespace;
using BattlePrep;
using CameraNamespace;
namespace OverworldNamespace {
public class OverworldState : GameState
{
    private readonly OverworldManager manager;
    private Army newArmyOfSelectedUnits = null;
    private Army selectedArmy = null;
    private List<GameObject> displyedUnits = new List<GameObject>();

    public OverworldState(GameMaster gameMaster) : base(gameMaster)
    {
        this.manager = gameMaster.GetComponent<OverworldManager>();
    }

    public override void Enter()
    {
        base.Enter();
        if (!gameMaster.actionQueue.IsEmpty())
        {
            // I just like the wait
            System.Threading.Thread.Sleep(100);
            Action a = gameMaster.actionQueue.Pop();
            a.Invoke();
            // Return to maintain camera
            // If we don't return, the script continues to the next steps,
            // despite exiting this state. And the camera is set _after_ the camera
            // for the battlePrepState for some reason.
            return;
        }
        Logger.Success("Overworld State Entered");
        manager.DrawOverworld(gameMaster.GetDataManager().GetClearingRegistry());
        manager.RedrawArmies(gameMaster.GetDataManager().GetClearingRegistry(), gameMaster.GetDataManager().GetArmyRegistry());
        gameMaster.GetComponent<CameraManager>().SetActiveCamera(CameraVariant.Main);
        CalculateBestEnemyMove();
        UIGuy.SetBottomRightButtonType(UIGuy.BottomRightButtonType.EndTurn, 0);
    }

    public override void Exit()
    {
        base.Exit();
        Logger.Success("Overworld State Exited");
        manager.RemoveEntireOverworld();

        UIGuy.SetBottomRightButtonType(UIGuy.BottomRightButtonType.Invisible, 0);
        newArmyOfSelectedUnits = null;
        selectedArmy = null;
        displyedUnits.Clear();
    }

    // Events and handlers
    protected override void RegisterEvents()
    {
        Subscribe(GameEvents.ClearingSelected, OnClearingSelected);
        Subscribe(GameEvents.BottomRightButtonClicked, OnBottomRightButtonClicked);
        Subscribe(GameEvents.ArmyCubeSelected, OnArmyCubeSelected);
        Subscribe(GameEvents.IndividualUnitSelected, OnIndividualUnitSelected);
        Subscribe(GameEvents.ArrowInvoked, OnArrowInvocation);
        Subscribe(GameEvents.AllArmiesDefeated, OnAllArmiesDefeated);
        Subscribe(GameEvents.LeaveAmbush, OnLeaveAmbush);
        Subscribe(GameEvents.GoToMainMenu, OnGoToMainMenu);
    }

    private void OnLeaveAmbush(Army leavingArmy)
    {
        String leavingClearing = leavingArmy.currentClearing.clearingName;
        foreach (Army otherArmy in gameMaster.GetDataManager().GetArmyRegistry().Values)
        {
            if (otherArmy.currentClearing.clearingName == leavingClearing)
            {
                gameMaster.GetDataManager().JoinArmies(leavingArmy, otherArmy);
                return;
            }
        }
    }
    
    private void OnArrowInvocation(ClickableArrow arrow)
    {
        if (arrow == null) return;
        if (arrow.state == ArrowState.NoStamina) return;
        
        OnClearingSelected(arrow.targetClearing);
    }

    private void OnAllArmiesDefeated(Ownership loser){
        base.Cleanup();
        gameMaster.actionQueue.Clear();
        SceneManager.LoadScene(loser == Ownership.Player ? "DefeatScreen" : "VictoryScreen");
    }

    private void OnGoToMainMenu(EmptyEventArgs args){
        base.Cleanup();
        manager.DeselectAll();
        manager.RemoveEntireOverworld();
        gameMaster.CleanQueue();
        SceneManager.LoadScene("MainMenu");
    }

    private void OnIndividualUnitSelected(Vector3 position)
    {
        GameObject go = PositionExtensions.GetGameObject(position, "ArmyCube");
        if (go == null) return;
        UnitDataHolder unit = go.GetComponent<UnitDataHolder>();
        if (unit == null) return;
        Logger.Warning("Selected unit: " + unit.unitData.characterName);

        Unit addUnit = unit.unitData;
        manager.AddToSelectedUnits(addUnit);
    }

    private void OnStopLookingAtArmy()
    {     
        UINamespace.UIGuy.SetBottomRightButtonType(UINamespace.UIGuy.BottomRightButtonType.EndTurn, 0);
        UINamespace.UIGuy.SetBottomRightButtonType(UINamespace.UIGuy.BottomRightButtonType.Invisible, 1);
        UINamespace.UIGuy.SetBottomRightButtonType(UINamespace.UIGuy.BottomRightButtonType.Invisible, 2);
       
        Clearing clearing = manager.globalSelectedArmy.currentClearing;
        if (manager.ArmyIsAmbushingFromClearing(clearing))
        {
            UIGuy.SetBottomRightButtonType(UINamespace.UIGuy.BottomRightButtonType.Overworld_Leave_Ambush, 1);
            manager.DeselectAll();
            manager.DisableArmyCamera();
            return;
        }
        // Pass the army registry to DrawPlayerPaths
       
        CreateNewArmyFromUnits(manager.globalSelectedArmy, manager.globalSelectedArmy.currentClearing, manager.individuallySelectedUnits);
        manager.DisableArmyCamera();
        
        manager.globalSelectedArmy = newArmyOfSelectedUnits;
        if (newArmyOfSelectedUnits == null) return;
        foreach (Unit u in newArmyOfSelectedUnits.units)
        {
            manager.AddToSelectedUnits(u);
        }
        manager.DrawPlayerPaths(clearing, gameMaster.GetDataManager().GetArmyRegistry()); 
     }

    private Army GetArmyFromClearing(string clearingName)
    {
        Army army = gameMaster.GetDataManager().GetArmyInClearing(clearingName);
        return army;
    }
    
    private void OnArmyCubeSelected(Vector3 position)
    {
        GameObject army =  PositionExtensions.GetGameObject(position, "ArmyCube");

        // Return if there are no army here for some reason. But how did you click it then? Who knows
        if (!army)
        {
            Logger.Warning("No army found. How did you get here?");
            return;
        }
        manager.RemovePlayerPaths();
        newArmyOfSelectedUnits = null;
        selectedArmy = null;
        manager.RemoveTextForCubes();
        Logger.Success("Army cube selected: " + army.name);
        string clearingName = army.GetComponent<OverworldArmyClickable>().clearingName;
        if (string.IsNullOrEmpty(clearingName)) return;
        selectedArmy = GetArmyFromClearing(army.GetComponent<OverworldArmyClickable>().clearingName);
        if (manager.globalSelectedArmy != null && manager.globalSelectedArmy.regimentName != selectedArmy.regimentName)
        {
            manager.DeselectAll();
        }
        manager.globalSelectedArmy = selectedArmy;
        manager.EnableArmyCamera();
        manager.SpawnArmyInCircle(selectedArmy);
        UINamespace.UIGuy.SetBottomRightButtonType(UINamespace.UIGuy.BottomRightButtonType.ExitUnitSelection, 0);
        if (selectedArmy.ownership == Ownership.Player)
        {
            UINamespace.UIGuy.SetBottomRightButtonType(UINamespace.UIGuy.BottomRightButtonType.Select_All_Units, 1);
            UINamespace.UIGuy.SetBottomRightButtonType(UINamespace.UIGuy.BottomRightButtonType.Deselect_All_Units, 2);
        }
    }
    
    private void OnClearingSelected(string clearingName)
    {
        Logger.Success("Clearing Selected: " + clearingName);
        // Check if there are units in this clearing

        Army army = gameMaster.GetDataManager().GetArmyInClearing(clearingName);
        Clearing clearing = gameMaster.GetDataManager().GetClearing(clearingName);
        manager.RemovePlayerPaths();
        manager.DeselectAll();
        UIGuy.SetBottomRightButtonType(UIGuy.BottomRightButtonType.EndTurn, 0);


        if (army == null || army.units.Length == 0) HandleNoUnitsInClearing(clearing);
        else if (army.units.Length > 0) HandleUnitsInClearing(army, clearing, army.ownership);
    }

    // === Clearing selection === //
    private void HandleNoUnitsInClearing(Clearing clearing)
    {
        if (newArmyOfSelectedUnits == null) Logger.Info("This is:" + clearing.clearingName + (clearing.specialFeature != SpecialFeature.None ? " with special feature: " + clearing.specialFeature : "No special features"));
        else
        {
            gameMaster.GetDataManager().SplitArmy(newArmyOfSelectedUnits, clearing);
            manager.DeselectAll();
            newArmyOfSelectedUnits = null;
            
            manager.RedrawArmies(gameMaster.GetDataManager().GetClearingRegistry(), gameMaster.GetDataManager().GetArmyRegistry());
        }
    }

    private void HandleUnitsInClearing(Army army, Clearing clearing, Ownership ownership)
    {
        if (ownership == Ownership.Player)
        {
            if (newArmyOfSelectedUnits == null)
            {
                if (manager.ArmyIsAmbushingFromClearing(clearing))
                {
                    UIGuy.SetBottomRightButtonType(UIGuy.BottomRightButtonType.Overworld_Leave_Ambush, 1);
                    return;
                }
                CreateNewArmy(army, clearing);
                manager.DeselectAll();
                manager.globalSelectedArmy = newArmyOfSelectedUnits;
                foreach (Unit u in newArmyOfSelectedUnits.units)
                {
                    manager.AddToSelectedUnits(u);
                }
                manager.DrawPlayerPaths(clearing, gameMaster.GetDataManager().GetArmyRegistry());
                
            }
            else if (newArmyOfSelectedUnits.currentClearing != clearing)
            {
                manager.DeselectAll();
                gameMaster.GetDataManager().JoinArmies(newArmyOfSelectedUnits, army);
                newArmyOfSelectedUnits = null;
                manager.RedrawArmies(gameMaster.GetDataManager().GetClearingRegistry(), gameMaster.GetDataManager().GetArmyRegistry());
            }
            else
            {
                manager.DeselectAll();
                if(clearing.specialFeature == SpecialFeature.Ambush && manager.ambush.isPreparing){
                    UIGuy.SetBottomRightButtonType(UIGuy.BottomRightButtonType.Overworld_Leave_Ambush, 1);
                    return;
                } 

                AppendAUnitFromArmy(army);
                manager.globalSelectedArmy = newArmyOfSelectedUnits;
                foreach (Unit u in newArmyOfSelectedUnits.units)
                {
                    manager.AddToSelectedUnits(u);
                }
                manager.DrawPlayerPaths(clearing, gameMaster.GetDataManager().GetArmyRegistry());

            }
        }
        else
        {
            if (newArmyOfSelectedUnits == null)
            {
                Logger.Info("Enemy units in clearing: " + army.units.Length + "Clearing: " + clearing.clearingName);
                manager.DeselectAll();
                manager.RemoveTextForCubes();
            }
            else 
            {
                if(gameMaster.dataManager.GetClearingRegistry()[newArmyOfSelectedUnits.currentClearing.clearingName].inputLinks.Any(link => link.clearingName == clearing.clearingName))
                {
                    gameMaster.ChangeState(new BattlePrepState(gameMaster, newArmyOfSelectedUnits, army,
                        SpawnPointType.Attacking));
                }
                newArmyOfSelectedUnits = null;
                manager.DeselectAll();
                manager.RemoveTextForCubes();
            }
        }
    }

    private void CreateNewArmy(Army army, Clearing clearing)
    {
        Logger.Info("Creating new army");
        newArmyOfSelectedUnits = ScriptableObject.CreateInstance<ArmyData>();
        newArmyOfSelectedUnits.regimentName = army.regimentName;
        newArmyOfSelectedUnits.units = new Unit[] { army.units[0] };
        newArmyOfSelectedUnits.currentClearing = clearing;
        newArmyOfSelectedUnits.ownership = army.ownership;
        newArmyOfSelectedUnits.actionPoints = army.actionPoints;
    }

    private void CreateNewArmyFromUnits(Army army, Clearing clearing, List<Unit> units)
    {
        if (units == null || units.Count == 0) return;
        newArmyOfSelectedUnits = ScriptableObject.CreateInstance<ArmyData>();
        newArmyOfSelectedUnits.regimentName = army.regimentName;
        newArmyOfSelectedUnits.units = units.ToArray();
        newArmyOfSelectedUnits.currentClearing = clearing;
        newArmyOfSelectedUnits.ownership = army.ownership;
        newArmyOfSelectedUnits.actionPoints = army.actionPoints;
        manager.DeselectAll();
    }

    private void AppendAUnitFromArmy(Army army)
    {
        Logger.Info("Appending a unit from army");
        Unit newUnit = null;
        foreach (Unit u in army.units)
        {
            if (!newArmyOfSelectedUnits.units.Contains(u))
            {
                newUnit = u;
                break;
            }
        }
        if (newUnit == null) return;
        newArmyOfSelectedUnits.units = newArmyOfSelectedUnits.units.Append(newUnit).ToArray();
        Logger.Success("New army includes these units: " + string.Join(", ", newArmyOfSelectedUnits.units.Select(u => u.characterName)));
        Logger.Success("Old army includes these units: " + string.Join(", ", army.units.Select(u => u.characterName)));
    }

    // === Clearing selection === //
    // === Turn switching === //
    private void HandleTurnEnded(Ownership ownership){
        Logger.Success("Turn ended for " + ownership);
        manager.DeselectAll();
        manager.RemoveTextForCubes();
        manager.RemovePlayerPaths(true);
        selectedArmy = null;
        newArmyOfSelectedUnits = null;
        
        if(ownership == Ownership.Enemy){
            gameMaster.GetDataManager().ResetAllArmyActionPoints(Ownership.Player);
        } else {
            // Calculate best enemy move
            EnemyMove[] enemyMoves = CalculateBestEnemyMove();
            foreach(EnemyMove enemyMove in enemyMoves){
                PerformEnemyMove(enemyMove);
            }
            HandleTurnEnded(Ownership.Enemy);
        }

    }


    class EnemyMove{
        public string[] path;
        public string regimentName;
    }

    private EnemyMove[] CalculateBestEnemyMove(){
        DataManager dataManager = gameMaster.GetDataManager();
        Army[] enemyArmies = dataManager.GetAllEnemyArmies();
        EnemyMove[] enemyMoves = new EnemyMove[enemyArmies.Length];

        for(int i = 0; i < enemyArmies.Length; i++){
            Clearing destination = dataManager.GetPlayerBaseClearing();
            Clearing currentClearing = enemyArmies[i].currentClearing;
            Clearing[] clearings = dataManager.GetClearingRegistry().Values.ToArray();

            Graph graph = new Graph(clearings.Length);

            for(int j = 0; j < clearings.Length; j++){
                graph.AddVertexData(j, clearings[j].clearingName);
            }

            for(int j = 0; j < clearings.Length; j++){
                for(int k = 0; k < clearings[j].inputLinks.Length; k++){
                    int indexOfLink = Array.IndexOf(graph.vertexData, clearings[j].inputLinks[k].clearingName);
                    graph.AddEdge(j, indexOfLink, clearings[j].inputLinks[k].cost);
                }
            }

            (int distance, string[] path) = graph.Dijkstra(destination.clearingName, currentClearing.clearingName);
            string[] reversedPath = path.Reverse().ToArray();
            Logger.Success("Distance: " + distance);
            Logger.Success("Path: " + string.Join("->", reversedPath));
            manager.PaintEnemyArmyPath(reversedPath);
            enemyMoves[i] = new EnemyMove{path = reversedPath, regimentName = enemyArmies[i].regimentName};
        }

        return enemyMoves;
    }
    
    private void PerformEnemyMove(EnemyMove enemyMove){
        DataManager dataManager = gameMaster.GetDataManager();
        Logger.Warning($"Length of path is {enemyMove.path.Length}");
        Army destinationArmy = dataManager.GetArmyInClearing(enemyMove.path[1], Ownership.Player);
        Army otherEnemyArmy = dataManager.GetArmyInClearing(enemyMove.path[1], Ownership.Enemy);
        if(enemyMove.path[1] == manager.fallenTree.clearingAfterFallenTree && manager.fallenTree.isPreparing){
            manager.fallenTree.Fall();
            // Only half the army will get to the destination
            Army halfArmy = ScriptableObject.CreateInstance<ArmyData>();
            Army currentArmy = dataManager.GetArmy(enemyMove.regimentName);
            halfArmy.regimentName = enemyMove.regimentName;
            halfArmy.units = currentArmy.units.Take(Mathf.CeilToInt(currentArmy.units.Length / 2f)).ToArray();
            halfArmy.currentClearing = dataManager.GetClearing(enemyMove.path[0]);
            halfArmy.ownership = currentArmy.ownership;
            halfArmy.actionPoints = currentArmy.actionPoints;

            dataManager.SplitArmy(halfArmy, dataManager.GetClearing(enemyMove.path[1])); // This will work because the regiment name is the same
            manager.RedrawArmies(dataManager.GetClearingRegistry(), dataManager.GetArmyRegistry());

            // The link between the two clearing is destroyed
            dataManager.DestroyLink(enemyMove.path[0], enemyMove.path[1]);

            if(destinationArmy == null || destinationArmy.units.Length == 0 || destinationArmy.ownership == Ownership.Enemy){
                return;
            } else {
                // Start battle
                Action toQueue = () => gameMaster.ChangeState(new BattlePrepState(gameMaster, destinationArmy, dataManager.GetArmyInClearing(enemyMove.path[1], Ownership.Enemy), SpawnPointType.Defending));
                gameMaster.actionQueue.Push(toQueue);
                toQueue = () =>
                {
                    Army enemy = gameMaster.dataManager.GetArmyInClearing(enemyMove.path[1], Ownership.Enemy);
                    Army allyArmy = gameMaster.dataManager.GetArmyInClearing(enemyMove.path[1], Ownership.Player);
                    if (allyArmy == null || enemy == null)
                    {
                        gameMaster.ChangeState(new OverworldState(gameMaster));
                    }
                    else
                    {
                        gameMaster.ChangeState(new BattlePrepState(gameMaster, destinationArmy,
                            dataManager.GetArmyInClearing(enemyMove.path[1], Ownership.Enemy),
                            SpawnPointType.Defending));
                    }
                };
                gameMaster.actionQueue.Push(toQueue);
            }
        } else if(destinationArmy == null || destinationArmy.units.Length == 0 || destinationArmy.ownership == Ownership.Enemy){
            if (otherEnemyArmy != null)
            {
                dataManager.JoinArmies(dataManager.GetArmy(enemyMove.regimentName), otherEnemyArmy);
            }
            else
            {
                dataManager.SplitArmy(dataManager.GetArmy(enemyMove.regimentName),
                    dataManager.GetClearing(enemyMove.path[1])); // 0 is current clearing, 1 is next clearing
            }

            manager.RedrawArmies(dataManager.GetClearingRegistry(), dataManager.GetArmyRegistry());
            Logger.Success("Path: " + string.Join("<-", enemyMove.path.Reverse()));
            if (enemyMove.path[1] == "P_Spawn")
            {
                manager.StartCoroutine(manager.EnemyBaseVictory("P_Spawn"));
            }
        } else {
            // Start battle
            
            // Check if the destination clearing is an ambush property
            Clearing destinationClearing = dataManager.GetClearing(enemyMove.path[1]);
            SpawnPointType spawnPointType = SpawnPointType.Defending;
            if(destinationClearing.specialFeature == SpecialFeature.Ambush && manager.ambush.isPreparing){
                spawnPointType = SpawnPointType.Ambushing;
            }
            Action toQueue = () => gameMaster.ChangeState(new BattlePrepState(gameMaster, destinationArmy, dataManager.GetArmy(enemyMove.regimentName), spawnPointType));
            gameMaster.actionQueue.Push(toQueue);
        }
    }

    private void OnSelectAll()
    {
        if (manager.globalSelectedArmy == null)
        {
            return;
        }

        foreach (Unit unit in manager.globalSelectedArmy.units)
        {
            if (!manager.individuallySelectedUnits.Contains(unit))
            {
                manager.AddToSelectedUnits(unit);
            } 
        }
    }

    private void OnDeselctAll()
    {
        List<Unit> selectedUnits = new List<Unit>(manager.individuallySelectedUnits);
        foreach (Unit unit in selectedUnits)
        {
            // Add to selected updates colors as well, so we wish to call this function
            manager.AddToSelectedUnits(unit);
        }
    }

    // === Turn switching === //
    // === Bottom right button === //
    private void OnBottomRightButtonClicked(UIGuy.BottomRightButtonType buttonType){
        Logger.Success("Bottom right button clicked");
        if(buttonType == UIGuy.BottomRightButtonType.EndTurn){
            HandleTurnEnded(Ownership.Player);
            if (!gameMaster.actionQueue.IsEmpty())
            {
                Action first = gameMaster.actionQueue.Pop();
                first.Invoke();
            }
        } else if(buttonType == UIGuy.BottomRightButtonType.ExitUnitSelection){
            OnStopLookingAtArmy();
        } else if(buttonType == UIGuy.BottomRightButtonType.Overworld_Ambush){
            // Set ambush
            manager.VisualizeAmbush();
            newArmyOfSelectedUnits = null;
            selectedArmy = null;
        } else if(buttonType == UIGuy.BottomRightButtonType.Overworld_Leave_Ambush){
            // Leave ambush
            Army ambushingArmy = gameMaster.GetDataManager().GetArmy(manager.ambush.ambushingArmyName);
            newArmyOfSelectedUnits = ambushingArmy;
            selectedArmy = ambushingArmy;
            foreach (Unit u in selectedArmy.units)
            {
                manager.AddToSelectedUnits(u);
            }
            manager.LeaveAmbush(ambushingArmy);
            manager.globalSelectedArmy = selectedArmy;
            manager.DrawPlayerPaths(selectedArmy.currentClearing, gameMaster.GetDataManager().GetArmyRegistry());
            UIGuy.SetBottomRightButtonType(UIGuy.BottomRightButtonType.Overworld_Ambush, 1);
            
        } else if(buttonType == UIGuy.BottomRightButtonType.Overworld_FallenTree){
            // Fell tree
            manager.fallenTree.PrepareToFall();
            UIGuy.SetBottomRightButtonType(UIGuy.BottomRightButtonType.Invisible, 1);
            
        } else if (buttonType == UIGuy.BottomRightButtonType.Select_All_Units) {
            // Select all units from the currently displayed army
            OnSelectAll();

        } else if (buttonType == UIGuy.BottomRightButtonType.Deselect_All_Units) {
            // Deselect all units from the currently displayed army
            OnDeselctAll();

        }
    }
}
}
