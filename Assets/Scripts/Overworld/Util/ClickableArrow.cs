using Unity.VisualScripting;
using UnityEngine;

public enum ArrowState 
{
    Ok,
    NoStamina,
    Enemy,
    Ally
}
public class ClickableArrow : MonoBehaviour
{
    public float dimV = 0.8f;
    public string targetClearing = "";
    public Clearing clearing = null;
    public ArrowState state = ArrowState.Ok;
    private Material currentMaterial;

    private void Start()
    {
        currentMaterial = this.GetComponent<Renderer>().material;
    } 
    
    private void OnMouseDown()
    {
        GameEvents.ArrowInvoked.Invoke(this);
    }
         
    private void OnMouseEnter()
    {
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
