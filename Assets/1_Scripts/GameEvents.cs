using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEvents : MonoBehaviour
{
    public static GameEvents Instance;

    public GameEvent enemyDied;
    public GameEvent mouseSensitivityChanged;
    public GameEvent bossTookDamage;
    public GameEvent lungsHealthChanged;
    
    public GameEvent oxigenNodeIsTargatable;
    public GameEvent oxigenNodeIsUntargatable;

    public GameEvent playerDied;
    public GameEvent playerWasDamaged;
    private void Awake()
    {
        Instance = this;
    }
    
    

}
