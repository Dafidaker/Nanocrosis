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
    private GameObject _player;
    private WeaponController _weaponController;
    private Color _enhancedColor;

    private void Start()
    {
        _player = GameManager.Instance.player.gameObject;
        _weaponController = _player.GetComponent<PlayerController>().CurrentWeapon.GetComponent<WeaponController>();

        _enhancedColor = new Color(1f, 0.34f, 0f);

        WeaponName.SetText(_weaponController.Name);
        Mag.SetText(_weaponController.MagSize.ToString());
        Reserve.SetText(_weaponController.AmmoReserve.ToString());

        BombText.SetActive(false);
    }

    public void SetValues()
    {
        _weaponController = _player.GetComponent<PlayerController>().CurrentWeapon.GetComponent<WeaponController>();
        WeaponName.SetText(_weaponController.Name);
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
        BombText.SetActive(true);
        GunStats.SetActive(false);
    }

    public void BombUnattached()
    {
        BombText.SetActive(false);
        GunStats.SetActive(true);
    }

    public void EnhancedWeapon()
    {
        Mag.color = _enhancedColor;
        WeaponName.color = _enhancedColor;
    }

    public void ResetEnhancedWeapon()
    {
        Mag.color = Color.white;
        WeaponName.color = Color.white;
    }
}