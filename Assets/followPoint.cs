using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class followPoint : MonoBehaviour
{
    [field: SerializeField] private Transform follow;
    
    void Update()
    {
        transform.position = follow.position;
        transform.rotation = follow.rotation;
    }
}
