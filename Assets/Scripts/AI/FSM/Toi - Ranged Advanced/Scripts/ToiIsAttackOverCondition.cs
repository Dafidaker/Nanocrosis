using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Finite State Machine/Conditions/Toi/Is Attack Over")]
public class ToiIsAttackOverCondition : Condition
{
    [field: SerializeField] private bool negation;
    public override bool Test(FiniteStateMachine fsm)
    {
        if (negation) { return !fsm.GetNavMeshAgent().toiAgent.isAttackOver; }
        return fsm.GetNavMeshAgent().toiAgent.isAttackOver;
    }
    
}