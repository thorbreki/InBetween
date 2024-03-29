using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class ApplicationManager : MonoBehaviour
{
    public static ApplicationManager instance;

    private PlayerData playerData;
    [HideInInspector] public LevelData currLevelData;

    [Header("PistolAccuracyUpgrade")]
    [SerializeField] private float pistolAccuracyUpgradeAmount;
    [SerializeField] private int pistolAccuracyUpgradeCost;
    [SerializeField] private int pistolAccuracyUpgradeCostMultiplier;
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
    }

    public PlayerData GetPlayerData()
    {
        if (playerData == null)
        {
            if (!Directory.Exists(DIR_NAME))
            {
                Directory.CreateDirectory(DIR_NAME);
            }

            if (!File.Exists(DIR_NAME + "/" + FILE_NAME)) // If the file has never been created before
            {
                InitializePlayerData();
                SavePlayerData();
            }
            else // The player data file is already created, so only need to load in the data
            {
                BinaryFormatter formatter = new BinaryFormatter();
                FileStream saveFile = File.Open(DIR_NAME + "/" + FILE_NAME, FileMode.Open);

                playerData = (PlayerData)formatter.Deserialize(saveFile);

                saveFile.Close();
            }
        }
        return playerData;
    }

    public void SetPlayerData(PlayerData newPlayerData)
    {
        playerData = newPlayerData;
    }

    private void InitializePlayerData()
    {
        playerData = GetBasePlayerData();
    }

    public void SavePlayerData()
    {
        BinaryFormatter binaryFormatter = new BinaryFormatter();
        FileStream saveFile = File.Create(DIR_NAME + "/" + FILE_NAME);
        binaryFormatter.Serialize(saveFile, ApplicationManager.instance.playerData);

        saveFile.Close();
    }

    /// <summary>
    /// Returns the abilities of a starting player
    /// </summary>
    /// <returns></returns>
    public PlayerData GetBasePlayerData()
    {
        PlayerData basePlayerData = new PlayerData();
        basePlayerData.currentLevel = 1; // 1
        basePlayerData.prevLevel = 1; // 1
        basePlayerData.levelFinishedStatus = LevelFinishedStatus.No; // LevelFinishedStatus.No

        return basePlayerData;
    }


    /// <summary>
    /// Returns the abilities of a fully upgraded player
    /// </summary>
    /// <returns></returns>
    public PlayerData GetMaxedOutPlayerData()
    {
        PlayerData maxPlayerData = new PlayerData();
        maxPlayerData.currentLevel = 1;
        maxPlayerData.prevLevel = 1;
        maxPlayerData.levelFinishedStatus = LevelFinishedStatus.No;

        return maxPlayerData;
    }
}
