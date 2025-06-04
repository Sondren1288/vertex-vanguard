using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Rendering.Universal;
using System.Linq;
using PaletteNamespace;

public enum HighlightType{
    None,
    Selected,
    Move,
    Attack,
    PredictedMove,
    PredictedAttack,
    SpawnPointPlayer,
    SpawnPointEnemy,
    Water
}

public class Tile : MonoBehaviour
{
  public float dimV = 0.9f;
  private Material currentMaterial;
  private HighlightType currentHighlightType;
  public ClearingGridTile tileData {get; set;}
  [SerializeField] private GameObject attackIndicator;
  [SerializeField] private GameObject moveIndicator;
  [SerializeField] private GameObject elevationBedrock;
  [SerializeField] private GameObject waterEffect;

  private GameObject[] instantiatedMoveIndicators;
  private Dictionary<HighlightType, Palette.PaletteColours> highlightColors = new Dictionary<HighlightType, Palette.PaletteColours>()
  {
    {HighlightType.None, Palette.PaletteColours.DefaultColor},
    {HighlightType.Selected, Palette.PaletteColours.SelectedColor},
    {HighlightType.Move, Palette.PaletteColours.TileHighlightMove},
    {HighlightType.Attack, Palette.PaletteColours.TileHighlightAttack},
    {HighlightType.PredictedMove, Palette.PaletteColours.TileHighlightPredictedMove},
    {HighlightType.PredictedAttack, Palette.PaletteColours.TileHighlightPredictedAttack},
    {HighlightType.SpawnPointPlayer, Palette.PaletteColours.TileHighlightSpawnPointPlayer},
    {HighlightType.SpawnPointEnemy, Palette.PaletteColours.TileHighlightSpawnPointEnemy},
    {HighlightType.Water, Palette.PaletteColours.TileHighlightWater},
  };
  
  private Color defaultColor;
  private Palette colourPaletteInstance;
  private void Awake(){
    currentMaterial = this.GetComponent<Renderer>().material;
    defaultColor = currentMaterial.color;
    instantiatedMoveIndicators = new GameObject[0];
    colourPaletteInstance = Palette.Instance;
  }

  public void Highlight(HighlightType highlightType){
    if(currentHighlightType == HighlightType.Water) return;
    currentHighlightType = highlightType;
    currentMaterial.color = Palette.Instance.GetColor(highlightColors[highlightType]);
  }

  public void ShowActionIndicator(AIActionType actionType, Vector3 startPosition)
  {
    StartGlowing(actionType);
    DrawDirectionIndicators(actionType, startPosition);
  }

  public void StartGlowing(AIActionType actionType){
    var tileBaseOverlay = attackIndicator.transform.GetChild(0).GetComponent<Renderer>().material;
    var wallsOfLight = attackIndicator.transform.GetChild(1).GetComponentsInChildren<Renderer>();
    Palette.PaletteColours baseColor = actionType == AIActionType.Attack ? highlightColors[HighlightType.PredictedAttack] : highlightColors[HighlightType.PredictedMove];

    Color transparentColor = Palette.Instance.GetColor(baseColor);
    transparentColor.a = 0.3f;
    tileBaseOverlay.SetColor("_FillColor", transparentColor);
    tileBaseOverlay.SetColor("_BorderColor", Palette.Instance.GetColor(baseColor));
    
    foreach(var wall in wallsOfLight){
      wall.material.SetColor("_Color", Palette.Instance.GetColor(baseColor));
    }

    attackIndicator.SetActive(true);
  }

  public void DrawDirectionIndicators(AIActionType actionType, Vector3 startPosition){
    Vector3 endPosition = transform.position;
    float distance = Vector3.Distance(startPosition, endPosition);
    int totalIndicators = Mathf.CeilToInt(2 * distance) + 1; // +1 to include both start and end
    Palette.PaletteColours baseColor = actionType == AIActionType.Attack ? highlightColors[HighlightType.PredictedAttack] : highlightColors[HighlightType.PredictedMove];

    // Place indicators at evenly spaced intervals, including start and end
    for(int i = 0; i < totalIndicators; i++){
        float t = (float)i / (totalIndicators - 1); // t=0 at start, t=1 at end
        Vector3 position = Vector3.Lerp(startPosition, endPosition, t);
        Vector3 adjustedPosition = new Vector3(position.x, 0.168f, position.z);
        GameObject indicator = Instantiate(moveIndicator, adjustedPosition, Quaternion.Euler(0, 45, 0));
        indicator.transform.SetParent(this.transform, true);
        indicator.transform.position = adjustedPosition;
        indicator.GetComponent<Renderer>().material.SetColor("_Color", Palette.Instance.GetColor(baseColor));
        instantiatedMoveIndicators = instantiatedMoveIndicators.Append(indicator).ToArray();
    }
  }

  public void HideActionIndicator(){
    attackIndicator.SetActive(false);
    foreach(var indicator in instantiatedMoveIndicators){
      Destroy(indicator);
    }
  }

  public void SetTileData(ClearingGridTile tileData){
    Logger.Success("Setting tile data for " + tileData.name);
    Logger.Success("Tile data: " + tileData.ToString());
    this.tileData = tileData;
    elevationBedrock.transform.localScale = new Vector3(1, tileData.elevation, 1);
    this.transform.position = new Vector3(this.transform.position.x, tileData.elevation * 0.3f, this.transform.position.z);
    if (tileData.type == ClearingGridTileType.Water){
      waterEffect.SetActive(true);
      currentMaterial.color = Palette.Instance.GetColor(highlightColors[HighlightType.Water]);
      currentHighlightType = HighlightType.Water;
      this.GetComponent<Renderer>().material.color = Palette.Instance.GetColor(highlightColors[HighlightType.Water]);
      this.transform.position = new Vector3(this.transform.position.x, -0.15f, this.transform.position.z);
      waterEffect.GetComponent<Renderer>().material.color = Palette.Instance.GetColor(highlightColors[HighlightType.Water]);
      elevationBedrock.transform.GetChild(0).GetComponent<Renderer>().material.color = Palette.Instance.GetColor(highlightColors[HighlightType.Water]);
    }
  }
    
    private void OnMouseDown(){
      GameEvents.TileClicked.Invoke(transform.position);
    }

    private void OnMouseEnter(){
      // make it slightly darker
      Color.RGBToHSV(currentMaterial.color, out float h, out float s, out float v);
      Color color = Color.HSVToRGB(h, s, dimV);
      this.GetComponent<Renderer>().material.color = color;
    }
    
    private void OnMouseExit(){
      this.GetComponent<Renderer>().material.color = Palette.Instance.GetColor(highlightColors[currentHighlightType]);
    }
}
