using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Finite State Machine/Actions/Toi/Chase")]
public class ToiChase : Action
{
    public override void Act(FiniteStateMachine fsm)
    {
        Debug.Log("Toi Chase");
        fsm.GetNavMeshAgent().toiAgent.ChaseAction();
    }
}

