using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIFunctions : MonoBehaviour
{
    [SerializeField] private RawImage levelSceneOverlay;
    [SerializeField] private LevelSceneController levelSceneController;
    private Color levelSceneOverlayColor;

    public void GoToMainScene()
    {
        StartCoroutine(GoToMainSceneCor());
    }

    private IEnumerator GoToMainSceneCor()
    {
        levelSceneOverlayColor = levelSceneOverlay.color;
        float t = 0;

        while (t < 1f)
        {
            levelSceneOverlayColor.a = t;
            levelSceneOverlay.color = levelSceneOverlayColor;
            t += levelSceneController.sceneFadeInSpeed * Time.deltaTime;
            yield return null;
        }
        levelSceneOverlayColor.a = 1;
        levelSceneOverlay.color = levelSceneOverlayColor;
        SceneManager.LoadScene(1);
    }
}
