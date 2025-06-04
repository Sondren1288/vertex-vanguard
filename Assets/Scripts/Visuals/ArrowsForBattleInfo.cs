using CameraNamespace;
using UnityEngine;

public class ArrowsForBattleInfo : MonoBehaviour
{
  public enum ArrowType {
    Left,
    Right,
    FromInfo,
    ToInfo
  }

  private Material currentMaterial;
  public float dimV = 0.8f;
  public ArrowType arrowType;
  public CameraManager cameraManager;
  

      private void Start(){
        currentMaterial = this.GetComponent<Renderer>().material;
        cameraManager = FindFirstObjectByType<CameraManager>();
      }

     private void OnMouseEnter(){
        // make it slightly darker
        Color.RGBToHSV(currentMaterial.color, out float h, out float s, out float v);
        Color color = Color.HSVToRGB(h, s, dimV);
        this.GetComponent<Renderer>().material.color = color;
    }
             
    private void OnMouseExit()
    {
       Color.RGBToHSV(currentMaterial.color, out float h, out float s, out float v);
        Color color = Color.HSVToRGB(h, s, 1f);
        this.GetComponent<Renderer>().material.color = color; 
    }

    private void OnMouseDown(){
      switch(arrowType){
        case ArrowType.Left:
          cameraManager.RotateCamera(Direction.Left);
          break;
        case ArrowType.Right:
          cameraManager.RotateCamera(Direction.Right);
          break;
        case ArrowType.ToInfo:
          cameraManager.SetActiveCamera(CameraVariant.BattleInfo);
          break;
        case ArrowType.FromInfo:
          cameraManager.SetActiveCamera(CameraVariant.Battle);
          break;
      }
    }
}