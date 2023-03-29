using UnityEngine;

[CreateAssetMenu(menuName = "Finite State Machine/State")]
public class State : ScriptableObject
{
    [SerializeField] private Action entryAction;
    [SerializeField] private Action[] stateActions;
    [SerializeField] private Action exitAction;
    [SerializeField] private Transition[] transitions;

    public Action GetEntryAction()
    {
        return entryAction;
    }
    
    public Action[] GetStateActions()
    {
        return stateActions;
    }
    
    public Action GetExitAction()
    {
        return exitAction;
    }
    
    public Transition[] GetTransitions()
    {
        return transitions;
    }
}
