using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform playerTransform; // The player's Transform component
    [SerializeField] private float yOffset; // The y-offset the camera can have

    private Vector3 positionVector;

    private void Start()
    {
        positionVector = new Vector3(0, 0, transform.position.z);
    }
    void Update()
    {
        positionVector.x = playerTransform.position.x;
        positionVector.y = playerTransform.position.y + yOffset;

        transform.position = positionVector;
    }
}
