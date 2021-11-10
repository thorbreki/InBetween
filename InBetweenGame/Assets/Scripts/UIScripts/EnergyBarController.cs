using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergyBarController : MonoBehaviour
{
    [SerializeField] private PlayerCombat playerCombatController;

    private Vector3 scaleVector;

    private void Start()
    {
        scaleVector = new Vector3(1, 1, 1);
    }


    private void Update()
    {
        SetEnergyRatio(playerCombatController.shootEnergy / playerCombatController.maxShootEnergy);
    }

    /// <summary>
    /// When given a ratio of the player's energy, it sets the scale of the bar to that specific ratio
    /// </summary>
    /// <param name="ratio"> the state of the energy divided by max: 0 <= ratio <= 1</param>
    private void SetEnergyRatio(float ratio)
    {
        scaleVector.x = ratio;
        transform.localScale = scaleVector;
    }


}
