using UnityEngine;
using System;
using UINamespace;
using CameraNamespace;

public struct EmptyEventArgs {}
public static class GameEvents{
    /* Overworld */
    public static readonly Event<string> ClearingSelected = new();
    public static readonly Event<Vector3> ArmyCubeSelected = new();
    public static readonly Event<Vector3> IndividualUnitSelected = new();
    public static readonly Event<ClickableArrow> ArrowInvoked = new();
    public static readonly Event<Ownership> AllArmiesDefeated = new();
    public static readonly Event<Army> LeaveAmbush = new();
    
    /* Battle Prep */

    /* Both battle and prep */
    public static readonly Event<Vector3> DeployedUnitClicked = new();
    public static readonly Event<Vector3> TileClicked = new();
    public static readonly Event<EmptyEventArgs> BattleStart = new();
    public static readonly Event<UINamespace.UIGuy.BottomRightButtonType> BottomRightButtonClicked = new();

    /* Camera */
    public static readonly Event<Direction> RotateCameraClicked = new();

    /* Main Menu */
    public static readonly Event<EmptyEventArgs> GoToMainMenu = new();


    /* Clear all event listeners */
    public static void ResetAllEvents()
    {
        ClearingSelected.ClearListeners();
        ArmyCubeSelected.ClearListeners();
        IndividualUnitSelected.ClearListeners();
        ArrowInvoked.ClearListeners();
        AllArmiesDefeated.ClearListeners();
        DeployedUnitClicked.ClearListeners();
        TileClicked.ClearListeners();
        BattleStart.ClearListeners();
        BottomRightButtonClicked.ClearListeners();
        LeaveAmbush.ClearListeners();
        ArrowInvoked.ClearListeners();
        RotateCameraClicked.ClearListeners();
        GoToMainMenu.ClearListeners();
        Logger.Success("All game events have been reset");
    }
}