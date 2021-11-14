using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaminaBarController : MonoBehaviour
{
    [SerializeField] private PlayerMovement playerMovementScript;

    private Vector3 scaleVector;

    private void Start()
    {
        scaleVector = Vector3.one;
    }

    private void Update()
    {
        UpdateStaminaBar();
    }

    private void UpdateStaminaBar()
    {
        scaleVector.x = (playerMovementScript.stamina / playerMovementScript.maxStamina);
        transform.localScale = scaleVector;
    }
}
