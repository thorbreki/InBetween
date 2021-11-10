using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CanvasFunctions : MonoBehaviour
{
    private void Start()
    {
        Time.timeScale = 1f;
    }
    /// <summary>
    /// Player can restart the game
    /// </summary>
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
