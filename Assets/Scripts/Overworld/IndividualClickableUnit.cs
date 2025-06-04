using System;
using UnityEngine;

public class IndividualClickableUnit : MonoBehaviour
{
    public float dimV = 0.9f;
    public string clearingName = "";
    private Material currentMaterial;

    private void Start()
    {
        currentMaterial = this.GetComponent<Renderer>().material;
    } 
    
    private void OnMouseDown(){
        GameEvents.IndividualUnitSelected.Invoke(transform.position);
    }
         
    private void OnMouseEnter(){
        // make it slightly darker
        Color.RGBToHSV(currentMaterial.color, out float h, out float s, out float v);
        Color color = Color.HSVToRGB(h, s, dimV);
        this.GetComponent<Renderer>().material.color = color;
    }
             
    private void OnMouseExit()
    {
       Color.RGBToHSV(currentMaterial.color, out float h, out float s, out float v);
        Color color = Color.HSVToRGB(h, s, 1f);
        this.GetComponent<Renderer>().material.color = color; 
    }
}