using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Finite State Machine/Conditions/Chikai/Timer Is Over")]

public class ChikaiIsTimerOverCondition : Condition
{
    [field: SerializeField] private bool negation;
    public override bool Test(FiniteStateMachine fsm)
    {
        if (fsm.GetNavMeshAgent().chikaiAgent.attackTimer <= 0f)
        {
            return !negation;
        }
        return negation;
    }
    
}