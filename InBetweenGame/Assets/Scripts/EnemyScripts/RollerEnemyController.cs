using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RollerEnemyController : MonoBehaviour
{
    [HideInInspector] public Transform playerTransform;

    [SerializeField] Rigidbody2D rigidBody; // The Roller's rigidBody component

    // The range of how much force the Roller adds to its rigidbody when trying to roll into the player
    [SerializeField] private float minMovementForce;
    [SerializeField] private float maxMovementForce;

    // The maximum x-velocity the roller can add to itself
    [SerializeField] private float minMovementSpeed;
    [SerializeField] private float maxMovementSpeed;

    private float movementForce; // How much force is added to the Roller when trying to roll into the player
    private float movementSpeedLimit; // How fast the Roller can go before stop being able to add force to itself (its maximum speed)
    private bool isSpeedingUp; // A useful booleans which tells whether or not the enemy is speeding up or not

    private Coroutine attackPlayerCor;

    private Vector2 attackPlayerVector; // The vector the roller uses to roll into the Player

    private void Start()
    {
        // Initializing the movement values so they can be used
        movementForce = Random.Range(minMovementForce, maxMovementForce);
        movementSpeedLimit = Random.Range(minMovementSpeed, maxMovementSpeed);

        attackPlayerVector = Vector2.zero; // Initialize the vector
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        // When Roller speeds up into a wall, it must stop trying to move and start over
        if (collision.transform.CompareTag("LevelBorder") && isSpeedingIntoAWall()) // The Roller just bumped into a wall but is still trying to speed up
        {
            print("Whoops, just bumped into a wall. Starting over :)");
            if (attackPlayerCor != null) { StopCoroutine(attackPlayerCor); }
            attackPlayerCor = StartCoroutine(AttackPlayerCoroutine());
        }


        // Start trying to attack the player
        if (attackPlayerCor == null) { attackPlayerCor = StartCoroutine(AttackPlayerCoroutine()); }
    }

    private IEnumerator AttackPlayerCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(2f, 7f)); // First the enemy waits for a random amount of time

            bool isLeftOfPlayer = transform.position.x < playerTransform.position.x; // Know this seperately in order to evaluate whether Roller has already passed the Player
            attackPlayerVector.x = isLeftOfPlayer ? movementForce : -movementForce; // the x force of the attackVector is the only one that matters
            print("Started attacking!");
            while (Mathf.Abs(rigidBody.velocity.x) < movementSpeedLimit) // This loop runs until the Roller achieves maximum speed
            {
                rigidBody.AddForce(attackPlayerVector); // Add force over time
                if ((transform.position.x < playerTransform.position.x) != isLeftOfPlayer) { break; } // If Roller has already passed the Player, stop adding force
                yield return null; // Just wait until next frame
            }
            attackPlayerVector.x = 0; // Setting this to zero so it's completely known that this Roller is not trying to move
            print("Ended attacking!");
        }
    }

    private bool isSpeedingIntoAWall()
    {
        bool speedingRightIntoRightWall = attackPlayerVector.x > 0 && transform.position.x > 0; // Is speeding to the right into the right level border
        bool speedingLeftIntoLeftWall = attackPlayerVector.x < 0 && transform.position.x < 0; // Is speeding to the left into the left level border

        return speedingRightIntoRightWall || speedingLeftIntoLeftWall;
    }

}
