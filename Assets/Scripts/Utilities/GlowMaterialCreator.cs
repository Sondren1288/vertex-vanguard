using UnityEngine;

namespace VertexVanguard.Utilities
{
    /// <summary>
    /// Utility class for creating glow materials for damage arcs
    /// This helps ensure consistent visual styling across your game
    /// </summary>
    public static class GlowMaterialCreator
    {
        /// <summary>
        /// Creates a basic glow material for damage arcs
        /// </summary>
        /// <param name="color">The base color of the glow</param>
        /// <param name="emission">The emission intensity</param>
        /// <returns>A material with glow properties</returns>
        public static Material CreateGlowMaterial(Color color, float emission = 2f)
        {
            Material glowMaterial = new Material(GetGlowShader());
            
            glowMaterial.SetColor("_Color", color);
            glowMaterial.SetColor("_EmissionColor", color * emission);
            glowMaterial.EnableKeyword("_EMISSION");
            
            // Make it render in front
            glowMaterial.renderQueue = 3000;
            
            return glowMaterial;
        }
        
        /// <summary>
        /// Creates a damage-specific glow material with predefined colors
        /// </summary>
        /// <param name="damageType">The type of damage for color coding</param>
        /// <returns>A material styled for the damage type</returns>
        public static Material CreateDamageGlowMaterial(DamageType damageType)
        {
            Color baseColor = GetDamageTypeColor(damageType);
            return CreateGlowMaterial(baseColor, 1.5f);
        }
        
        /// <summary>
        /// Creates an animated glow material that pulses
        /// </summary>
        /// <param name="color">Base color</param>
        /// <param name="pulseSpeed">Speed of the pulse animation</param>
        /// <returns>Material with pulsing glow effect</returns>
        public static Material CreatePulsingGlowMaterial(Color color, float pulseSpeed = 2f)
        {
            Material glowMaterial = CreateGlowMaterial(color);
            
            // Note: This would require a custom shader for full effect
            // For now, we'll create a basic version
            glowMaterial.SetFloat("_PulseSpeed", pulseSpeed);
            
            return glowMaterial;
        }
        
        private static Shader GetGlowShader()
        {
            // Try to find the Universal Render Pipeline shader first
            Shader urpShader = Shader.Find("Universal Render Pipeline/Unlit");
            if (urpShader != null) return urpShader;
            
            // Fall back to standard shader
            Shader standardShader = Shader.Find("Standard");
            if (standardShader != null) return standardShader;
            
            // Last resort - use sprites shader
            return Shader.Find("Sprites/Default");
        }
        
        private static Color GetDamageTypeColor(DamageType damageType)
        {
            switch (damageType)
            {
                case DamageType.Physical:
                    return new Color(1f, 0.3f, 0.3f); // Red
                case DamageType.Magical:
                    return new Color(0.3f, 0.3f, 1f); // Blue
                case DamageType.Fire:
                    return new Color(1f, 0.5f, 0f); // Orange
                case DamageType.Ice:
                    return new Color(0.5f, 0.8f, 1f); // Light Blue
                case DamageType.Poison:
                    return new Color(0.5f, 1f, 0.3f); // Green
                case DamageType.Healing:
                    return new Color(0.3f, 1f, 0.3f); // Bright Green
                default:
                    return Color.white;
            }
        }
    }
    
    /// <summary>
    /// Enum for different damage types to help with color coding
    /// </summary>
    public enum DamageType
    {
        Physical,
        Magical,
        Fire,
        Ice,
        Poison,
        Healing
    }
    
    /// <summary>
    /// ScriptableObject for creating and managing glow material presets
    /// </summary>
    [CreateAssetMenu(fileName = "GlowMaterialSettings", menuName = "VertexVanguard/Glow Material Settings")]
    public class GlowMaterialSettings : ScriptableObject
    {
        [Header("Default Colors")]
        public Color defaultArcColor = Color.cyan;
        public Color criticalHitColor = Color.red;
        public Color healingColor = Color.green;
        
        [Header("Emission Settings")]
        public float defaultEmission = 1.5f;
        public float criticalEmission = 2.5f;
        
        [Header("Animation")]
        public bool enablePulsing = true;
        public float pulseSpeed = 2f;
        public AnimationCurve pulseCurve = AnimationCurve.EaseInOut(0, 0.5f, 1, 1.5f);
        
        /// <summary>
        /// Gets a material for normal damage
        /// </summary>
        public Material GetNormalDamageMaterial()
        {
            return GlowMaterialCreator.CreateGlowMaterial(defaultArcColor, defaultEmission);
        }
        
        /// <summary>
        /// Gets a material for critical hit damage
        /// </summary>
        public Material GetCriticalDamageMaterial()
        {
            return GlowMaterialCreator.CreateGlowMaterial(criticalHitColor, criticalEmission);
        }
        
        /// <summary>
        /// Gets a material for healing effects
        /// </summary>
        public Material GetHealingMaterial()
        {
            return GlowMaterialCreator.CreateGlowMaterial(healingColor, defaultEmission);
        }
    }
} 