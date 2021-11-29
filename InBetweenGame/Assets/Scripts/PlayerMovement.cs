using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private float moveHorizontal;
    private float moveVertical;

    private bool isJumping = false;
    private bool alreadyJumped = false;
    private bool damageCooldown = false;

    private Vector2 moveVector;
    private Vector2 currentVelocity;

    [Header("Components and Objects")]
    [SerializeField] private Rigidbody2D playerRigidBody;

    [Header("Walking attributes")]
    [SerializeField] private float movementSpeed = 3f;

    [Header("Jumping attributes")]
    [SerializeField] private float jumpForce = 300f;

    [Header("Running attributes")]
    public float maxStamina; // The max amount of stamina the player can have
    [SerializeField] private float staminaDepletionRate; // How fast the stamina is spent
    [SerializeField] private float staminaRechargeRate; // How fast the stamina recharges
    [SerializeField] private float runningSpeed; // How fast the player moves when running
    [HideInInspector] public float stamina; // The stamina the player has currently
    private bool allowedToRun = true; // This is primarily used for when player depletes the stamina and therefore is not allowed to run after that 

    private void Start()
    {
        stamina = maxStamina; // Always starts with as much stamina as possible

        moveVector = new Vector2(0, 0); // Initializing the movementVector
    }

    private void Update()
    {
        if (damageCooldown) { return; } // No movement while player is being damaged

        HandleWalkingAndRunning();
        HandleJumping();

    }

    private void FixedUpdate()
    {
        if (moveHorizontal != 0)
        {
            playerRigidBody.velocity = moveVector;
        }

        if (isJumping && !alreadyJumped)
        {
            playerRigidBody.AddForce(Vector2.up * jumpForce, ForceMode2D.Force);
            alreadyJumped = true;
        }
    }
    

    /// <summary>
    /// Handles the movement vector according to when the player is still, walks, and runs
    /// </summary>
    private void HandleWalkingAndRunning()
    {
        moveHorizontal = Input.GetAxis("Horizontal");
        moveVertical = Input.GetAxis("Vertical");
        currentVelocity = playerRigidBody.velocity;
        moveVector.y = currentVelocity.y;

        bool isPlayerMoving = moveHorizontal != 0f;
        bool isPlayerRunning = isPlayerMoving && Input.GetKey(KeyCode.LeftShift);

        if (isPlayerRunning && allowedToRun)
        {
            moveVector.x = moveHorizontal * runningSpeed;
            HandlePlayerRunning();

            // When stamina is depleted, the player is not allowed to run again until he stops pressing on the right mouse button
            if (stamina <= 0f)
            {
                allowedToRun = false;
            }
        } else
        {
            moveVector.x = moveHorizontal * movementSpeed;
            HandlePlayerWalking(isPlayerMoving);

            // When the player stops pressing on the right mouse button after depleting the stamina
            if (!allowedToRun && Input.GetKeyUp(KeyCode.LeftShift))
            {
                allowedToRun = true; // Then they are finally able to run again, yay!
            }
        }
    }

    /// <summary>
    /// Handles the jumping inputs and movement vector so the player can jump
    /// </summary>
    private void HandleJumping()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (!isJumping)
            {
                isJumping = true;
                alreadyJumped = false;
            }
        }
    }

    /// <summary>
    /// This function does everything necessary when the player runs
    /// </summary>
    private void HandlePlayerRunning()
    {
        stamina = Mathf.Max(0f, stamina - (staminaDepletionRate * Time.deltaTime));
    }

    /// <summary>
    /// Do stuff when the player is walking or still instead of running
    /// </summary>
    private void HandlePlayerWalking(bool playerIsMoving)
    {
        if (playerIsMoving)
        {
            stamina = Mathf.Min(maxStamina, stamina + (staminaRechargeRate * Time.deltaTime * 0.7f));
        } else
        {
            stamina = Mathf.Min(maxStamina, stamina + (staminaRechargeRate * Time.deltaTime));
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag.Equals("Ground"))
        {
            isJumping = false;
        }
    }
}
