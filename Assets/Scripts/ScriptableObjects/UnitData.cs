using System.Collections.Generic;
using UnityEngine;

public enum Skills
{
    None,
    Glyph_of_dexterity, /* Allows for a diagonal move at the cost of 2 actions */
}

public abstract class Unit: ScriptableObject
{
    public string characterName; /* Used as an id */
    public int actionPoints;
    public float health;
    public Skills[] skills;
    public int movePoints = 2;
}

[CreateAssetMenu(fileName = "NewUnit", menuName = "Game/Unit")]
public class UnitData : Unit
{
      
    // Optional: More sophisticated name generation
    public static string GenerateRandomName()
    {
        string[] prefixes = { "Brave", "Swift", "Mighty", "Clever", "Noble", "Mark", "Rog", "Gor", "Thorg", "Borg", "Dorg", "Sorg", "Torg", "Uorg", "Vorg", "Worg", "Xorg", "Yorg", "Zorg" };
        string[] names = { "Warrior", "Scout", "Knight", "Archer", "Mage", "Woodchuck", "Boat-builder", "Stone-carver", "Farmer", "Blacksmith", "Tailor", "Weaver", "Potter", "Mason", "Carpenter", "Blacksmith", "Tailor", "Weaver", "Potter", "Mason", "Carpenter" };
        
        return $"{prefixes[UnityEngine.Random.Range(0, prefixes.Length)]} {names[UnityEngine.Random.Range(0, names.Length)]}";
    }
}