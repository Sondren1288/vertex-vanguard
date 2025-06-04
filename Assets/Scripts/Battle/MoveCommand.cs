using System.Linq;
using GetPositionInfo;
using UnityEngine;
using static BattleGrid;
using static GetPositionInfo.PositionExtensions;



public class MoveCommand : ICommand{
    private readonly Unit unitData;
    private readonly DeployedUnit deployedUnit;
    private Vector3 destinationPosition;
    private Vector3 startPosition;
    private bool? checkExhausted;
    private readonly TileRangeInfo tileRangeInfo;

    public MoveCommand(Unit unitData, TileRangeInfo tileRangeInfo, Vector3 destinationPosition, bool? checkExhausted = true){
        this.unitData = unitData;
        this.tileRangeInfo = tileRangeInfo;
        this.checkExhausted = checkExhausted;
        this.destinationPosition = destinationPosition; // Cannot use tileRangeInfo as it is all possible movements, not desired movements
        if(tileRangeInfo == null) return;
        this.startPosition = tileRangeInfo.tiles[0].position;
        GameObject thisUnit = PositionExtensions.GetGameObject(startPosition, "DeployedUnit");
        if(thisUnit == null) return;
        this.deployedUnit = thisUnit.GetComponent<DeployedUnit>();
       
    }

    public bool CanExecute(){
        // Allows for a larger move than it should ( probably because of the lack of a +1 that exists in the attack command)
        // Higlight doesn't show attackers correctly
        if(tileRangeInfo == null || this.deployedUnit == null) return false;
        int steps = Mathf.RoundToInt(Vector3.Distance(startPosition, destinationPosition));
        if(steps == 0) return false;
        var trimmedMovementCosts = tileRangeInfo.movementCosts.Take(steps +1);
        var trimmedTiles = tileRangeInfo.tiles.Take(steps +1);

        float accumulatedCost = 0;

        if(this.checkExhausted == true && deployedUnit.Exhausted) return false;
       
        foreach(SingleTileDetailInfo tile in trimmedTiles){
            if(tile.position == startPosition) continue;
            int index = System.Array.IndexOf(trimmedTiles.ToArray(), tile);
            accumulatedCost += trimmedMovementCosts.ElementAt(index);
            if(accumulatedCost > unitData.actionPoints) return false;
            if(Mathf.Abs(accumulatedCost - unitData.actionPoints) < 0.1f && index != trimmedTiles.Count()-1) return false;
            if(tile.tileInfo.tileData.blocking || tile.tileInfo.tileData.type == ClearingGridTileType.Water) return false;
            if(tile.unitInfo != null && index != 0) return false;
        }
        Logger.Success("All checks passed - can execute");
        return true;
    } 

    public string Execute(){
        Logger.Emphasize("Moving");
        deployedUnit.Move(destinationPosition);
        deployedUnit.Exhaust();
        return null;
    }

    public void Undo(){
        deployedUnit.Move(deployedUnit.transform.position);
    }
}
