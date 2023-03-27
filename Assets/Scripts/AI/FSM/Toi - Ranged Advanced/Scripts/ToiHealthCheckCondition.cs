using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Finite State Machine/Conditions/Toi/Health check")]
public class ToiHealthCheckCondition : Condition
{
    [field: SerializeField] private bool above;
    [field: SerializeField] private int threshold;
    public override bool Test(FiniteStateMachine fsm)
    {
        var toiAgent = fsm.GetNavMeshAgent().toiAgent;
        if (above) return toiAgent.currentHealth >= toiAgent.currentHealth * threshold/100; 

        return toiAgent.currentHealth <= toiAgent.currentHealth * threshold/100;;
    }
    
}

