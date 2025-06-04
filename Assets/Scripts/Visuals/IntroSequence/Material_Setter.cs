using UnityEngine;
using UnityEngine.Rendering;
using PaletteNamespace;

public class Material_Setter : MonoBehaviour
{
    public Palette.PaletteColours materialType;

    private void Start(){
        this.GetComponent<MeshRenderer>().material.color = Palette.Instance.GetColor(materialType);
    }
}
