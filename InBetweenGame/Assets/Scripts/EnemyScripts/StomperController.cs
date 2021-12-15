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
                     public PlayerCombat playerCombatScript;

    [Header("Movement Attributes")]
    [SerializeField] private float minMovementSpeed;
    [SerializeField] private float maxMovementSpeed;

    [Header("Attack Attributes")]
    [SerializeField] private float minAttackForce; // The minimum speed the enemy goes down to kill
    [SerializeField] private float maxAttackForce; // The maximum speed the enemy goes down to kill

    [Header("Paralyzed Attributes")]
    [SerializeField] private Color normalColor;
    [SerializeField] private Color paralyzedColor;

    private GameObject indicator; // The transform component of the indicator object

    private float movementSpeed;
    private float attackForce;

    // SHIELD FUNCTIONALITY
    private ShieldController shieldControllerScript; // The shield's controller script to know important info
    private bool isParalyzed = false;
    private Coroutine paralyzeCoroutine; // Coroutine object for when the enemy gets paralyzed by the shield
    private Vector2 knockBackVector; // The vector that knocks the enemy back

    private Vector2 desiredDirection;
    private Vector2 attackVector;

    private Coroutine moveCor;
    private Coroutine attackPlayerCor;

    private void Start()
    {
        // Initialize vectors
        desiredDirection = Vector2.one;
        knockBackVector = Vector2.zero;

        indicator = Instantiate(indicatorObject, transform.position, Quaternion.identity);

        movementSpeed = Random.Range(minMovementSpeed, maxMovementSpeed);
        attackForce = Random.Range(minAttackForce, maxAttackForce);
        attackVector = new Vector2(0, -attackForce);

        moveCor = StartCoroutine(MoveToDesiredPos());
    }

    private void Update()
    {
        indicator.transform.position = transform.position; // Always update the indicator object's position

        if (playerTransform == null || isParalyzed) { return; }


        if (Mathf.Abs(playerTransform.position.x - transform.position.x) <= 0.99f)
        {
            if (attackPlayerCor != null) { return; } // Make sure to not activate the attack coroutine again and again
            if (moveCor != null) { StopCoroutine(moveCor); moveCor = null; } // Make sure that the enemy is not trying to get to its desired spot anymore
            attackPlayerCor = StartCoroutine(AttackPlayer()); // Start the attack coroutine
        }
    }

    private IEnumerator MoveToDesiredPos()
    {
        attackPlayerCor = null; // This might be confusing, but this ensures that the enemy can attack again, since not sure if StopCoroutine makes coroutines == null
        while (true)
        {
            // Decide on a random direction
            desiredDirection.x = Random.Range(-5, 5);
            desiredDirection.y = Random.Range(-5, 5);
            rigidBody.velocity = desiredDirection.normalized * movementSpeed;
            yield return new WaitForSeconds(Random.Range(0.5f, 5f));
        }
    }

    private IEnumerator AttackPlayer()
    {
        print("Attacking!");
        yield return new WaitForSeconds(0.1f);
        desiredDirection.x = 0;
        desiredDirection.y = 0;
        rigidBody.velocity = desiredDirection;

        rigidBody.AddForce(attackVector, ForceMode2D.Impulse);

        yield return new WaitForSeconds(3f);
        moveCor = StartCoroutine(MoveToDesiredPos());
    }


    /// <summary>
    /// Basic Enemy gets knocked back and paralyzed when hit with the shield
    /// </summary>
    /// <param name="collision"></param>
    private void OnShieldCollision(Collider2D collider)
    {
        if (shieldControllerScript == null) { shieldControllerScript = collider.gameObject.GetComponent<ShieldController>(); } // Get the shield controller script if not already

        if (paralyzeCoroutine != null) { StopCoroutine(paralyzeCoroutine); } // Stop previous paralyzation if it is currently active
        paralyzeCoroutine = StartCoroutine(ParalyzeCor(shieldControllerScript.enemyParalyzationSeconds)); // Start paralyzing the enemy

        // Call the playercombat script through the shield object to make the player lose the correct amount of energy
        shieldControllerScript.playerCombatScript.OnShieldProtect();
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
        if (!isParalyzed && collision.transform.CompareTag("Player"))
        {
            playerCombatScript.TakeDamage(2);
        }
    }

    private IEnumerator ParalyzeCor(float secondsOfParalyzation)
    {
        isParalyzed = true;
        spriteRenderer.color = paralyzedColor;

        // Make sure all movement and attacks are completely stopped
        if (moveCor != null) { StopCoroutine(moveCor); }
        if (attackPlayerCor != null) { StopCoroutine(attackPlayerCor); }

        yield return new WaitForSeconds(secondsOfParalyzation);
        moveCor = StartCoroutine(MoveToDesiredPos()); // Start moving again!
        spriteRenderer.color = normalColor;
        isParalyzed = false;
    }


    private void OnDestroy()
    {
        Destroy(indicator);
    }
}
