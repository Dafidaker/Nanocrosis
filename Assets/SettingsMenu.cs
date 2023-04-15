using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;

public class SettingsMenu : MonoBehaviour
{
    [field: SerializeField]private TextMeshProUGUI mouseSentivityXValue;
    [field: SerializeField]private TextMeshProUGUI mouseSentivityYValue;
    
    public void ToggleSprint(bool value)
    {
        Settings.IsSprintToggle = value;
    }
    
    public void SprintDirection(int value)
    {
        Debug.Log(value);
        Settings.DashMovementDirection = value switch
        {
            1 => false, // camera direction
            0 => true, // movement direction
            _ => Settings.DashMovementDirection
        };
    }

    public void UpdateMouseXSentivity(float value)
    {
        value = (float)Math.Round(value, 2);
        mouseSentivityXValue.text = value.ToString(CultureInfo.CurrentCulture);
        Settings.MouseSentivity.x = value;
        Settings.ChangeSentivity();
    }
    
    public void UpdateMouseYSentivity(float value)
    {
        value = (float)Math.Round(value, 2);
        mouseSentivityYValue.text = value.ToString(CultureInfo.CurrentCulture);
        Settings.MouseSentivity.y = value;
        Settings.ChangeSentivity();
    }
}
