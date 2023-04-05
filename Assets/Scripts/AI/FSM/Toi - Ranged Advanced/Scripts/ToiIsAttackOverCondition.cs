using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Finite State Machine/Conditions/Toi/Is Attack Over")]
public class ToiIsAttackOverCondition : Condition
{
    [field: SerializeField] private bool negation;
    public override bool Test(FiniteStateMachine fsm)
    {
        if (fsm.GetNavMeshAgent()._agent.GetComponent<FiniteStateMachine>().currentState.name == "Toi Melee Attack State" && !fsm.GetNavMeshAgent().toiAgent.isAttacking)
        {
            Debug.Log("is attacking " + fsm.GetNavMeshAgent().toiAgent.isAttacking);
        }
        
        if (negation) { return fsm.GetNavMeshAgent().toiAgent.isAttacking; }
        return !fsm.GetNavMeshAgent().toiAgent.isAttacking;
    }
    
}