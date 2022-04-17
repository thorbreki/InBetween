using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class ApplicationManager : MonoBehaviour
{
    public static ApplicationManager instance;

    [HideInInspector] public PlayerData playerData;
    [HideInInspector] public LevelData currLevelData;

    private const string DIR_NAME = "Player";
    private const string FILE_NAME = "data.binary";

    public enum LevelFinishedStatus
    {
        Loss = 2,
        Win = 1,
        No = 0 // Player has not played a level before coming into the Level Scene
    }

    // This enum shows whether or not the player finished a level before arriving in the Level Scene, and shows wether the player lost or won
    
    private void Awake()
    {
        // So there will always be just one ApplicationManager
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        Application.targetFrameRate = 60;

        // TODO: REMOVE THIS LINE WHEN DONE IMPLEMENTING BASIC LEVELS SCENE FUNCITONALITY
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
        playerData.levelFinishedStatus = ApplicationManager.LevelFinishedStatus.No;
    }

    public void SavePlayerData()
    {
        BinaryFormatter binaryFormatter = new BinaryFormatter();
        FileStream saveFile = File.Create(DIR_NAME + "/" + FILE_NAME);
        binaryFormatter.Serialize(saveFile, ApplicationManager.instance.playerData);

        saveFile.Close();
    }
}
