using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform playerTransform; // The player's Transform component
    [SerializeField] private Camera cameraComponent; // The camera component of the main camera object

    [SerializeField] private float yOffset; // The y-offset the camera can have
    [SerializeField] private float zoomSpeed; // The speed of the zoom with mouse wheel

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

        HandleCameraZoom(); // Allow the player to zoom in and out with the mouse wheel
    }

    private void HandleCameraZoom()
    {
        cameraComponent.orthographicSize -= Input.mouseScrollDelta.y * zoomSpeed * Time.deltaTime;
    }
}
