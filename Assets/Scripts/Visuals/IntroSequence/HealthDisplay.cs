using UnityEngine;

public class HealthDisplay : MonoBehaviour
{
  public float healthToDisplay;
  public Camera camera;

  public void Start(){
    UnitDataHolder unitDataHolder = this.GetComponent<UnitDataHolder>();
    if(unitDataHolder == null){
      Logger.Error("No unit data holder found");
      return;
    }
    Unit unit = Instantiate(unitDataHolder.unitData);
    unit.health = healthToDisplay;
    unitDataHolder.unitData = unit;
    this.gameObject.DisplayUnitHealth(camera);
  }
}