using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PauseMenuController : MonoBehaviour
{
    public static PauseMenuController Instance;

    [SerializeField] private GameObject PauseMenu;
    [SerializeField] private GameObject AreYouSureWindow;
    [SerializeField] private GameObject AliveUI;
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
        GameManager.Instance.GamePaused = !GameManager.Instance.GamePaused;

        if (GameManager.Instance.GamePaused) ActivateMenu();
        else
        {
            DeactivateMenu();
        }
    }

    private void ActivateMenu()
    {
        Cursor.lockState = CursorLockMode.Confined;
        PauseMenu.SetActive(true);
        AliveUI.SetActive(false);
        Time.timeScale = 0;
    }

    public void DeactivateMenu()
    {
        GameManager.Instance.GamePaused = false;
        Cursor.lockState = CursorLockMode.Locked;
        PauseMenu.SetActive(false);
        AliveUI.SetActive(true);
        Time.timeScale = 1;
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
