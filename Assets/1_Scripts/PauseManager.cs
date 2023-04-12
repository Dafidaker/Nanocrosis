using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseManager : MonoBehaviour
{
    Controls action;

    public static bool paused = false;

    public GameObject PauseMenu;

    private void Awake()
    {
        action = new Controls();
    }

    private void OnEnable()
    {
        action.Enable();
    }

    private void OnDisable()
    {
        action.Disable();
    }

    private void Start()
    {
        action.Pause.PauseGame.performed += _ => DeterminePause();
    }

    private void DeterminePause()
    {
        if(paused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;
        AudioListener.pause = true;
        paused = true;
        PauseMenu.SetActive(true);
        GameManager.Instance.player.GetComponent<Afonso_PlayerController>().DisableInputSystem();
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        paused = false;
        PauseMenu.SetActive(false);
        GameManager.Instance.player.GetComponent<Afonso_PlayerController>().EnableInputSystem();
    }
}
