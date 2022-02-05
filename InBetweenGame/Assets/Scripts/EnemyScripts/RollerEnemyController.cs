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

    [Header("Player Shield functionality")]
    [SerializeField] private GameObject playerShieldObject;
    [SerializeField] private CircleCollider2D circleCollider;
    [SerializeField] private int playerShieldDamage;
    private Coroutine playerShieldModeCoroutine;

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

        // Initializing the movement values so they can be used
        movementForce = Random.Range(minMovementForce, maxMovementForce);
        movementSpeedLimit = Random.Range(minMovementSpeed, maxMovementSpeed);

        attackPlayerVector = Vector2.zero; // Initialize the vector
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

        // Colliding with a Player Shield
        else if (collision.transform.CompareTag("PlayerShield"))
        {
            healthScript.TakeDamage(1);
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
        playerShieldObject.SetActive(true); // Set the Player Shield sprite renderer object active
        gameObject.tag = "PlayerShield"; // Change the tag so other enemies know what they are colliding with
        circleCollider.radius = playerShieldObject.transform.localScale.x / 2; // Set the collider to be as big as the Player Shield
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
