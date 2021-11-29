using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RollerEnemyController : MonoBehaviour
{
    public Transform playerTransform;

    [SerializeField] Rigidbody2D rigidBody; // The Roller's rigidBody component

    // The range of how much force the Roller adds to its rigidbody when trying to roll into the player
    [SerializeField] private float minMovementForce;
    [SerializeField] private float maxMovementForce;

    // The maximum x-velocity the roller can add to itself
    [SerializeField] private float minMovementSpeed;
    [SerializeField] private float maxMovementSpeed;

    // How long does the roller have to wait before able to roll again
    [SerializeField] private float minMovementCooldown;
    [SerializeField] private float maxMovementCooldown;

    private float movementForce; // How much force is added to the Roller when trying to roll into the player
    private float movementSpeedLimit; // How fast the Roller can go before stop being able to add force to itself (its maximum speed)


    // SHIELD FUNCTIONALITY
    private ShieldController shieldControllerScript; // The shield's controller script to know important info
    private bool isParalyzed = false;
    private Coroutine paralyzeCoroutine; // Coroutine object for when the enemy gets paralyzed by the shield
    private Vector2 knockBackVector; // The vector that knocks the enemy back

    private Coroutine attackPlayerCor;

    private Vector2 attackPlayerVector; // The vector the roller uses to roll into the Player

    private void Start()
    {
        // Initializing the movement values so they can be used
        movementForce = Random.Range(minMovementForce, maxMovementForce);
        movementSpeedLimit = Random.Range(minMovementSpeed, maxMovementSpeed);

        attackPlayerVector = Vector2.zero; // Initialize the vector
    }

    /// <summary>
    /// Basic Enemy gets knocked back and paralyzed when hit with the shield
    /// </summary>
    /// <param name="collision"></param>
    private void OnShieldCollision(Collision2D collision)
    {
        if (shieldControllerScript == null) { shieldControllerScript = collision.gameObject.GetComponent<ShieldController>(); } // Get the shield controller script if not already

        // Paralyze the Rolller
        if (paralyzeCoroutine != null) { StopCoroutine(paralyzeCoroutine); } // Stop previous paralyzation if it is currently active
        paralyzeCoroutine = StartCoroutine(ParalyzeCor(shieldControllerScript.enemyParalyzationSeconds)); // Start paralyzing the enemy

        // Knock back the roller
        knockBackVector = transform.position - collision.transform.position; // Now it is known what direction this enemy must go in
        rigidBody.AddForce(knockBackVector.normalized * shieldControllerScript.enemyKnockbackForce, ForceMode2D.Impulse);

        // Call the playercombat script through the shield object to make the player lose the correct amount of energy
        shieldControllerScript.playerCombatScript.OnShieldProtect();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Shield"))
        {
            OnShieldCollision(collision);
        }


        // When Roller speeds up into a wall, it must stop trying to move and start over
        else if (collision.transform.CompareTag("LevelBorder") && isSpeedingIntoAWall()) // The Roller just bumped into a wall but is still trying to speed up
        {
            print("Whoops, just bumped into a wall. Starting over :)");
            if (attackPlayerCor != null) { StopCoroutine(attackPlayerCor); }
            attackPlayerCor = StartCoroutine(AttackPlayerCoroutine());
        }

        // This code runs when the Roller lands on the ground for the first time, thus starting the movement process
        else if (collision.transform.CompareTag("Ground") && attackPlayerCor == null)
        {
            attackPlayerCor = StartCoroutine(AttackPlayerCoroutine());
        }
    }

    private IEnumerator AttackPlayerCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(minMovementCooldown, maxMovementCooldown)); // First the enemy waits for a random amount of time

            bool isLeftOfPlayer = transform.position.x < playerTransform.position.x; // Know this seperately in order to evaluate whether Roller has already passed the Player
            attackPlayerVector.x = isLeftOfPlayer ? movementForce : -movementForce; // the x force of the attackVector is the only one that matters
            while (Mathf.Abs(rigidBody.velocity.x) < movementSpeedLimit) // This loop runs until the Roller achieves maximum speed
            {
                rigidBody.AddForce(attackPlayerVector); // Add force over time
                if ((transform.position.x < playerTransform.position.x) != isLeftOfPlayer) { break; } // If Roller has already passed the Player, stop adding force
                yield return null; // Just wait until next frame
            }
            attackPlayerVector.x = 0; // Setting this to zero so it's completely known that this Roller is not trying to move
        }
    }

    private IEnumerator ParalyzeCor(float secondsOfParalyzation)
    {

        // Make sure all movement and attacks are completely stopped
        if (attackPlayerCor != null) { StopCoroutine(attackPlayerCor); }
        attackPlayerVector.x = 0; attackPlayerVector.y = 0;
        rigidBody.velocity = attackPlayerVector;

        yield return new WaitForSeconds(secondsOfParalyzation);
        attackPlayerCor = StartCoroutine(AttackPlayerCoroutine()); // Start moving again!
    }

    private bool isSpeedingIntoAWall()
    {
        bool speedingRightIntoRightWall = attackPlayerVector.x > 0 && transform.position.x > 0; // Is speeding to the right into the right level border
        bool speedingLeftIntoLeftWall = attackPlayerVector.x < 0 && transform.position.x < 0; // Is speeding to the left into the left level border

        return speedingRightIntoRightWall || speedingLeftIntoLeftWall;
    }

}
