using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileEnemyController : ParentEnemyController
{
    [Header("PROJECTILE ENEMY")]

    [Header("Components and Objects")]
    [SerializeField] private GameObject projectileObject; // The projectile game object

    [Header("Movement Attributes")]
    [SerializeField] private float minDistance; // The lower bound of the random distance the enemy wants to keep from the player
    [SerializeField] private float maxDistance; // The higher bound (EXCLUSIVE IN THE RANDOM FUNCTION)
    [SerializeField] private float velocityChangeSpeed; // How fast the enemy can change its course
    private float targetDistanceFromGround;
    private float targetXDistanceFromPlayer;

    [Header("Attack Attributes")]
    [SerializeField] private float minProjectileSpeed; // the min possible speed of the projectiles
    [SerializeField] private float maxProjectileSpeed; // The max possible speed of the projectiles
    [SerializeField] private float minProjectileCooldown; // the min possible time between shooting (in seconds)
    [SerializeField] private float maxProjectileCooldown; // the max possible time between shooting (in seconds)
    [SerializeField] private float shootingInaccuracy; // How inaccurate the player's shooting is

    private float projectileSpeed; // The speed of the projectiles this enemy shoots
    private float projectileCooldown; // The seconds between the enemy can shoot projectiles
    private int targetSideOfPlayer; // The enemy will always want to be either side of the player. 0 = left of player, 1 = right of player

    private Vector2 currVelocity; // The vector to store the current movement velocity the Enemy has

    private Vector2 toPlayerVector; // The vector which defines where the enemy

    private Coroutine movementCoroutine; // Coroutine object for movement of Projectile Enemy
    private Coroutine shootPlayerCoroutine; // Coroutine object for shooting the player


    protected override void Start()
    {
        base.Start();

        targetDistanceFromGround = Random.Range(minDistance, maxDistance); // Make the enemy randomly far from the floor
        targetXDistanceFromPlayer = Random.Range(minDistance, maxDistance); // Make the enemy randomly far from the player on the x coordinate

        projectileSpeed = Random.Range(minProjectileSpeed, maxProjectileSpeed); // Initializing the speed of the projectiles this enemy shoots
        projectileCooldown = Random.Range(minProjectileCooldown, maxProjectileCooldown); // Initializing how fast the enemy can shoot

        targetSideOfPlayer = Random.Range(0, 2);

        // Initialize the vectors so they can be used
        toPlayerVector = Vector2.zero;
        currVelocity = Vector2.zero;

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

    protected override IEnumerator ParalyzeCor(float secondsOfParalyzation)
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
        isPlayerShield = false;
        rigidBody.mass = 1;
        gameObject.tag = "Enemy";
        boxCollider.size = Vector2.one;

        movementCoroutine = StartCoroutine(MoveTowardsPlayer()); // Movement again!
        shootPlayerCoroutine = StartCoroutine(ShootPlayerCor()); // Start shooting again
        isParalyzed = false; // No longer paralyzed
    }
}
