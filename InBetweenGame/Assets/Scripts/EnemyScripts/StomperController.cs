using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StomperController : MonoBehaviour
{
    [HideInInspector] public Transform playerTransform;

    [SerializeField] private Rigidbody2D rigidBody;
    [SerializeField] private float minMovementSpeed;
    [SerializeField] private float maxMovementSpeed;

    private float movementSpeed;

    private int positionIndex; // The index into the stomperPositions array in GameManager

    private Vector3 desiredSpot;

    private Coroutine attackPlayerCor;

    private void Start()
    {
        desiredSpot = new Vector3(0, 0, transform.position.z);

        movementSpeed = Random.Range(minMovementSpeed, maxMovementSpeed);
        // Find the target spot this enemy wants to go to
        FindDesiredSpot();
    }

    private void Update()
    {
        MoveToDesiredSpot();
    }

    private void FindDesiredSpot()
    {
        (int targetXPosition, int targetYPosition, int newIndex) = GameManager.instance.GetRandomStomperPosition();
        positionIndex = newIndex; // Now the enemy knows where the index of its position is in the stomperPositions List

        if (positionIndex == -1) // If no position was found
        {
            desiredSpot.x = transform.position.x;
            desiredSpot.y = transform.position.y;
        } else
        {
            desiredSpot.x = targetXPosition;
            desiredSpot.y = targetYPosition;
        }
    }

    private void MoveToDesiredSpot()
    {
        rigidBody.velocity = (desiredSpot - transform.position).normalized * movementSpeed;
    }

    private void AttackPlayer()
    {

    }

    private void OnDestroy()
    {
        GameManager.instance.FreeUpStomperPosition(positionIndex); // Free up the position for other stompers
    }
}
