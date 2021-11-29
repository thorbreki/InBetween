using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject gameRunningUI; // The UI which is visible when the game is running
    [SerializeField] private GameObject gameOverUI; // The UI which is visible when the player is dead
    [SerializeField] private TextMeshProUGUI coinUIText;

    private int coinCount = 0;

    public static GameManager instance;

    private void Awake()
    {
        Application.targetFrameRate = 60;
    }

    private void Start()
    {
        instance = this;
        gameRunningUI.SetActive(true); // Making sure
        gameOverUI.SetActive(false); // Making sure

        coinUIText.text = coinCount.ToString();
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

    /// <summary>
    /// Add 1 coin to the player's coin count
    /// </summary>
    public void AddCoin()
    {
        coinCount++;
        coinUIText.text = coinCount.ToString();
    }
}
