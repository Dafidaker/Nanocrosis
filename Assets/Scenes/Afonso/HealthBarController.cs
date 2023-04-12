using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarController : MonoBehaviour
{
    [SerializeField] private GameObject Player;
    [SerializeField] private Slider Slider;
    [SerializeField] private Gradient Gradient;
    [SerializeField] private Image Fill;
    private PlayerStats _playerStats;

    private void Start()
    {
        _playerStats = Player.GetComponent<PlayerStats>();
        Slider.maxValue = _playerStats.MaxHealth;
        Slider.value = _playerStats.MaxHealth;
        Fill.color = Gradient.Evaluate(1f);
    }
    
    public void SetHealth()
    {
        Slider.value = _playerStats.CurrentHealth;
        Fill.color = Gradient.Evaluate(Slider.normalizedValue);
    }
}
