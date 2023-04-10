using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder;

public class UnexpectedBombController : MonoBehaviour
{
    //[field: HideInInspector] 
    public Vector3 finalPosition;
    public float projetileSpeed;

    private Rigidbody _rb;
    // Start is called before the first frame update
    void Start()
    {
        var direction = (finalPosition - transform.position).normalized;
        transform.forward = new Vector3(direction.x, transform.forward.y, direction.z);
        
        _rb = GetComponent<Rigidbody>();

        var horizontal = getHorizontalSpeed();

        _rb.velocity = new Vector3(0, projetileSpeed, horizontal*10);

    }

    private void Update()
    {
        //Vector3 newDirection = Vector3.RotateTowards(transform.forward, finalPosition - transform.position, 1 ,0);
        
       //Debug.DrawRay(transform.position, newDirection * 100f, Color.red);
       
       var direction = (finalPosition - transform.position).normalized;
       transform.forward = new Vector3(direction.x, transform.forward.y, direction.z);
        
        Debug.DrawRay(transform.position, transform.forward * 100f, Color.blue);
        
        Debug.DrawRay(transform.position, _rb.velocity * 100f, Color.green);
        
       // transform.rotation = Quaternion.LookRotation(newDirection);
    }

    private float getHorizontalSpeed()
    {
        float range = transform.position.z - finalPosition.z;
        float verticalSpeed = projetileSpeed;
        float gravity = Physics.gravity.y;
        float height = transform.position.y - finalPosition.y;

        float a = range;
        float b = verticalSpeed;
        float c = Mathf.Sqrt(Mathf.Abs(Mathf.Pow(verticalSpeed, 2) + 2 * gravity * height));
        float d = gravity;
        
        Debug.Log("Mathf.Pow(verticalSpeed,2) " + Mathf.Pow(verticalSpeed,2));
        Debug.Log(" Mathf.Sqrt(Mathf.Abs(Mathf.Pow(verticalSpeed, 2) + 2 * gravity * height)):  " + (
            Mathf.Sqrt(Mathf.Abs(Mathf.Pow(verticalSpeed, 2) + 2 * gravity * height))));
        
        
        //var horizontalSpeed = range / (verticalSpeed + Mathf.Sqrt(Mathf.Pow(verticalSpeed,2) + 2 * gravity * height)) / gravity);
        var horizontalSpeed = a / (b + c / d);
        Debug.Log("horizontalSpeed: " + horizontalSpeed);
        
        return horizontalSpeed;
    }
}
