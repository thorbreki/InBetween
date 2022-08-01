using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject gameRunningUI; // The UI which is visible when the game is running
    [SerializeField] private GameObject gameOverUI; // The UI which is visible when the player is dead
    [SerializeField] private GameObject gameWonUI; // The UI which is visible when the player has beaten the level
    [SerializeField] private TextMeshProUGUI enemyCountText;
    [SerializeField] private CanvasFunctions canvasFunctions;

    public Transform playerTransform;
    public PlayerMovement playerMovementScript;
    public PlayerCombat playerCombatScript;
    public StaminaShieldController staminaShieldControllerScript;
    public ShieldController shieldControllerScript;
    public BombController bombControllerScript;


    [HideInInspector] public int numOfEnemiesLeft;

    public int activeEnemies = 0;

    public static GameManager instance;

    private void Start()
    {
        instance = this;
        gameRunningUI.SetActive(true); // Making sure
        gameOverUI.SetActive(false); // Making sure
        gameWonUI.SetActive(false);

        // SET UP NECESSARY VALUES REGARDING THE LEVEL

        // TODO: exchange debug line for the actual line
        numOfEnemiesLeft = ApplicationManager.instance.currLevelData.totalAmountOfEnemies; // Set up the actual amount of enemies left
        //numOfEnemiesLeft = 5;

        enemyCountText.text = numOfEnemiesLeft.ToString();

    }

    /// <summary>
    /// This function runs whether the player dies or wins the level
    /// </summary>
    /// <param name="newLevelStatus"></param>
    private void OnLevelFinish(ApplicationManager.LevelFinishedStatus newLevelStatus)
    {
        ApplicationManager.instance.SavePlayerData();
    }

    /// <summary>
    /// This function runs when the player dies
    /// </summary>
    public void PlayerDied()
    {
        gameRunningUI.SetActive(false);
        gameOverUI.SetActive(true);
        PlayerData oldPlayerData = ApplicationManager.instance.GetPlayerData();
        oldPlayerData.levelFinishedStatus = ApplicationManager.LevelFinishedStatus.Loss;
        ApplicationManager.instance.SetPlayerData(oldPlayerData);
        OnLevelFinish(ApplicationManager.LevelFinishedStatus.Loss);
    }


    /// <summary>
    /// Decrease the current enemy count by 1, also update enemy text object
    /// </summary>
    public void OnEnemyDeath()
    {
        numOfEnemiesLeft--;
        activeEnemies--;
        enemyCountText.text = numOfEnemiesLeft.ToString();

        if (numOfEnemiesLeft == 0) { OnPlayerWin(); }
    }

    /// <summary>
    /// When the player wins, this function tells the UI to show the game won UI
    /// </summary>
    public void OnPlayerWin()
    {
        if (gameOverUI.activeSelf) { return; }
        if (gameRunningUI != null) { gameRunningUI.SetActive(false); }
        if (gameWonUI != null) { gameWonUI.SetActive(true); }
        PlayerData oldPlayerData = ApplicationManager.instance.GetPlayerData();
        oldPlayerData.levelFinishedStatus = ApplicationManager.LevelFinishedStatus.Win;
        ApplicationManager.instance.SetPlayerData(oldPlayerData);
        OnLevelFinish(ApplicationManager.LevelFinishedStatus.Win);
    }
}
