using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApplicationManager : MonoBehaviour
{
    public static ApplicationManager instance;

    [HideInInspector] public PlayerData playerData;

    public enum LevelFinishedStatus
    {
        Win,
        Loss,
        No // Player has not played a level before coming into the Level Scene
    }

    // This enum shows whether or not the player finished a level before arriving in the Level Scene, and shows wether the player lost or won
    [HideInInspector] public LevelFinishedStatus finishedLevelStatus = LevelFinishedStatus.Win;
    
    private void Awake()
    {
        instance = this;
        Application.targetFrameRate = 60;

        // TODO: REMOVE THIS LINE WHEN DONE IMPLEMENTING BASIC LEVELS SCENE FUNCITONALITY
        finishedLevelStatus = LevelFinishedStatus.Loss;
        print(finishedLevelStatus.ToString());
    }

    public void InitializePlayerData()
    {
        playerData = new PlayerData();
        playerData.currentLevel = 1;
        playerData.prevLevel = 1;
        playerData.maxHealth = 8;
        playerData.pistolDamage = 1;
        playerData.bombDamage = 2;
        playerData.staminaShieldDamage = 1;
        playerData.gunShieldParalyzationSeconds = 5f;
        playerData.bombParalyzationSeconds = 5f;
        playerData.maxStamina = 10;
        playerData.maxGunEnergy = 10;
    }
}
