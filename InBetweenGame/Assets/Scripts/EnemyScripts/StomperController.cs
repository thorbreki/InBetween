using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StomperController : ParentEnemyController
{

    [Header("-- STOMPER ENEMY --")]
    [Header("Components and Objects")]
    [SerializeField] private GameObject indicatorObject;
                     public PlayerCombat playerCombatScript;

    [Header("Movement Attributes")]
    [SerializeField] private float velocityChangeSpeed; // How fast the enemy can change his velocity's direction

    [Header("Attack Attributes")]
    [SerializeField] private float minAttackForce; // The minimum speed the enemy goes down to kill
    [SerializeField] private float maxAttackForce; // The maximum speed the enemy goes down to kill

    private GameObject indicator; // The transform component of the indicator object

    private float attackForce;

    private Vector2 desiredDirection;
    private Vector2 attackVector;
    private Vector2 currVelocity; // Keeping track of current velocity of the StomperEnemy

    private Coroutine moveCor;
    private Coroutine updateDesiredDirectionCoroutine;
    private Coroutine attackPlayerCor;

    protected override void Start()
    {
        base.Start();

        // Initialize vectors
        desiredDirection = Vector2.one;
        currVelocity = Vector2.zero;

        indicator = Instantiate(indicatorObject, transform.position, Quaternion.identity);

        attackForce = Random.Range(minAttackForce, maxAttackForce);
        attackVector = new Vector2(0, -attackForce);

        moveCor = StartCoroutine(MoveToDesiredPosCor());
        updateDesiredDirectionCoroutine = StartCoroutine(UpdateDesiredDirectionCor());
    }

    protected override void Update()
    {
        base.Update();

        indicator.transform.position = transform.position; // Always update the indicator object's position

        if (isParalyzed) { return; } // Past this point, all code will not run if this enemy is paralyzed

        if (Mathf.Abs(GameManager.instance.playerTransform.position.x - transform.position.x) <= 0.99f)
        {
            if (attackPlayerCor != null) { return; } // Make sure to not activate the attack coroutine again and again
            if (moveCor != null) { StopCoroutine(moveCor); moveCor = null; } // Make sure that the enemy is not trying to get to its desired spot anymore
            attackPlayerCor = StartCoroutine(AttackPlayer()); // Start the attack coroutine
        }
    }

    /// <summary>
    /// Always, constantly try to move towards the player to get close enough to hurt him
    /// </summary>
    private IEnumerator MoveToDesiredPosCor()
    {
        attackPlayerCor = null; // Just to tell the script that the enemy has stopped attacking and is moving normally

        // Initialize movement vectors

        float squaredMovementSpeed = Mathf.Pow(movementSpeed, 2);
        float movementSpeedHalfed = movementSpeed / 2;
        while (true)
        {
            currVelocity.x = rigidBody.velocity.x; currVelocity.y = rigidBody.velocity.y;

            // 1st case: Enemy is moving too slow
            if ((currVelocity.sqrMagnitude - squaredMovementSpeed) < -0.05f)
            {
                currVelocity += desiredDirection.normalized * velocityChangeSpeed;
            }
            // 3rd case: Enemy is moving too fast
            else if ((currVelocity.sqrMagnitude - squaredMovementSpeed) > 0.05f)
            {
                currVelocity -= currVelocity.normalized * velocityChangeSpeed;
            }
            // 2nd case: Enemy is moving at perfect speed
            else
            {
                currVelocity += (-currVelocity.normalized * movementSpeedHalfed) + (desiredDirection.normalized * movementSpeedHalfed);
            }

            // Finally, set the velocity of the enemy
            rigidBody.velocity = currVelocity;
            yield return null;
        }
    }

    /// <summary>
    /// Updates the desired direction of the movement of the StomperEnemy
    /// </summary>
    /// <returns></returns>
    private IEnumerator UpdateDesiredDirectionCor()
    {
        float x; float y;

        while (true)
        {
            // Generate random x and y values for the target vector
            x = Random.Range(-1f, 1f);
            y = Random.Range(-1f, 1f);

            desiredDirection.x = x; desiredDirection.y = y;
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
        moveCor = StartCoroutine(MoveToDesiredPosCor());
    }


    /// <summary>
    /// Basic Enemy gets knocked back and paralyzed when hit with the shield
    /// </summary>
    /// <param name="collision"></param>
    private void OnShieldCollision(Collider2D collider)
    {
        if (isParalyzed) { return; } // If enemy is already paralyzed, don't do anything
        paralyzeCoroutine = StartCoroutine(ParalyzeCor(GameManager.instance.shieldControllerScript.enemyParalyzationSeconds)); // Start paralyzing the enemy
    }


    protected override IEnumerator ParalyzeCor(float secondsOfParalyzation)
    {
        isParalyzed = true;
        spriteRenderer.color = paralyzedColor;
        gameObject.layer = 4; // Enemy now has the bouncy layer equipped
        // Make sure all movement and attacks are completely stopped
        if (moveCor != null) { StopCoroutine(moveCor); }
        if (attackPlayerCor != null) { StopCoroutine(attackPlayerCor); }

        yield return new WaitForSeconds(secondsOfParalyzation);
        gameObject.layer = 10; // Back to the normal StomperEnemy layer
        spriteRenderer.color = normalColor;

        // Take away Player Shield mode just in case
        playerShieldObject.SetActive(false);
        rigidBody.mass = 1;
        isPlayerShield = false;
        gameObject.tag = "Enemy";
        boxCollider.size = Vector2.one;

        moveCor = StartCoroutine(MoveToDesiredPosCor()); // Start moving again!
        isParalyzed = false;
    }


    protected override void OnDestroy()
    {
        Destroy(indicator);
        StopAllCoroutines();
    }
}
