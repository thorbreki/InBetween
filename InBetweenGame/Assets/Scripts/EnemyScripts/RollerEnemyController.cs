using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RollerEnemyController : MonoBehaviour
{
    public Transform playerTransform;

    [SerializeField] private Rigidbody2D rigidBody; // The Roller's rigidBody component
    [SerializeField] private SpriteRenderer spriteRenderer; // The Roller's SpriteRenderer component
    [SerializeField] private EnemyHealthController healthScript; // The Roller's health controller script

    // The range of how much force the Roller adds to its rigidbody when trying to roll into the player
    [SerializeField] private float minMovementForce;
    [SerializeField] private float maxMovementForce;

    // The maximum x-velocity the roller can add to itself
    [SerializeField] private float minMovementSpeed;
    [SerializeField] private float maxMovementSpeed;

    // How long does the roller have to wait before able to roll again
    [SerializeField] private float minMovementCooldown;
    [SerializeField] private float maxMovementCooldown;

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
    [SerializeField] private CircleCollider2D circleCollider;
    [SerializeField] private int playerShieldDamage;
    [SerializeField] private float doubleDamageSpeedLimit;
    private float sqrDoubleDamageSpeedLimit;
    private Coroutine playerShieldModeCoroutine;
    private bool isPlayerShield = false; // To let other parts of the code know that the enemy is currently a Player Shield 

    private float movementForce; // How much force is added to the Roller when trying to roll into the player
    private float movementSpeedLimit; // How fast the Roller can go before stop being able to add force to itself (its maximum speed)

    // SHIELD FUNCTIONALITY
    private bool isParalyzed = false;
    private Coroutine paralyzeCoroutine; // Coroutine object for when the enemy gets paralyzed by the shield
    private Vector2 knockBackVector; // The vector that knocks the enemy back

    private Coroutine attackPlayerCor;

    private Vector2 attackPlayerVector; // The vector the roller uses to roll into the Player

    private void Start()
    {
        // Initialize objects and components
        playerShieldObject.SetActive(false);
        circleCollider.radius = 0.5f;

        sqrDoubleDamageSpeedLimit = doubleDamageSpeedLimit * doubleDamageSpeedLimit;

        // Initializing the movement values so they can be used
        movementForce = Random.Range(minMovementForce, maxMovementForce);
        movementSpeedLimit = Random.Range(minMovementSpeed, maxMovementSpeed);
        sqrWallDamageMagnitudeLimit = wallDamageMagnitudeLimit * wallDamageMagnitudeLimit;

        attackPlayerVector = Vector2.zero; // Initialize the vector
    }

    private void Update()
    {
        if (isPlayerShield)
        {
            ManagePlayerShieldSpeedDamage();
        }
    }

    /// <summary>
    /// Basic Enemy gets knocked back and paralyzed when hit with the shield
    /// </summary>
    /// <param name="collision"></param>
    private void OnShieldCollision(Collider2D collider)
    {
        // Paralyze the Rolller
        if (paralyzeCoroutine != null) { StopCoroutine(paralyzeCoroutine); } // Stop previous paralyzation if it is currently active
        paralyzeCoroutine = StartCoroutine(ParalyzeCor(GameManager.instance.shieldControllerScript.enemyParalyzationSeconds)); // Start paralyzing the enemy

        // Call the playercombat script through the shield object to make the player lose the correct amount of energy
        GameManager.instance.playerCombatScript.OnShieldProtect();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // When Roller speeds up into a wall, it must stop trying to move and start over
        if (collision.transform.CompareTag("LevelBorder") && isSpeedingIntoAWall()) // The Roller just bumped into a wall but is still trying to speed up
        {
            if (attackPlayerCor != null) { StopCoroutine(attackPlayerCor); }
            attackPlayerCor = StartCoroutine(AttackPlayerCoroutine());
        }

        // This code runs when the Roller lands on the ground for the first time, thus starting the movement process
        else if (collision.transform.CompareTag("LevelBorder") && attackPlayerCor == null)
        {
            attackPlayerCor = StartCoroutine(AttackPlayerCoroutine());
            return;
        }
        else if (collision.transform.CompareTag("LevelBorder"))
        {
            if (isParalyzed && (collision.relativeVelocity.sqrMagnitude >= sqrWallDamageMagnitudeLimit))
            {
                healthScript.TakeDamage(wallCollisionDamage);
            }
        }
        // Colliding with a Player Shield
        else if (collision.transform.CompareTag("PlayerShield"))
        {
            healthScript.TakeDamage(1);
        }
        else if (collision.transform.CompareTag("PlayerShield2"))
        {
            healthScript.TakeDamage(playerShieldDamage * 2);
        }

        // When it collides with player
        else if (collision.transform.CompareTag("Player"))
        {
            if (GameManager.instance.playerMovementScript.isRunning)
            {
                knockBackVector = transform.position - playerTransform.position;
                healthScript.TakeDamage(1);
                rigidBody.AddForce(knockBackVector.normalized * GameManager.instance.staminaShieldControllerScript.knockbackForce, ForceMode2D.Impulse);

                if (isParalyzed)
                {
                    StartCoroutine(PlayerShieldModeCor());
                }
                GameManager.instance.playerMovementScript.OnStaminaShieldProtect();
            }
            else if (!isParalyzed) // Player does not have stamina shield active so hurt him!
            {
                GameManager.instance.playerCombatScript.TakeDamage(2);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("Shield") && !isParalyzed)
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

    private IEnumerator PlayerShieldModeCor()
    {
        yield return new WaitForSeconds(0.1f);
        isPlayerShield = true;
        ManagePlayerShieldSpeedDamage();
        rigidBody.mass = 10;
        playerShieldObject.SetActive(true); // Set the Player Shield sprite renderer object active
        gameObject.tag = "PlayerShield"; // Change the tag so other enemies know what they are colliding with
        circleCollider.radius = playerShieldObject.transform.localScale.x / 2; // Set the collider to be as big as the Player Shield
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

    private IEnumerator ParalyzeCor(float secondsOfParalyzation)
    {
        isParalyzed = true;
        // Make sure all movement and attacks are completely stopped
        if (attackPlayerCor != null) { StopCoroutine(attackPlayerCor); }
        spriteRenderer.color = paralyzedColor;
        gameObject.layer = 3; // Set the Roller to layer "UnBouncy enemy" so he collides with all other enemies but does not bounce
        yield return new WaitForSeconds(secondsOfParalyzation);
        spriteRenderer.color = normalColor;
        gameObject.layer = 11; // Go back to RollerEnemy layer

        // Take away Player Shield mode just in case
        playerShieldObject.SetActive(false);
        rigidBody.mass = 1;
        isPlayerShield = false;
        gameObject.tag = "Enemy";
        circleCollider.radius = 0.5f;

        attackPlayerCor = StartCoroutine(AttackPlayerCoroutine()); // Start moving again!
        isParalyzed = false;
    }

    private bool isSpeedingIntoAWall()
    {
        bool speedingRightIntoRightWall = attackPlayerVector.x > 0 && transform.position.x > 0; // Is speeding to the right into the right level border
        bool speedingLeftIntoLeftWall = attackPlayerVector.x < 0 && transform.position.x < 0; // Is speeding to the left into the left level border

        return speedingRightIntoRightWall || speedingLeftIntoLeftWall;
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }
}
