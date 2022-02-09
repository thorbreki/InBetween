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

    [Header("Player Shield functionality")]
    [SerializeField] private GameObject playerShieldObject;
    [SerializeField] private SpriteRenderer playerShieldSpriteRenderer;
    [SerializeField] private Color playerShieldColor; // Player Shield 1 color
    [SerializeField] private Color playerShield2Color; // Player Shield 2 color
    [SerializeField] private BoxCollider2D boxCollider;
    [SerializeField] private int playerShieldDamage; // Base Player Shield damage
    [SerializeField] private float doubleDamageSpeedLimit;
    private float sqrDoubleDamageSpeedLimit;
    private Coroutine playerShieldModeCoroutine;
    private bool isPlayerShield = false; // To let other parts of the code know that the enemy is currently a Player Shield 

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
        // Initialize objects and components
        playerShieldObject.SetActive(false);
        boxCollider.size = new Vector2(1, 1);

        // Initialize values
        playerTransform = GameManager.instance.playerTransform;
        movementSpeed = Random.Range(minMovementSpeed, maxMovementSpeed);
        sqrDoubleDamageSpeedLimit = doubleDamageSpeedLimit * doubleDamageSpeedLimit;


        // Initialize vectors
        toPlayerVector = Vector2.zero;
        currVelocity = Vector2.zero;
        knockBackVector = Vector2.zero;
        moveToPlayerCoroutine = StartCoroutine(MoveTowardsPlayer());

    }

    private void Update()
    {
        if (isPlayerShield)
        {
            ManagePlayerShieldSpeedDamage();
        }
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
        if (isParalyzed) { return; } // If previous paralyzation already active, don't do anything
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
        else if (collider.name == "Bomb(Clone)") // Being caught in an explosion
        {
            healthScript.TakeDamage(2); // Take damage
            float distance = Vector2.Distance(transform.position, collider.transform.position);
            float distanceMultiplier = Mathf.Max(1 - (distance / GameManager.instance.playerCombatScript.explosionRadius), 0f); // The farther the enemy is from explosion, less force
            print("distanceMultiplier: " + distanceMultiplier);

            // If the enemy is within the nearer half of the explosion: 2 damage, get paralyzed and Player Shield activate!
            if (distance <= (GameManager.instance.playerCombatScript.explosionRadius / 2f))
            {
                // Get paralyzed
                if (isParalyzed) { return; } // If previous paralyzation already active, don't do anything
                paralyzeCoroutine = StartCoroutine(ParalyzeCor(GameManager.instance.playerCombatScript.explosionParalyzationSeconds)); // Start paralyzing the enemy

                // Get knocked away from explosion
                rb.velocity = Vector2.zero; // To make sure the enemy gets knocked away from the explosion always the same amount of force no matter what
                rb.AddForce(distanceMultiplier * (GameManager.instance.playerCombatScript.explosionMaxForce * (transform.position - collider.transform.position).normalized), ForceMode2D.Impulse);

                StartCoroutine(PlayerShieldModeCor()); // Then, go into Player Shield mode
            }
            else
            {
                // Get knocked away from explosion
                rb.velocity = Vector2.zero; // To make sure the enemy gets knocked away from the explosion always the same amount of force no matter what
                rb.AddForce(distanceMultiplier * (GameManager.instance.playerCombatScript.explosionMaxForce * (transform.position - collider.transform.position).normalized), ForceMode2D.Impulse);
            }

            // NOTICE: The reason why there are two rb.AddForce lines instead of just one, is because this ensures that the enemy is paralyzed before being
            // pushed away from the bomb if it is close enough, so it will always go as fast it is supposed to. That would not be ensured if I would start
            // pushin the enemy away before paralyzing it
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
                rb.AddForce(knockBackVector.normalized * GameManager.instance.staminaShieldControllerScript.knockbackForce, ForceMode2D.Impulse);

                // The basic enemy spawns in the Player Shield to hurt other enemies
                if (isParalyzed)
                {
                    StartCoroutine(PlayerShieldModeCor());
                }
                GameManager.instance.playerMovementScript.OnStaminaShieldProtect();
            }
            else if (!isParalyzed) // Player does not have stamina shield active so hurt him!
            {
                GameManager.instance.playerCombatScript.TakeDamage(2);
                Destroy(gameObject);
            }
        }
        else if (collision.transform.CompareTag("PlayerShield"))
        {
            healthScript.TakeDamage(playerShieldDamage);
        }
        else if (collision.transform.CompareTag("PlayerShield2"))
        {
            healthScript.TakeDamage(playerShieldDamage * 2);
        }
    }

    private IEnumerator PlayerShieldModeCor()
    {
        yield return new WaitForSeconds(0.1f);
        isPlayerShield = true;
        ManagePlayerShieldSpeedDamage(); // Just to make sure the color will be correct when first initializing
        playerShieldObject.SetActive(true); // Set the Player Shield sprite renderer object active
        gameObject.tag = "PlayerShield"; // Change the tag so other enemies know what they are colliding with
        boxCollider.size = playerShieldObject.transform.localScale; // Set the collider to be as big as the Player Shield
    }

    /// <summary>
    /// Only runs when player shield is active. Handles the damage and color of the Player Shield according to the paralyzed enemy's movement speed
    /// </summary>
    private void ManagePlayerShieldSpeedDamage()
    {
        if (rb.velocity.sqrMagnitude < sqrDoubleDamageSpeedLimit)
        {
            if (!gameObject.CompareTag("PlayerShield")) { gameObject.tag = "PlayerShield"; } // Change tag so other enemies know the correct tag
            playerShieldSpriteRenderer.color = playerShieldColor; // Set the color
        }
        else
        {
            if (!gameObject.CompareTag("PlayerShield2")) { gameObject.tag = "PlayerShield2"; } // Change tag so other enemies know the correct tag
            playerShieldSpriteRenderer.color = playerShield2Color; // Set the color
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
        gameObject.layer = 14; // Set layer to Enemies, it collides with other enemies and bounces of walls
        spriteRenderer.color = paralyzedColor;
        yield return new WaitForSeconds(secondsOfParalyzation);
        gameObject.layer = 8; // Setting the layer to Basic Enemy layer yet again
        spriteRenderer.color = normalColor;

        // Take away Player Shield mode just in case
        playerShieldObject.SetActive(false);
        isPlayerShield = false;
        gameObject.tag = "Enemy";
        boxCollider.size = Vector2.one;

        moveToPlayerCoroutine = StartCoroutine(MoveTowardsPlayer());
        isParalyzed = false;
    }


    private void OnDestroy()
    {
        StopAllCoroutines();
    }
}
