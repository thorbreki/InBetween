using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBarController : MonoBehaviour
{
    private Vector3 ratioVector; // Good to have vector for setting the ratio of the Health Bar

    private void Start()
    {
        ratioVector = Vector3.one;

    }
    public void SetHealthBarRatio(float inputRatio)
    {
        ratioVector.x = inputRatio;
        transform.localScale = ratioVector;
    }
}
