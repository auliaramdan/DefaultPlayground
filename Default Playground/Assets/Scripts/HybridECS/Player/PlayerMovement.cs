using UnityEngine;

public class PlayerMovement
{
    private readonly Transform playerTransform;
    private readonly Rigidbody playerRigidbody;
    private readonly Camera mainCam;
    private readonly IPlayerInput playerInput;
    private readonly CharacterController playerController;
    private readonly Transform groundChecker;

    private float speed;
    private float jumpPower;
    
    private Vector3 camForward, camRight, desiredMoveDirection;
    private bool isGrounded = false;
    private float groundingRadius = .1f;

    public PlayerMovement(Transform playerTransform, Rigidbody playerRigidbody, Camera mainCam, IPlayerInput playerInput, float speed, CharacterController playerController, float jumpPower, Transform groundChecker)
    {
        this.playerTransform = playerTransform;
        this.playerRigidbody = playerRigidbody;
        this.mainCam = mainCam;
        this.playerInput = playerInput;
        this.speed = speed;
        this.playerController = playerController;
        this.groundChecker = groundChecker;
    }

    public PlayerMovement(Transform playerTransform, Camera mainCam, IPlayerInput playerInput, float speed, CharacterController playerController, float jumpPower, Transform groundChecker)
    {
        this.playerTransform = playerTransform;
        this.mainCam = mainCam;
        this.playerInput = playerInput;
        this.speed = speed;
        this.playerController = playerController;
        this.jumpPower = jumpPower;
        this.groundChecker = groundChecker;
    }

    public PlayerMovement(Transform playerTransform, Camera mainCam, IPlayerInput playerInput, float speed, Rigidbody playerRigidbody, float jumpPower, Transform groundChecker)
    {
        this.playerTransform = playerTransform;
        this.mainCam = mainCam;
        this.playerInput = playerInput;
        this.speed = speed;
        this.playerRigidbody = playerRigidbody;
        this.jumpPower = jumpPower;
        this.groundChecker = groundChecker;
    }

    public void Inject(float speed, float jumpPower)
    {
        this.speed = speed;
        this.jumpPower = jumpPower;
    }

    public void Move()
    {
        desiredMoveDirection = new Vector3(playerInput.Destination.x * Time.deltaTime * speed, playerRigidbody.velocity.y, playerInput.Destination.y * Time.deltaTime * speed);

        if(playerRigidbody != null)
        {
            playerRigidbody.velocity = desiredMoveDirection;
            
        }
            
        else
            playerController.Move(desiredMoveDirection * Time.deltaTime * speed);
        
    }

    public void Jump()
    {
        isGrounded = Physics.CheckSphere(groundChecker.position, groundingRadius, LayerMask.GetMask("Ground"), QueryTriggerInteraction.Ignore);
        
        if (playerInput.Jump && isGrounded)
        {
            if(playerRigidbody != null)
            {
                playerRigidbody.AddForce(Vector3.up * jumpPower);
            }
        }
    }
}