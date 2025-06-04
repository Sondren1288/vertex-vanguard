using System.Collections.Generic;
using UnityEngine;
using PaletteNamespace;
public class OverworldClearing : MonoBehaviour
{
    public string clearingName;

    public float dimV = 0.9f;

    private void Start()
    {
        GetComponent<Renderer>().material.color = Palette.Instance.overworld_tile;
    }

    private void OnMouseEnter()
    {
        // Change the V value of the colour in HSV format to 0.5
        Color.RGBToHSV(Palette.Instance.overworld_tile, out float h, out float s, out float v);
        Color color = Color.HSVToRGB(h, s, dimV);
        this.GetComponent<Renderer>().material.color = color;
    }

    private void OnMouseExit()
    {
        Color.RGBToHSV(Palette.Instance.overworld_tile, out float h, out float s, out float v);
        Color color = Color.HSVToRGB(h, s, 1f);
        this.GetComponent<Renderer>().material.color = color;
    }

    private void OnMouseDown()
    {
        GameEvents.ClearingSelected.Invoke(clearingName);
    }

    public void SetClearingName(string name){
        clearingName = name;
    }
}
