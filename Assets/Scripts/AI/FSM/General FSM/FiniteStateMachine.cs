using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

//[CreateAssetMenu(menuName = "Finite State Machine")]
public class FiniteStateMachine : MonoBehaviour
{
    private FSMNavMeshAgent NavMeshAgent;
    public State InitialState;

    
    public State currentState;

    public LayerMask groundAndWallsLayerMask;
    
    void Start()
    {
        currentState = InitialState;
        NavMeshAgent = GetComponent<FSMNavMeshAgent>();
    }
    
    void Update()
    {
        Transition triggerTransition = null;
        foreach (var transition in currentState.GetTransitions())
        {
            if (transition.isTriggered(this))
            {
                triggerTransition = transition;
                break;
            }
        }

        List<Action> actions = new List<Action>();
        if (triggerTransition != null)
        {
            actions.Add(currentState.GetExitAction());
            actions.Add(triggerTransition.GetAction());
            actions.Add(triggerTransition.GetTargetState().GetEntryAction());
            
            currentState = triggerTransition.GetTargetState();
            actions.AddRange(currentState.GetStateActions());
        }
        else
        {
            actions.AddRange(currentState.GetStateActions());
        }
        
        DoActions(actions);
    }

    private void DoActions(IEnumerable<Action> actions)
    {
        foreach (var action in actions)
        {
            if (action != null)
            {
                action.Act(this);
            }
        }
    }

    public FSMNavMeshAgent GetNavMeshAgent()
    {
        return NavMeshAgent;
    }

    private void OnGUI()
    {
        GUI.Label(new Rect(5, 0, 200, 100), currentState.name);
    }
    
    
}
