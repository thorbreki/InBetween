using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject gameRunningUI; // The UI which is visible when the game is running
    [SerializeField] private GameObject gameOverUI; // The UI which is visible when the player is dead

    public static GameManager instance;

    private void Start()
    {
        instance = this;
        gameRunningUI.SetActive(true); // Making sure
        gameOverUI.SetActive(false); // Making sure
    }

    /// <summary>
    /// This function runs when the player dies
    /// </summary>
    public void PlayerDied()
    {
        gameRunningUI.SetActive(false);
        gameOverUI.SetActive(true);
        Time.timeScale = 0f;
    }
}
