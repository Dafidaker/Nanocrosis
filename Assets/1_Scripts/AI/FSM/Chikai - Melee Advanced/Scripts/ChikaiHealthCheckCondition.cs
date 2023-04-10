using UnityEngine;

[CreateAssetMenu(menuName = "Finite State Machine/Conditions/Chikai/Health check")]
public class ChikaiHealthCheckCondition : Condition
{
    [field: SerializeField] private bool above;
    [field: SerializeField] private int threshold;
    public override bool Test(FiniteStateMachine fsm)
    {
        var chikaiAgent = fsm.GetNavMeshAgent().chikaiAgent;
        if (above) return chikaiAgent.currentHealth >= chikaiAgent.maxHealth * threshold/100; 

        return chikaiAgent.currentHealth <= chikaiAgent.maxHealth * threshold/100;;
    }
    
}
