using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    private void Start()
    {
        StartCoroutine("DeathCor");
    }
    private IEnumerator DeathCor()
    {
        yield return new WaitForSeconds(20f);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Ground") || collision.CompareTag("Player"))
        {
            Destroy(gameObject);
        }
        else if (collision.CompareTag("Shield"))
        {
            collision.gameObject.GetComponent<ShieldController>().playerCombatScript.OnShieldProtect(); // The shield protected the player from me so it loses energy
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }
}
