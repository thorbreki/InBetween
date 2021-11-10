using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileEnemyController : MonoBehaviour
{
    [Header("Components and Objects")]
    [SerializeField] private Rigidbody2D rigidBody; // The rigidbody component of the enemy
    [SerializeField] private GameObject projectileObject; // The projectile game object

    [Header("Movement Attributes")]
    [SerializeField] private float minDistance; // The lower bound of the random distance the enemy wants to keep from the player
    [SerializeField] private float maxDistance; // The higher bound (EXCLUSIVE IN THE RANDOM FUNCTION)
    [SerializeField] private float movementSpeed; // The enemy's movementSpeed

    [Header("Attack Attributes")]
    [SerializeField] private float minProjectileSpeed; // the min possible speed of the projectiles
    [SerializeField] private float maxProjectileSpeed; // The max possible speed of the projectiles
    [SerializeField] private float minProjectileCooldown; // the min possible time between shooting (in seconds)
    [SerializeField] private float maxProjectileCooldown; // the max possible time between shooting (in seconds)

    private float projectileSpeed; // The speed of the projectiles this enemy shoots
    private float projectileCooldown; // The seconds between the enemy can shoot projectiles

    private Transform playerTransform;
    private float targetDistanceFromGround;
    private float targetXDistanceFromPlayer;

    private Vector3 toPlayerVector; // The vector which defines where the enemy moves
    private float currDistanceFromPlayer; // The actual current distance from the player (only check on x-positions)


    private void Start()
    {
        targetDistanceFromGround = Random.Range(minDistance, maxDistance); // Make the enemy randomly far from the floor
        targetXDistanceFromPlayer = Random.Range(minDistance, maxDistance); // Make the enemy randomly far from the player on the x coordinate

        projectileSpeed = Random.Range(minProjectileSpeed, maxProjectileSpeed); // Initializing the speed of the projectiles this enemy shoots
        projectileCooldown = Random.Range(minProjectileCooldown, maxProjectileCooldown); // Initializing how fast the enemy can shoot

        toPlayerVector = Vector3.zero; // Initialize the vector so it can be used

        StartCoroutine("ShootPlayerCor");
    }

    private void Update()
    {
        if (playerTransform == null)
        {
            return;
        }

        SetUpMovement();
    }

    private void FixedUpdate()
    {
        MoveToProximityOfPlayer();
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
    /// Sets all the movement variables to correct values before the actual move call in FixedUpdate
    /// </summary>
    private void SetUpMovement()
    {
        // Set the value for the y-movement of the enemy
        if (transform.position.y < (targetDistanceFromGround - 0.5f)) // If the enemy is farther from the ground than it wants to be
        {
            toPlayerVector.y = 1;
            rigidBody.velocity = toPlayerVector.normalized * movementSpeed;
        }
        else if (transform.position.y > (targetDistanceFromGround + 0.5f)) // If the enemy is closer to the ground than it wants to be
        {
            toPlayerVector.y = -1;
        }
        else // If the enemy is happy about the distance from the ground
        {
            toPlayerVector.y = 0;
        }

        // Set the value for the x-movement of the enemy
        currDistanceFromPlayer = Mathf.Abs(transform.position.x - playerTransform.position.x);
        if (currDistanceFromPlayer < (targetXDistanceFromPlayer - 0.2f)) // If the enemy is closer from the player than it wants
        {
            toPlayerVector.x = transform.position.x > playerTransform.position.x ? 1 : -1; // Go right if enemy is to the right of player, else left
        }
        else if (currDistanceFromPlayer > (targetXDistanceFromPlayer + 0.2f)) // If the enemy is farther from the player than it wants
        {
            toPlayerVector.x = transform.position.x > playerTransform.position.x ? -1 : 1; // Go left if enemy is to the right of player, else right
        }
        else // The enemy is happy :)
        {
            toPlayerVector.x = 0;
        }
    }

    private void MoveToProximityOfPlayer()
    {
        rigidBody.velocity = toPlayerVector * movementSpeed;
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
            projectile.GetComponent<Rigidbody2D>().velocity = (playerTransform.position - transform.position).normalized * projectileSpeed; // Make it start moving towards player
        }
    }

    private void OnDestroy()
    {
        Destroy(gameObject);
    }
}
