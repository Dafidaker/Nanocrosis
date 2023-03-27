using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Finite State Machine/Actions/Kuroru/Attack")]
public class KuroruAttack : Action
{
    public override void Act(FiniteStateMachine fsm)
    {
        //Debug.Log("Attack");
        fsm.GetNavMeshAgent().kuroruAgent.CallAttack();
    }
}
