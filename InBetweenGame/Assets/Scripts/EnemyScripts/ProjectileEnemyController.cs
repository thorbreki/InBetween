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
    [SerializeField] private float shootingInaccuracy; // How inaccurate the player's shooting is

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

    private Vector3 toPlayerVector; // The vector which defines where the enemy

    private Coroutine shootPlayerCoroutine; // Coroutine object for shooting the player


    private void Start()
    {
        targetDistanceFromGround = Random.Range(minDistance, maxDistance); // Make the enemy randomly far from the floor
        targetXDistanceFromPlayer = Random.Range(minDistance, maxDistance); // Make the enemy randomly far from the player on the x coordinate

        projectileSpeed = Random.Range(minProjectileSpeed, maxProjectileSpeed); // Initializing the speed of the projectiles this enemy shoots
        projectileCooldown = Random.Range(minProjectileCooldown, maxProjectileCooldown); // Initializing how fast the enemy can shoot

        targetSideOfPlayer = Random.Range(0, 2);

        // Initialize the vectors so they can be used
        toPlayerVector = Vector3.zero;
        knockBackVector = Vector2.zero;

        shootPlayerCoroutine = StartCoroutine(ShootPlayerCor());
    }

    private void Update()
    {
        if (playerTransform == null || isParalyzed)
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
        // The target x position will now be randomly right or left side of the player always
        float targetXPosition = targetSideOfPlayer == 1 ? playerTransform.position.x + targetXDistanceFromPlayer : playerTransform.position.x - targetXDistanceFromPlayer;
        toPlayerVector.x = targetXPosition - transform.position.x;
        toPlayerVector.y = targetDistanceFromGround - transform.position.y;

        if (toPlayerVector.sqrMagnitude <= 0.005f)
        {
            toPlayerVector.x = 0;
            toPlayerVector.y = 0;
        }
    }

    private void MoveToProximityOfPlayer()
    {
        if (isParalyzed) { return; }
        rigidBody.velocity = toPlayerVector.normalized * movementSpeed;
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
    private void OnShieldCollision(Collision2D collision)
    {
        if (shieldControllerScript == null) { shieldControllerScript = collision.gameObject.GetComponent<ShieldController>(); } // Get the shield controller script if not already

        if (paralyzeCoroutine != null) { StopCoroutine(paralyzeCoroutine); } // Stop previous paralyzation if it is currently active
        paralyzeCoroutine = StartCoroutine(ParalyzeCor(shieldControllerScript.enemyParalyzationSeconds)); // Start paralyzing the enemy

        // Being knocked back
        knockBackVector = transform.position - collision.transform.position; // Now it is known what direction this enemy must go in
        rigidBody.AddForce(knockBackVector.normalized * shieldControllerScript.enemyKnockbackForce, ForceMode2D.Impulse);

        // Call the playercombat script through the shield object to make the player lose the correct amount of energy
        shieldControllerScript.playerCombatScript.OnShieldProtect();
    }
    private IEnumerator ParalyzeCor(float secondsOfParalyzation)
    {
        isParalyzed = true;

        // Make sure all movement is completely stopped
        toPlayerVector.x = 0; toPlayerVector.y = 0; toPlayerVector.z = 0;
        rigidBody.velocity = toPlayerVector;

        // Make sure all shooting is stopped
        if (shootPlayerCoroutine != null) { StopCoroutine(shootPlayerCoroutine); } // Stop the shooting coroutine

        yield return new WaitForSeconds(secondsOfParalyzation);
        isParalyzed = false; // No longer paralyzed
        shootPlayerCoroutine = StartCoroutine(ShootPlayerCor()); // Start shooting again
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Shield"))
        {
            OnShieldCollision(collision);
        }
    }

    private void OnDestroy()
    {
        Destroy(gameObject);
    }
}
