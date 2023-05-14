using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class move : MonoBehaviour
{
     [SerializeField] private Rigidbody _rb;

     [SerializeField]private float jump;
    
     [SerializeField]private int numberofJumps = 2;
    // Start is called before the first frame update
    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        
    }

    // Update is called once per frame
    void Update()
    {
        
        if (Input.GetKey(KeyCode.W))
        {
            var pos = transform.position;
            
            
            transform.position = new Vector3(pos.x , pos.y + 0.01f , pos.z);
            
        }
        if (Input.GetKey(KeyCode.S))
        {
            var pos = transform.position;
            
            
            transform.position = new Vector3(pos.x , pos.y - 0.01f , pos.z);
            
        }
        if (Input.GetKey(KeyCode.A))
        {
            var pos = transform.position;
            
            
            transform.position = new Vector3(pos.x-0.01f, pos.y , pos.z);
            
        }
        if (Input.GetKey(KeyCode.D))
        {
            var pos = transform.position;
            
            transform.position = new Vector3(pos.x + 0.01f, pos.y, pos.z);

        }
        if (Input.GetKeyDown(KeyCode.Space) && numberofJumps > 0 )
        {
            numberofJumps -= 1;
            _rb.AddForce(new Vector3(0,jump));
        }
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        numberofJumps = 2;
    }
}
