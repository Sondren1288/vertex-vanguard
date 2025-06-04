using UnityEngine;
using System.Collections;

public class Ambush : MonoBehaviour
{
    public GameObject hidingPosition;
    public GameObject ambushingArmy {get; set;} = null;
    public string ambushingArmyName {get; private set;} = null;
    public bool isPreparing {get; private set;} = false;
    
    public void SetAmbush(GameObject ambushingArmy, string ambushingArmyName)
    {
        if (!isPreparing)
        {
            isPreparing = true;
            this.ambushingArmy = ambushingArmy;
            this.ambushingArmyName = ambushingArmyName;
        }
    }

    public void LeaveAmbush(){
        if(isPreparing){
            isPreparing = false;
            ambushingArmy = null;
            ambushingArmyName = null;
        }
    }
}
