using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using PathCreation;
using PathCreation.Examples;
using UnityEngine;

public class ToiMeleeAttackController : MonoBehaviour
{
    
    [Header("Path"),Space(10)] 
    [field: SerializeField] public List<Vector3> pathNodes;
    [field: HideInInspector] public Transform toiTransform;
    private bool _followPath;
    private bool _isInPath;
    private Vector3? _closestNode;
    private int _currentNodeIndex;
    
    [Header("Attack Specifications"),Space(10)] 
    [field: SerializeField] private float forceLeaving;
    [field: SerializeField] private float speedRotating;
    [field: SerializeField] public float timeRotating;
    [field: SerializeField] private float maxDistanceTraveled;
    private Vector3 _oldPosition;
    private float _distanceTraveled;
    
    //Components
    private Rigidbody _rb;
    
    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        CalculateDistance();

        GoThroughPath();
    }
    
    private void CalculateDistance()
    {
        Vector3 distanceVector = transform.position - _oldPosition;
        float distanceThisFrame = distanceVector.magnitude;
        _distanceTraveled += distanceThisFrame;
        _oldPosition = transform.position;

        if (_distanceTraveled >= maxDistanceTraveled)
        {
            Destroy(gameObject);
        }
    }

    private void GoThroughPath()
    {
        //if its not following the path it returns
        if (!_followPath) { return; }

        //it gets the closest path if it isnt on the path
        if (!_isInPath && pathNodes.Count >= 1)
        {
            var distanceToNode = float.MaxValue;
            _closestNode = pathNodes[0];
            foreach (Vector3 pathNode in pathNodes)
            {
                if (Vector3.Distance(pathNode, transform.position) < distanceToNode)
                {
                    distanceToNode = Vector3.Distance(pathNode, transform.position); 
                    _closestNode = pathNode;
                    _currentNodeIndex = pathNodes.FindIndex(a => a == pathNode);
                }
            }
        }

        //it goes the closest path if it isnt on the path
        if (!_isInPath && _closestNode != null)
        {
            transform.position = Vector3.MoveTowards(transform.position , (Vector3)_closestNode, 0.01f * speedRotating);
            if (Vector3.Distance((Vector3) _closestNode, transform.position) < 0.01f)
            {
                _isInPath = true;
                _currentNodeIndex++;
                if (_currentNodeIndex > pathNodes.Count - 1)
                {
                    _currentNodeIndex = 0;
                }
            }
        }

        //if isn't on the path it returns
        if (!_isInPath) { return; }
        
        Debug.Log("Moving towards the next node ");
        //if its on the path it moves to the next node on path
        transform.position = Vector3.MoveTowards(transform.position , pathNodes[_currentNodeIndex], 0.01f * speedRotating);
        if (Vector3.Distance(pathNodes[_currentNodeIndex], transform.position) < 0.01f)
        {
            _currentNodeIndex++;
            if (_currentNodeIndex > pathNodes.Count - 1)
            {
                _currentNodeIndex = 0;
            }
        }
        
    }

    public void CreateAttack()
    {
        StartCoroutine(FollowPath());
    }
    
    private IEnumerator FollowPath()
    {
        _followPath = true;
        _isInPath = false;
        
        yield return new WaitForSeconds(timeRotating);
        
        //stops following path and goes forward
        _followPath = false;
        var newDirection =(transform.position - toiTransform.position).normalized;
        transform.forward = new Vector3(newDirection.x, transform.forward.y, newDirection.z);
        _rb.velocity = transform.forward.normalized * forceLeaving;
        
    }
    
    
    
}
