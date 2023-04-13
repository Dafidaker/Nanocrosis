using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public  class CustomGameEvent : UnityEvent<Component, object>{ }
public class GameEventListener : MonoBehaviour
{
    public GameEvent GameEvent;

    public CustomGameEvent response;

    private void OnEnable()
    {
        GameEvent.RegisterListerner(this);
    }
    
    private void OnDisable()
    {
        GameEvent.UnregisterListener(this);
    }

    public void OnEventPinged(Component sender, object data)
    {
        response.Invoke(sender, data);
    }
}
