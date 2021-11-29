using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldController : MonoBehaviour
{
    [SerializeField] private Transform playerTransform;
    public PlayerCombat playerCombatScript;
    [SerializeField] private Rigidbody2D rigidBody;

    [SerializeField] private float width; // The width of the shield
    [SerializeField] private float height; // The height of the shield

    [SerializeField] private float movementSpeed; // How fast the shield is to catch up to the mouse pointer

    public float enemyKnockbackForce; // How much force the enemies are thrown back when colliding with the shield
    public float enemyParalyzationSeconds; // How long the enemies are paralyzed when being knocked back by the shield

    

    Vector3 mousePosition; // The vector which keeps track of the mouse's position
    Vector3 lookAtPlayerVector;
    Vector2 toMouseVector;

    private void Start()
    {
        SetScale();

        lookAtPlayerVector = Vector3.zero;
        mousePosition = Vector3.zero;
        toMouseVector = Vector2.zero;
    }

    private void Update()
    {
        LookAtPlayer();
    }

    private void FixedUpdate()
    {
        MoveToMousePosition();
    }

    private void MoveToMousePosition()
    {
        mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = transform.position.z;
        toMouseVector = mousePosition - transform.position;

        if (toMouseVector.sqrMagnitude <= 0.005f) // If shield is already close enough
        {
            toMouseVector.x = 0;
            toMouseVector.y = 0;
            transform.position = mousePosition;
        }

        rigidBody.velocity = toMouseVector.normalized * movementSpeed;
    }


    private void LookAtPlayer()
    {
        lookAtPlayerVector = playerTransform.position - transform.position;
        float angle = Mathf.Atan2(lookAtPlayerVector.y, lookAtPlayerVector.x) * Mathf.Rad2Deg + 90f;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    /// <summary>
    /// Sets the scale of the shield (its width and height)
    /// </summary>
    private void SetScale()
    {
        transform.localScale = new Vector3(width, height, 1);
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        // If the shield collides with an enemy or projectile
        if (collision.transform.CompareTag("Enemy") || collision.transform.CompareTag("Projectile"))
        {
            playerCombatScript.OnShieldProtect(); // Alert the player that you were protecting them, you're very welcome player!
        }
    }
}
