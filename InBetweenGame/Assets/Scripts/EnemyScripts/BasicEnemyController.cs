using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicEnemyController : ParentEnemyController
{
    [Header("-- BASIC ENEMY --")]

    [Header("Movement Variables")]
    [SerializeField] private float velocityChangeSpeed; // How fast the enemy can change his velocity's direction
    private Coroutine moveToPlayerCoroutine; // Coroutine object for enemy moving towards player



    private Vector2 toPlayerVector; // The vector which keeps track of what the actual direction the enemy wants to go to
    private Vector2 currVelocity; // Vector for storing the current movement velocity

    protected override void Start()
    {
        base.Start();

        // Initialize vectors
        toPlayerVector = Vector2.zero;
        currVelocity = Vector2.zero;
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
            currVelocity.x = rigidBody.velocity.x; currVelocity.y = rigidBody.velocity.y;

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
            rigidBody.velocity = currVelocity;
            yield return null;
        }
    }


    /* DEFAULT METHODS */

    /// <summary>
    /// The Basic Enemy dies when colliding with the player
    /// </summary>
    protected override void WhenCollidingWithPlayer()
    {
        if (GameManager.instance.playerMovementScript.isRunning)
        {
            knockBackVector = transform.position - playerTransform.position;
            healthScript.TakeDamage(1);
            rigidBody.AddForce(knockBackVector.normalized * GameManager.instance.staminaShieldControllerScript.knockbackForce, ForceMode2D.Impulse);

            // The basic enemy spawns in the Player Shield to hurt other enemies
            if (isParalyzed)
            {
                StartCoroutine(PlayerShieldModeCor());
            }
            GameManager.instance.playerMovementScript.OnStaminaShieldProtect();
        }
        else if (!isParalyzed) // Player does not have stamina shield active so hurt him!
        {
            GameManager.instance.playerCombatScript.TakeDamage(damageToPlayer);
            healthScript.Die(); // Promptly die
        }
    }

    /* PARALYZATION METHODS */
    /// <summary>
    /// Updates the paralyzation boolean so the enemy knows to stop doing necessary work for limited time
    /// </summary>
    /// <param name="secondsOfParalyzation"></param>
    /// <returns></returns>
    protected override IEnumerator ParalyzeCor(float secondsOfParalyzation)
    {
        isParalyzed = true;
        StopCoroutine(moveToPlayerCoroutine);
        gameObject.layer = 14; // Set layer to Enemies, it collides with other enemies and bounces of walls
        spriteRenderer.color = paralyzedColor;
        yield return new WaitForSeconds(secondsOfParalyzation);
        gameObject.layer = 8; // Setting the layer to Basic Enemy layer yet again
        spriteRenderer.color = normalColor;

        // Take away Player Shield mode just in case
        playerShieldObject.SetActive(false);
        rigidBody.mass = 1;
        isPlayerShield = false;
        gameObject.tag = "Enemy";
        boxCollider.size = Vector2.one;

        moveToPlayerCoroutine = StartCoroutine(MoveTowardsPlayer());
        isParalyzed = false;
    }
}
