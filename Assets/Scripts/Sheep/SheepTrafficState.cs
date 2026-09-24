using System;
using UnityEngine;

public static class SheepTrafficState
{
    private static bool canMove = false;

    public static bool IsMovementAllowed => canMove;

    public static event Action<bool> OnStateChanged;

    public static void SetMovementAllowed(bool value)
    {
        if (canMove == value)
            return;

        canMove = value;
        OnStateChanged?.Invoke(canMove);
    }

    public static void Toggle()
    {
        SetMovementAllowed(!canMove);
    }
}
