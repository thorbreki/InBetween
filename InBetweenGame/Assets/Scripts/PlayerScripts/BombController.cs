using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BombController : MonoBehaviour
{
    [Header("Objects and Components")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private CircleCollider2D circleCollider;

    // ATTRIBUTES ARE IN THE PLAYER COMBAT SCRIPT, TO ALLOW OTHER OBJECTS TO REFERENCE THEM

    [SerializeField] private float deltaT;
    [HideInInspector] public Vector3 targetPosition;

    private Vector3 startingPosition;
    private float distanceFromStartToTarget;

    public void StartBomb(Vector3 startPos, Vector3 targetPos)
    {
        startingPosition = startPos;
        targetPosition = targetPos; targetPosition.z = transform.position.z;
        distanceFromStartToTarget = Vector2.Distance(targetPosition, startingPosition);
        StartCoroutine(LerpToTargetPosCor());
    }

    public IEnumerator LerpToTargetPosCor()
    {
        float t = 0f;
        float realT;
        while (t < distanceFromStartToTarget)
        {
            realT = t / distanceFromStartToTarget;
            transform.position = Vector3.Lerp(startingPosition, targetPosition, realT);
            t += deltaT * Time.deltaTime;
            yield return null;
        }
        StartCoroutine(ExplodeCor());
    }

    public IEnumerator ExplodeCor()
    {
        transform.localScale = new Vector3(GameManager.instance.playerCombatScript.explosionRadius * 2, GameManager.instance.playerCombatScript.explosionRadius * 2, 1f);
        circleCollider.enabled = true;
        yield return new WaitForSeconds(GameManager.instance.playerCombatScript.explosionTimeToLive);
        Die();
    }

    private void Die()
    {
        StopAllCoroutines();
        Destroy(gameObject);
    }
}
