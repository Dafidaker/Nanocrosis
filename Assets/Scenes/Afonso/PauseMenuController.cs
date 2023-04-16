using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PauseMenuController : MonoBehaviour
{
    public static PauseMenuController Instance;

    [SerializeField] private GameObject PauseMenu;
    [SerializeField] private GameObject SettingsMenu;
    [SerializeField] private GameObject AreYouSureWindow;
    [SerializeField] private GameObject AliveUI;
    [SerializeField] private GameObject WinScreen;
    [SerializeField] private GameObject LoseScreen;
    private Controls _playerControls;
    private InputAction _menu;

    public bool Paused;
    private void Awake()
    {
        Instance = this;

        _playerControls = new Controls();
    }

    private void OnEnable()
    {
        _menu = _playerControls.Pause.PauseGame;
        _menu.performed += PauseGame;
        _menu.Enable();
    }

    private void OnDisable()
    {
        _menu.Disable();
    }

    private void PauseGame(InputAction.CallbackContext context)
    {
        if(GameManager.Instance.isGameOver) return;
        
        GameManager.Instance.gamePaused = !GameManager.Instance.gamePaused;

        if (GameManager.Instance.gamePaused) 
                ActivateMenu();
        else
        {
            DeactivateMenu();
        }
    }

    private void ActivateMenu()
    {
        Debug.Log("ActivateMenu");
        Cursor.lockState = CursorLockMode.Confined;
        PauseMenu.SetActive(true);
        AliveUI.SetActive(false);
        Time.timeScale = 0;
    }

    public void DeactivateMenu()
    {
        Debug.Log("DeactivateMenu");
        GameManager.Instance.gamePaused = false;
        Cursor.lockState = CursorLockMode.Locked;
        PauseMenu.SetActive(false);
        SettingsMenu.SetActive(false);
        AreYouSureWindow.SetActive(false);
        AliveUI.SetActive(true);
        Time.timeScale = 1;
    }

    public void WonGame()
    {
        Cursor.lockState = CursorLockMode.Confined;
        AliveUI.SetActive(false);
        WinScreen.SetActive(true);
    }
    
    public void LostGame()
    {
        Cursor.lockState = CursorLockMode.Confined;
        AliveUI.SetActive(false);
        LoseScreen.SetActive(true);
    }

    public void OpenSetting()
    {
        Debug.Log("OpenSetting");
        PauseMenu.SetActive(false);
        SettingsMenu.SetActive(true);
    }

    public void QuitGame()
    {
        Debug.Log("QUIT GAME");
        //Load Main Menu scene
    }

    public void CloseGame()
    {
        Debug.Log("GAME CLOSED");
        Application.Quit();
    }
}
