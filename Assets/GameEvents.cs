using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEvents : MonoBehaviour
{
    public static GameEvents Instance;

    public GameEvent enemyDied;
    public GameEvent mouseSentivityChanged;
    private void Awake()
    {
        Instance = this;
    }
    
    

}
