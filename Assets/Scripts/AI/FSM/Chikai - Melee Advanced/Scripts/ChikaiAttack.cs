using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Finite State Machine/Actions/Chikai/Attack")]
public class ChikaiAttack : Action
{
    public override void Act(FiniteStateMachine fsm)
    {
        Debug.Log("Attack");
        fsm.GetNavMeshAgent().chikaiAgent.Attack();
    }
}
