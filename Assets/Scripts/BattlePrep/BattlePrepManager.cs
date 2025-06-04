using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using System.Linq;
using System;
using System.Collections;
using PaletteNamespace;
public class BattlePrepManager : MonoBehaviour
{

  public GameObject unitPrefab;
  public Dictionary<string, GameObject> spawnedUnits;
  
  [Header("The death of a unit")]
  [SerializeField] private ParticleSystem deathEffect;

  public void ClearSpawnedUnits(){
    spawnedUnits = new Dictionary<string, GameObject>();
  }

  public void SpawnUnits(Unit[] engagedUnits,Vector3[] positions, Ownership ownership){
    GameObject parent = GameObject.FindGameObjectWithTag("Battle");
    for(int i = 0; i < engagedUnits.Length; i++){
      Vector3 spawnPosition = new Vector3(positions[i].x, 1, positions[i].z);
      GameObject unit = Instantiate(unitPrefab, spawnPosition, Quaternion.identity, parent.transform);
      unit.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
      unit.GetComponent<Renderer>().material.color = ownership == Ownership.Player ? Palette.Instance.playerColor : Palette.Instance.enemyColor;
      unit.GetComponent<DeployedUnit>().SetDeployedUnitName(engagedUnits[i].characterName);
      unit.GetComponent<DeployedUnit>().SetOwner(ownership);
      unit.GetComponent<DeployedUnit>().SetUnitData(engagedUnits[i]);
      spawnedUnits.Add(engagedUnits[i].characterName, unit);
      
    }
  }
  public IEnumerator KillCube(string cubeName)
  {
    GameObject cube = spawnedUnits[cubeName];
    if(cube == null) yield break;
    Renderer cubeRenderer = cube.GetComponent<Renderer>();
    ParticleSystem deathPs = Instantiate(deathEffect);
    deathPs.transform.position = cube.transform.position;
    ParticleSystem.MainModule deathPsMain = deathPs.main;
    ParticleSystemRenderer deathPsRenderer = deathPs.GetComponent<ParticleSystemRenderer>();
    deathPsRenderer.material = cubeRenderer.material;
    deathPs.Play();
    yield return new WaitForSeconds(0.5f);
    while (deathPs.isPlaying)
    {
      yield return null;
    }
    Destroy(deathPs.gameObject);
  }
  public void HighlightUnit(Unit unit){
    spawnedUnits[unit.characterName].GetComponent<Renderer>().material.color = Palette.Instance.selectedColor;
  }

  public void UnhighlightUnit(Unit unit){
    spawnedUnits[unit.characterName].GetComponent<Renderer>().material.color = Palette.Instance.playerColor;
  }

  public void MoveUnit(Unit unit, Vector3 position){
    spawnedUnits[unit.characterName].transform.position = position;
  }

  public void SwitchPositions(Unit unit, Unit unit2){
    Vector3 position1 = spawnedUnits[unit.characterName].transform.position;
    Vector3 position2 = spawnedUnits[unit2.characterName].transform.position;
    spawnedUnits[unit.characterName].transform.position = position2;
    spawnedUnits[unit2.characterName].transform.position = position1;
  }

  public void DestroyUnitGameObject(string unitName){
    Destroy(spawnedUnits[unitName]);
    spawnedUnits.Remove(unitName);
  }

  public void DestroyAllUnitGameObjects(){
    foreach(GameObject unit in spawnedUnits.Values){
      Destroy(unit);
    }
    spawnedUnits.Clear();
  }
}
