using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Settings
{
    public static bool IsSprintToggle;
    public static bool DashMovementDirection;
    public static float MouseSentivityX;
    public static float MouseSentivityY;
    public static Vector2 MouseSentivity;
    public static void GameStart()
    {
        IsSprintToggle = true;
        DashMovementDirection = false;
    }

    public static void ChangeSentivity()
    {
        if (GameEvents.Instance != null)
        {
            GameEvents.Instance.mouseSentivityChanged.Ping(null, MouseSentivity);
        }
    }
    
}
