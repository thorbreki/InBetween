using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootEffect : MonoBehaviour
{
   private void Start()
    {
        StartCoroutine(DeathDelay());
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }

    private IEnumerator DeathDelay()
    {
        yield return new WaitForSeconds(0.5f);
        Destroy(gameObject);
    }
}
