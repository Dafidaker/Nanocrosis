using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class DetectGround : MonoBehaviour
{
    [field: SerializeField] private PlayerController player;
    [field: SerializeField] private Joel_PlayerMovement playerJoel;
    public LayerMask layerToDetect;
    
    private void OnTriggerEnter(Collider other)
    {
        if(((1<<other.gameObject.layer) & layerToDetect) != 0)
        {
            /*player.isGrounded = true;
            player.canDoubleJump = true;
            player.isJumpOver = false;
            player.gravityMultiplier = player.normalGravityMultiplier;*/

            playerJoel.isGrounded = true;
            playerJoel.isJumpOver = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(((1<<other.gameObject.layer) & layerToDetect) != 0)
        {
            playerJoel.isGrounded = false;
        }
    }

    
}
