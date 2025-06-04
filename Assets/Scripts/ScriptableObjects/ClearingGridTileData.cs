using System.Collections.Generic;
using UnityEngine;

public abstract class ClearingGridTile: ScriptableObject
{
    public bool blocking;
    public int elevation;
    public ClearingGridTileType type;
}

public enum ClearingGridTileType
{
    Default,
    Water,
}

[CreateAssetMenu(fileName = "NewClearingGridTile", menuName = "Game/ClearingGridTile")]
public class ClearingGridTileData : ClearingGridTile {}