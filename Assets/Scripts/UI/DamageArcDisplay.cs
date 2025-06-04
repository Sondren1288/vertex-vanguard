using UnityEngine;
using TMPro;
using PaletteNamespace;

namespace VertexVanguard.UI
{
    public class DamageArcDisplay : MonoBehaviour
    {
        [Header("Arc Settings")]
        [SerializeField] private LineRenderer lineRenderer;
        [SerializeField] private Material glowMaterial;
        [SerializeField] private int arcResolution = 50;
        [SerializeField] private float arcHeight = 2f;
        [SerializeField] private float lineWidth = 0.1f;
        
        [Header("Animation")]
        
        private Transform sourceTransform;
        private Transform targetTransform;
        private bool isDisplaying = false;
        private bool attackerIsExhausted = false;
        private void Awake()
        {
            InitializeComponents();
        }
        
        private void Start()
        {
            Hide();
        }
        
        private void Update()
        {
            if (isDisplaying)
            {
                UpdateDisplay();
                
            }
        }
        
        private void InitializeComponents()
        {
            if (lineRenderer == null)
            {
                lineRenderer = GetComponent<LineRenderer>();
                if (lineRenderer == null)
                {
                    lineRenderer = gameObject.AddComponent<LineRenderer>();
                }
            }
            
            SetupLineRenderer();
        }
        
        private void SetupLineRenderer()
        {
            lineRenderer.material = glowMaterial;
            lineRenderer.startWidth = lineWidth;
            lineRenderer.endWidth = lineWidth;
            lineRenderer.positionCount = arcResolution;
            lineRenderer.useWorldSpace = true;
            lineRenderer.sortingOrder = 10;
        }
        
        public void ShowDamageArc(Transform source, Transform target, bool attackerIsExhausted = false)
        {
            if (source == null || target == null) return;
            
            sourceTransform = source;
            targetTransform = target;
            this.attackerIsExhausted = attackerIsExhausted;
            UpdateArcLine();
            
            isDisplaying = true;
            
            // Reset alpha to full when showing
            ResetAlpha();
            
            lineRenderer.enabled = true;
        }
        
        public void Hide()
        {
            isDisplaying = false;
            lineRenderer.enabled = false;
        }
        
        private void ResetAlpha()
        {
            // Reset line alpha
            if (lineRenderer != null && lineRenderer.material != null)
            {
                Color lineColor = lineRenderer.material.GetColor("_FillColor");
                lineColor.a = 1f;
                lineRenderer.material.SetColor("_FillColor", lineColor);
            }
        }
        
        private void UpdateDisplay()
        {
            if (sourceTransform != null && targetTransform != null)
            {
                UpdateArcLine();
            }
        }
        
        
        private void UpdateArcLine()
        {
            Vector3 startPos = sourceTransform.position;
            Vector3 endPos = targetTransform.position;
            
            // Calculate arc points
            for (int i = 0; i < arcResolution; i++)
            {
                float t = (float)i / (arcResolution - 1);
                Vector3 point = CalculateArcPoint(startPos, endPos, t);
                lineRenderer.SetPosition(i, point);
                
                Color reducedOpactiyColor = attackerIsExhausted ? Palette.Instance.text_disabled : Palette.Instance.text_red;
                reducedOpactiyColor.a = 0.3f;
                lineRenderer.material.SetColor("_FillColor", reducedOpactiyColor);
            }
        }
        
        private Vector3 CalculateArcPoint(Vector3 start, Vector3 end, float t)
        {
            // Create a parabolic arc
            Vector3 midPoint = Vector3.Lerp(start, end, t);
            
            // Add height based on arc curve
            float arcHeightMultiplier = 4f * t * (1f - t); // Parabolic curve
            midPoint.y += arcHeight * arcHeightMultiplier;
            
            return midPoint;
        }

        // Public methods for external control
        public void SetArcHeight(float height) => arcHeight = height;
        public void SetLineWidth(float width)
        {
            lineWidth = width;
            if (lineRenderer != null)
            {
                lineRenderer.startWidth = lineWidth;
                lineRenderer.endWidth = lineWidth;
            }
        }
        
        // Utility methods
        public bool IsDisplaying => isDisplaying;
    }
} 