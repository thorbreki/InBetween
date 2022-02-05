using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
