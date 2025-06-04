using UnityEngine;
using System;

namespace PaletteNamespace{
    [CreateAssetMenu(fileName = "ColorPalette", menuName = "Game/Color Palette")]
    public class Palette : ScriptableObject
    {
        [Header("Player & Enemy Colors")]
        public Color playerColor = new(0.44f, 0.61f, 0.98f); // #719BFB
        public Color enemyColor = new(0.98f, 0.44f, 0.61f); // #FE729A
        public Color defaultColor = new(1f, 1f, 1f); // #FFFFFF
        public Color selectedColor = new(0.94f, 0.93f, 0.45f); //rgb(240, 238, 151)

        [Header("Tile Highlighting")]
        public Color tile_highlight_move = new(0.3f, 0.7f, 0.8f); //rgb(48, 112, 128)
        public Color tile_highlight_attack = new(0.0f, 0.7f, 0.8f); //rgb(0, 112, 128)
        public Color tile_highlight_predicted_move = new(0.3f, 0.5f, 0.8f); //rgb(48, 80, 128)
        public Color tile_highlight_predicted_attack = new(0.0f, 0.5f, 0.8f); //rgb(0, 80, 128)

        [Header("Spawn Points")]
        public Color tile_highlight_spawn_point_player = new(0.6f, 0.7f, 0.8f); //rgb(96, 112, 128)
        public Color tile_highlight_spawn_point_enemy = new(0.0f, 0.7f, 0.8f); //rgb(0, 112, 128)
        
        [Header("Environment")]
        public Color tile_highlight_water = new(0.56f, 0.85f, 0.98f); //rgb(94, 185, 238)
        public Color overworld_tile = new(0.56f, 0.85f, 0.98f); //rgb(94, 238, 178)

        [Header("Text Colors")]
        public Color text_white = new(1f, 1f, 1f); //rgb(255, 255, 255)
        public Color text_red = new(0.98f, 0.44f, 0.61f); //rgb(255, 112, 156)
        public Color text_disabled = new(0.5f, 0.5f, 0.5f); //rgb(128, 128, 128)

        [Header("Info Button Colors")]
        // A easily visible color for the info button
        public Color info_button_highlight = new(0.94f, 0.93f, 0.45f); //rgb(240, 238, 151)

        // A gray-ish disabled colour for the info button
        public Color info_button_disabled = new(0.5f, 0.5f, 0.5f); //rgb(128, 128, 128)

        // Event for when colors change
        public static event Action OnColorsChanged;

        // Static instance for easy access
        public void Initialize(){
            _instance = Resources.Load<Palette>("ColorPalette");
            if (_instance == null)
            {
                Debug.LogError("ColorPalette asset not found in Resources folder! Please create one using the Create menu.");
            }
        }
        private static Palette _instance;
        public static Palette Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<Palette>("ColorPalette");
                    if (_instance == null)
                    {
                        Debug.LogError("ColorPalette asset not found in Resources folder! Please create one using the Create menu.");
                    }
                }
                return _instance;
            }
        }

        // Keep your enum for easy reference
        public enum PaletteColours 
        {
            PlayerColor,
            EnemyColor,
            DefaultColor,
            SelectedColor,
            TileHighlightMove,
            TileHighlightAttack,
            TileHighlightPredictedMove,
            TileHighlightPredictedAttack,
            TileHighlightSpawnPointPlayer,
            TileHighlightSpawnPointEnemy,
            TileHighlightWater,
            OverworldTile,
            TextWhite,
            TextRed,
            TextDisabled,
            InfoButtonHighlight,
            InfoButtonDisabled
        }

        // Method to get color by enum
        public Color GetColor(PaletteColours colorType)
        {
            return colorType switch
            {
                PaletteColours.PlayerColor => playerColor,
                PaletteColours.EnemyColor => enemyColor,
                PaletteColours.DefaultColor => defaultColor,
                PaletteColours.SelectedColor => selectedColor,
                PaletteColours.TileHighlightMove => tile_highlight_move,
                PaletteColours.TileHighlightAttack => tile_highlight_attack,
                PaletteColours.TileHighlightPredictedMove => tile_highlight_predicted_move,
                PaletteColours.TileHighlightPredictedAttack => tile_highlight_predicted_attack,
                PaletteColours.TileHighlightSpawnPointPlayer => tile_highlight_spawn_point_player,
                PaletteColours.TileHighlightSpawnPointEnemy => tile_highlight_spawn_point_enemy,
                PaletteColours.TileHighlightWater => tile_highlight_water,
                PaletteColours.OverworldTile => overworld_tile,
                PaletteColours.TextWhite => text_white,
                PaletteColours.TextRed => text_red,
                PaletteColours.TextDisabled => text_disabled,
                PaletteColours.InfoButtonHighlight => info_button_highlight,
                PaletteColours.InfoButtonDisabled => info_button_disabled,
                _ => defaultColor
            };
        }

        // Method to programmatically change colors and notify listeners
        public void SetColor(PaletteColours colorType, Color newColor)
        {
            switch (colorType)
            {
                case PaletteColours.PlayerColor: playerColor = newColor; break;
                case PaletteColours.EnemyColor: enemyColor = newColor; break;
                case PaletteColours.DefaultColor: defaultColor = newColor; break;
                case PaletteColours.SelectedColor: selectedColor = newColor; break;
                case PaletteColours.TileHighlightMove: tile_highlight_move = newColor; break;
                case PaletteColours.TileHighlightAttack: tile_highlight_attack = newColor; break;
                case PaletteColours.TileHighlightPredictedMove: tile_highlight_predicted_move = newColor; break;
                case PaletteColours.TileHighlightPredictedAttack: tile_highlight_predicted_attack = newColor; break;
                case PaletteColours.TileHighlightSpawnPointPlayer: tile_highlight_spawn_point_player = newColor; break;
                case PaletteColours.TileHighlightSpawnPointEnemy: tile_highlight_spawn_point_enemy = newColor; break;
                case PaletteColours.TileHighlightWater: tile_highlight_water = newColor; break;
                case PaletteColours.OverworldTile: overworld_tile = newColor; break;
                case PaletteColours.TextWhite: text_white = newColor; break;
                case PaletteColours.TextRed: text_red = newColor; break;
                case PaletteColours.TextDisabled: text_disabled = newColor; break;
                case PaletteColours.InfoButtonHighlight: info_button_highlight = newColor; break;
                case PaletteColours.InfoButtonDisabled: info_button_disabled = newColor; break;
            }
            
            OnColorsChanged?.Invoke();
            
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            // This triggers when values change in the Inspector
            OnColorsChanged?.Invoke();
        }
#endif
    }
}
