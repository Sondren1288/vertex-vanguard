using System;
using System.Collections;
using CameraNamespace;
using GetPositionInfo;
using PaletteNamespace;
using Unity.VisualScripting;
using UnityEngine;
public class DeployedUnit : MonoBehaviour
{
    public string deployedUnitName;
    public bool exhausted = false;
    public bool Exhausted {get{return exhausted;}}
    private Material material;
    private Ownership owner;
    public Ownership Owner {get{return owner;}}
    private Unit unitData;
    public Unit UnitData {get{return unitData;}}
    public float dimV = 0.7f;

    private Vector3 originalPosition;

    private CameraManager cameraManager;
    private bool isDisplayingIncomingDamage = false;
    private bool isMovementAnimationPlaying = false;

    private void Start(){
        material = GetComponent<Renderer>().material;
        cameraManager = FindFirstObjectByType<CameraManager>();
    }

    public void OnMouseDown(){
        GameEvents.DeployedUnitClicked.Invoke(transform.position);
    }

    public void OnMouseEnter()
    {  
        // TODO create color on hover as well (i guess?)
        if (isDisplayingIncomingDamage) return;
        if (this.gameObject.GetComponent<UnitDataHolder>() == null)
            this.gameObject.AddComponent<UnitDataHolder>();
        UnitDataHolder unitDataHolder = this.gameObject.GetComponent<UnitDataHolder>();
        unitDataHolder.unitData = this.unitData;
        if(gameObject.GetComponent<UnitDataHolder>().unitData != null){ 
        // TODO: Should probably have this script on the prefab by default.
        // Currently there is an edge case when it doesn't exist and it breaks the game.
        this.gameObject.DisplayUnitHealth(cameraManager.battleCamera);
        }
    }

    public void ShowIncomingDamage(float damage, bool attackerIsExhausted = false){
        if (isDisplayingIncomingDamage) return;
        if (this.gameObject.GetComponent<UnitDataHolder>() == null)
            this.gameObject.AddComponent<UnitDataHolder>();
        UnitDataHolder unitDataHolder = this.gameObject.GetComponent<UnitDataHolder>();
        unitDataHolder.unitData = this.unitData;
        
        Color textColor = attackerIsExhausted ? Palette.Instance.text_disabled : Palette.Instance.text_red;
        this.gameObject.DisplayUnitHealth(cameraManager.battleCamera, damage, textColor);
        isDisplayingIncomingDamage = true;
    }

    public void HideIncomingDamage(){
        if (isDisplayingIncomingDamage){
            this.gameObject.HideText();
            isDisplayingIncomingDamage = false;
        }
    }

    public void OnMouseExit()
    {
        if (isDisplayingIncomingDamage) return;
        this.gameObject.HideText();
    }

    public void SetDeployedUnitName(string name){
        deployedUnitName = name;
    }

    public void SetOwner(Ownership owner){
        this.owner = owner;
    }

    public void SetUnitData(Unit unitData){
        this.unitData = unitData;
    }

    public string GetDeployedUnitName(){
        return deployedUnitName;
    }

    public void Move(Vector3 position){
        isMovementAnimationPlaying = true;	
        StartCoroutine(SmoothMovement(position));
    }
    
    public void Exhaust(){
        exhausted = true;
        Color.RGBToHSV(material.color, out float h, out float s, out float v);
        Color color = Color.HSVToRGB(h, s, dimV);
        this.GetComponent<Renderer>().material.color = color;
    }

    public void Restore(){
        exhausted = false;
        Color.RGBToHSV(material.color, out float h, out float s, out float v);
        Color color = Color.HSVToRGB(h, s, 1f);
        this.GetComponent<Renderer>().material.color = color;
    }

    public void AttackAnim(Vector3 targetPosition){
        StartCoroutine(BumpAnimation(targetPosition));
    }

    private IEnumerator BumpAnimation(Vector3 targetPosition){
        // If movement coroutine is playing wait for it to finish
        while(isMovementAnimationPlaying){
            yield return null;
        }
        originalPosition = transform.position;
        Vector3 startPosition = originalPosition;
        Vector3 direction = (targetPosition - startPosition).normalized;
        Vector3 bumpPosition = startPosition + direction * 0.5f;
        
        // Move towards target
        float duration = 0.1f;
        float elapsedTime = 0f;
        
        while (elapsedTime < duration){
            // Use the stored original position as reference
            transform.position = Vector3.Lerp(startPosition, bumpPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // Move back to original position
        elapsedTime = 0f;
        while (elapsedTime < duration){
            // Force the return to the exact original position
            transform.position = Vector3.Lerp(bumpPosition, originalPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // Ensure we end at the exact original position, regardless of what happened during animation
        transform.position = originalPosition;
    }

    private IEnumerator SmoothMovement(Vector3 targetPosition){
        Vector3 startPosition = transform.position;
        Vector3 destination = new Vector3(targetPosition.x, targetPosition.y + 0.3f, targetPosition.z);
        float duration = 0.3f;
        float elapsedTime = 0f;
        
        // Calculate height difference and arc parameters
        float heightDifference = destination.y - startPosition.y;
        float baseArcHeight = 0.5f; // Base arc height for same-level movement
        float arcHeightMultiplier = Mathf.Max(1f, 1f + heightDifference * 0.8f); // More arc when going up
        float totalArcHeight = baseArcHeight * arcHeightMultiplier;
        
        while (elapsedTime < duration){
            float t = elapsedTime / duration;
            
            // Linear interpolation for X and Z
            float x = Mathf.Lerp(startPosition.x, destination.x, t);
            float z = Mathf.Lerp(startPosition.z, destination.z, t);
            
            // Parabolic interpolation for Y (creates the arc)
            // The parabola peaks at t = 0.5
            float parabolicOffset = totalArcHeight * (4f * t * (1f - t));
            float y = Mathf.Lerp(startPosition.y, destination.y, t) + parabolicOffset;
            
            transform.position = new Vector3(x, y, z);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // Ensure we end at the exact destination
        transform.position = destination;
        isMovementAnimationPlaying = false;
    }

    public void TakeDamage(float damage){
        unitData.health -= damage;
    }
    
}