using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WeaponInfoController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI WeaponName;
    [SerializeField] private TextMeshProUGUI Mag;
    [SerializeField] private TextMeshProUGUI Reserve;
    [SerializeField] private GameObject GunStats;
    [SerializeField] private GameObject BombText;

    [SerializeField]private RectTransform rifle;
    [SerializeField]private RectTransform shotgun;
    [SerializeField]private GameObject bomb;
    
    private PlayerController _player;
    private WeaponController _weaponController;
    private Color _enhancedColor;

    private void Start()
    {
        _player = GameManager.Instance.playerController;
        _weaponController = _player.CurrentWeapon.GetComponent<WeaponController>();

        _enhancedColor = new Color(1f, 0.34f, 0f);

        //WeaponName.SetText(_weaponController.Name);
        Mag.SetText(_weaponController.MagSize.ToString());
        Reserve.SetText(_weaponController.AmmoReserve.ToString());

        bomb.SetActive(false);
        //BombText.SetActive(false);
    }
    
    public void SetValues()
    {
        if (_player.BombAttached)
        {
            return;
        }
        _player = GameManager.Instance.playerController;
        _weaponController = _player.CurrentWeapon.GetComponent<WeaponController>();
        if (_weaponController.Name == "Rifle")
        {
            rifle.localScale = new Vector2(1f, 1f);
            shotgun.localScale = new Vector2(0.5f, 0.5f);
        }
        else
        {
            shotgun.localScale = new Vector2(1f, 1f);
            rifle.localScale = new Vector2(0.5f, 0.5f);
        }
        
        //WeaponName.SetText(_weaponController.Name);
        Mag.SetText(_weaponController.CurrentMag.ToString());
        Reserve.SetText(_weaponController.CurrentAmmoReserve.ToString());
    }

    public void LowAmmo()
    {
        Mag.color = Color.red;
    }

    public void ResetMagColor()
    {
        Mag.color = Color.white;
    }

    public void LowReserve()
    {
        Reserve.color = Color.red;
    }

    public void ResetReserveColor()
    {
        Reserve.color = Color.white;
    }

    public IEnumerator FlashMag()
    {
        Mag.color = Color.white;
        yield return new WaitForSeconds(0.1f);
        Mag.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        Mag.color = Color.white;
        yield return new WaitForSeconds(0.1f);
        Mag.color = Color.red;
    }

    public IEnumerator FlashReserve()
    {
        Reserve.color = Color.white;
        yield return new WaitForSeconds(0.1f);
        Reserve.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        Reserve.color = Color.white;
        yield return new WaitForSeconds(0.1f);
        Reserve.color = Color.red;
    }

    public void BombAttached()
    {
        bomb.SetActive(true);
        rifle.localScale = new Vector2(0.5f, 0.5f);
        shotgun.localScale = new Vector2(0.5f, 0.5f);
        /*BombText.SetActive(true);
        GunStats.SetActive(false);*/
    }

    public void BombUnattached()
    {
        bomb.SetActive(false);
        
        SetValues();
        
        /*BombText.SetActive(false);
        GunStats.SetActive(true);*/
    }

    public void EnhancedWeapon()
    {
        Mag.color = _enhancedColor;
        //WeaponName.color = _enhancedColor;
    }

    public void ResetEnhancedWeapon()
    {
        Mag.color = Color.white;
        //WeaponName.color = Color.white;
    }
}