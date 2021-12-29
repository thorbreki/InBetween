using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicEnemyController : MonoBehaviour
{
    [SerializeField] private Transform playerTransform; // Set by the parent spawner
    [SerializeField] private EnemyHealthController healthScript;
    [SerializeField] private float minMovementSpeed; // The minimum possible movement speed this enemy could have
    [SerializeField] private float maxMovementSpeed; // The maximum possible movement speed this enemy could have
    [SerializeField] private float velocityChangeSpeed; // How fast the enemy can change his velocity's direction
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color normalColor;
    [SerializeField] private Color paralyzedColor;

    private float movementSpeed;
    private Coroutine moveToPlayerCoroutine; // Coroutine object for enemy moving towards player

    // SHIELD FUNCTIONALITY
    private bool isParalyzed = false;
    private Coroutine paralyzeCoroutine; // Coroutine object for when the enemy gets paralyzed by the shield
    private Vector2 knockBackVector; // The vector that knocks the enemy back

    private Vector2 toPlayerVector; // The vector which keeps track of what the actual direction the enemy wants to go to
    private Vector2 currVelocity; // Vector for storing the current movement velocity

    private void Start()
    {
        // Initialize values
        playerTransform = GameManager.instance.playerTransform;
        movementSpeed = Random.Range(minMovementSpeed, maxMovementSpeed);


        // Initialize vectors
        toPlayerVector = Vector2.zero;
        currVelocity = Vector2.zero;
        knockBackVector = Vector2.zero;
        moveToPlayerCoroutine = StartCoroutine(MoveTowardsPlayer());

    }

    /* MOVEMENT METHODS */
    /// <summary>
    /// Always, constantly try to move towards the player to get close enough to hurt him
    /// </summary>
    private IEnumerator MoveTowardsPlayer()
    {
        float squaredMovementSpeed = Mathf.Pow(movementSpeed, 2);
        float movementSpeedHalfed = movementSpeed / 2;
        while (true)
        {
            // Initialize movement vectors
            toPlayerVector = playerTransform.position - transform.position;
            currVelocity.x = rb.velocity.x; currVelocity.y = rb.velocity.y;

            // 1st case: Enemy is moving too slow
            if ((currVelocity.sqrMagnitude - squaredMovementSpeed) < -0.05f)
            {
                currVelocity += toPlayerVector.normalized * velocityChangeSpeed;
            }
            // 3rd case: Enemy is moving too fast
            else if ((currVelocity.sqrMagnitude - squaredMovementSpeed) > 0.05f)
            {
                currVelocity -= currVelocity.normalized * velocityChangeSpeed;
            }
            // 2nd case: Enemy is moving at perfect speed
            else
            {
                currVelocity += (-currVelocity.normalized * movementSpeedHalfed) + (toPlayerVector.normalized * movementSpeedHalfed);
            }

            // Finally, set the velocity of the enemy
            rb.velocity = currVelocity;
            yield return null;
        }
    }

    /* SHIELD METHODS */

    /// <summary>
    /// Basic Enemy gets knocked back and paralyzed when hit with the shield
    /// </summary>
    /// <param name="collision"></param>
    private void OnShieldCollision(Collider2D collider)
    {
        if (paralyzeCoroutine != null) { StopCoroutine(paralyzeCoroutine); } // Stop previous paralyzation if it is currently active
        paralyzeCoroutine = StartCoroutine(ParalyzeCor(GameManager.instance.shieldControllerScript.enemyParalyzationSeconds)); // Start paralyzing the enemy

        // Call the playercombat script through the shield object to make the player lose the correct amount of energy
        GameManager.instance.shieldControllerScript.playerCombatScript.OnShieldProtect();
    }

    /* DEFAULT METHODS */

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("Shield"))
        {
            OnShieldCollision(collider);
            return;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Player"))
        {
            if (GameManager.instance.playerMovementScript.isRunning)
            {
                knockBackVector = transform.position - playerTransform.position;
                healthScript.TakeDamage(1);
                if (!isParalyzed)
                {
                    print("Pushing enemy back!");
                    rb.AddForce(knockBackVector.normalized * GameManager.instance.staminaShieldControllerScript.knockbackForce, ForceMode2D.Impulse);
                }
                if (isParalyzed)
                {
                    rb.AddForce(knockBackVector.normalized * GameManager.instance.staminaShieldControllerScript.knockbackForce, ForceMode2D.Impulse);
                }
                GameManager.instance.playerMovementScript.OnStaminaShieldProtect();
            }
            else // Player does not have stamina shield active so hurt him!
            {
                GameManager.instance.playerCombatScript.TakeDamage(2);
                Destroy(gameObject);
            }
        }
    }

    /* PARALYZATION METHODS */
    /// <summary>
    /// Updates the paralyzation boolean so the enemy knows to stop doing necessary work for limited time
    /// </summary>
    /// <param name="secondsOfParalyzation"></param>
    /// <returns></returns>
    private IEnumerator ParalyzeCor(float secondsOfParalyzation)
    {
        isParalyzed = true;
        StopCoroutine(moveToPlayerCoroutine);
        rb.gravityScale = 1f;
        spriteRenderer.color = paralyzedColor;
        yield return new WaitForSeconds(secondsOfParalyzation);
        rb.gravityScale = 0f;
        spriteRenderer.color = normalColor;
        moveToPlayerCoroutine = StartCoroutine(MoveTowardsPlayer());
        isParalyzed = false;
    }

}
