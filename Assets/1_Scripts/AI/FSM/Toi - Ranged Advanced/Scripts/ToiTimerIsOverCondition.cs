using UnityEngine;

[CreateAssetMenu(menuName = "Finite State Machine/Conditions/Toi/Timer Is Over")]
public class ToiTimerIsOverCondition : Condition
{
    [field: SerializeField] private bool negation;
    public override bool Test(FiniteStateMachine fsm)
    {
        //Debug.Log("attackTimer: " + fsm.GetNavMeshAgent().toiAgent.attackTimer);
        if (fsm.GetNavMeshAgent().toiAgent.attackTimer <= 0f)
        {
            return !negation;
        }
        return negation;
    }
    
}
