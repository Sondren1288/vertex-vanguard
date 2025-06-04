using UnityEngine;
using VertexVanguard.UI;
using System.Collections.Generic;
using GetPositionInfo;
using System.Linq;
public class BattleGrid : MonoBehaviour
{
    public float gridGap = 0.03f;
    public GameObject gridCellPrefab;
    public GameObject battleBoard;
    private GameObject[,] builtGrid;
    private GameObject[,] battlePrepGrid;
    private List<DeployedUnit> deployedUnitsShowingIncomingDamage = new();

    public void GenerateGrid(ClearingGrid grid, SpawnPointType spawnPointType){
        // Build grid
        battleBoard.SetActive(true);
        if(Mathf.Sqrt(grid.tiles.Length) % 1 != 0){
            throw new System.Exception("The number of tiles is not a square");
        }
        int size = (int)Mathf.Sqrt(grid.tiles.Length);

        builtGrid = new GameObject[size, size];
        NewGrid(grid, spawnPointType, size);
    }


    private void NewGrid(ClearingGrid grid, SpawnPointType spawnPointType, int size){
        GameObject parent = GameObject.FindGameObjectWithTag("Battle");
        for(int i = 0; i < size; i++){
            for(int j = 0; j < size; j++){
                GameObject cell = Instantiate(gridCellPrefab, new Vector3(i + i * gridGap, 0, j + j * gridGap), Quaternion.identity, parent.transform);
                Logger.Warning("Setting tile data for " + grid.tiles[i * size + j].name + " is it blocking? " + grid.tiles[i * size + j].blocking);
                cell.GetComponent<Tile>().SetTileData(grid.tiles[i * size + j]);
                builtGrid[i, j] = cell;

                foreach(Vector2 spawnPoint in grid.SpawnPoints[spawnPointType][SpawnPointSide.Player]){
                    if(spawnPoint == new Vector2(i, j)){
                        builtGrid[i, j].GetComponent<Tile>().Highlight(HighlightType.SpawnPointPlayer);
                    }
                }
                foreach(Vector2 spawnPoint in grid.SpawnPoints[spawnPointType][SpawnPointSide.Enemy]){
                    if(spawnPoint == new Vector2(i, j)){
                        builtGrid[i, j].GetComponent<Tile>().Highlight(HighlightType.SpawnPointEnemy);
                    }
                }
            }
        }
    }

    public void GenerateBattlePrepGrid(int size){
        battlePrepGrid = new GameObject[size, 1];
        GameObject parent = GameObject.FindGameObjectWithTag("Battle");
        // Build a line of tiles that will be on the left side of the grid
        float offset = -2f;
        for(int i = 0; i < size; i++){
            int columnPosition = i%4;
            GameObject cell = Instantiate(gridCellPrefab, new Vector3(offset, 0, columnPosition + i * gridGap), Quaternion.identity, parent.transform);
            battlePrepGrid[i, 0] = cell;
            if((i+1) % 4 == 0){
                int offsetMultiplier = (i+1) / 4;
                offset -= 1 + gridGap * offsetMultiplier;
            }
        }
    }

    public void DestroyGrid(){
        UnHighlightAllTiles();
        battleBoard.SetActive(false);
        foreach(GameObject cell in builtGrid){
            Destroy(cell);
        }
        builtGrid = null;
    }

    public void DestroyPrepGrid(){
        foreach(GameObject cell in battlePrepGrid){
            Destroy(cell);
        }
        battlePrepGrid = null;
    }

    public Vector3[] GetGridTilePositions(){
        List<Vector3> positions = new List<Vector3>();
        foreach(GameObject cell in builtGrid){
            positions.Add(cell.transform.position);
        }
        return positions.ToArray();
    }

    public Vector3[] GetPrepGridTilePositions(){
        List<Vector3> positions = new List<Vector3>();
        foreach(GameObject cell in battlePrepGrid){
            positions.Add(cell.transform.position);
        }
        return positions.ToArray();   
    }

    public Vector3 GetClosestCell(Vector3 position){
        Vector3[] positions = GetGridTilePositions();
        float minDistance = float.MaxValue;
        Vector3 closestCell = Vector3.zero;
        foreach(Vector3 cell in positions){
            float distance = Vector3.Distance(cell, position);
            if(distance < minDistance){
                minDistance = distance;
                closestCell = cell;
            }
        }   
        return closestCell;
    }

     public Vector3 GetClosestCellWithoutY(Vector3 position){
        Vector3[] positions = GetGridTilePositions();
        float minDistance = float.MaxValue;
        Vector3 closestCell = Vector3.zero;
        foreach(Vector3 cell in positions){
            Vector3 withoutY = new Vector3(cell.x, 0, cell.z);
            float distance = Vector3.Distance(withoutY, position);
            if(distance < minDistance){
                minDistance = distance;
                closestCell = cell;
            }
        }   
        return closestCell;
    }

    public Vector3[] GetNeighboringCells(Vector3 position){
        Vector3[] positions = GetGridTilePositions();
        List<Vector3> neighboringCells = new List<Vector3>();
        foreach(Vector3 cell in positions){
            if(Vector3.Distance(cell, position) == 1){
                neighboringCells.Add(cell);
            }
        }
        return neighboringCells.ToArray();
    }

    public void UnHighlightAllTiles(){
        if(builtGrid == null) return;
        foreach(GameObject cell in builtGrid){
            cell.GetComponent<Tile>().Highlight(HighlightType.None);
            DamageArcManager.Instance.HideAllArcsImmediate();
            foreach(DeployedUnit deployedUnit in deployedUnitsShowingIncomingDamage){
                deployedUnit.HideIncomingDamage();
            }
            deployedUnitsShowingIncomingDamage.Clear();
        }
    }

    public void HighlightMovement(Vector3 center, int actionPoints){
        // For each tile in a plus shape around the center highlight the tile
        TileRangeInfo[] tileRanges = new TileRangeInfo[4];
        GameObject thisUnit = PositionExtensions.GetGameObject(center, "DeployedUnit");
        if(thisUnit == null) return;
        DeployedUnit thisUnitData = thisUnit.GetComponent<DeployedUnit>();
        if(thisUnitData == null) return;
        
        tileRanges[0] = GetTileRangeInfoBasedOnActionPoints(center, Vector3.left, actionPoints);
        tileRanges[1] = GetTileRangeInfoBasedOnActionPoints(center, Vector3.right, actionPoints);
        tileRanges[2] = GetTileRangeInfoBasedOnActionPoints(center, Vector3.forward, actionPoints);
        tileRanges[3] = GetTileRangeInfoBasedOnActionPoints(center, Vector3.back, actionPoints);

        foreach(TileRangeInfo tileRange in tileRanges){
            if(tileRange == null) continue;
            
            float accumulatedCost = 0;

            bool skipDirection = false;
            foreach(SingleTileDetailInfo tile in tileRange.tiles){
                // Skip if this is the center tile
                if(tile.position == center) continue;
                if(skipDirection) continue;
                // Get the index to find the movement cost
                int index = System.Array.IndexOf(tileRange.tiles, tile);
                if(index > 0) {
                    accumulatedCost += tileRange.movementCosts[index];
                }
                
                // If movement cost exceeds action points, stop going in this direction
                if(accumulatedCost > actionPoints) {
                    break;
                }

                
                // If tile is blocking, stop going in this direction
                if(tile.tileInfo.tileData.blocking || tile.tileInfo.tileData.type == ClearingGridTileType.Water) {
                    break;
                }
                
                // Check if there's a unit on this tile
                if(tile.unitInfo != null) {
                    // Get the current unit's team from the center position
                    
                    if(thisUnitData != null && tile.unitInfo.Owner == thisUnitData.Owner) {
                        // Friendly unit - don't highlight and stop going in this direction
                        if(index == 0) continue;
                        break;
                    } else {
                        // Enemy unit - highlight and stop going in this direction
                        bool attackerIsExhausted = thisUnitData.Exhausted;
                        tile.tileInfo.Highlight(HighlightType.Attack);
                        DamageArcManager.Instance.ShowDamageArc(thisUnit.transform, tile.tileInfo.transform, attackerIsExhausted);
                        GameObject unit = PositionExtensions.GetGameObject(tile.tileInfo.transform.position, "DeployedUnit");
                        if(unit != null){
                            DeployedUnit newDeployedUnit = unit.GetComponent<DeployedUnit>();
                            if(newDeployedUnit != null){
                                newDeployedUnit.ShowIncomingDamage(3 - accumulatedCost, attackerIsExhausted);
                                deployedUnitsShowingIncomingDamage.Add(newDeployedUnit);
                            }
                        }

                        skipDirection = true;
                        continue;
                    }
                }
                
                // If we got here, highlight for movement
                tile.tileInfo.Highlight(HighlightType.Move);
                if(Mathf.Abs(accumulatedCost - actionPoints) < 0.1f && index != tileRange.tiles.Length) break;
            }
        }

        // Highlight the center tile
        Vector3 closestCell = GetClosestCell(center);
        builtGrid[Mathf.RoundToInt(closestCell.x), Mathf.RoundToInt(closestCell.z)].GetComponent<Tile>().Highlight(HighlightType.Selected);
    }

    public void ShowAttackHighlight(Vector3 tilePosition, AIActionType actionType, Vector3 startPosition){
        Vector3 closestCell = GetClosestCell(tilePosition);
        Tile tile = builtGrid[Mathf.RoundToInt(closestCell.x), Mathf.RoundToInt(closestCell.z)].GetComponent<Tile>();
        tile.ShowActionIndicator(actionType, startPosition);
    }

    public void HideAttackHighlight(Vector3 tilePosition){
        Vector3 closestCell = GetClosestCell(tilePosition);
        builtGrid[Mathf.RoundToInt(closestCell.x), Mathf.RoundToInt(closestCell.z)].GetComponent<Tile>().HideActionIndicator();
    }

    // public bool HighlightTile(Vector3 position, bool isSelected = false){
    //     Vector3 closestCell = GetClosestCell(position);
    //     if(isSelected){
    //         builtGrid[Mathf.RoundToInt(closestCell.x), Mathf.RoundToInt(closestCell.z)].GetComponent<Tile>().Highlight(HighlightType.Selected);
    //         return true;
    //     }
    //     string unitName = PositionExtensions.GetUnitName(closestCell);
    //     if(unitName != null){
    //         builtGrid[Mathf.RoundToInt(closestCell.x), Mathf.RoundToInt(closestCell.z)].GetComponent<Tile>().Highlight(HighlightType.Attack);
    //         return true;
    //     }
    //     builtGrid[Mathf.RoundToInt(closestCell.x), Mathf.RoundToInt(closestCell.z)].GetComponent<Tile>().Highlight(HighlightType.Move);
    //     return false;
    // }

  

    public class SingleTileDetailInfo{
        public Tile tileInfo;
        public DeployedUnit unitInfo = null;
        public Vector3 position;
    }

    public class TileRangeInfo{
        public SingleTileDetailInfo[] tiles;
        public Vector3 direction;
        public float[] movementCosts;

    }

    public TileRangeInfo CollectInformationAboutPositionRange(Vector3 startingPosition, Vector3 targetPosition){
        Vector3 start = GetClosestCell(startingPosition);
        Vector3 end = GetClosestCell(targetPosition);
        Vector3 direction = Vector3.Normalize(end - start);
        int steps = Mathf.RoundToInt(Vector3.Distance(start, end));
        Logger.Info("Start: " + start + " End: " + end + " Direction: " + direction + " Steps: " + steps);
        if(steps == 0) return null;
        TileRangeInfo tileRangeInfo = GetTileRangeInfoBasedOnActionPoints(start, direction, steps);
        // Print the tile range info
        if(tileRangeInfo != null) Logger.TileRangeInfo(tileRangeInfo);
        return tileRangeInfo;
    }

    public TileRangeInfo GetTileRangeInfoBasedOnActionPoints(Vector3 start, Vector3 inputDirection, float actionPoints){
        // Note that here movement costs are not cumulative in the returned tilerangeinfo it also includes central tile
        Vector3 direction = inputDirection;
        if (direction.x != 0 && direction.z != 0) return null;
        // Normalize direction to cardinal directions (up, down, left, right)
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.z)) {
            // Horizontal movement is dominant
            direction = new Vector3(Mathf.Sign(direction.x), 0, 0);
        } else if (Mathf.Abs(direction.z) > Mathf.Abs(direction.x)) {
            // Vertical movement is dominant
            direction = new Vector3(0, 0, Mathf.Sign(direction.z));
        } 
        
        TileRangeInfo tileRangeInfo = new()
        {
            direction = direction
        };
        
        List<SingleTileDetailInfo> tiles = new();
        List<float> movementCosts = new();
        float accumulatedCost = 0;

        bool brokeLegs = false;
        int i = 0;
        while(accumulatedCost <= actionPoints){
            SingleTileDetailInfo tile = new();
            Vector3 tilePosition = GetClosestCellWithoutY(new Vector3(start.x + direction.x * i, 0, start.z + direction.z * i));
            if(tilePosition.x < 0 || tilePosition.z < 0 || tilePosition.x >= builtGrid.GetLength(0) || tilePosition.z >= builtGrid.GetLength(1)) return null;
      
            tile.tileInfo = builtGrid[Mathf.RoundToInt(tilePosition.x), Mathf.RoundToInt(tilePosition.z)].GetComponent<Tile>();
            tile.position = tile.tileInfo.transform.position;
            GameObject unit = PositionExtensions.GetGameObject(tile.tileInfo.transform.position, "DeployedUnit");
            if(unit != null){
                tile.unitInfo = unit.GetComponent<DeployedUnit>();
            }
            tiles.Add(tile);
            if(i > 0){
                if(tile.tileInfo.tileData.blocking || brokeLegs){ // ER
                    movementCosts.Add(float.MaxValue);
                    tileRangeInfo.tiles = tiles.ToArray();
                    tileRangeInfo.movementCosts = movementCosts.ToArray();
                    return tileRangeInfo;
                } else {
                    int tile1Elevation = tiles[i-1].tileInfo.tileData.elevation;
                    int tile2Elevation = tile.tileInfo.tileData.elevation;
                    
                    if(tile1Elevation - tile2Elevation > 0){
                        // This means we are going downhill
                        // Going downhill more than 1 elevation step is allowed but forces the unit to stop
                        if(tile1Elevation - tile2Elevation > 2){
                            // The first movement should cost nothing but then no more actions are allowed
                            movementCosts.Add(0);
                            brokeLegs = true;
                        } else {
                            movementCosts.Add(1/(1f + tile1Elevation - tile2Elevation));
                        }


                    } else if(tile1Elevation - tile2Elevation < 0) {
                        // This means we are going uphill
                        // We can never go uphill more than 1 elevation step
                        if(tile2Elevation - tile1Elevation > 1){ // ER
                            movementCosts.Add(float.MaxValue);
                            tileRangeInfo.tiles = tiles.ToArray();
                            tileRangeInfo.movementCosts = movementCosts.ToArray();
                            return tileRangeInfo;
                        }
                        // Otherwise movement costs double
                        movementCosts.Add(2);
                    } else { // Same elevation
                        movementCosts.Add(1);
                    }
                }
            } else {
                movementCosts.Add(0);
            }
            accumulatedCost += movementCosts[^1];
            i++;
        }
        tileRangeInfo.tiles = tiles.ToArray();
        tileRangeInfo.movementCosts = movementCosts.ToArray();
        return tileRangeInfo;
    }
}

