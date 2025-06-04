using System.Collections.Generic;
using UnityEngine;
using GetPositionInfo;
using static BattleGrid;
using System.Linq;
using System.Threading.Tasks;
using OverworldNamespace;
using UINamespace;
using System.Diagnostics;
using UnityEngine.SceneManagement;

public class BattleState: GameState
{
    private readonly Dictionary<string, Unit> playerUnits;
    private readonly Dictionary<string, Unit> playerUnitsInitial;
    private readonly Dictionary<string, Unit> enemyUnits;
    private readonly Dictionary<string, Unit> enemyUnitsInitial;
    private readonly BattleGrid grid;
    private readonly BattlePrepManager battlePrepManager;
    private Unit selectedUnit;
    private Vector3 selectedUnitPosition;
    private readonly CommandManager commandManager;
    private Ownership activeTurn;
    private string[] exhaustedUnits;
    private readonly AITacticalSystem aiSystem;
    private List<AIAction> actions;
    private readonly SpawnPointType engagementType;
    private readonly string[] regimentNames;
    private readonly GameObject overworldGO;
    
   
    
    public BattleState(GameMaster gameMaster, Dictionary<string, Unit> playerUnits, Dictionary<string, Unit> enemyUnits, string[] regimentNames, SpawnPointType engagementType, GameObject overworldGO) : base(gameMaster)
    {
        this.grid = gameMaster.GetComponent<BattleGrid>();
        this.battlePrepManager = gameMaster.GetComponent<BattlePrepManager>();
        // Filter out player and enemy units based on which are in battlePrepManager.spawnedUnits
        this.playerUnits = playerUnits.Where(unit => battlePrepManager.spawnedUnits.ContainsKey(unit.Key)).ToDictionary(unit => unit.Key, unit => unit.Value);
        this.enemyUnits = enemyUnits.Where(unit => battlePrepManager.spawnedUnits.ContainsKey(unit.Key)).ToDictionary(unit => unit.Key, unit => unit.Value);
        this.playerUnitsInitial = playerUnits;
        this.enemyUnitsInitial = enemyUnits;
        this.commandManager = new CommandManager();
        this.activeTurn = Ownership.Player;
        this.exhaustedUnits = new string[0];
        this.aiSystem = new AITacticalSystem(grid, enemyUnits, playerUnits);
        this.engagementType = engagementType;
        this.regimentNames = regimentNames;
        this.overworldGO = overworldGO;
    }

    public override void Enter(){
        base.Enter();
        selectedUnit = null;
        // Compute enemy unit actions
        CalculateEnemyActions();
        Logger.Success("Battle State Entered");
    }

    public override void Exit(){
        base.Exit();
        grid.DestroyGrid();
        Logger.Success("Battle State Exited");
        // Destroy all units from all states
        battlePrepManager.DestroyAllUnitGameObjects();
        overworldGO.SetActive(true);
    }

    protected override void RegisterEvents(){
        Subscribe(GameEvents.DeployedUnitClicked, HandleClick);
        Subscribe(GameEvents.TileClicked, HandleClick);
        Subscribe(GameEvents.BottomRightButtonClicked, HandleBottomRightButtonClick);
        Subscribe(GameEvents.GoToMainMenu, OnGoToMainMenu);
    }

    // Moving should check if the cost of the movement range is less than or equal to the action points of the unit
    // If there are no units on the way

    enum SelectedUnitVariant{
        None,
        Ally,
        Enemy
    }

    private void HandleClick(Vector3 position){
        // Guard clause: Don't handle events if this state is no longer active
        if (gameMaster.CurrentState != this) return;
        
        GameObject unitClicked = PositionExtensions.GetGameObject(position, "DeployedUnit");
        DeployedUnit unitClickedDeployedUnit = unitClicked?.GetComponent<DeployedUnit>();
        string unitClickedName = unitClickedDeployedUnit?.GetDeployedUnitName();
        UIGuy.SetBottomRightButtonType(UIGuy.BottomRightButtonType.Invisible, 0);
        // Rewrite start
         SelectedUnitVariant selectedUnitVariant = EvaluateSelectedUnitVariant(selectedUnit);
        if(unitClickedName == null){
            if(selectedUnit == null){
                ClearSelectedUnit();
            }
            else {
                if(selectedUnitVariant == SelectedUnitVariant.Ally){
                    PerformMove(selectedUnit, position);
                    ClearSelectedUnit();
                    CheckTurnStatus();
                }
                ClearSelectedUnit();
            }
        } else{
            // You have clicked on a unit
            UnitInfo unitInfo = GetUnitInfo(unitClickedName);
            if(unitInfo == null) return;

            // Clicked on a previously selected unit
            if(selectedUnit != null && selectedUnit.characterName == unitInfo.unit.characterName){
                ClearSelectedUnit();
                return;
            }
            // You have clicked on an ally unit
            if(unitInfo.ownership == Ownership.Player){
                ClearSelectedUnit();
                if(unitClickedDeployedUnit.Exhausted) return;
                SetSelectedUnit(unitInfo.unit, position);
            }
            // You have clicked on an enemy unit
            else {
                switch(selectedUnitVariant){
                    case SelectedUnitVariant.Ally:
                        GameObject selectedUnitGameObject = battlePrepManager.spawnedUnits[selectedUnit.characterName];
                        if(selectedUnitGameObject.GetComponent<DeployedUnit>().Exhausted) return;

                        PerformAttack(unitInfo.unit.characterName, position);
                        ClearSelectedUnit();
                        CheckTurnStatus();
                        break;
                    case SelectedUnitVariant.Enemy:
                    case SelectedUnitVariant.None:
                        ClearSelectedUnit();
                        SetSelectedUnit(unitInfo.unit, position);
                        break;
                }
            }
        }
    }

    private SelectedUnitVariant EvaluateSelectedUnitVariant(Unit unit){
        if(unit == null) return SelectedUnitVariant.None;
        UnitInfo unitInfo = GetUnitInfo(unit.characterName);
        if(unitInfo == null) return SelectedUnitVariant.None;
        if(unitInfo.ownership == Ownership.Player) return SelectedUnitVariant.Ally;
        return SelectedUnitVariant.Enemy;
    }

    private void SetSelectedUnit(Unit unit, Vector3 position){
        selectedUnit = unit;
        selectedUnitPosition = grid.GetClosestCell(position);
        grid.HighlightMovement(selectedUnitPosition, unit.actionPoints);
        UIGuy.SetBottomRightButtonType(UIGuy.BottomRightButtonType.SkipUnitTurn, 0);
    }

    private void ClearSelectedUnit(){
    // Guard clause: Don't handle events if this state is no longer active
        if (gameMaster.CurrentState != this) return;
        grid.UnHighlightAllTiles();
        selectedUnit = null;
        selectedUnitPosition = Vector3.zero;

        UIGuy.SetBottomRightButtonType(UIGuy.BottomRightButtonType.Invisible, 0);
    }

    private void HandleBottomRightButtonClick(UIGuy.BottomRightButtonType buttonType){
        // Guard clause: Don't handle events if this state is no longer active
        if (gameMaster.CurrentState != this) return;
        
        Logger.Info("Bottom right button clicked: " + buttonType);
        if(buttonType == UIGuy.BottomRightButtonType.SkipUnitTurn){
            GameObject unitPosition = PositionExtensions.GetUnitGameObject(selectedUnit.characterName);
            DeployedUnit u = unitPosition.GetComponent<DeployedUnit>();
            if (u.exhausted) return;
            u.Exhaust();
            exhaustedUnits = exhaustedUnits.Append(selectedUnit.characterName).ToArray();

            UIGuy.SetBottomRightButtonType(UIGuy.BottomRightButtonType.Invisible, 0);
            selectedUnit = null;
            grid.UnHighlightAllTiles();
        }

        CheckTurnStatus();
    }

    // Create a class to hold both unit and ownership information
    private class UnitInfo
    {
        public Ownership ownership;
        public Unit unit;

        public UnitInfo(Ownership ownership, Unit unit)
        {
            this.ownership = ownership;
            this.unit = unit;
        }
    }

    private UnitInfo GetUnitInfo(string unitName){
        if(playerUnits.ContainsKey(unitName)){
            return new UnitInfo(Ownership.Player, playerUnits[unitName]);
        }
        else if(enemyUnits.ContainsKey(unitName)){
            return new UnitInfo(Ownership.Enemy, enemyUnits[unitName]);
        }

        Logger.Error("Unit info is null");
        return null;
    }

    private void PerformAttack(string unitClickedName, Vector3 position){
        Logger.Info("Performing attack");

        UnitInfo defenderInfo = GetUnitInfo(unitClickedName);
        UnitInfo attackerInfo = GetUnitInfo(selectedUnit.characterName);
        if(defenderInfo == null || attackerInfo == null || defenderInfo.ownership == attackerInfo.ownership) return;
        TileRangeInfo tileRangeInfo = grid.CollectInformationAboutPositionRange(selectedUnitPosition, position);

        var attackCommand = new AttackCommand(attackerInfo.unit, tileRangeInfo, position);
        if(attackCommand.CanExecute()){
            string result = attackCommand.Execute();
            IsUnitDead(result);
            exhaustedUnits = exhaustedUnits.Append(selectedUnit.characterName).ToArray();
        }
    }

    private void PerformMove(Unit unit,Vector3 destinationPosition){
        Logger.Info("Performing move");

        TileRangeInfo tileRangeInfo = grid.CollectInformationAboutPositionRange(selectedUnitPosition, destinationPosition);
        var moveCommand = new MoveCommand(unit, tileRangeInfo, destinationPosition);
        if(moveCommand.CanExecute()){
            commandManager.ExecuteCommand(moveCommand);
            exhaustedUnits = exhaustedUnits.Append(unit.characterName).ToArray();
        }
    }

    private async void CheckTurnStatus(){
        if(playerUnits.Count == 0 || enemyUnits.Count == 0) return;
        switch(activeTurn){
            case Ownership.Player:
                if(exhaustedUnits.Length < playerUnits.Count) return;
                activeTurn = Ownership.Enemy;
                exhaustedUnits = new string[0];
                foreach(var action in actions){
                    grid.HideAttackHighlight(action.TargetPosition);
                }
                await PerformEnemyTurn();
                if(playerUnits.Count == 0 || enemyUnits.Count == 0) return;
                CalculateEnemyActions();  
                CheckTurnStatus();
                break;
            case Ownership.Enemy:
                if(exhaustedUnits.Length < enemyUnits.Count) return;
                activeTurn = Ownership.Player;
                exhaustedUnits = new string[0];
                // Unexhaust all player units
                GameObject[] playerUnitsGameObjects = battlePrepManager.spawnedUnits.Values.Where(unit => playerUnits.ContainsKey(unit.GetComponent<DeployedUnit>().GetDeployedUnitName())).ToArray();
                foreach(var unit in playerUnitsGameObjects){
                    unit.GetComponent<DeployedUnit>().Restore();
                }

                break;
        }
    }

    private void CalculateEnemyActions(){
        actions = aiSystem.CalculateActions();
        foreach(var action in actions){
            grid.ShowAttackHighlight(action.TargetPosition, action.ActionType, action.StartPosition);
        }
        
    }

    private async Task<bool> PerformEnemyTurn()
    {
        await Task.Delay(500);
        // Unexhaust all enemy units
        GameObject[] enemyUnitsGameObjects = battlePrepManager.spawnedUnits.Values.Where(unit => enemyUnits.ContainsKey(unit.GetComponent<DeployedUnit>().GetDeployedUnitName())).ToArray();
        foreach(var unit in enemyUnitsGameObjects){
            unit.GetComponent<DeployedUnit>().Restore();
        }
        
        foreach (var action in actions)
        {
            EnemyActionResult actionResult = EnemyActionResult.Failed;
            TileRangeInfo tileRangeInfo = grid.CollectInformationAboutPositionRange(action.StartPosition, action.TargetPosition);
            
            if (action.ActionType == AIActionType.Attack) {
                var attackCommand = new AttackCommand(action.ActingUnit, tileRangeInfo, action.TargetPosition);
                if (attackCommand.CanExecute()) {
                    string result = attackCommand.Execute();
                    IsUnitDead(result);
                    if(playerUnits.Count == 0 || enemyUnits.Count == 0) return true;
                    actionResult = EnemyActionResult.Success;
                }
            } else { // Move
                var moveCommand = new MoveCommand(action.ActingUnit, tileRangeInfo, action.TargetPosition);
                if (moveCommand.CanExecute()) {
                    commandManager.ExecuteCommand(moveCommand);
                    actionResult = EnemyActionResult.Success;
                }
            }
            
            if(actionResult == EnemyActionResult.Failed){
                // If primary action failed, try alternative attack
                actionResult = CheckForAlternativeAttack(action.ActingUnit, tileRangeInfo, action.StartPosition);
                if(actionResult == EnemyActionResult.Killed) return true;
            }
            await Task.Delay(500);
        }
        foreach(var unit in enemyUnitsGameObjects){
            unit.GetComponent<DeployedUnit>().Exhaust();
        }
        exhaustedUnits = enemyUnits.Keys.ToArray();
        return true;
    }

    private void IsUnitDead(string unitName){
        if(unitName == null) return;
        if(playerUnits.ContainsKey(unitName))
        {
            playerUnits.Remove(unitName);
            aiSystem.SetAIUnits(playerUnits, AITacticalSystemType.Player);
            Logger.Warning("Player unit dead: " + unitName);
        }
        if(enemyUnits.ContainsKey(unitName)){
            enemyUnits.Remove(unitName);
            aiSystem.SetAIUnits(enemyUnits, AITacticalSystemType.Enemy);
            foreach(var action in actions){
                if(action.ActingUnit.characterName == unitName){
                    grid.HideAttackHighlight(action.TargetPosition);
                }
                Logger.Warning("Enemy unit dead: " + unitName);
            }
        }

        battlePrepManager.StartCoroutine(battlePrepManager.KillCube(unitName));
        battlePrepManager.DestroyUnitGameObject(unitName);
        CheckBattleStatus();
    }
    
    private void CheckBattleStatus(){
        if(playerUnits.Count == 0){
            Logger.Success("Enemy wins");
            DataManager dataManager = gameMaster.GetDataManager();

            Clearing playerArmyClearing = dataManager.GetArmy(regimentNames[0]).currentClearing;
            SufferCasualties(regimentNames[1], regimentNames[0], Ownership.Enemy);
            Army enemyArmy = dataManager.GetArmy(regimentNames[1]);
            Army isPlayerArmyDead = dataManager.GetArmy(regimentNames[0]);
            if(engagementType == SpawnPointType.Defending && !isPlayerArmyDead){
                dataManager.SplitArmy(enemyArmy, playerArmyClearing);
            }
            gameMaster.ChangeState(new OverworldState(gameMaster));
        }
        if(enemyUnits.Count == 0){
            Logger.Success("Player wins");
            DataManager dataManager = gameMaster.GetDataManager();

            Clearing enemyArmyClearing = dataManager.GetArmy(regimentNames[1]).currentClearing;
            SufferCasualties(regimentNames[0], regimentNames[1], Ownership.Player);
            Army playerArmy = dataManager.GetArmy(regimentNames[0]);
            // Enemy army must be completely wiped out to move in
            Army isEnemyArmyDead = dataManager.GetArmy(regimentNames[1]);
            if(engagementType == SpawnPointType.Attacking && !isEnemyArmyDead){
                dataManager.SplitArmy(playerArmy, enemyArmyClearing);
            }
            gameMaster.ChangeState(new OverworldState(gameMaster));
        }
    }

    private void SufferCasualties(string winningRegimentName, string losingRegimentName, Ownership ownership){
        DataManager dataManager = gameMaster.GetDataManager();
        Dictionary<string, Unit> survivingUnits = ownership == Ownership.Enemy ? enemyUnits : playerUnits;
        Dictionary<string, Unit> allUnits = ownership == Ownership.Enemy ? enemyUnitsInitial : playerUnitsInitial;
        Dictionary<string, Unit> losingArmyUnits = ownership == Ownership.Enemy ? playerUnitsInitial : enemyUnitsInitial;

        foreach(var unit in losingArmyUnits.Values){
            dataManager.RemoveUnitFromRegistry(unit, losingRegimentName);
        }
        // Update own units

        foreach(var unit in allUnits.Values){
            if(!survivingUnits.ContainsKey(unit.characterName)){
                dataManager.RemoveUnitFromRegistry(unit, winningRegimentName);
            } else {
                dataManager.UpdateUnitRegistry(unit, winningRegimentName);
            }   
        }
    }

    private enum EnemyActionResult{
        Killed,
        Failed,
        Success
    }
    private EnemyActionResult CheckForAlternativeAttack(Unit unit, TileRangeInfo tileRangeInfo, Vector3 startPosition){
        for(int i = tileRangeInfo.tiles.Length - 1; i >= 0; i--){
            var altTileRangeInfo = grid.CollectInformationAboutPositionRange(startPosition, tileRangeInfo.tiles[i].position);
            var altAttackCommand = new AttackCommand(unit, altTileRangeInfo, tileRangeInfo.tiles[i].position);
            if(altAttackCommand.CanExecute()){
                string result = altAttackCommand.Execute();
                IsUnitDead(result);
                if(playerUnits.Count == 0 || enemyUnits.Count == 0) return EnemyActionResult.Killed;
                return EnemyActionResult.Success;
            } else {
                altAttackCommand.AttackFailed();
            }
        }
        return EnemyActionResult.Failed;
    }

     private void OnGoToMainMenu(EmptyEventArgs args){
            Exit();
            gameMaster.CleanQueue();
            SceneManager.LoadScene("MainMenu");
        }
}

