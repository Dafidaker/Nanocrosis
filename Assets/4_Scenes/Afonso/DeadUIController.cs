using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DeadUIController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI Counter;
    [SerializeField] private TextMeshProUGUI Title;
    private PlayerController _player;
    private float _timeToRepair;

    private void Start()
    {
        _player = GameManager.Instance.playerController;
    }

    private void OnEnable()
    {
        _player = GameManager.Instance.playerController;
        _timeToRepair = _player.TimeToRepair;
        Counter.SetText(_timeToRepair.ToString());
        StartCoroutine(Flash());
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
