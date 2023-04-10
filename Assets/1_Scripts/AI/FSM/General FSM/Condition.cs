using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Finite State Machine/Conditions")]
public abstract class Condition : ScriptableObject
{
    public abstract bool Test(FiniteStateMachine fsm);
}
