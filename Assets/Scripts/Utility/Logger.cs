using UnityEngine;
using System.Runtime.CompilerServices;
using static BattleGrid;

public static class Logger
{
    private static readonly string INFO_COLOR = "#87CEEB";    // Sky blue
    private static readonly string SUCCESS_COLOR = "#90EE90"; // Light green
    private static readonly string WARNING_COLOR = "#FFD700"; // Gold
    private static readonly string ERROR_COLOR = "#FF6B6B";   // Light red
    private static readonly string DEBUG_COLOR = "#0c0d0e";   // Black

    public static void Info(string message,
        [CallerFilePath] string sourceFilePath = "",
        [CallerMemberName] string memberName = "")
    {
        Log(message, "info", INFO_COLOR, sourceFilePath, memberName);
    }

    public static void Success(string message,
        [CallerFilePath] string sourceFilePath = "",
        [CallerMemberName] string memberName = "")
    {
        Log(message, "success", SUCCESS_COLOR, sourceFilePath, memberName);
    }

    public static void Warning(string message,
        [CallerFilePath] string sourceFilePath = "",
        [CallerMemberName] string memberName = "")
    {
        Log(message, "warning", WARNING_COLOR, sourceFilePath, memberName);
    }

    public static void Error(string message,
        [CallerFilePath] string sourceFilePath = "",
        [CallerMemberName] string memberName = "")
    {
        Log(message, "error", ERROR_COLOR, sourceFilePath, memberName);
    }

    public static void Emphasize(string message,
        [CallerFilePath] string sourceFilePath = "",
        [CallerMemberName] string memberName = "")
    {
        Log(message, "debug", DEBUG_COLOR, sourceFilePath, memberName);
    }

    public static void TileRangeInfo(TileRangeInfo tileRangeInfo){
        string message = "Tile range info: ";
        int i = 0;
        foreach(float movementCost in tileRangeInfo.movementCosts){
            message += "Tile: " + tileRangeInfo.tiles[i].position + " Movement cost: " + movementCost + " ";
            i++;
        }
        Info(message);
    }

    private static void Log(string message, string type, string color,
        string sourceFilePath, string memberName)
    {
        string className = System.IO.Path.GetFileNameWithoutExtension(sourceFilePath);
        string coloredClassName = $"<color={color}>{className}</color>";
        Debug.Log($"{coloredClassName} -> {message}");
    }
}
