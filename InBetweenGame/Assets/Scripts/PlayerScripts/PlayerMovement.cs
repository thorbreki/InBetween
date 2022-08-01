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
    [HideInInspector] public bool isRunning = false; // Lets enemies know if the player is running or not

    private Vector2 moveVector;
    private Vector2 currentVelocity;
    private Vector2 colliderSizeVector;

    [Header("Components and Objects")]
    [SerializeField] private Rigidbody2D playerRigidBody;
    [SerializeField] private GameObject staminaShieldObject; // The stamina shield object
    [SerializeField] private BoxCollider2D boxCollider; // The box collider component

    [Header("Walking attributes")]
    [SerializeField] private float movementSpeed = 3f;

    [Header("Jumping attributes")]
    [SerializeField] private float jumpForce = 300f;

    [Header("Running attributes")]
    [SerializeField] private float runningSpeed; // How fast the player moves when running

    [HideInInspector] public bool playerRanIntoUnparalyzedEnemy = false;

    private void Start()
    {
        staminaShieldObject.SetActive(false);

        moveVector = new Vector2(0, 0); // Initializing the movementVector
        colliderSizeVector = Vector2.one;

    }

    private void Update()
    {
        if (damageCooldown) { return; } // No movement while player is being damaged

        HandleWalkingAndRunning();
        HandleJumping();
        HandleStaminaShield();

    }

    private void FixedUpdate()
    {
        if (moveHorizontal != 0)
        {
            playerRigidBody.velocity = moveVector;
        }

        if (isJumping && !alreadyJumped)
        {
            currentVelocity.y = Mathf.Max(0, currentVelocity.y);
            playerRigidBody.velocity = currentVelocity;
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
        bool isPlayerRunning = Input.GetKey(KeyCode.LeftShift);

        if (isPlayerRunning && !playerRanIntoUnparalyzedEnemy)
        {
            
            moveVector.x = moveHorizontal * runningSpeed;
        }
        else
        {
            moveVector.x = moveHorizontal * movementSpeed;

            // When the player stops pressing on the right mouse button after depleting the stamina
            if (playerRanIntoUnparalyzedEnemy && Input.GetKeyUp(KeyCode.LeftShift))
            {
                playerRanIntoUnparalyzedEnemy = false; // Then they are finally able to run again, yay!
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
            isJumping = true;
            alreadyJumped = false;
        }
    }

    private void HandleStaminaShield()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) && !playerRanIntoUnparalyzedEnemy)
        {
            isRunning = true;
            colliderSizeVector.x = 1.5f; colliderSizeVector.y = 1.5f;
            boxCollider.size = colliderSizeVector;
            staminaShieldObject.SetActive(true);

        } 
        else if (Input.GetKeyUp(KeyCode.LeftShift) || playerRanIntoUnparalyzedEnemy)
        {
            isRunning = false;
            colliderSizeVector.x = 1f; colliderSizeVector.y = 1f;
            boxCollider.size = colliderSizeVector;
            staminaShieldObject.SetActive(false);
        } // Set the stamina shield as inactive when not running
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag.Equals("Ground"))
        {
            isJumping = false;
        }
    }
}
