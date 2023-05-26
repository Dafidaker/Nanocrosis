using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;
using Image = UnityEngine.UI.Image;

public class PauseMenuController : MonoBehaviour
{
    public static PauseMenuController Instance;

    [SerializeField] private GameObject PauseMenu;
    [SerializeField] private GameObject SettingsMenuVisuals;
    [SerializeField] private GameObject AreYouSureWindow;
    [SerializeField] private GameObject AliveUI;
    [SerializeField] private GameObject WinScreen;
    [SerializeField] private GameObject LoseScreen;
    
    [SerializeField] private TextMeshProUGUI LungsHealth;
    [SerializeField] private TextMeshProUGUI PhageHealth;
    [SerializeField] private TextMeshProUGUI TimeUi;

    [SerializeField] private Image image;
    
    [SerializeField] private Image hitmaker;
    private bool _hitmakerCourotineGoing;
    [SerializeField] private float hitmakerTimer;
    
    private Controls _playerControls;
    private InputAction _menu;
    private float _v;

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

    private void Update()
    {
        TimeUi.text = TimeSpan.FromSeconds(GameManager.Instance.seconds).ToString(@"mm\:ss");
    }

    private void PauseGame(InputAction.CallbackContext context)
    {
        if(GameManager.Instance.isGameOver) return;
        
        GameManager.Instance.gamePaused = !GameManager.Instance.gamePaused;

        if (GameManager.Instance.gamePaused) 
            ActivateMenu();
        else
            DeactivateMenu();
        
    }

    private void ActivateMenu()
    {
        Debug.Log("Pause game");
        Cursor.lockState = CursorLockMode.Confined;
        GameManager.Instance.gamePaused = true;
        GameManager.Instance.playerController.DisableInputSystem();
        PauseMenu.SetActive(true);
        AliveUI.SetActive(false);
        Time.timeScale = 0;
    }

    public void DeactivateMenu()
    {
        Debug.Log("Unpause game");
        GameManager.Instance.gamePaused = false;
        Cursor.lockState = CursorLockMode.Locked;
        GameManager.Instance.playerController.EnableInputSystem();
        PauseMenu.SetActive(false);
        SettingsMenuVisuals.SetActive(false);
        AreYouSureWindow.SetActive(false);
        AliveUI.SetActive(true);
        AreYouSureWindow.SetActive(false);
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
        SettingsMenuVisuals.SetActive(true);
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

    public void UpdateLungsUi(Component sender, object data)
    {
        LungsHealth.text =  ObjectiveManager.Instance.GetCurrentValue(true, true).ToString(CultureInfo.CurrentCulture)+ "%"; // Math.Round(ObjectiveManager.Instance.GetPercentageOfCurrentValue() * 100, 2).ToString(CultureInfo.CurrentCulture)+ "%";
    }
    
    
    public void UpdatePhageUi(Component sender, object data)
    {
        PhageHealth.text = (GameManager.Instance.phage.GetCurrentHealthPercentage() * 100).ToString(CultureInfo.CurrentCulture) + "%";
    }
    
    public void CallHitmaker()
    {
        if (!_hitmakerCourotineGoing) StartCoroutine(Hitmaker());
    }
    private IEnumerator Hitmaker()
    {   
        _hitmakerCourotineGoing = true;
        hitmaker.enabled = true;
        yield return new WaitForSeconds(hitmakerTimer);
        hitmaker.enabled = false;
        _hitmakerCourotineGoing = false;
    }
    
    public void Reload(float duration)
    {
        image.enabled = true;
        _v = image.fillAmount;
        image.fillAmount = _v;
        StartCoroutine( Decrease( _v, duration, OnValueChanged )) ;
    }
     
    private void OnValueChanged( float value )
    {
        image.fillAmount = value;
    }
     
    private IEnumerator Decrease( float value, float duration, Action<float> onValueChange )
    {
        if( value < Mathf.Epsilon || duration < Mathf.Epsilon )
        {
            onValueChange( 0 );
            yield break;
        }
        
        var delta = value / duration;
         
        for( float t = 0 ; t < duration ; t += Time.deltaTime )
        {
            yield return null ;
            value -= Time.deltaTime * delta ;
            value = value < 0 ? 0 : value ;
            
            onValueChange?.Invoke( value ) ;
        }
        
        image.enabled = false;
        image.fillAmount = 1;
    }
    
    
    
    
}
