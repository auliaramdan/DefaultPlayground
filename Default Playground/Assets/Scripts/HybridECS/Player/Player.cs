using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[RequireComponent(typeof(CharacterController))]
public class Player : MonoBehaviour
{
    private enum InputOptions
    {
        KEYBOARD,
        GAMEPAD
    }

    #region Variables
    [Header("Player Motor")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private float rotationSpeed = 30f;
    [SerializeField] private float jumpPower = 5f;
    [SerializeField] private Transform groundChecker;
    
    [Header("Input")]
    [SerializeField] private InputOptions inputOptions = InputOptions.KEYBOARD;



    private IPlayerInput playerInput;
    private PlayerRotation playerRotation;
    private PlayerMovement playerMovement;
    #endregion

    private void Awake()
    {
        switch (inputOptions)
        {
            case InputOptions.KEYBOARD:
                playerInput = new KeyboardInput();
                break;
            case InputOptions.GAMEPAD:
                break;
        }

        playerMovement = new PlayerMovement(transform, Camera.main, playerInput, speed, GetComponent<Rigidbody>(), jumpPower, groundChecker);
        playerRotation = new PlayerRotation(rotationSpeed, transform, Camera.main, playerInput, GetComponent<Rigidbody>());
    }

    public void Inject()
    {
        playerMovement.Inject(speed, jumpPower);
        playerRotation.Inject(rotationSpeed);
    }

    // Update is called once per frame
    void Update()
    {
        playerInput.ReadInput();
        playerMovement.Jump();
    }

    private void FixedUpdate()
    {
        playerMovement.Move();
        playerRotation.Rotate();
    }    
}
