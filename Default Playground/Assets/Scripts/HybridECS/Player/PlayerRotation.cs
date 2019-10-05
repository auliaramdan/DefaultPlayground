using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRotation
{
    private readonly Transform playerTransform;
    private readonly Camera mainCam;
    private readonly IPlayerInput playerInput;
    private readonly Rigidbody playerRigidbody;

    private float desiredRotationSpeed;

    private Vector3 camForward, camRight, desiredMoveDirection;

    public PlayerRotation(float desiredRotationSpeed, Transform playerTransform, Camera mainCam, IPlayerInput playerInput, Rigidbody playerRigidbody)
    {
        this.desiredRotationSpeed = desiredRotationSpeed;
        this.playerTransform = playerTransform;
        this.mainCam = mainCam;
        this.playerInput = playerInput;
        this.playerRigidbody = playerRigidbody;
    }

    public void Inject(float desiredRotationSpeed)
    {
        this.desiredRotationSpeed = desiredRotationSpeed;
    }

    public void Rotate()
    {
        camForward = mainCam.transform.forward;
        camRight = mainCam.transform.right;

        camForward.y = 0f;
        camRight.y = 0f;

        camForward.Normalize();
        camRight.Normalize();

        desiredMoveDirection = camForward * playerInput.Destination.y + camRight * playerInput.Destination.x;


        if(desiredMoveDirection != Vector3.zero)
            playerRigidbody.rotation = Quaternion.Slerp(playerTransform.rotation, Quaternion.LookRotation(desiredMoveDirection), desiredRotationSpeed * Time.deltaTime);
        //playerTransform.rotation = Quaternion.Slerp(playerTransform.rotation, Quaternion.LookRotation(desiredMoveDirection), desiredRotationSpeed * Time.deltaTime);
    }
}