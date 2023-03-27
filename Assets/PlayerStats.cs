using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public int Health;

    [Header("HealthIndicator"), Space(10)] 
    public int minHealth1; 
    public int maxHealth1;
    public Color color1;
    [Space(5)]
    public int minHealth2; 
    public int maxHealth2;
    public Color color2;
    [Space(5)]
    public int minHealth3; 
    public int maxHealth3;
    public Color color3;
    
    private Dictionary<int[], Color> _healthIndicatorColors;
    private Color _currentHealthColor;


    #region Unity Functions

    private void OnEnable()
    {
        
    }
    
    private void OnDisable()
    {
        
    }

    private void Awake()
    {
    }
    
    private void Start()
    {
        CreateHealthIndicatorsDictionary();
    }
    
    private void Update()
    {
        
    }

    private void FixedUpdate()
    {
        
    }

    #endregion

    #region Public Funtions

    public void DamageTaken(int damagedTaken)
    {
        Health -= damagedTaken;
        foreach (var healthIndicatorColors in _healthIndicatorColors)
        {
            if (Health < healthIndicatorColors.Key[0] || Health > healthIndicatorColors.Key[1]) continue;
            _currentHealthColor = healthIndicatorColors.Value;
            break;
        }
    }
    
    #endregion
    
    #region Private Functions

    private void CreateHealthIndicatorsDictionary()
    {
        _healthIndicatorColors = new Dictionary<int[], Color>();
        int[] colorRange1 = { minHealth1, maxHealth1 };
        int[] colorRange2 = { minHealth2, maxHealth2 };
        int[] colorRange3 = { minHealth3, maxHealth3 };
        _healthIndicatorColors[colorRange1] = color1;
        _healthIndicatorColors[colorRange2] = color2;
        _healthIndicatorColors[colorRange3] = color3;
    }

    #endregion
}
