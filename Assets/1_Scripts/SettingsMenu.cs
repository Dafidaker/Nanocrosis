using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    public static SettingsMenu Instance;
    
    [field: SerializeField] private TextMeshProUGUI mouseSentivityXValue;
    [field: SerializeField] private TextMeshProUGUI mouseSentivityYValue;

    [field: SerializeField] private TMP_Dropdown dashDirection;
    [field: SerializeField] private Toggle sprintToggle;
    
    [field: SerializeField] private Slider mouseX;
    [field: SerializeField] private Slider mouseY;
    
    [field: SerializeField] private Slider masterSound;
    [field: SerializeField] private Slider musicSound;
    [field: SerializeField] private Slider effectsSound;
    
    public void Awake()
    {
        Instance = this;
        masterSound.onValueChanged.AddListener(SetMasterVolume);
        musicSound.onValueChanged.AddListener(SetMusicVolume);
        effectsSound.onValueChanged.AddListener(SetEffectsVolume);
    }

    public void UpdateAllSettings()
    {
        var dashValue = dashDirection.value;
        Settings.DashMovementDirection = dashValue switch
        {
            1 => false, // camera direction
            0 => true, // movement direction
            _ => Settings.DashMovementDirection
        };

        var toggleValue = sprintToggle.isOn;
        Settings.IsSprintToggle = toggleValue;

        Settings.MouseSentivity = new Vector2((float)Math.Round(mouseX.value, 2), (float)Math.Round(mouseY.value, 2));
        Settings.ChangeSentivity();
    }
    
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

    private void SetMasterVolume(float value)
    {
        AudioManager.Instance.SetMasterVolume(value);
    }
    
    private void SetMusicVolume(float value)
    {
        AudioManager.Instance.SetMusicVolume(value);
    }
    
    private void SetEffectsVolume(float value)
    {
        AudioManager.Instance.SetSFXVolume(value);
    }
    
}
