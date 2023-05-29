using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NanobotLightController : MonoBehaviour
{
    private PlayerStats _playerStats;
    [SerializeField] private Renderer sphere;
    [SerializeField] private List<Light> lights;
    void Start()
    {
        _playerStats = GameManager.Instance.player.GetComponent<PlayerStats>();
        
    }

    public void UpdateColor(Color color)
    {
        
        sphere.material.color = color;
        foreach (var light in lights)
        {
            light.color = color;
        }
    }
}
