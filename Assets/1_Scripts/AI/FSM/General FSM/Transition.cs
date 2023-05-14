using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

[CreateAssetMenu(menuName = "Finite State Machine/Transition")]
public class Transition : ScriptableObject
{
    [SerializeField] private Condition[] andDecisions;
    [SerializeField] private Decisions andOrDecisions;
    [SerializeField] private Action action;
    [SerializeField] private State targetState;

    public bool isTriggered(FiniteStateMachine fsm)
    {
        if (andDecisions.Length == 1)
        {
            return andDecisions[0].Test(fsm);
        }

        if (andDecisions.Length > 0)
        {
            var result = true;
            foreach (var condition in andDecisions)
            {
                result = result && condition.Test(fsm);
            }

            return result;
        }
        
        return AndOrCondition(fsm);
    }

    private bool AndOrCondition(FiniteStateMachine fsm)
    {
        int externalCounter = 0;
        //And_Or externalOperator = And_Or.And;
        int internalCounter = 0;
        And_Or internalOperator = And_Or.And;
        var thisResult = true;
        
        //goes through all the internal operations of each external operations
        //and attributes the overall result to result of the external operation
        while (externalCounter < andOrDecisions.combinedConditions.Length)
        {
            while (internalCounter < andOrDecisions.combinedConditions[externalCounter].condition.Length)
            {
                if (internalCounter == 0)
                {
                    thisResult = andOrDecisions.combinedConditions[externalCounter].condition[internalCounter].Test(fsm);
                    internalOperator = andOrDecisions.combinedConditions[externalCounter].operation[internalCounter];
                    internalCounter++;
                    continue;
                }
                
                thisResult = DoOperation(thisResult,
                    andOrDecisions.combinedConditions[externalCounter].condition[internalCounter].Test(fsm),
                    internalOperator);

                internalOperator = andOrDecisions.combinedConditions[externalCounter].operation[internalCounter];
                
                internalCounter++;
            }

            internalCounter = 0;
            andOrDecisions.combinedConditionsResults[externalCounter] = thisResult;
            externalCounter++;
        }

        externalCounter = 0;
        //Finally goes through all the general conditions and then it returns the final result
        while (externalCounter < andOrDecisions.combinedConditions.Length)
        {
            if (externalCounter == 0)
            {
                thisResult = andOrDecisions.combinedConditionsResults[externalCounter];
                externalCounter++;
                continue;
            }
            
            thisResult = DoOperation(thisResult,
                andOrDecisions.combinedConditionsResults[externalCounter],
                andOrDecisions.operation[externalCounter -1]);
            
            externalCounter++;
        }

        return thisResult;
    }
    
    private bool DoOperation(bool bool1, bool bool2, And_Or andOr)
    {
        switch (andOr)
        {
            case And_Or.And:
                return bool1 && bool2;
            case And_Or.Or:
                return bool1 || bool2;
            case And_Or.Final:
            default:
                Debug.Log("operator: " + andOr);
                throw new ArgumentOutOfRangeException(nameof(andOr), andOr, null);
        }
    }

    public Action GetAction()
    {
        return action;
    }

    public State GetTargetState()
    {
        return targetState;
    }
}
[Serializable]
public class Decisions
{
    public ConditionsDictionary[] combinedConditions;
    public And_Or[] operation;
    public bool[] combinedConditionsResults;
}

public enum And_Or
{
    And,
    Or,
    Final
}

[Serializable]
public class ConditionsDictionary
{
    public Condition[] condition;
    public And_Or[] operation;
}

