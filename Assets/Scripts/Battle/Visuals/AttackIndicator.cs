using UnityEngine;

public class AttackIndicator : MonoBehaviour
{
    [SerializeField] private Material indicatorMaterial;
    [SerializeField] private float heightOffset = 0.01f; // Smaller offset since we want it closer to tile
    [SerializeField] private Color fillColor = new Color(1f, 0f, 0f, 0.3f); // Semi-transparent red
    [SerializeField] private Color borderColor = new Color(1f, 0f, 0f, 0.8f); // More opaque red for border
    
    private MeshRenderer meshRenderer;
    private static readonly int FillColorProperty = Shader.PropertyToID("_FillColor");
    private static readonly int BorderColorProperty = Shader.PropertyToID("_BorderColor");
    
    private void Awake()
    {
        var mesh = new GameObject("AttackIndicatorPlane");
        mesh.transform.SetParent(transform);
        
        var meshFilter = mesh.AddComponent<MeshFilter>();
        meshFilter.mesh = CreateQuad();
        
        meshRenderer = mesh.AddComponent<MeshRenderer>();
        meshRenderer.material = new Material(indicatorMaterial);
        meshRenderer.material.SetColor(FillColorProperty, fillColor);
        meshRenderer.material.SetColor(BorderColorProperty, borderColor);
        
        // Position slightly above the tile
        mesh.transform.localPosition = new Vector3(0, heightOffset, 0);
        mesh.transform.localRotation = Quaternion.Euler(90, 0, 0);
    }

    private Mesh CreateQuad()
    {
        var mesh = new Mesh();
        
        var vertices = new[]
        {
            new Vector3(-0.5f, 0, -0.5f),
            new Vector3(0.5f, 0, -0.5f),
            new Vector3(-0.5f, 0, 0.5f),
            new Vector3(0.5f, 0, 0.5f)
        };
        
        var triangles = new[] { 0, 2, 1, 2, 3, 1 };
        var uv = new[]
        {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(0, 1),
            new Vector2(1, 1)
        };
        
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;
        mesh.RecalculateNormals();
        
        return mesh;
    }
}
