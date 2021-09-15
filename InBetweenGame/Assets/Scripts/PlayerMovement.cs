using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private float moveHorizontal;
    private float moveVertical;

    private bool isJumping = false;
    private bool alreadyJumped = false;

    private Vector2 moveVector;
    private Vector2 currentVelocity;

    [SerializeField] private Rigidbody2D playerRigidBody;
    [SerializeField] private float movementSpeed = 3f;
    [SerializeField] private float jumpForce = 300f;

    private void Start()
    {
        moveVector = new Vector2(0, 0);
    }

    private void Update()
    {
        moveHorizontal = Input.GetAxis("Horizontal");
        moveVertical = Input.GetAxis("Vertical");
        currentVelocity = playerRigidBody.velocity;
        moveVector.x = moveHorizontal * movementSpeed;
        moveVector.y = currentVelocity.y;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (!isJumping)
            {
                isJumping = true;
                alreadyJumped = false;
            }
        }
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


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag.Equals("Ground"))
        {
            isJumping = false;
        }
    }
}
