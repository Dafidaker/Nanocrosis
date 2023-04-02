using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Finite State Machine/Actions/Chikai/Ranged Attack")]
public class ChikaiRangedAttack : Action
{
    public override void Act(FiniteStateMachine fsm)
    { 
        fsm.GetNavMeshAgent().chikaiAgent.Chase();
    }
}
