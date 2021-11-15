using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject gameRunningUI; // The UI which is visible when the game is running
    [SerializeField] private GameObject gameOverUI; // The UI which is visible when the player is dead

    public static GameManager instance;

    // Stompers
    public List<(int, int)> stomperPositions;
    private List<int> availableStomperPositions;

    private void Start()
    {
        instance = this;
        gameRunningUI.SetActive(true); // Making sure
        gameOverUI.SetActive(false); // Making sure

        CreateStomperPositions(); // Create the different positions the stompers will want to go to
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

    // --------------------------------------------------------------
    // STOMPERS

    private void CreateStomperPositions()
    {
        stomperPositions = new List<(int, int)>();
        for (int x = -24; x < 24; x++)
        {
            stomperPositions.Add((x, Random.Range(3, 14)));
        }

        availableStomperPositions = new List<int>();

        for (int i =  0; i < stomperPositions.Count; i++)
        {
            availableStomperPositions.Add(i);
        }
    }

    public (int, int, int) GetRandomStomperPosition()
    {
        // Get index of a random stomper position

        if (availableStomperPositions.Count == 0) { return (0, 0, -1); } // Return this position if there were no positions that were found as an indicator of an error
        int randomIndex = Random.Range(0, availableStomperPositions.Count);
        int indexOfStomperPosition = availableStomperPositions[randomIndex];

        availableStomperPositions.RemoveAt(randomIndex); // Remove that position from the available stomper list since it is now reserved

        (int newXPosition, int newYPosition) = stomperPositions[indexOfStomperPosition];
        return (newXPosition, newYPosition, indexOfStomperPosition); // Return the actual position
    }

    public void FreeUpStomperPosition(int newIndex)
    {
        availableStomperPositions.Add(newIndex);
    }
}
