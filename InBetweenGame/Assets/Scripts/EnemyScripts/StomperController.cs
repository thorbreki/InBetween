using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StomperController : MonoBehaviour
{
    public Transform playerTransform;

    [Header("Components and Objects")]
    [SerializeField] private Rigidbody2D rigidBody;
    [SerializeField] private GameObject indicatorObject;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private EnemyHealthController healthScript;
                     public PlayerCombat playerCombatScript;

    [Header("Movement Attributes")]
    [SerializeField] private float minMovementSpeed;
    [SerializeField] private float maxMovementSpeed;
    [SerializeField] private float velocityChangeSpeed; // How fast the enemy can change his velocity's direction
    private float movementSpeed;

    [Header("Attack Attributes")]
    [SerializeField] private float minAttackForce; // The minimum speed the enemy goes down to kill
    [SerializeField] private float maxAttackForce; // The maximum speed the enemy goes down to kill

    [Header("Paralyzed Attributes")]
    [SerializeField] private Color normalColor;
    [SerializeField] private Color paralyzedColor;
    [SerializeField] private float wallDamageMagnitudeLimit;
    [SerializeField] private float wallCollisionDamage;
    private float sqrWallDamageMagnitudeLimit;

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

    private GameObject indicator; // The transform component of the indicator object

    private float attackForce;

    // SHIELD FUNCTIONALITY
    private bool isParalyzed = false;
    private Coroutine paralyzeCoroutine; // Coroutine object for when the enemy gets paralyzed by the shield
    private Vector2 knockBackVector; // The vector that knocks the enemy back

    private Vector2 desiredDirection;
    private Vector2 attackVector;
    private Vector2 currVelocity; // Keeping track of current velocity of the StomperEnemy

    private Coroutine moveCor;
    private Coroutine updateDesiredDirectionCoroutine;
    private Coroutine attackPlayerCor;

    private void Start()
    {
        // Initialize objects and components
        playerShieldObject.SetActive(false);
        boxCollider.size = new Vector2(1, 1);

        // Initialize vectors
        desiredDirection = Vector2.one;
        knockBackVector = Vector2.zero;
        currVelocity = Vector2.zero;

        indicator = Instantiate(indicatorObject, transform.position, Quaternion.identity);

        movementSpeed = Random.Range(minMovementSpeed, maxMovementSpeed);
        attackForce = Random.Range(minAttackForce, maxAttackForce);
        sqrDoubleDamageSpeedLimit = doubleDamageSpeedLimit * doubleDamageSpeedLimit;
        sqrWallDamageMagnitudeLimit = wallDamageMagnitudeLimit * wallDamageMagnitudeLimit;
        attackVector = new Vector2(0, -attackForce);

        moveCor = StartCoroutine(MoveToDesiredPosCor());
        updateDesiredDirectionCoroutine = StartCoroutine(UpdateDesiredDirectionCor());
    }

    private void Update()
    {
        if (isPlayerShield)
        {
            ManagePlayerShieldSpeedDamage();
        }

        indicator.transform.position = transform.position; // Always update the indicator object's position

        if (isParalyzed) { return; } // Past this point, all code will not run if this enemy is paralyzed

        if (Mathf.Abs(GameManager.instance.playerTransform.position.x - transform.position.x) <= 0.99f)
        {
            if (attackPlayerCor != null) { return; } // Make sure to not activate the attack coroutine again and again
            if (moveCor != null) { StopCoroutine(moveCor); moveCor = null; } // Make sure that the enemy is not trying to get to its desired spot anymore
            attackPlayerCor = StartCoroutine(AttackPlayer()); // Start the attack coroutine
        }
    }

    /// <summary>
    /// Always, constantly try to move towards the player to get close enough to hurt him
    /// </summary>
    private IEnumerator MoveToDesiredPosCor()
    {
        attackPlayerCor = null; // Just to tell the script that the enemy has stopped attacking and is moving normally

        // Initialize movement vectors

        float squaredMovementSpeed = Mathf.Pow(movementSpeed, 2);
        float movementSpeedHalfed = movementSpeed / 2;
        while (true)
        {
            currVelocity.x = rigidBody.velocity.x; currVelocity.y = rigidBody.velocity.y;

            // 1st case: Enemy is moving too slow
            if ((currVelocity.sqrMagnitude - squaredMovementSpeed) < -0.05f)
            {
                currVelocity += desiredDirection.normalized * velocityChangeSpeed;
            }
            // 3rd case: Enemy is moving too fast
            else if ((currVelocity.sqrMagnitude - squaredMovementSpeed) > 0.05f)
            {
                currVelocity -= currVelocity.normalized * velocityChangeSpeed;
            }
            // 2nd case: Enemy is moving at perfect speed
            else
            {
                currVelocity += (-currVelocity.normalized * movementSpeedHalfed) + (desiredDirection.normalized * movementSpeedHalfed);
            }

            // Finally, set the velocity of the enemy
            rigidBody.velocity = currVelocity;
            yield return null;
        }
    }

    /// <summary>
    /// Updates the desired direction of the movement of the StomperEnemy
    /// </summary>
    /// <returns></returns>
    private IEnumerator UpdateDesiredDirectionCor()
    {
        float x; float y;

        while (true)
        {
            // Generate random x and y values for the target vector
            x = Random.Range(-1f, 1f);
            y = Random.Range(-1f, 1f);

            desiredDirection.x = x; desiredDirection.y = y;
            yield return new WaitForSeconds(Random.Range(0.5f, 5f));

        }
    }


    private IEnumerator AttackPlayer()
    {
        yield return new WaitForSeconds(0.1f);
        desiredDirection.x = 0;
        desiredDirection.y = 0;
        rigidBody.velocity = desiredDirection;

        rigidBody.AddForce(attackVector, ForceMode2D.Impulse);

        yield return new WaitForSeconds(3f);
        moveCor = StartCoroutine(MoveToDesiredPosCor());
    }


    /// <summary>
    /// Basic Enemy gets knocked back and paralyzed when hit with the shield
    /// </summary>
    /// <param name="collision"></param>
    private void OnShieldCollision(Collider2D collider)
    {
        if (isParalyzed) { return; } // If enemy is already paralyzed, don't do anything
        paralyzeCoroutine = StartCoroutine(ParalyzeCor(GameManager.instance.shieldControllerScript.enemyParalyzationSeconds)); // Start paralyzing the enemy

        // Call the playercombat script through the shield object to make the player lose the correct amount of energy
        GameManager.instance.shieldControllerScript.playerCombatScript.OnShieldProtect();
    }


    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("Shield"))
        {
            OnShieldCollision(collider);
        }

        else if (collider.name == "Bomb(Clone)") // Being caught in an explosion
        {
            healthScript.TakeDamage(2); // Take damage
            float distance = Vector2.Distance(transform.position, collider.transform.position);
            float distanceMultiplier = Mathf.Max(1 - (distance / GameManager.instance.playerCombatScript.explosionRadius), 0f); // The farther the enemy is from explosion, less force
            print("distanceMultiplier: " + distanceMultiplier);

            // If the enemy is within the nearer half of the explosion: 2 damage, get paralyzed and Player Shield activate!
            if (distanceMultiplier >= 0.5f)
            {
                // Get paralyzed
                if (paralyzeCoroutine != null)
                {
                    StopCoroutine(paralyzeCoroutine);
                }     // If previous paralyzation already active, don't do anything
                paralyzeCoroutine = StartCoroutine(ParalyzeCor(GameManager.instance.playerCombatScript.explosionParalyzationSeconds)); // Start paralyzing the enemy

                // Get knocked away from explosion
                rigidBody.velocity = Vector2.zero; // To make sure the enemy gets knocked away from the explosion always the same amount of force no matter what
                rigidBody.AddForce(distanceMultiplier * (GameManager.instance.playerCombatScript.explosionMaxForce * (transform.position - collider.transform.position).normalized), ForceMode2D.Impulse);

                StartCoroutine(PlayerShieldModeCor()); // Then, go into Player Shield mode
            }
            else
            {
                // Get knocked away from explosion
                rigidBody.velocity = Vector2.zero; // To make sure the enemy gets knocked away from the explosion always the same amount of force no matter what
                rigidBody.AddForce(distanceMultiplier * (GameManager.instance.playerCombatScript.explosionMaxForce * (transform.position - collider.transform.position).normalized), ForceMode2D.Impulse);
            }

            // NOTICE: The reason why there are two rb.AddForce lines instead of just one, is because this ensures that the enemy is paralyzed before being
            // pushed away from the bomb if it is close enough, so it will always go as fast it is supposed to. That would not be ensured if I would start
            // pushin the enemy away before paralyzing it
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Player")) {
            if (GameManager.instance.playerMovementScript.isRunning)
            {
                knockBackVector = transform.position - playerTransform.position;
                healthScript.TakeDamage(1);
                rigidBody.AddForce(knockBackVector.normalized * GameManager.instance.staminaShieldControllerScript.knockbackForce, ForceMode2D.Impulse);

                if (isParalyzed)
                {
                    StartCoroutine(PlayerShieldModeCor());
                }
                GameManager.instance.playerMovementScript.OnStaminaShieldProtect(); // Let shield know it protected the player
            }

            else if (!isParalyzed) // The player does not have staminashield active and you're good to go so hurt that b-word!
            {
                playerCombatScript.TakeDamage(2);
            }
        }
        else if (collision.transform.CompareTag("LevelBorder"))
        {
            if (isParalyzed && (collision.relativeVelocity.sqrMagnitude >= sqrWallDamageMagnitudeLimit))
            {
                healthScript.TakeDamage(wallCollisionDamage);
            }
        }
        else if (collision.transform.CompareTag("PlayerShield"))
        {
            healthScript.TakeDamage(1);
        }
        else if (collision.transform.CompareTag("PlayerShield2"))
        {
            healthScript.TakeDamage(playerShieldDamage * 2);
        }
    }

    private IEnumerator ParalyzeCor(float secondsOfParalyzation)
    {
        isParalyzed = true;
        spriteRenderer.color = paralyzedColor;
        gameObject.layer = 4; // Enemy now has the bouncy layer equipped
        // Make sure all movement and attacks are completely stopped
        if (moveCor != null) { StopCoroutine(moveCor); }
        if (attackPlayerCor != null) { StopCoroutine(attackPlayerCor); }

        yield return new WaitForSeconds(secondsOfParalyzation);
        gameObject.layer = 10; // Back to the normal StomperEnemy layer
        spriteRenderer.color = normalColor;

        // Take away Player Shield mode just in case
        playerShieldObject.SetActive(false);
        rigidBody.mass = 1;
        isPlayerShield = false;
        gameObject.tag = "Enemy";
        boxCollider.size = Vector2.one;

        moveCor = StartCoroutine(MoveToDesiredPosCor()); // Start moving again!
        isParalyzed = false;
    }


    private IEnumerator PlayerShieldModeCor()
    {
        yield return new WaitForSeconds(0.1f);
        isPlayerShield = true;
        ManagePlayerShieldSpeedDamage(); // Just to make sure the color will be correct when first initializing
        rigidBody.mass = 10;
        playerShieldObject.SetActive(true); // Set the Player Shield sprite renderer object active
        gameObject.tag = "PlayerShield"; // Change the tag so other enemies know what they are colliding with
        boxCollider.size = playerShieldObject.transform.localScale; // Set the collider to be as big as the Player Shield
    }


    /// <summary>
    /// Only runs when player shield is active. Handles the damage and color of the Player Shield according to the paralyzed enemy's movement speed
    /// </summary>
    private void ManagePlayerShieldSpeedDamage()
    {
        if (rigidBody.velocity.sqrMagnitude < sqrDoubleDamageSpeedLimit)
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


    private void OnDestroy()
    {
        Destroy(indicator);
        StopAllCoroutines();
    }
}
