using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelSceneKickstarter : MonoBehaviour
{
    [SerializeField] private LevelSceneController levelSceneController;


    private void Start()
    {
        KickstartLevelScene();
    }

    /// <summary>
    /// This method starts the functionality of the Level Scene, i.e sends a message to LevelSceneController to begin going to the next level
    /// </summary>
    private void KickstartLevelScene()
    {
        levelSceneController.GoThroughLevelScene();
    }
}
