using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Finite State Machine/Actions/Kuroru/Chase")]
public class KuroruChase : Action
{
    public override void Act(FiniteStateMachine fsm)
    {
       // Debug.Log("Chase");
        fsm.GetNavMeshAgent().kuroruAgent.Chase();
    }
}
