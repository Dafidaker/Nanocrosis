
using UnityEngine;

public class ChikaiAgent : FSMNavMeshAgent
{
    private FSMNavMeshAgent _fsmNavMeshAgent;

    [field: SerializeField] public bool isAttacking;
    
    public float viewDistance = 0;
    public float viewAngle = 0;
    
    #region Unity Functions

    private void OnEnable()
    {
        
    }
    
    private void OnDisable()
    {
        
    }

    private void Awake()
    {
        _fsmNavMeshAgent = GetComponent<FSMNavMeshAgent>();
    }
    
    private void Start()
    {
        
    }
    
    private void Update()
    {
        
    }

    private void FixedUpdate()
    {
        
    }

    #endregion

    #region Utilities
    
    
    #endregion
    
    #region Actions Functions

    public void Attack()
    {
        Debug.Log("ChikaiAgent _ Attack");
    }
    
    public void Chase()
    {
        Debug.Log("ChikaiAgent _ Chase");
    }

    #endregion

    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, viewDistance);
        Gizmos.DrawLine(transform.position, Quaternion.Euler(0, viewAngle/2, 0) * transform.forward);
        Gizmos.DrawLine(transform.position, Quaternion.Euler(0, -viewAngle/2, 0) * transform.forward);
    }
}


