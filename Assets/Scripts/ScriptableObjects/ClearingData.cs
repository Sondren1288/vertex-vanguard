using System.Collections.Generic;
using UnityEngine;

public enum SpecialFeature
{
    None,
    Ambush, /* Allows for the ambush engagement */
    FallenTree, /* A tree that falls down on activation - breaks connection point to clearing */
}

public abstract class Clearing: ScriptableObject
{
    public string clearingName; /* Used as an id */
    public Vector2 position;
    public ClearingGrid grid;
    public Clearing[] inputLinks;
    public SpecialFeature specialFeature;
    public int cost = 1;
}

[CreateAssetMenu(fileName = "NewClearing", menuName = "Game/Clearing")]
public class ClearingData : Clearing {}
