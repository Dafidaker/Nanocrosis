using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Joel_PlayerMovement : MonoBehaviour
{
    [field: SerializeField] private float speed;
    [field: SerializeField] private Vector3 direction;


    private Rigidbody _rb;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    private void Update()
    {
        direction = Vector3.zero;
        
        if (Input.GetKey(KeyCode.W))
        {
            direction = new Vector3(1, direction.y, direction.z);
        }
        if (Input.GetKey(KeyCode.S))
        {
            direction = new Vector3(-1, direction.y, direction.z);
        }
        if (Input.GetKey(KeyCode.A))
        {
            direction = new Vector3(direction.x, direction.y, 1);
        }
        if (Input.GetKey(KeyCode.D))
        {
            direction = new Vector3(1, direction.y, -1);
        }
        
    }

    private void FixedUpdate()
    {
        _rb.velocity = direction * (speed * Time.fixedDeltaTime);
    }
}
