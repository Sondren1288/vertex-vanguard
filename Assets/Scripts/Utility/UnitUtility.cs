using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using PaletteNamespace;
using Unity.VisualScripting;

public static class UnitUtility
{
    private static string fullBlock = "\u2588";
    private static string halfBlock = "\u2584";

    private static string emptyBlock = "\u2591";

    //private static string movementRemains = "\u2B24";
    //private static string movementUsed = "\u25EF";
    private static string movementRemains = "\u25CF";
    private static string movementUsed = "\u25CB";

    private static float fontSize = 3f;
    private static float hoverHeight = 1.2f;

    private static Color vanta = new Color(0, 0, 0);

    /// <summary>
    /// A function to display the health of a unit. 
    /// </summary>
    /// <param name="unit">The unit in question</param>
    /// <param name="camera">The camera the text is to face</param>
    public static void DisplayUnitHealth(this GameObject go, Camera camera, float healthModifier = 0f,
        Color? color = null)
    {
        Unit unit;
        if (go.GetComponent<UnitDataHolder>().unitData != null)
        {
            unit = go.GetComponent<UnitDataHolder>().unitData;
        }
        else
        {
            throw new NullReferenceException();
        }

        if (unit == null) return;
        float health = unit.health - healthModifier;
        string healthbar = "";

        while (health >= 1f)
        {
            healthbar += fullBlock;
            health -= 1f;
        }

        if (health > 0)
        {
            healthbar += halfBlock;
        }

        while (healthbar.Length < 3)
        {
            healthbar += emptyBlock;
        }

        GameObject textContainerGO;
        TextMeshPro textMesh;
        GameObject backgroundQuadGO;

        const string TextContainerGOName = "UnitHealthDisplay_Container";
        const string BackgroundQuadName = "UnitHealthDisplay_Background";
        Color backgroundColor = new Color(0f, 0f, 0f, 0.85f);

        // Attempt to find an existing container. This allows updating if already present.
        Transform existingContainerTransform = go.transform.Find(TextContainerGOName);

        if (existingContainerTransform == null)
        {
            // Create the main container for text and background
            textContainerGO = new GameObject(TextContainerGOName);
            textContainerGO.transform.SetParent(go.transform);
            // Reset local position and rotation in case parent has non-identity transform
            textContainerGO.transform.localPosition = Vector3.zero;
            textContainerGO.transform.localRotation = Quaternion.identity;


            // Add TextMeshPro component to the container
            textMesh = textContainerGO.AddComponent<TextMeshPro>();

            // Create the background Quad
            backgroundQuadGO = GameObject.CreatePrimitive(PrimitiveType.Quad);
            backgroundQuadGO.name = BackgroundQuadName;
            backgroundQuadGO.transform.SetParent(textContainerGO.transform);
            // Remove the default collider from the Quad as it's not needed for display
            if (backgroundQuadGO.TryGetComponent<Collider>(out var collider))
            {
                UnityEngine.Object.Destroy(collider);
            }

            Renderer quadRenderer = backgroundQuadGO.GetComponent<Renderer>();
            // Use a simple unlit material for the background
            Material quadMaterial = new Material(Shader.Find("Sprites/Default"));
            quadMaterial.color = backgroundColor;
            quadRenderer.material = quadMaterial;
        }
        else
        {
            // If container already exists, get its components
            textContainerGO = existingContainerTransform.gameObject;
            textMesh = textContainerGO.GetComponent<TextMeshPro>();
            // Ensure textMesh is not null if container exists but TMP was removed
            if (textMesh == null) textMesh = textContainerGO.AddComponent<TextMeshPro>();

            Transform quadChild = textContainerGO.transform.Find(BackgroundQuadName);
            if (quadChild != null)
            {
                backgroundQuadGO = quadChild.gameObject;
                // Optional: Ensure material is still correct if it could be changed elsewhere
                Renderer quadRenderer = backgroundQuadGO.GetComponent<Renderer>();
                if (quadRenderer.sharedMaterial == null || quadRenderer.sharedMaterial.shader.name != "Sprites/Default")
                {
                    Material quadMaterial = new Material(Shader.Find("Sprites/Default"));
                    quadMaterial.color = backgroundColor;
                    quadRenderer.material = quadMaterial;
                }
            }
            else
            {
                // Fallback: Recreate background quad if it's missing from an existing container
                backgroundQuadGO = GameObject.CreatePrimitive(PrimitiveType.Quad);
                backgroundQuadGO.name = BackgroundQuadName;
                backgroundQuadGO.transform.SetParent(textContainerGO.transform);
                if (backgroundQuadGO.TryGetComponent<Collider>(out var collider)) UnityEngine.Object.Destroy(collider);
                Renderer quadRenderer = backgroundQuadGO.GetComponent<Renderer>();
                Material quadMaterial = new Material(Shader.Find("Sprites/Default"));
                quadMaterial.color = backgroundColor;
                quadRenderer.material = quadMaterial;
            }
        }

        // Configure TextMeshPro properties
        textMesh.horizontalAlignment = HorizontalAlignmentOptions.Center;
        textMesh.verticalAlignment = VerticalAlignmentOptions.Middle;
        textMesh.fontSize = fontSize;
        textMesh.text = healthbar;
        textMesh.color = color ?? Palette.Instance.text_white;
        textMesh.outlineWidth = 0f; // Outline not needed with a background
        textMesh.characterSpacing = 14f;
        // The TextMeshPro text will render at the pivot of its GameObject (textContainerGO)
        // So, no localPosition adjustment needed for textMesh itself relative to textContainerGO.

        // Configure Background Quad
        // Position the Quad slightly behind the text (relative to textContainerGO)
        backgroundQuadGO.transform.localPosition = new Vector3(0, 0, 0.01f); // Small Z offset

        // Auto-size quad based on text's preferred dimensions
        textMesh.ForceMeshUpdate(); // Ensure preferredWidth/Height are up-to-date
        float textWidth = textMesh.preferredWidth;
        float textHeight = textMesh.preferredHeight;

        // Define padding for the background (adjust as needed)
        float horizontalPadding = textMesh.fontSize * 0.05f;
        float verticalPadding = textMesh.fontSize * 0.05f;

        backgroundQuadGO.transform.localScale =
            new Vector3(textWidth + horizontalPadding, textHeight + verticalPadding, 1f);

        // Position and billboard the main container (which holds text and quad)
        textContainerGO.transform.localPosition = new Vector3(0f, hoverHeight, 0f);
        // Billboard: Make the container (and its children) face the camera
        // This uses the AlignTextMeshCamera logic but applies it to the container
        AlignGameObjectToCamera(textContainerGO, camera);
        AlignGameObjectToCamera(backgroundQuadGO, camera);
    }


    public static void AlignTextMeshCamera(this TextMeshPro textMesh, Camera camera)
    {
        // This method can still be used if direct TextMeshPro billboarding is needed elsewhere
        textMesh.transform.LookAt(textMesh.transform.position + camera.transform.rotation * Vector3.forward,
            camera.transform.rotation * Vector3.up);
    }

    // Helper method to billboard any GameObject
    private static void AlignGameObjectToCamera(GameObject objToAlign, Camera camera)
    {
        if (objToAlign == null || camera == null) return;
        // objToAlign.transform.LookAt(objToAlign.transform.position + camera.transform.rotation * Vector3.forward,
        //                            camera.transform.rotation * Vector3.up);  
        objToAlign.transform.rotation = camera.transform.rotation;
    }

    public static void HideUnitHealth(this GameObject go)
    {
        TextMeshPro tmp = go.GetComponentInChildren<TextMeshPro>();
        if (tmp == null) return;
        const string TextContainerGOName = "UnitHealthDisplay_Container";
        const string quadName = "QuadCont";
        const string otherContainerName = "MoveCont";
        Transform otherContainer = go.transform.Find(otherContainerName);
        Transform quadContainer = go.transform.Find(quadName);
        Transform existingContainer = go.transform.Find(TextContainerGOName);
        //UnityEngine.Object.Destroy(existingContainer.gameObject);
        if (existingContainer != null)
        {
            UnityEngine.Object.Destroy(existingContainer.gameObject);
        }
        if (otherContainer != null)
        {
            UnityEngine.Object.Destroy(otherContainer.gameObject);
        }
        if (quadContainer != null)
        {
            UnityEngine.Object.Destroy(quadContainer.gameObject);
        }
        int counter = 0;
        while (tmp != null)
        {
            UnityEngine.Object.Destroy(tmp.gameObject);
            UnityEngine.Object.Destroy(tmp);
            tmp = go.GetComponentInChildren<TextMeshPro>();
            counter++;
            if (counter >= 10) break;
        }
    }

    public static void HideText(this GameObject go)
    {
        HideUnitHealth(go);
    }

    public static void DisplayTextAboveObject(this GameObject go, Camera camera, string text, float hoverHeight_ = 0f,
        String containerName = "", String quadName = "")
    {
        Logger.Info("Now displaying text above target");
        if (hoverHeight_ == 0f)
        {
            hoverHeight_ = hoverHeight;
        }

        GameObject textContainerGO;
        TextMeshPro textMesh;
        GameObject backgroundQuadGO;

        string TextContainerGOName = "UnitHealthDisplay_Container";
        string BackgroundQuadName = "UnitHealthDisplay_Background";
        if(containerName != "") 
        {
            TextContainerGOName = containerName;
        }

        if (quadName != "")
        {
            BackgroundQuadName = quadName;
        }

        Color backgroundColor = new Color(0f, 0f, 0f, 0.85f);

        // Attempt to find an existing container. This allows updating if already present.
        Transform existingContainerTransform = go.transform.Find(TextContainerGOName);

        if (existingContainerTransform == null)
        {
            // Create the main container for text and background
            textContainerGO = new GameObject(TextContainerGOName);
            textContainerGO.transform.SetParent(go.transform);
            // Reset local position and rotation in case parent has non-identity transform
            textContainerGO.transform.localPosition = Vector3.zero;
            textContainerGO.transform.localRotation = Quaternion.identity;


            // Add TextMeshPro component to the container
            textMesh = textContainerGO.AddComponent<TextMeshPro>();

            // Create the background Quad
            backgroundQuadGO = GameObject.CreatePrimitive(PrimitiveType.Quad);
            backgroundQuadGO.name = BackgroundQuadName;
            backgroundQuadGO.transform.SetParent(textContainerGO.transform);
            // Remove the default collider from the Quad as it's not needed for display
            if (backgroundQuadGO.TryGetComponent<Collider>(out var collider))
            {
                UnityEngine.Object.Destroy(collider);
            }

            Renderer quadRenderer = backgroundQuadGO.GetComponent<Renderer>();
            // Use a simple unlit material for the background
            Material quadMaterial = new Material(Shader.Find("Sprites/Default"));
            quadMaterial.color = backgroundColor;
            quadRenderer.material = quadMaterial;
        }
        else
        {
            // If container already exists, get its components
            textContainerGO = existingContainerTransform.gameObject;
            textMesh = textContainerGO.GetComponent<TextMeshPro>();
            // Ensure textMesh is not null if container exists but TMP was removed
            if (textMesh == null) textMesh = textContainerGO.AddComponent<TextMeshPro>();

            Transform quadChild = textContainerGO.transform.Find(BackgroundQuadName);
            if (quadChild != null)
            {
                backgroundQuadGO = quadChild.gameObject;
                // Optional: Ensure material is still correct if it could be changed elsewhere
                Renderer quadRenderer = backgroundQuadGO.GetComponent<Renderer>();
                if (quadRenderer.sharedMaterial == null || quadRenderer.sharedMaterial.shader.name != "Sprites/Default")
                {
                    Material quadMaterial = new Material(Shader.Find("Sprites/Default"));
                    quadMaterial.color = backgroundColor;
                    quadRenderer.material = quadMaterial;
                }
            }
            else
            {
                // Fallback: Recreate background quad if it's missing from an existing container
                backgroundQuadGO = GameObject.CreatePrimitive(PrimitiveType.Quad);
                backgroundQuadGO.name = BackgroundQuadName;
                backgroundQuadGO.transform.SetParent(textContainerGO.transform);
                if (backgroundQuadGO.TryGetComponent<Collider>(out var collider)) UnityEngine.Object.Destroy(collider);
                Renderer quadRenderer = backgroundQuadGO.GetComponent<Renderer>();
                Material quadMaterial = new Material(Shader.Find("Sprites/Default"));
                quadMaterial.color = backgroundColor;
                quadRenderer.material = quadMaterial;
            }
        }

        // Configure TextMeshPro properties
        textMesh.horizontalAlignment = HorizontalAlignmentOptions.Center;
        textMesh.verticalAlignment = VerticalAlignmentOptions.Middle;
        textMesh.fontSize = fontSize;
        textMesh.text = text;
        textMesh.color = Color.white;
        textMesh.outlineWidth = 0f; // Outline not needed with a background
        textMesh.characterSpacing = 14f;
        // The TextMeshPro text will render at the pivot of its GameObject (textContainerGO)
        // So, no localPosition adjustment needed for textMesh itself relative to textContainerGO.

        // Configure Background Quad
        // Position the Quad slightly behind the text (relative to textContainerGO)
        backgroundQuadGO.transform.localPosition = new Vector3(0, 0, 0.01f); // Small Z offset

        // Auto-size quad based on text's preferred dimensions
        textMesh.ForceMeshUpdate(); // Ensure preferredWidth/Height are up-to-date
        float textWidth = textMesh.preferredWidth;
        float textHeight = textMesh.preferredHeight;

        // Define padding for the background (adjust as needed)
        float horizontalPadding = textMesh.fontSize * 0.05f;
        float verticalPadding = textMesh.fontSize * 0.05f;

        backgroundQuadGO.transform.localScale =
            new Vector3(textWidth + horizontalPadding, textHeight + verticalPadding, 1f);

        // Position and billboard the main container (which holds text and quad)
        textContainerGO.transform.localPosition = new Vector3(0f, hoverHeight_, 0f);
        // Billboard: Make the container (and its children) face the camera
        // This uses the AlignTextMeshCamera logic but applies it to the container
        AlignGameObjectToCamera(textContainerGO, camera);
        AlignGameObjectToCamera(backgroundQuadGO, camera);

    }

    public static void DisplayColoredText(this GameObject go, Camera camera, string text, Color color)
    {
        go.DisplayTextAboveObject(camera, text);
        TextMeshPro textMesh = go.GetComponentInChildren<TextMeshPro>();
        textMesh.color = color;
    }

    public static void DisplayMovementRemainingForObject(this GameObject go, Camera camera, Army army = null)
    {
        Unit unit = null;
        if (go.GetComponent<UnitDataHolder>() != null)
        {
            unit = go.GetComponent<UnitDataHolder>().unitData;
        }
        else if (army == null)
        {
            Logger.Warning("Neither unit nor army was set; cannot display movement");
            return;
        }

        int ap;
        if (unit != null)
        {
            ap = unit.movePoints;
        }
        else if (army != null)
        {
            ap = 2;
            foreach (Unit u in army.units)
            {
                if (u.movePoints < ap)
                {
                    ap = u.movePoints;
                }
            }
        }
        else
        {
            Logger.Error("Neither army nor unit was found");
            return;
        }

        string text = "";
        for (int i = 0; i < ap; i++)
        {
            text += movementRemains;
        }

        int maxAp = 2;
        for (int i = ap; i < maxAp; i++)
        {
            text += movementUsed;
        }

        string containerName = "MoveCont";
        string quadName = "QuadCont";

        go.DisplayTextAboveObject(camera, text, -0.4f, containerName, quadName);

        Transform createdContainer = go.transform.Find(containerName);
        Transform bgQuad = createdContainer.transform.Find(quadName);
        

        if (bgQuad == null && createdContainer == null)
        {
            Logger.Error("One of the containers for movement was not properly created.");
            return;
        }
        TextMeshPro textMesh = createdContainer.GetComponentInChildren<TextMeshPro>();

        if (textMesh == null)
        {
            Logger.Error("Text mesh not found.");
            return;
        }
        textMesh.fontSize = fontSize - 0.8f;

        float distance = 0f;
        float heightRed = 0f;
        if (unit != null)
        {
            distance = 0.7f;
            heightRed = 0.2f;
        }
        else if (army != null)
        {
            distance = 0.7f;
            heightRed = 0.3f;
        }
        createdContainer.Translate(0, -heightRed, -distance, Space.Self);
        textMesh.ForceMeshUpdate(); // Ensure preferredWidth/Height are up-to-date
        float textWidth = textMesh.preferredWidth;
        float textHeight = textMesh.preferredHeight;

        // Define padding for the background (adjust as needed)
        float horizontalPadding = textMesh.fontSize * 0.01f;
        float verticalPadding = -textMesh.fontSize * 0.005f;
        //createdContainer.transform.localScale = new Vector3(textWidth + horizontalPadding, textHeight + verticalPadding, 1f);
        if (bgQuad == null) return;
        bgQuad.transform.localScale = new Vector3(textWidth + horizontalPadding, textHeight + verticalPadding, 1f);
        AlignGameObjectToCamera(createdContainer.gameObject, camera);
        AlignGameObjectToCamera(bgQuad.gameObject, camera);
    }
}
