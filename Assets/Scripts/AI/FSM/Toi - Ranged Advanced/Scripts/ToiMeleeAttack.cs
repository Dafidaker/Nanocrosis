using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Finite State Machine/Actions/Toi/Melee Attack")]
public class ToiMeleeAttack : Action
{
    public override void Act(FiniteStateMachine fsm)
    {
        //Debug.Log("Attack");
        fsm.GetNavMeshAgent().toiAgent.MeleeAttackAction();
    }
}


