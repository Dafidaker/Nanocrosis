using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawLine : MonoBehaviour
{
    public GameObject Target;

    public GameObject Bullet;

    public float addPosition;

    public int NumBullets;

    public float AngleBetween;

    private void Start()
    {
        /*for (int N = 1; N <= NumBullets; N++)
        {
            var go = Instantiate(Bullet);
            go.transform.position = transform.position;
            go.transform.rotation = Quaternion.LookRotation(Target.transform.position - transform.position);
            
            /go.transform.position *= addPosition;
        }*/
    }

    // Start is called before the first frame update
    void FixedUpdate()
    {
        Debug.DrawLine(transform.position, Target.transform.position, Color.red);
    }

    
}
