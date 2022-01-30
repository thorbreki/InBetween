using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileEnemyController : MonoBehaviour
{
    [Header("Components and Objects")]
    [SerializeField] private EnemyHealthController healthScript;
    [SerializeField] private Rigidbody2D rigidBody; // The rigidbody component of the enemy
    [SerializeField] private GameObject projectileObject; // The projectile game object
    [SerializeField] private SpriteRenderer spriteRenderer; // The sprite renderer component

    [Header("Movement Attributes")]
    [SerializeField] private float minDistance; // The lower bound of the random distance the enemy wants to keep from the player
    [SerializeField] private float maxDistance; // The higher bound (EXCLUSIVE IN THE RANDOM FUNCTION)
    [SerializeField] private float movementSpeed; // The enemy's movementSpeed
    [SerializeField] private float velocityChangeSpeed; // How fast the enemy can change its course

    [Header("Attack Attributes")]
    [SerializeField] private float minProjectileSpeed; // the min possible speed of the projectiles
    [SerializeField] private float maxProjectileSpeed; // The max possible speed of the projectiles
    [SerializeField] private float minProjectileCooldown; // the min possible time between shooting (in seconds)
    [SerializeField] private float maxProjectileCooldown; // the max possible time between shooting (in seconds)
    [SerializeField] private float shootingInaccuracy; // How inaccurate the player's shooting is

    [Header("Paralyzed Attributes")]
    [SerializeField] private Color normalColor;
    [SerializeField] private Color paralyzedColor;

    [Header("Player Shield functionality")]
    [SerializeField] private GameObject playerShieldObject;
    [SerializeField] private BoxCollider2D boxCollider;
    [SerializeField] private int playerShieldDamage;
    private Coroutine playerShieldModeCoroutine;

    private float projectileSpeed; // The speed of the projectiles this enemy shoots
    private float projectileCooldown; // The seconds between the enemy can shoot projectiles
    private int targetSideOfPlayer; // The enemy will always want to be either side of the player. 0 = left of player, 1 = right of player

    public Transform playerTransform;
    private float targetDistanceFromGround;
    private float targetXDistanceFromPlayer;

    // SHIELD FUNCTIONALITY
    private ShieldController shieldControllerScript; // The shield's controller script to know important info
    private bool isParalyzed = false;
    private Coroutine paralyzeCoroutine; // Coroutine object for when the enemy gets paralyzed by the shield
    private Vector2 knockBackVector; // The vector that knocks the enemy back
    private Vector2 currVelocity; // The vector to store the current movement velocity the Enemy has

    private Vector2 toPlayerVector; // The vector which defines where the enemy

    private Coroutine movementCoroutine; // Coroutine object for movement of Projectile Enemy
    private Coroutine shootPlayerCoroutine; // Coroutine object for shooting the player


    private void Start()
    {
        // Initialize objects and components
        playerShieldObject.SetActive(false);
        boxCollider.size = new Vector2(1, 1);

        targetDistanceFromGround = Random.Range(minDistance, maxDistance); // Make the enemy randomly far from the floor
        targetXDistanceFromPlayer = Random.Range(minDistance, maxDistance); // Make the enemy randomly far from the player on the x coordinate

        projectileSpeed = Random.Range(minProjectileSpeed, maxProjectileSpeed); // Initializing the speed of the projectiles this enemy shoots
        projectileCooldown = Random.Range(minProjectileCooldown, maxProjectileCooldown); // Initializing how fast the enemy can shoot

        targetSideOfPlayer = Random.Range(0, 2);

        // Initialize the vectors so they can be used
        toPlayerVector = Vector2.zero;
        currVelocity = Vector2.zero;
        knockBackVector = Vector2.zero;

        // Start functionality coroutines
        movementCoroutine = StartCoroutine(MoveTowardsPlayer());
        shootPlayerCoroutine = StartCoroutine(ShootPlayerCor());
    }

    /// <summary>
    /// Run this function to let this enemy know the Transform component of the player
    /// </summary>
    /// <param name="thePlayerTransform">The player's Transform component</param>
    public void SetPlayerTransform(Transform thePlayerTransform)
    {
        playerTransform = thePlayerTransform;
    }

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
            SetUpPlayerVector();
            currVelocity.x = rigidBody.velocity.x; currVelocity.y = rigidBody.velocity.y;

            // 0th case: Enemy has managed to get to target place
            if (toPlayerVector.x == 0 && toPlayerVector.y == 0) { currVelocity.x = 0; currVelocity.y = 0; }

            // 1st case: Enemy is moving too slow
            else if ((currVelocity.sqrMagnitude - squaredMovementSpeed) < -0.05f)
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

    /// <summary>
    /// Sets all the movement variables to correct values before the actual move call in the movement coroutine
    /// </summary>
    private void SetUpPlayerVector()
    {
        // The target x position will now be randomly right or left side of the player always
        float targetXPosition = targetSideOfPlayer == 1 ? playerTransform.position.x + targetXDistanceFromPlayer : playerTransform.position.x - targetXDistanceFromPlayer;
        toPlayerVector.x = targetXPosition - transform.position.x;
        toPlayerVector.y = targetDistanceFromGround - transform.position.y;

        if (toPlayerVector.sqrMagnitude <= 0.05f)
        {
            toPlayerVector.x = 0;
            toPlayerVector.y = 0;
        }
    }

    private IEnumerator ShootPlayerCor()
    {
        while (playerTransform == null)
        {
            yield return new WaitForSeconds(0.1f);
        }

        while (true)
        {
            yield return new WaitForSeconds(projectileCooldown);
            //if (transform.position.y > maxDistance) { continue; } // Make sure that the shooter can't shoot when it is really far away from the player
            GameObject projectile = Instantiate(projectileObject, transform.position, transform.rotation); // Create the projectile
            //projectile.GetComponent<Rigidbody2D>().velocity = (playerTransform.position - transform.position).normalized * projectileSpeed; // Make it start moving towards player
            projectile.GetComponent<Rigidbody2D>().velocity = Quaternion.Euler(0f, 0f, Random.Range(-shootingInaccuracy, shootingInaccuracy)) * (playerTransform.position - transform.position).normalized * projectileSpeed; // Make it start moving towards player
        }
    }


    /// <summary>
    /// Basic Enemy gets knocked back and paralyzed when hit with the shield
    /// </summary>
    /// <param name="collision"></param>
    private void OnShieldCollision(Collider2D collider)
    {
        if (shieldControllerScript == null) { shieldControllerScript = collider.gameObject.GetComponent<ShieldController>(); } // Get the shield controller script if not already

        if (isParalyzed) { return; } // Stop previous paralyzation if it is currently active
        paralyzeCoroutine = StartCoroutine(ParalyzeCor(shieldControllerScript.enemyParalyzationSeconds)); // Start paralyzing the enemy

        // Call the playercombat script through the shield object to make the player lose the correct amount of energy
        shieldControllerScript.playerCombatScript.OnShieldProtect();
    }
    private IEnumerator ParalyzeCor(float secondsOfParalyzation)
    {
        isParalyzed = true;
        // Make sure all shooting is stopped
        if (shootPlayerCoroutine != null) { StopCoroutine(shootPlayerCoroutine); } // Stop the shooting coroutine
        if (movementCoroutine != null) { StopCoroutine(movementCoroutine); } // Stop all movement!
        gameObject.layer = 14; // Setting the enemy to the enemies layer to bounce of walls and interact with all other enemies
        spriteRenderer.color = paralyzedColor;
        yield return new WaitForSeconds(secondsOfParalyzation);
        gameObject.layer = 9; // Setting the object back to the projectile enemy layer
        spriteRenderer.color = normalColor;

        // Take away Player Shield mode just in case
        playerShieldObject.SetActive(false);
        gameObject.tag = "Enemy";
        boxCollider.size = Vector2.one;

        movementCoroutine = StartCoroutine(MoveTowardsPlayer()); // Movement again!
        shootPlayerCoroutine = StartCoroutine(ShootPlayerCor()); // Start shooting again
        isParalyzed = false; // No longer paralyzed
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("Shield"))
        {
            OnShieldCollision(collider);
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

        else if (collision.transform.CompareTag("PlayerShield"))
        {
            healthScript.TakeDamage(1);
        }
    }


    private IEnumerator PlayerShieldModeCor()
    {
        yield return new WaitForSeconds(0.1f);
        playerShieldObject.SetActive(true); // Set the Player Shield sprite renderer object active
        gameObject.tag = "PlayerShield"; // Change the tag so other enemies know what they are colliding with
        boxCollider.size = playerShieldObject.transform.localScale; // Set the collider to be as big as the Player Shield
    }


    private void OnDestroy()
    {
        StopAllCoroutines();
        //Destroy(gameObject);
    }
}
