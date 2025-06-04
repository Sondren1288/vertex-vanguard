using System.Collections.Generic;
using UnityEngine;

public abstract class Army: ScriptableObject
{
    public string regimentName; /* Used as an id */
    public Unit[] units;
    public Clearing currentClearing;
    public Ownership ownership;
    public int actionPoints = 2;
}

[CreateAssetMenu(fileName = "NewArmy", menuName = "Game/Army")]
public class ArmyData : Army {
    public static string GenerateRandomName(){
        string[] prefixes = { "Death", "Thunder", "", "Maroon", "Hallowed", "Ruthless", "Destiny", "Sanguine", "Black", "Ebon", "Final"};
        string[] postfixes = { "Corps", "Troops", "Pillagers", "Marauders", "Herd", "Host", "Vuphis", "Drumanir", "Eagolm", "Flakir"};
        return $"{"The " +prefixes[Random.Range(0, prefixes.Length)]} {postfixes[Random.Range(0, postfixes.Length)]}";
    }
}

public enum Ownership{
    Player,
    Enemy,
    
}

