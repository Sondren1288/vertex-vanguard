using System.Collections.Generic;
using UnityEngine;

public abstract class Overworld: ScriptableObject
{
    public Clearing[] clearings;
    public Army[] playerArmies;
    public Army[] enemyArmies;
    public Clearing playerStartingClearing;
    public Clearing enemyStartingClearing;
    public Clearing playerBaseClearing;
}

[CreateAssetMenu(fileName = "NewOverworld", menuName = "Game/Overworld")]
public class OverworldData : Overworld {}