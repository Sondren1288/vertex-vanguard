using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GetPositionInfo;
using UINamespace;
using OverworldNamespace;
using CameraNamespace;
using UnityEngine.SceneManagement;

namespace BattlePrep {

    public class BattlePrepState: GameState
    {
        private readonly Army engagedArmyPlayer;
        private readonly Army engagedArmyEnemy;
        private Army fightingArmyPlayer;
        private readonly BattlePrepManager manager;
        private readonly BattleGrid grid;
        private Unit selectedUnit;
        private readonly SpawnPointType engagementType;
        private int numberOfEnemiesSpawned = 0;
        private GameObject overworldGO;
        public BattlePrepState(GameMaster gameMaster, Army playerArmy, Army enemyArmy, SpawnPointType engagementType) : base(gameMaster)
        {
            engagedArmyPlayer = playerArmy;
            engagedArmyEnemy = enemyArmy;
            this.manager = gameMaster.GetComponent<BattlePrepManager>();
            this.grid = gameMaster.GetComponent<BattleGrid>();
            this.engagementType = engagementType;
            overworldGO = GameObject.FindGameObjectWithTag("Overworld");
        }

        public override void Enter(){
            base.Enter();
            overworldGO.SetActive(false);
            manager.ClearSpawnedUnits();
            gameMaster.GetComponent<CameraManager>().SetActiveCamera(CameraVariant.Battle);
            ClearingGrid activeGrid = engagementType == SpawnPointType.Attacking ? engagedArmyEnemy.currentClearing.grid : engagedArmyPlayer.currentClearing.grid;
            grid.GenerateGrid(activeGrid, engagementType);
            grid.GenerateBattlePrepGrid(engagedArmyPlayer.units.Count());
            manager.SpawnUnits(engagedArmyPlayer.units, grid.GetPrepGridTilePositions(), engagedArmyPlayer.ownership);
            // Convert vector 2 to vector 3 keeping Y = 0
            Vector3[] enemySpawnPoints = activeGrid.SpawnPoints[engagementType][SpawnPointSide.Enemy].Select(spawnPoint => new Vector3(spawnPoint.x, 0, spawnPoint.y)).ToArray();
            // Find the closest cells from grid tile positions and use them to spawn
            Vector3[] enemySpawnCells = enemySpawnPoints.Select(spawnPoint => grid.GetClosestCell(spawnPoint)).ToArray();

            numberOfEnemiesSpawned = enemySpawnCells.Length;      
            manager.SpawnUnits(engagedArmyEnemy.units.Take(enemySpawnCells.Length).ToArray(), enemySpawnCells, engagedArmyEnemy.ownership);
            
            selectedUnit = ScriptableObject.CreateInstance<UnitData>();
            selectedUnit = null;
            Logger.Success("Battle Prep State Entered");
            if(engagementType == SpawnPointType.Attacking){
                UINamespace.UIGuy.SetBottomRightButtonType(UINamespace.UIGuy.BottomRightButtonType.ExitBattlePrep, 0);
            }
            UINamespace.UIGuy.SetBottomRightButtonType(UINamespace.UIGuy.BottomRightButtonType.StartBattle, 1);
        }
        
        public override void Exit(){
            base.Exit();
            grid.DestroyPrepGrid();
            Logger.Success("Battle Prep State Exited");

            UINamespace.UIGuy.SetBottomRightButtonType(UINamespace.UIGuy.BottomRightButtonType.Invisible, 0);
            UINamespace.UIGuy.SetBottomRightButtonType(UINamespace.UIGuy.BottomRightButtonType.Invisible, 1);
        }

        protected override void RegisterEvents(){
            Subscribe(GameEvents.DeployedUnitClicked, HandleClick);
            Subscribe(GameEvents.BottomRightButtonClicked, OnBottomRightButtonClicked);
            Subscribe(GameEvents.TileClicked, HandleClick);
            Subscribe(GameEvents.GoToMainMenu, OnGoToMainMenu);
        }

      

        private void HandleClick(Vector3 position){
            string unitClickedName = PositionExtensions.GetUnitName(position);
            if(selectedUnit != null){
                Logger.Success("Selected Unit: " + selectedUnit.characterName);
                if(TileIsSpawnPoint(position) || DestinationIsPrepTile(position)){
                    if(unitClickedName == null){
                        MoveUnit(selectedUnit, position);
                    }
                    else if (unitClickedName != selectedUnit.characterName){
                        SwitchPositions(selectedUnit, engagedArmyPlayer.units.FirstOrDefault(unit => unit.characterName == unitClickedName));
                        manager.UnhighlightUnit(selectedUnit);
                        selectedUnit = null;
                        return;
                    }
                }
                manager.UnhighlightUnit(selectedUnit);
                selectedUnit = null;
            }
            if(unitClickedName != null){
                Logger.Success("Unit Clicked: " + unitClickedName);
                selectedUnit = engagedArmyPlayer.units.FirstOrDefault(unit => unit.characterName == unitClickedName);
                manager.HighlightUnit(selectedUnit);
            }
        }

        

        private void MoveUnit(Unit unit, Vector3 position){
            Logger.Success("Moving Unit: " + unit.characterName + " to: " + position);
            // Update battleprepmanager
            manager.MoveUnit(unit, position);
        }

        private void SwitchPositions(Unit unit, Unit unit2){
            Logger.Success("Switching positions of " + unit.characterName + " and " + unit2.characterName);
            manager.SwitchPositions(unit, unit2);
        }

        private bool TileIsSpawnPoint(Vector3 position){
            ClearingGrid activeGrid = engagementType == SpawnPointType.Attacking ? engagedArmyEnemy.currentClearing.grid : engagedArmyPlayer.currentClearing.grid;
            SpawnPointSide? side = activeGrid.TileSpawnPointSide(position, engagementType);
            Logger.Warning("Tile is spawn point: " + side + " " + engagementType);
            return side == SpawnPointSide.Player;
        }

        private bool DestinationIsPrepTile(Vector3 position){
            return grid.GetPrepGridTilePositions().Contains(position);
        }

   

        private Dictionary<string, Unit> RemoveBattlePrepUnits(){
            Vector3[] battlePrepPositions = grid.GetPrepGridTilePositions();
            Dictionary<string, Unit> unitsOnGrid = new();
            foreach(Unit unit in engagedArmyPlayer.units){
                    unitsOnGrid.Add(unit.characterName, unit);
            }

            foreach(Unit unit in engagedArmyEnemy.units){
                unitsOnGrid.Add(unit.characterName, unit);
            }

            foreach(Vector3 position in battlePrepPositions){
                string unitName = PositionExtensions.GetUnitName(position);
                if(unitName != null){
                    manager.DestroyUnitGameObject(unitName);
                    unitsOnGrid.Remove(unitName);
                }
            }

            return unitsOnGrid;
        }

        // === Bottom right button === //
        private void OnBottomRightButtonClicked(UINamespace.UIGuy.BottomRightButtonType buttonType){
            if(buttonType == UINamespace.UIGuy.BottomRightButtonType.StartBattle)
            {
                Vector3[] battlePrepPositions = grid.GetPrepGridTilePositions();
                Dictionary<string, Unit> unitsOnGrid = new();
                foreach(Unit unit in engagedArmyPlayer.units)
                {
                    unitsOnGrid.Add(unit.characterName, unit);
                } 
                foreach(Vector3 position in battlePrepPositions)
                {
                    string unitName = PositionExtensions.GetUnitName(position);
                    if(unitName != null){
                        unitsOnGrid.Remove(unitName);
                    }
                }
                
                if(engagedArmyPlayer.units.Any() && engagedArmyEnemy.units.Any() && unitsOnGrid.Any()){ // TODO fix this, it starts anyways
                    OnBattleStart();
                }
                else{
                    Logger.Error("Not enough units to start battle");
                }
            } else if(buttonType == UINamespace.UIGuy.BottomRightButtonType.ExitBattlePrep){
                overworldGO.SetActive(true);
                OnBattlePrepQuit();
            }
        }

          private void OnBattlePrepQuit(){
            grid.DestroyGrid();
            manager.DestroyAllUnitGameObjects();
            gameMaster.ChangeState(new OverworldState(gameMaster));
        }

        private void OnBattleStart(){
            Dictionary<string, Unit> unitsOnGrid = RemoveBattlePrepUnits();
            grid.UnHighlightAllTiles();
            Dictionary<string, Unit> playerUnitsOnGrid = new();
            Dictionary<string, Unit> enemyUnitsOnGrid = new();
            foreach(Unit unit in unitsOnGrid.Values){
                if(engagedArmyEnemy.units.Take(numberOfEnemiesSpawned).Contains(unit)){
                    enemyUnitsOnGrid.Add(unit.characterName, unit);
                }
                else if(engagedArmyPlayer.units.Contains(unit)){
                    playerUnitsOnGrid.Add(unit.characterName, unit);
                }
            }
            gameMaster.GetDataManager().ReduceActionPoints(engagedArmyPlayer.regimentName);
            gameMaster.ChangeState(new BattleState(gameMaster, playerUnitsOnGrid, enemyUnitsOnGrid, new string[] {engagedArmyPlayer.regimentName, engagedArmyEnemy.regimentName}, engagementType, overworldGO));
        }

        // === Bottom right button === //
        private void OnGoToMainMenu(EmptyEventArgs args){
            Exit();
            gameMaster.CleanQueue();
            SceneManager.LoadScene("MainMenu");
        }
    }

}
