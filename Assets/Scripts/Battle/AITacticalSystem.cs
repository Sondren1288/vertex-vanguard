using System.Collections.Generic;
using UnityEngine;
using GetPositionInfo;
using static BattleGrid;
using System.Linq;

public enum AITacticalSystemType{
    Enemy,
    Player
}

public class AITacticalSystem
{
    private readonly BattleGrid grid;
    private Dictionary<string, Unit> enemyUnits;
    private Dictionary<string, Unit> playerUnits;
    private readonly AITacticalSystemType aiType;

    // TODO: Not all are removed propa when they die

    public AITacticalSystem(BattleGrid grid, Dictionary<string, Unit> enemyUnits, Dictionary<string, Unit> playerUnits)
    {
        this.grid = grid;
        this.enemyUnits = enemyUnits;
        this.playerUnits = playerUnits;
    }

    public void SetAIUnits(Dictionary<string, Unit> units, AITacticalSystemType aiType){
        if(aiType == AITacticalSystemType.Enemy){
            this.enemyUnits = units;
        } else {
            this.playerUnits = units;
        }
    }

    public List<AIAction> CalculateActions()
    {
        var actions = new List<AIAction>();
        
        foreach (var enemyUnit in enemyUnits)
        {
            var bestAction = CalculateBestActionForUnit(enemyUnit.Value);
            if (bestAction != null)
            {
                actions.Add(bestAction);
            } else {
                Logger.Error("No action for unit: " + enemyUnit.Value.characterName);
            }
        }

        return actions;
    }

    private AIAction CalculateBestActionForUnit(Unit unit)
    {
        var possibleActions = new List<AIAction>();
        Vector3 unitPosition = GetUnitPosition(unit);
        
        // Calculate possible attack actions
        foreach (var playerUnit in playerUnits)
        {
            Vector3 targetPosition = GetUnitPosition(playerUnit.Value);
            if (CanAttack(unit, unitPosition, targetPosition))
            {
                float score = EvaluateAttackAction(unit, playerUnit.Value);
                possibleActions.Add(new AIAction(unit, unitPosition, targetPosition, AIActionType.Attack, score));
            }
        }

        // Calculate possible move actions
        var possibleMoves = GetPossibleMoves( unitPosition);
        Logger.Info("Possible moves: " + possibleMoves.Count);
        foreach (var movePosition in possibleMoves)
        {
            float score = EvaluateMoveAction(unit, movePosition);
            possibleActions.Add(new AIAction(unit, unitPosition, movePosition, AIActionType.Move, score));
        }


        return possibleActions.OrderByDescending(a => a.Score).FirstOrDefault();
    }

    private bool CanAttack(Unit attacker, Vector3 attackerPos, Vector3 targetPos)
    {
        var tileRangeInfo = grid.CollectInformationAboutPositionRange(attackerPos, targetPos);
        if(tileRangeInfo == null) return false;
        
        var attackCommand = new AttackCommand(attacker, tileRangeInfo, targetPos, false);
        return attackCommand.CanExecute();
    }

    private List<Vector3> GetPossibleMoves(Vector3 currentPos)
    {
        var possibleMoves = new List<Vector3>();
        TileRangeInfo[] tileRanges = new TileRangeInfo[4];
        int actionPoints = 2;

        GameObject thisUnit = PositionExtensions.GetGameObject(currentPos, "DeployedUnit");
        if(thisUnit == null) return null;
        DeployedUnit thisUnitData = thisUnit.GetComponent<DeployedUnit>();
        if(thisUnitData == null) return null;

        tileRanges[0] = grid.GetTileRangeInfoBasedOnActionPoints(currentPos, Vector3.left, actionPoints);
        tileRanges[1] = grid.GetTileRangeInfoBasedOnActionPoints(currentPos, Vector3.right, actionPoints);
        tileRanges[2] = grid.GetTileRangeInfoBasedOnActionPoints(currentPos, Vector3.forward, actionPoints);
        tileRanges[3] = grid.GetTileRangeInfoBasedOnActionPoints(currentPos, Vector3.back, actionPoints);

        foreach(TileRangeInfo tileRange in tileRanges){
            if(tileRange == null) continue;
            Logger.TileRangeInfo(tileRange);           
            
            float accumulatedCost = 0;

            bool skipDirection = false;
            foreach(SingleTileDetailInfo tile in tileRange.tiles){
                // Skip if this is the center tile
                if(tile.position == currentPos) continue;
                if(skipDirection) continue;
                Logger.Success("Tile: " + tile.position);
                // Get the index to find the movement cost
                int index = System.Array.IndexOf(tileRange.tiles, tile);
                if(index > 0) {
                    accumulatedCost += tileRange.movementCosts[index];
                }
                Logger.Success("Accumulated cost: " + accumulatedCost);
                
                // If movement cost exceeds action points, stop going in this direction
                if(accumulatedCost > actionPoints) {
                    Logger.Success("Accumulated cost exceeds action points: " + accumulatedCost);
                    skipDirection = true;
                    continue;
                }
                
                // If tile is blocking, stop going in this direction
                if(tile.tileInfo.tileData.blocking || tile.tileInfo.tileData.type == ClearingGridTileType.Water) {
                    Logger.Success("Tile is blocking: " + tile.position);
                    skipDirection = true;
                    continue;
                }
                
                // Check if there's a unit on this tile
                if(tile.unitInfo != null) {
                    Logger.Success("Tile has unit: " + tile.position);

                    if(index != 0)skipDirection = true;
                    continue;
                }
                
                // If we got here, Add to possible moves
                Logger.Success("Adding to possible moves: " + tile.position);
                possibleMoves.Add(tile.position);
                if(Mathf.Abs(accumulatedCost - actionPoints) < 0.1f && index != tileRange.tiles.Length) break;
                Logger.Success("Legs not broken");
            }
        }

        // Log all possible moves
        foreach(Vector3 move in possibleMoves){
            Logger.Info("Unit s: " + thisUnitData.deployedUnitName);
            Logger.Info("Possible move actual: " + move);
        }

        return possibleMoves;
    }

    private float EvaluateAttackAction(Unit attacker, Unit target)
    {
        float score = 100; // Base score for attack actions
        
        // Prioritize attacking low health targets
        score += (1 - (target.health / 100)) * 50;
        
        // Bonus for doing more damage
        Vector3 attackerPos = GetUnitPosition(attacker);
        Vector3 defenderPos = GetUnitPosition(target);
        var tileRangeInfo = grid.CollectInformationAboutPositionRange(attackerPos, defenderPos);
        if(tileRangeInfo == null) return float.MinValue;
        int steps = Mathf.RoundToInt(Vector3.Distance(attackerPos, defenderPos));
        var trimmedMovementCosts = tileRangeInfo.movementCosts.Take(steps +1).ToArray();

        float damage = attacker.actionPoints + 1;
        if(tileRangeInfo.movementCosts.Length > 0){
            damage -= trimmedMovementCosts.Sum();
        }
        
        score += damage * 10;
        
        return score;
    }

    private float EvaluateMoveAction(Unit unit, Vector3 targetPosition)
    {
        float score = 50; // Base score for move actions
        
        // Get information about the move
        var tileRangeInfo = grid.CollectInformationAboutPositionRange(GetUnitPosition(unit), targetPosition);
        if (tileRangeInfo == null) return float.MinValue; // Invalid move
        
        // Check if any tiles in the path (except the first one) have a unit
        for (int i = 1; i < tileRangeInfo.tiles.Length; i++)
        {
            if (tileRangeInfo.tiles[i].unitInfo != null)
            {
                return float.MinValue; // Path is blocked by a unit should never execute
            }
        }

        // Consider terrain elevation for tactical advantage
        float finalElevation = tileRangeInfo.tiles[^1].tileInfo.tileData.elevation;
        score += finalElevation * 5; // Bonus for higher ground

        // Find closest player unit
        float minDistanceToPlayer = float.MaxValue;
        foreach (var playerUnit in playerUnits)
        {
            Vector3 playerPos = GetUnitPosition(playerUnit.Value);
            float distance = Vector3.Distance(targetPosition, playerPos);
            minDistanceToPlayer = Mathf.Min(minDistanceToPlayer, distance);
        }
        
        // Prefer moves that get us closer to player units but not too close
        float optimalDistance = 1f; // We want to be 1 tile away ideally
        score -= Mathf.Abs(minDistanceToPlayer - optimalDistance) * 10;
        
        return score;
    }

    private Vector3 GetUnitPosition(Unit unit)
    {
        GameObject unitGO = GameObject.FindObjectsByType<DeployedUnit>(FindObjectsSortMode.None)
            .FirstOrDefault(du => du.deployedUnitName == unit.characterName)?.gameObject;
        return unitGO != null ? unitGO.transform.position : Vector3.zero;
    }
} 