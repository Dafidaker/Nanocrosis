using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Finite State Machine/Actions/Toi/Chase Carefully")]
public class ToiChaseCarefully : Action
{
    public override void Act(FiniteStateMachine fsm)
    {
        //Debug.Log("Attack");
        fsm.GetNavMeshAgent().toiAgent.ChaseCarefullyAction();
    }
}
