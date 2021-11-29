using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicEnemyController : MonoBehaviour
{
    public Transform playerTransform; // Set by the parent spawner
    [SerializeField] private float minMovementSpeed; // The minimum possible movement speed this enemy could have
    [SerializeField] private float maxMovementSpeed; // The maximum possible movement speed this enemy could have
    [SerializeField] private Rigidbody2D rb;

    private float movementSpeed;

    // SHIELD FUNCTIONALITY
    private ShieldController shieldControllerScript; // The shield's controller script to know important info
    private bool isParalyzed = false;
    private Coroutine paralyzeCoroutine; // Coroutine object for when the enemy gets paralyzed by the shield
    private Vector2 knockBackVector; // The vector that knocks the enemy back

    private Vector3 toPlayerVector;

    private void Start()
    {
        // Initialize values
        movementSpeed = Random.Range(minMovementSpeed, maxMovementSpeed);

        // Initialize vectors
        toPlayerVector = Vector3.zero;
        knockBackVector = Vector2.zero;
    }

    private void FixedUpdate()
    {
        if (playerTransform == null || isParalyzed)
        {
            return;
        }
        MoveTowardsPlayer();
    }


    /// <summary>
    /// Always, constantly try to move towards the player to get close enough to hurt him
    /// </summary>
    private void MoveTowardsPlayer()
    {
        toPlayerVector = playerTransform.position - transform.position;
        toPlayerVector.z = 0;
        rb.velocity = toPlayerVector.normalized * movementSpeed;
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

        knockBackVector = transform.position - collision.transform.position; // Now it is known what direction this enemy must go in
        rb.AddForce(knockBackVector.normalized * shieldControllerScript.enemyKnockbackForce, ForceMode2D.Impulse);

        // Call the playercombat script through the shield object to make the player lose the correct amount of energy
        shieldControllerScript.playerCombatScript.OnShieldProtect();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Shield"))
        {
            OnShieldCollision(collision);
            return;
        }
        else if (collision.transform.CompareTag("Player"))
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Updates the paralyzation boolean so the enemy knows to stop doing necessary work for limited time
    /// </summary>
    /// <param name="secondsOfParalyzation"></param>
    /// <returns></returns>
    private IEnumerator ParalyzeCor(float secondsOfParalyzation)
    {
        isParalyzed = true;

        // Make sure all movement is completely stopped
        toPlayerVector.x = 0; toPlayerVector.y = 0; toPlayerVector.z = 0; 
        rb.velocity = toPlayerVector;

        yield return new WaitForSeconds(secondsOfParalyzation);
        isParalyzed = false;
    }

}
