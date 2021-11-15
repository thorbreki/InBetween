using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicEnemyController : MonoBehaviour
{
    [HideInInspector] public Transform playerTransform; // Set by the parent spawner
    [SerializeField] private float movementSpeed;
    [SerializeField] private Rigidbody2D rb;

    private Vector3 toPlayerVector;

    private void Start()
    {
        toPlayerVector = Vector3.zero;
    }

    private void FixedUpdate()
    {
        if (playerTransform == null)
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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Destroy(gameObject);
    }
}
