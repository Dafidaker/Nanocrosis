using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DeadUIController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI Counter;
    [SerializeField] private TextMeshProUGUI Title;
    [SerializeField] private PlayerController Player;
    private float _timeToRepair;

    private void OnEnable()
    {
        Player = GameManager.Instance.player.gameObject.GetComponent<PlayerController>();
        _timeToRepair = Player.TimeToRepair;
        Counter.SetText(_timeToRepair.ToString());
        StartCoroutine(Flash());
        Debug.Log("Time to respawn: " + _timeToRepair);
    }

    private void Update()
    {
        _timeToRepair -= Time.deltaTime;
        int roundedTime = Mathf.RoundToInt(_timeToRepair);
        Counter.SetText(roundedTime.ToString());
        if (roundedTime <= 3) Counter.color = Color.red;
    }

    private IEnumerator Flash()
    {
        while (true)
        {
            Title.color = Color.red;
            yield return new WaitForSeconds(0.5f);
            Title.color = Color.white;
            yield return new WaitForSeconds(0.5f);
        }
    }
}
