using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Finite State Machine/Conditions/Chikai/Is Shield Active")]
public class ChikaiIsShieldActiveCondition : Condition
{
    [field: SerializeField] private bool negation;
    public override bool Test(FiniteStateMachine fsm)
    {
        if (negation) { return !fsm.GetNavMeshAgent().chikaiAgent.hasShield; }
        return fsm.GetNavMeshAgent().chikaiAgent.hasShield;
    }
    
}

