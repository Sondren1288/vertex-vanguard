using System.Collections.Generic;
using System;
using UnityEngine.Rendering;
using UnityEngine;
using System.Linq;
using Unity.VisualScripting;

public class DataManager
{
    private readonly Overworld overworld;
    private readonly Dictionary<string, Army> armyRegistry = new();
    private readonly Dictionary<string, Unit> unitRegistry = new();
    private readonly Dictionary<string, Clearing> clearingRegistry = new();

    public DataManager(OverworldData overworldTemplate)
    {
        Overworld tempOV = UnityEngine.Object.Instantiate(overworldTemplate);
        overworld = tempOV;

        RegisterGameEntities();
    } 

    private void RegisterGameEntities(){
        foreach(var clearing in overworld.clearings){
            Clearing clearingCopy = UnityEngine.Object.Instantiate(clearing);
            clearingRegistry[clearing.clearingName] = clearingCopy;
            clearingRegistry[clearing.clearingName].clearingName = clearingCopy.clearingName;
            clearingRegistry[clearing.clearingName].inputLinks = clearingCopy.inputLinks;
            clearingRegistry[clearing.clearingName].cost = clearingCopy.cost;
            clearingRegistry[clearing.clearingName].position = clearingCopy.position;
            clearingRegistry[clearing.clearingName].grid = clearingCopy.grid;
            clearingRegistry[clearing.clearingName].specialFeature = clearingCopy.specialFeature;
        }

        foreach(var army in overworld.playerArmies){
            army.currentClearing = overworld.playerStartingClearing; // DANGER SO OVERWRITE
            armyRegistry[army.regimentName] = UnityEngine.Object.Instantiate(army);
            armyRegistry[army.regimentName].regimentName = army.regimentName;
            armyRegistry[army.regimentName].currentClearing = clearingRegistry[army.currentClearing.clearingName];
            armyRegistry[army.regimentName].ownership = army.ownership;


            for(int i = 0; i < army.units.Length; i++){
                Unit unitData = UnityEngine.Object.Instantiate(army.units[i]);
                while(unitRegistry.ContainsKey(unitData.characterName)){
                    Logger.Warning("Unit name already exists, generating new name" + unitData.characterName);
                    unitData.characterName = UnitData.GenerateRandomName();
                    Logger.Warning("New unit name: " + unitData.characterName);
                }
                Logger.Success("Adding unit " + unitData.characterName + " to registry on index " + i);
                unitRegistry[unitData.characterName] = unitData;
                armyRegistry[army.regimentName].units[i] = unitData;
            }

        }

        foreach(var army in overworld.enemyArmies){
            // army.currentClearing = overworld.enemyStartingClearing; // DANGER SO OVERWRITE
            armyRegistry[army.regimentName] = UnityEngine.Object.Instantiate(army);
            armyRegistry[army.regimentName].regimentName = army.regimentName;
            armyRegistry[army.regimentName].currentClearing = clearingRegistry[army.currentClearing.clearingName];
            armyRegistry[army.regimentName].ownership = army.ownership;


            for(int i = 0; i < army.units.Length; i++){
                Unit unitData = UnityEngine.Object.Instantiate(army.units[i]);
                while(unitRegistry.ContainsKey(unitData.characterName)){
                    unitData.characterName = UnitData.GenerateRandomName();
                }
                unitRegistry[unitData.characterName] = unitData;
                armyRegistry[army.regimentName].units[i] = unitData;
                
            }

        }
    }

    // Accessor methods
    public Overworld GetOverworld() => overworld;
    public Army GetArmy(string regimentName) => armyRegistry.TryGetValue(regimentName, out var army) ? army : null;
    public Dictionary<string, Army> GetArmyRegistry() => armyRegistry;
    public Unit GetUnit(string characterName) => unitRegistry.TryGetValue(characterName, out var unit) ? unit : null;
    public Dictionary<string, Unit> GetUnitRegistry() => unitRegistry;
    public Clearing GetClearing(string clearingName) => clearingRegistry.TryGetValue(clearingName, out var clearing) ? clearing : null;
    public Dictionary<string, Clearing> GetClearingRegistry() => clearingRegistry;
    public Army[] GetAllEnemyArmies() => armyRegistry.Values.Where(a => a.ownership == Ownership.Enemy).ToArray();
    public Clearing GetPlayerBaseClearing() => overworld.playerBaseClearing;

    public Army GetArmyInClearing(string clearingName, Ownership? ownership = null){
 
        foreach(var army in armyRegistry.Values){
            if(army.currentClearing.clearingName == clearingName && (ownership == null || army.ownership == ownership)){
                Logger.Success($"Found army: {army.regimentName} in clearing: {clearingName}");
                return armyRegistry[army.regimentName];
            }
        }
        return null;
    }

    public void SplitArmy(Army sourceArmy, Clearing destination){
        // Ensure path is valid
        if(!clearingRegistry[sourceArmy.currentClearing.clearingName].inputLinks.Any(link => link.clearingName == destination.clearingName)){
            Logger.Warning("Destination clearing " + destination.clearingName + " is not a valid link for " + sourceArmy.regimentName + " from " + sourceArmy.currentClearing.clearingName);
            foreach(var link in clearingRegistry[sourceArmy.currentClearing.clearingName].inputLinks){
                Logger.Success("Link in clearing: " + sourceArmy.currentClearing.clearingName + " is: " + link.clearingName);
            }
            return;
        }
        Func<Unit[], int> minMov = uL =>
        {
            int retVal = 2;
            foreach (Unit u in uL)
            {
                if (u.movePoints < retVal) retVal = u.movePoints;
            }
            return retVal;
        };
        if(sourceArmy.actionPoints <= 0 && sourceArmy.ownership == Ownership.Player){
            Logger.Warning("Source army " + sourceArmy.regimentName + " has no action points");
            return;
        }
        
        string oldArmyName = sourceArmy.regimentName;
        Army newArmy = UnityEngine.Object.Instantiate(sourceArmy);
        // Add new army to registry
        newArmy.currentClearing = destination;
        newArmy.actionPoints = sourceArmy.actionPoints - 1;
        while(armyRegistry.ContainsKey(newArmy.regimentName)){
            Logger.Warning("Army name already exists, generating new name" + newArmy.regimentName);
            newArmy.regimentName = ArmyData.GenerateRandomName();
        }

        if (minMov(newArmy.units) <= 0 && newArmy.ownership == Ownership.Player)
        {
            Logger.Warning("No movement for units in new army");
            return;
        }

        foreach (Unit unit in newArmy.units)
        {
            unit.movePoints--;
        }
        armyRegistry.Add(newArmy.regimentName, newArmy);

        // Remove units from old army

        armyRegistry[oldArmyName].units = armyRegistry[oldArmyName].units.Where(u => !newArmy.units.Contains(u)).ToArray();

        if(armyRegistry[oldArmyName].units.Length == 0){
            Logger.Success("Removing old army " + oldArmyName + " from registry");
            armyRegistry.Remove(oldArmyName);
        }
    }

    public void JoinArmies(Army joiningArmy, Army mainArmy){
        if(!clearingRegistry[mainArmy.currentClearing.clearingName].inputLinks.Any(link => link.clearingName == joiningArmy.currentClearing.clearingName)){
            Logger.Warning("Joining army is not a valid link for " + joiningArmy.regimentName);
            return;
        }

        Func<Unit[], int> minMov = uL =>
        {
            int retVal = 2;
            foreach (Unit u in uL)
            {
                if (u.movePoints < retVal) retVal = u.movePoints;
            }
            return retVal;
        };
        if(joiningArmy.actionPoints <= 0 || minMov(joiningArmy.units) <= 0){
            Logger.Warning("Joining army " + joiningArmy.regimentName + " has no action points");
            return;
        }

        // Add units from joining army to main army
        joiningArmy.actionPoints--;
        foreach (Unit unit in joiningArmy.units)
        {
            unit.movePoints--;
        }
        mainArmy.units = mainArmy.units.Concat(joiningArmy.units).ToArray();
        armyRegistry[mainArmy.regimentName] = mainArmy;
        armyRegistry[joiningArmy.regimentName].units = armyRegistry[joiningArmy.regimentName].units.Where(u => !joiningArmy.units.Contains(u)).ToArray();
        if(armyRegistry[joiningArmy.regimentName].units.Length == 0){
            armyRegistry.Remove(joiningArmy.regimentName);
        }
    }

    public void ReduceActionPoints(string regimentName){
        armyRegistry[regimentName].actionPoints--;
        foreach (Unit u in armyRegistry[regimentName].units)
        {
            u.movePoints--;
        }
    }

    public void ResetAllArmyActionPoints(Ownership ownership){
        foreach(var army in armyRegistry.Values){
            if(army.ownership == ownership){
                army.actionPoints = 2;
                foreach (Unit unit in army.units)
                {
                    unit.movePoints = 2;
                }
            }
        }
    }

    public void FindCheapestNextStepTowardsBase(){
        

        // Set initial distances for all vertices, 0 for the source vertex and infinity for all the others

        // 2. Choose the unvisited vertex with the shortest distance from the start to be the current vertex. So the algorithm will always start with the source as the current vertex.

        // For each of the current vertex's unvisited neighbor vertices, calculate the distance from the source and update the distance if the new calculated distance is lower.

        // We are now done with the current vertex so we mark it as visited. A visited vertex is not checked again.

        // Go back to step 2 to choose a new current vertex and keep repeating these steps until all vertices are visited.

        // In the end we are left with the shortest path from the source vertex to every other vertex in the graph.
    }

    public void RemoveArmyFromRegistry(string regimentName){
        foreach(Unit unit in armyRegistry[regimentName].units){
            unitRegistry.Remove(unit.characterName);
        }
        armyRegistry.Remove(regimentName);
    }

    public void RemoveUnitFromRegistry(Unit unit, string regimentName){
        unitRegistry.Remove(unit.characterName);
        armyRegistry[regimentName].units = armyRegistry[regimentName].units.Where(u => u != unit).ToArray();
        if(armyRegistry[regimentName].units.Length == 0){
            armyRegistry.Remove(regimentName);
        }
    }

    public void UpdateUnitRegistry(Unit unit, string regimentName){
        unitRegistry[unit.characterName] = unit;
        foreach(Unit armyUnit in armyRegistry[regimentName].units){
            if(armyUnit.characterName == unit.characterName){
                unitRegistry[unit.characterName] = unit;
                return;
            }
        }
    }

    public void UpdateOverworldLinks(){
        foreach(Clearing clearing in overworld.clearings){
            clearing.inputLinks = clearingRegistry[clearing.clearingName].inputLinks;
        }
    }

    public void DestroyLink(string fromClearing, string toClearing){
        Clearing fromClearingData = clearingRegistry[fromClearing];
        Clearing toClearingData = clearingRegistry[toClearing];
        fromClearingData.inputLinks = fromClearingData.inputLinks.Where(c => c.clearingName != toClearing).ToArray();
        toClearingData.inputLinks = toClearingData.inputLinks.Where(c => c.clearingName != fromClearing).ToArray();
        clearingRegistry[fromClearing] = fromClearingData;
        clearingRegistry[toClearing] = toClearingData;
    }
}
