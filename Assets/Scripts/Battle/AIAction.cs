using UnityEngine;

public enum AIActionType
{
    Move,
    Attack
}

public class AIAction
{
    public Unit ActingUnit { get; private set; }
    public Vector3 StartPosition { get; private set; }
    public Vector3 TargetPosition { get; private set; }
    public AIActionType ActionType { get; private set; }
    public float Score { get; private set; }

    public AIAction(Unit actingUnit, Vector3 startPosition, Vector3 targetPosition, AIActionType actionType, float score)
    {
        ActingUnit = actingUnit;
        StartPosition = startPosition;
        TargetPosition = targetPosition;
        ActionType = actionType;
        Score = score;
    }
} 