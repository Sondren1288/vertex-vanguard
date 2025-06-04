using System.Collections;
using UnityEngine;
using GetPositionInfo;
public class BattleManager : MonoBehaviour
{
    
    
    BattlePrepManager battlePrepManager;
    void Start(){
        Logger.Success("BattleManager started");
        battlePrepManager = GetComponent<BattlePrepManager>();
    }

   
    
    public void MoveUnit(Unit unit, Vector3 position, GameObject unitGameObject){
        if(unitGameObject == null) return;
        if(!IsValidMove(unitGameObject.transform.position, position, unit.actionPoints)) return;
        Logger.Success("Moving unit " + unit.characterName + " to " + position);
        unitGameObject.transform.position = position;

    }

    private bool IsValidMove(Vector3 startPosition, Vector3 endPosition, int actionPoints){
        // Determine which axis (x or z) has changed
        bool isXAxis = startPosition.x != endPosition.x;
        bool isZAxis = startPosition.z != endPosition.z;

        if(isXAxis == isZAxis) {
            Logger.Error("Invalid move: " + startPosition + " to " + endPosition);
            return false;
        }
    
        // Calculate distance based on the changing axis
        int distance = 0;
        if (isXAxis) {
            distance = Mathf.Abs(Mathf.RoundToInt(endPosition.x - startPosition.x));
        } else if (isZAxis) {
            distance = Mathf.Abs(Mathf.RoundToInt(endPosition.z - startPosition.z));
        }
    
        // Check if move is within action points
        if (distance > actionPoints) return false;
    
        // Check path for obstacles
        Vector3 currentPosition = startPosition;
        for (int i = 0; i < distance; i++) {
            // Increment position along the correct axis
            if (isXAxis) {
                float step = endPosition.x > startPosition.x ? 1 : -1;
                currentPosition.x += step;
            } else if (isZAxis) {
                float step = endPosition.z > startPosition.z ? 1 : -1;
                currentPosition.z += step;
            }
        
            // Check if there's a unit blocking the path
            if (PositionExtensions.GetUnitName(currentPosition) != null) return false;
        }
    
        return true;
    }
}
