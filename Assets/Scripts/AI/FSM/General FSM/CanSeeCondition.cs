using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Finite State Machine/Conditions/Can See Target")]
public class CanSeeCondition : Condition
{
    [field: SerializeField] private bool negation;
    [field: SerializeField] private bool checkForWalls;
    [field: SerializeField] private float viewAngle;
    [field: SerializeField] private float viewDistance;

    private FSMNavMeshAgent _agent;
    private bool _clearViewToTarget;

    public override bool Test(FiniteStateMachine fsm)
    {
        _agent = fsm.GetNavMeshAgent();

        //to debug the distance and angle on the agent
        _agent.viewDistance = viewDistance;
        _agent.viewAngle = viewAngle;

        if (_agent.target == null) return negation;
        
        if (checkForWalls) _clearViewToTarget = !Physics.Linecast(_agent.transform.position, _agent.target.position
                ,out _ ,fsm.groundAndWallsLayerMask);
        
        if (checkForWalls && !negation && !_clearViewToTarget) return false; 
        
        var direction = _agent.target.position - _agent.transform.position;
        
        //if the distance not is smaller than the minDistance return negation
        if (!(direction.magnitude < viewDistance)) return negation;
        
        var angle = Vector3.Angle(direction.normalized, _agent.transform.forward);
        
        //if the angle not is smaller than the viewingAngle return negation
        if (!(angle < viewAngle)) return negation;
        
        //the target is within distance and view angle
        //check if are checking for walls and if so
        // send negative of negation and if theres a clear view between agent and target
        if (checkForWalls) return !negation && _clearViewToTarget; 
        
        //if we arent checking for the walls then
        return !negation;
    }
}
