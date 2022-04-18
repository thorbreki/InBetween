using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParentEnemyController : MonoBehaviour
{
    [Header("-- PARENT --")]
    [Header("Object and Component References")]
    [SerializeField] protected Transform playerTransform; // Set by the parent spawner
    [SerializeField] protected EnemyHealthController healthScript; // The health script attached to the same enemy object
    [SerializeField] protected BoxCollider2D boxCollider;

    [SerializeField] protected Rigidbody2D rigidBody;
    [SerializeField] protected SpriteRenderer spriteRenderer;

    [Header("Paralyzed Attributes")]
    [SerializeField] protected float wallDamageMagnitudeLimit;
    [SerializeField] protected float wallCollisionDamage;
    [SerializeField] protected Color normalColor;
    [SerializeField] protected Color paralyzedColor;
    protected bool isParalyzed = false;
    protected Coroutine paralyzeCoroutine; // Coroutine object for when the enemy gets paralyzed by the shield
    protected Vector2 knockBackVector; // The vector that knocks the enemy back
    protected float sqrWallDamageMagnitudeLimit;

    [Header("Player Shield functionality")]
    [SerializeField] protected GameObject playerShieldObject;
    [SerializeField] protected SpriteRenderer playerShieldSpriteRenderer;
    [SerializeField] protected Color playerShieldColor; // Player Shield 1 color
    [SerializeField] protected Color playerShield2Color; // Player Shield 2 color
    [SerializeField] protected int playerShieldDamage; // Base Player Shield damage
    [SerializeField] protected float doubleDamageSpeedLimit;
    protected float sqrDoubleDamageSpeedLimit;
    protected bool isPlayerShield = false; // To let other parts of the code know that the enemy is currently a Player Shield 
    protected Coroutine playerShieldModeCoroutine;

    [Header("Movement Attributes")]
    [SerializeField] protected float minMovementSpeed;
    [SerializeField] protected float maxMovementSpeed;
    protected float movementSpeed;

    [Header("Attack Attributes")]
    [SerializeField] protected int baseDamageToPlayer = 2;
    protected int damageToPlayer; // The damage this enemy does to the player


    /// <summary>
    /// Sets sqrWallDamageMagnitudeLimit;
    /// Sets sqrDoubleDamageSpeedLimit;
    /// Sets knockBackVector;
    /// Sets playerShieldObject to inactive;
    /// Sets playerTransform;
    /// </summary>
    protected virtual void Start()
    {
        boxCollider.size = new Vector2(1, 1);

        movementSpeed = Random.Range(minMovementSpeed, maxMovementSpeed) + ApplicationManager.instance.currLevelData.enemySpeedBoost;
        damageToPlayer = baseDamageToPlayer + ApplicationManager.instance.currLevelData.enemyDamageBoost;

        sqrWallDamageMagnitudeLimit = wallDamageMagnitudeLimit * wallDamageMagnitudeLimit;
        sqrDoubleDamageSpeedLimit = doubleDamageSpeedLimit * doubleDamageSpeedLimit;
        knockBackVector = Vector2.zero;
        playerShieldObject.SetActive(false);
        playerTransform = GameManager.instance.playerTransform;
    }

    protected virtual void Update()
    {
        if (isPlayerShield)
        {
            ManagePlayerShieldSpeedDamage();
        }
    }


    /// <summary>
    /// Only runs when player shield is active. Handles the damage and color of the Player Shield according to the paralyzed enemy's movement speed
    /// </summary>
    protected virtual void ManagePlayerShieldSpeedDamage()
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


    /// <summary>
    /// enemy gets knocked back and paralyzed when hit with the shield
    /// </summary>
    /// <param name="collider"></param>
    protected virtual void OnShieldCollision()
    {
        // Paralyze the enemy
        if (paralyzeCoroutine != null) { StopCoroutine(paralyzeCoroutine); } // Stop previous paralyzation if it is currently active
        paralyzeCoroutine = StartCoroutine(ParalyzeCor(GameManager.instance.shieldControllerScript.enemyParalyzationSeconds)); // Start paralyzing the enemy

        // Call the playercombat script through the shield object to make the player lose the correct amount of energy
        GameManager.instance.playerCombatScript.OnShieldProtect();
    }

    /// <summary>
    /// When the enemy becomes a Player Shield, this coroutine runs
    /// </summary>
    /// <returns></returns>
    protected virtual IEnumerator PlayerShieldModeCor()
    {
        yield return new WaitForSeconds(0.1f);
        isPlayerShield = true;
        ManagePlayerShieldSpeedDamage(); // Just to make sure the color will be correct when first initializing
        rigidBody.mass = 20;
        playerShieldObject.SetActive(true); // Set the Player Shield sprite renderer object active
        gameObject.tag = "PlayerShield"; // Change the tag so other enemies know what they are colliding with
        boxCollider.size = playerShieldObject.transform.localScale; // Set the collider to be as big as the Player Shield
    }

    /// <summary>
    /// Handles when enemy trigger collides with Gun Shield;
    /// </summary>
    /// <param name="collider"></param>
    protected virtual void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("Shield"))
        {
            OnShieldCollision();
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

    /// <summary>
    /// When enemy collides with player, this function runs
    /// </summary>
    protected virtual void WhenCollidingWithPlayer()
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
            GameManager.instance.playerCombatScript.TakeDamage(damageToPlayer);
        }
    }

    /// <summary>
    /// When enemy collides with level border, this function runs
    /// </summary>
    protected virtual void WhenCollidingWithLevelBorder(Collision2D collision)
    {
        if (isParalyzed && (collision.relativeVelocity.sqrMagnitude >= sqrWallDamageMagnitudeLimit))
        {
            healthScript.TakeDamage(wallCollisionDamage);
        }
    }

    /// <summary>
    /// When enemy collides with player shield, this function runs
    /// </summary>
    protected virtual void WhenCollidingWithPlayerShield()
    {
        healthScript.TakeDamage(playerShieldDamage);
    }

    /// <summary>
    /// When enemy collides with player shield 2, this function runs
    /// </summary>
    protected virtual void WhenCollidingWithPlayerShield2()
    {
        healthScript.TakeDamage(playerShieldDamage * 2);
    }

    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Player"))
        {
            WhenCollidingWithPlayer();
        }
        else if (collision.transform.CompareTag("LevelBorder"))
        {
            WhenCollidingWithLevelBorder(collision);
        }
        else if (collision.transform.CompareTag("PlayerShield"))
        {
            WhenCollidingWithPlayerShield();
        }
        else if (collision.transform.CompareTag("PlayerShield2"))
        {
            WhenCollidingWithPlayerShield2();
        }
    }

    /// <summary>
    /// A placeholder for the specific enemies to override, that way keeping OnShieldCollision method in parent class
    /// </summary>
    /// <param name="secondsOfParalyzation"></param>
    /// <returns>nothing</returns>
    protected virtual IEnumerator ParalyzeCor(float secondsOfParalyzation)
    {
        yield return new WaitForSeconds(5f);
    }

    protected virtual void OnDestroy()
    {
        StopAllCoroutines();
    }
}
