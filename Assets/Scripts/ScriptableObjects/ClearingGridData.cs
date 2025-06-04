using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Unity.VisualScripting;

public enum SpawnPointType
{
    Attacking,
    Defending,
    Ambushing,
}

public enum SpawnPointSide
{
    Player,
    Enemy,
}

public enum ClearingGridType
{
    Plains,
    Forest,
    Mountain,
}

[Serializable]
public class SpawnPointData
{
    public Vector2[] playerSpawnPoints;
    public Vector2[] enemySpawnPoints;
}

public abstract class ClearingGrid: ScriptableObject
{
    public ClearingGridTile[] tiles;
    
    [SerializeField]
    private SpawnPointData attackingSpawnPoints;
    
    [SerializeField]
    private SpawnPointData defendingSpawnPoints;
    
    [SerializeField]
    private SpawnPointData ambushingSpawnPoints;
    
    public int difficulty;
    public ClearingGridType type;
    
    private Dictionary<SpawnPointType, Dictionary<SpawnPointSide, Vector2[]>> _spawnPoints;
    
    public Dictionary<SpawnPointType, Dictionary<SpawnPointSide, Vector2[]>> SpawnPoints
    {
        get
        {
            if (_spawnPoints == null)
            {
                _spawnPoints = new Dictionary<SpawnPointType, Dictionary<SpawnPointSide, Vector2[]>>();
                
                // Initialize attacking spawn points
                _spawnPoints[SpawnPointType.Attacking] = new Dictionary<SpawnPointSide, Vector2[]>
                {
                    { SpawnPointSide.Player, attackingSpawnPoints?.playerSpawnPoints ?? new Vector2[0] },
                    { SpawnPointSide.Enemy, attackingSpawnPoints?.enemySpawnPoints ?? new Vector2[0] }
                };
                
                // Initialize defending spawn points
                _spawnPoints[SpawnPointType.Defending] = new Dictionary<SpawnPointSide, Vector2[]>
                {
                    { SpawnPointSide.Player, defendingSpawnPoints?.playerSpawnPoints ?? new Vector2[0] },
                    { SpawnPointSide.Enemy, defendingSpawnPoints?.enemySpawnPoints ?? new Vector2[0] }
                };
                
                // Initialize ambushing spawn points
                _spawnPoints[SpawnPointType.Ambushing] = new Dictionary<SpawnPointSide, Vector2[]>
                {
                    { SpawnPointSide.Player, ambushingSpawnPoints?.playerSpawnPoints ?? new Vector2[0] },
                    { SpawnPointSide.Enemy, ambushingSpawnPoints?.enemySpawnPoints ?? new Vector2[0] }
                };
            }
            
            return _spawnPoints;
        }
    }

    public SpawnPointSide? TileSpawnPointSide(Vector3 position, SpawnPointType spawnPointType){
        Vector2 pos2D = new (Mathf.Round(position.x), Mathf.Round(position.z));
        
        // Check player spawn points
        foreach (Vector2 spawnPoint in SpawnPoints[spawnPointType][SpawnPointSide.Player]) {
            Logger.Info("Checking spawn point player" + spawnPoint + " against " + pos2D);
            if (Vector2.Distance(spawnPoint, pos2D) < 0.01f) {
                return SpawnPointSide.Player;
            }
        }
        
        // Check enemy spawn points
        foreach (Vector2 spawnPoint in SpawnPoints[spawnPointType][SpawnPointSide.Enemy]) {
            Logger.Info("Checking spawn point enemy" + spawnPoint + " against " + pos2D);
            if (Vector2.Distance(spawnPoint, pos2D) < 0.01f) {
                return SpawnPointSide.Enemy;
            }
        }
        
        return null;
    }
}

[CreateAssetMenu(fileName = "NewClearingGrid", menuName = "Game/ClearingGrid")]
public class ClearingGridData : ClearingGrid {
}