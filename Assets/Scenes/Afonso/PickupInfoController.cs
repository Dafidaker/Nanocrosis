
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PickupInfoController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI AvailableBuffNum;
    [SerializeField] private PlayerController Player;

    private void Start()
    {
        Player = GameManager.Instance.player.gameObject.GetComponent<PlayerController>();
        AvailableBuffNum.SetText(Player.AvailableBuffs.ToString());
    }

    public void SetValue()
    {
        AvailableBuffNum.SetText(Player.AvailableBuffs.ToString());
    }

    public IEnumerator Flash()
    {
        AvailableBuffNum.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        AvailableBuffNum.color = Color.white;
        yield return new WaitForSeconds(0.1f);
        AvailableBuffNum.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        AvailableBuffNum.color = Color.white;
    }
}