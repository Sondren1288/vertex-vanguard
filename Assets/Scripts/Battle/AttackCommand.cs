using System.Collections.Generic;
using UnityEngine;
using GetPositionInfo;
using static GetPositionInfo.PositionExtensions;
using static BattleGrid;
using System.Linq;

public class AttackCommand : ICommand
{
    private readonly Unit attackerData;
    private readonly TileRangeInfo tileRangeInfo;

    // Store state for undo
    private Vector3 originalPosition;
    private float originalTargetHealth;

    private GameObject attacker;
    private GameObject defender;
    private MoveCommand moveCommand;
    private bool useExhaustedCheck;

    public AttackCommand(Unit attackerData, TileRangeInfo tileRangeInfo, Vector3 destinationPosition, bool useExhaustedCheck = true){
      this.tileRangeInfo = tileRangeInfo;
      this.attackerData = attackerData;
      if(tileRangeInfo != null){
        this.attacker = PositionExtensions.GetGameObject(tileRangeInfo.tiles[0].position, "DeployedUnit");
        this.defender = PositionExtensions.GetGameObject(destinationPosition, "DeployedUnit");
      }
      moveCommand = null;
    }

    public bool CanExecute()
    {
        if(tileRangeInfo == null || attackerData == null || attacker == null || defender == null) return false;
        float actionCost = 0;
        int steps = Mathf.RoundToInt(Vector3.Distance(attacker.transform.position, defender.transform.position));
        if(steps == 0) return false;
        var trimmedMovementCosts = tileRangeInfo.movementCosts.Take(steps +1).ToArray();
        var trimmedTiles = tileRangeInfo.tiles.Take(steps +1).ToArray();


        if(trimmedMovementCosts.Length > 1){
          actionCost = trimmedMovementCosts.Sum(cost => cost);
          if(actionCost > attackerData.actionPoints) return false;
          // Check if can move to 1 tile before defender
          var tempMoveCommand = new MoveCommand(attackerData, tileRangeInfo, trimmedTiles.Take(trimmedTiles.Length - 1).Last().position, false);
          if(trimmedTiles.Length > 2 ){
            if(tempMoveCommand.CanExecute()){
              this.moveCommand = tempMoveCommand;
            }
            else {
              return false;
            }
          }
          
        }
        DeployedUnit attackerDeployedUnit = this.attacker.GetComponent<DeployedUnit>();
        if(useExhaustedCheck && attackerDeployedUnit.Exhausted) return false;
        if(defender.GetComponent<DeployedUnit>().Owner == attackerDeployedUnit.Owner) return false;
        return true;  
    }

    public string Execute()
    {
        // Store original state for undo
        originalPosition = tileRangeInfo.tiles[0].position;

        float damage = attackerData.actionPoints +1; // Max damage ever is 3
        DeployedUnit attackerDeployedUnit = this.attacker.GetComponent<DeployedUnit>();
        DeployedUnit defenderDeployedUnit = this.defender.GetComponent<DeployedUnit>();

        int steps = Mathf.RoundToInt(Vector3.Distance(attacker.transform.position, defender.transform.position));
        var trimmedMovementCosts = tileRangeInfo.movementCosts.Take(steps +1).ToArray();

        // Move to attack position if needed
        Logger.Emphasize("Moving to attack position");
        moveCommand?.Execute();

        if(trimmedMovementCosts.Length > 1){
          damage -= trimmedMovementCosts.Sum();
        }


        Vector3 targetPosition = defender.transform.position;
        // Perform attack
        Vector3 targetPositionAdjusted = new(targetPosition.x, targetPosition.y + 1f, targetPosition.z);
        Logger.Emphasize("Attacking");
        attackerDeployedUnit.AttackAnim(targetPositionAdjusted);
        defenderDeployedUnit.TakeDamage(damage);
        if(damage >= 3){
          // Also move unit to the defender's position
          attackerDeployedUnit.Move(targetPositionAdjusted);
        }
        attackerDeployedUnit.Exhaust();
        if(defenderDeployedUnit.UnitData.health <= 0){
          return defenderDeployedUnit.deployedUnitName;
        }
        return null;
    }

    public void AttackFailed(){
      if(attacker == null) return;
      DeployedUnit attackerDeployedUnit = this.attacker.GetComponent<DeployedUnit>();
      attackerDeployedUnit.Exhaust();
    }

    public void Undo()
    {
        if (attacker == null || defender == null) return;

        attacker.GetComponent<DeployedUnit>().Restore();
        defender.GetComponent<DeployedUnit>().UnitData.health = originalTargetHealth;
        attacker.transform.position = originalPosition;
    }
}