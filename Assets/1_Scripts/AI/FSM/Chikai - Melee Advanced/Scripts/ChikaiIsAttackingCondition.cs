using UnityEngine;

[CreateAssetMenu(menuName = "Finite State Machine/Conditions/Chikai/Is Attacking")]
public class ChikaiIsAttackingCondition : Condition
{
    [field: SerializeField] private bool negation;
    public override bool Test(FiniteStateMachine fsm)
    {
        if (negation) { return !fsm.GetNavMeshAgent().chikaiAgent.isAttacking; }
        return fsm.GetNavMeshAgent().chikaiAgent.isAttacking;
    }
    
}
