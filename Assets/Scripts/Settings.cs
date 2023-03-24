using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Settings
{
    public static bool IsSprintToggle;
    public static bool DashMovementDirection;
    public static void GameStart()
    {
        IsSprintToggle = true;
        DashMovementDirection = false;
    }
}
