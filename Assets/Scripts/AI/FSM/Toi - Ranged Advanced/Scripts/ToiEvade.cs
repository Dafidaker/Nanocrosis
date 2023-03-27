using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Finite State Machine/Actions/Toi/Evade")]
public class ToiEvade : Action
{
    public override void Act(FiniteStateMachine fsm)
    {
        //Debug.Log("Attack");
        fsm.GetNavMeshAgent().toiAgent.EvadeAction();
    }
}
