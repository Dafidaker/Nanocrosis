using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BouncyPlatform : MonoBehaviour
{
    [SerializeField] private float pushForce;
    [SerializeField] private Transform[] direction;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<Rigidbody>().AddForce((GameManager.Instance.playerController.moveDirection.normalized + Vector3.up) * pushForce, ForceMode.Impulse);
        }
    }
}
