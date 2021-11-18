using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StomperController : MonoBehaviour
{
    [HideInInspector] public Transform playerTransform;

    [SerializeField] private Rigidbody2D rigidBody;
    [SerializeField] private float minMovementSpeed;
    [SerializeField] private float maxMovementSpeed;

    [SerializeField] private float minDesiredX = -24f;
    [SerializeField] private float maxDesiredX = 24f;
    [SerializeField] private float minDesiredY = 4f;
    [SerializeField] private float maxDesiredY = 14f;

    [SerializeField] private float minAttackForce; // The minimum speed the enemy goes down to kill
    [SerializeField] private float maxAttackForce; // The maximum speed the enemy goes down to kill

    private float movementSpeed;
    private float attackForce;

    private Vector2 desiredDirection;
    private Vector2 attackVector;

    private Coroutine moveCor;
    private Coroutine attackPlayerCor;

    private void Start()
    {
        desiredDirection = Vector2.one;

        movementSpeed = Random.Range(minMovementSpeed, maxMovementSpeed);
        attackForce = Random.Range(minAttackForce, maxAttackForce);
        attackVector = new Vector2(0, -attackForce);

        moveCor = StartCoroutine(MoveToDesiredPos());
    }

    private void Update()
    {
        if (playerTransform == null) { return; }

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
            print("Choosing a new direction!");
            desiredDirection.x = Random.Range(-5, 5);
            desiredDirection.y = Random.Range(-5, 5);
            rigidBody.velocity = desiredDirection.normalized * movementSpeed;
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
        moveCor = StartCoroutine(MoveToDesiredPos());
    }

    //private void OnTriggerEnter2D(Collider2D other)
    //{
    //    print("TRIGGERED!");
    //    if (!other.CompareTag("StomperBorder")) { return; } // if this enemy is not colliding with bottom stomper border, then return

    //    if (moveCor == null) { return; } // If this enemy is not moving (aka attacking) then we will not care

    //    desiredDirection.y = 5f; // Make the enemy want to go up
    //    rigidBody.velocity = desiredDirection.normalized * movementSpeed;

    //}

    //private void OnTriggerStay2D(Collider2D other)
    //{
    //    if (!other.CompareTag("StomperBorder")) { return; } // if this enemy is not colliding with bottom stomper border, then return

    //    if (moveCor == null) { return; } // If this enemy is not moving (aka attacking) then we will not care

    //    desiredDirection.y = 5f; // Make the enemy want to go up

    //}
}
