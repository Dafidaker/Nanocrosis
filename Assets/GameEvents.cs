using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEvents : MonoBehaviour
{
    public static GameEvents Instance;

    public GameEvent enemyDied;
    private void Awake()
    {
        Instance = this;
    }

    public event Action<int> actionName;

    public void mehod()
    {
        if (actionName != null)
        {
            actionName(1);
        }
    }
    

}
