using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Finite State Machine/Actions/Toi/Ranged Attack")]
public class ToiRangedAttack : Action
{
    public override void Act(FiniteStateMachine fsm)
    {
        Debug.Log("RangedAttackAction");
        fsm.GetNavMeshAgent().toiAgent.RangedAttackAction();
    }
}


