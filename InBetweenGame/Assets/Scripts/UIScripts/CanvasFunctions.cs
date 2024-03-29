using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class CanvasFunctions : MonoBehaviour
{
    [Header("Gun Mode Text")]
    [SerializeField] private TextMeshProUGUI gunModeText;

    private Coroutine displayGunModeTextCoroutine;

    private void Start()
    {
        Time.timeScale = 1f;

        // Initialize some stuff
        gunModeText.gameObject.SetActive(false);
    }
    /// <summary>
    /// Player can restart the game
    /// </summary>
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    /// <summary>
    /// Fades out the Main Level and loads up the Level Scene
    /// </summary>
    public void GoToLevelScene(RawImage inputRawImage)
    {
        StartCoroutine(GoToLevelSceneCor(inputRawImage));
    }

    private IEnumerator GoToLevelSceneCor(RawImage inputRawImage)
    {
        print("GOING TO LEVEL SCENE!");
        Color overlayColor = inputRawImage.color;
        float initial = inputRawImage.color.a;
        //print("INITIAL: " + initial.ToString());
        float target = 1f;
        float t = 0;

        while (t < 1f)
        {
            overlayColor.a = (initial * (1-t)) + (target * t);
            inputRawImage.color = overlayColor;
            t += 0.5f * Time.deltaTime;
            yield return null;
        }
        overlayColor.a = 1f;
        inputRawImage.color = overlayColor;

        SceneManager.LoadScene(0);
    }

    public void ChangeGunModeText(string newGunModeText)
    {
        if (displayGunModeTextCoroutine != null) { StopCoroutine(displayGunModeTextCoroutine); }
        displayGunModeTextCoroutine = StartCoroutine(DisplayGunModeTextCor(newGunModeText));
    }

    private IEnumerator DisplayGunModeTextCor(string newGunModeText)
    {
        gunModeText.text = newGunModeText;
        gunModeText.gameObject.SetActive(true);
        yield return new WaitForSeconds(2f);
        gunModeText.gameObject.SetActive(false);
    }
}
