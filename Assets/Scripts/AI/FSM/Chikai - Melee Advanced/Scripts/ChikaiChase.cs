using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Finite State Machine/Actions/Chikai/Chase")]
public class ChikaiChase : Action
{
    public override void Act(FiniteStateMachine fsm)
    { 
        fsm.GetNavMeshAgent().chikaiAgent.Chase();
    }
}
