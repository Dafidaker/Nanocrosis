using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
        
    public void PlayGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1); // This needs the Main Menu scene to be added BEFORE any level scenes, which must then be added in order (if / as applicable)
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void PressedButton()
    {
        AudioManager.Instance.PlaySFX("ClickButton");
    }
    
}
