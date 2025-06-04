using UnityEngine;

namespace UINamespace
{
    public static class UIGuy
    {
        public enum BottomRightButtonType{
            EndTurn,
            ExitUnitSelection,
            ExitBattlePrep,
            StartBattle,
            SkipUnitTurn,
            Disabled,
            Invisible,
            Overworld_Ambush,
            Overworld_Leave_Ambush,
            Overworld_FallenTree,
            Select_All_Units,
            Deselect_All_Units,
        }
        private static CameraRotationButtons unifiedUIManager;

        // Initialize should be called once at game start
        public static void Initialize()
        {
            unifiedUIManager = GameObject.FindGameObjectWithTag("BottomRightButton").GetComponent<CameraRotationButtons>();
            if (unifiedUIManager == null)
            {
                Debug.LogError("Could not find CameraRotationButtons component on BottomRightButton tagged GameObject!");
            }
        }

        public static void SetBottomRightButtonType(BottomRightButtonType buttonType, int index)
        {
            if (unifiedUIManager == null)
            {
                Initialize();
            }
            unifiedUIManager.SetBottomRightButtonType(index, buttonType);
        }
    }
}