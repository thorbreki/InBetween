using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaminaShieldController : MonoBehaviour
{
    [SerializeField] private Transform playerTransform; // The transform component of the player
    public float enemyKnockbackStaminaPenalty; // How much stamina the stamina shield uses when knocking back enemies 

    public float knockbackForce; // The amount of force the stamina shield knocks back the enemies

    public void OnEnemyCollision()
    {

    }
}
