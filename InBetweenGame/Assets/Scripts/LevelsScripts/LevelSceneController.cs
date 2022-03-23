using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using TMPro;

public class LevelSceneController : MonoBehaviour
{
    [SerializeField] private RawImage canvasOverlayImage;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private LevelsGenerator levelsGenerator;
    [SerializeField] private TextMeshProUGUI goodJobText; // Text that says good job to the player
    [SerializeField] private TextMeshProUGUI goAheadText; // Text that says "you get to go ahead"
    [SerializeField] private TextMeshProUGUI amountText; // Text that says the amount of levels the player can go
    [SerializeField] private TextMeshProUGUI levelsText; // Text that just says "Levels"
    [SerializeField] private TextMeshProUGUI welcomeText; // Text that bids the player welcome
    [SerializeField] private TextMeshProUGUI levelStatisticsText; // Text that shows statistics of the level

    [Header("Scene Level Attributes")]
    [SerializeField] private float sceneFadeInSpeed;
    [SerializeField] private float textFadeInSpeed;
    [SerializeField] private float cameraMovementSpeed;

    private Vector3 cameraPosition;

    private const string DIR_NAME = "Player";
    private const string FILE_NAME = "data.binary";


    private void Start()
    {
        canvasOverlayImage.gameObject.SetActive(true); // Just to make sure
        goodJobText.alpha = 0f;
        goAheadText.alpha = 0f;
        amountText.alpha = 0f;
        levelsText.alpha = 0f;
        welcomeText.alpha = 0f;

        cameraPosition = cameraTransform.position;

        InitializePlayerData(); // Whenever the game starts, the game retrieves all the data about the player it needs
    }

    private void InitializePlayerData()
    {
        if (!Directory.Exists(DIR_NAME))
        {
            Directory.CreateDirectory(DIR_NAME);
        }

        if (!File.Exists(DIR_NAME + "/" + FILE_NAME)) // If the file has never been created before
        {
            InitializePlayerFile();
        }
        else // The player data file is already created, so only need to load in the data
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream saveFile = File.Open(DIR_NAME + "/" + FILE_NAME, FileMode.Open);

            ApplicationManager.instance.playerData = (PlayerData)formatter.Deserialize(saveFile);

            saveFile.Close();
        }
    }

    private void InitializePlayerFile()
    {
        ApplicationManager.instance.InitializePlayerData();

        BinaryFormatter binaryFormatter = new BinaryFormatter();
        FileStream saveFile = File.Create(DIR_NAME + "/" + FILE_NAME);
        binaryFormatter.Serialize(saveFile, ApplicationManager.instance.playerData);

        saveFile.Close();
    }


    /// <summary>
    /// This method starts running when the Level Scene loads, and steps through everything to get to the next level
    /// </summary>
    public void GoThroughLevelScene()
    {
        CenterOnCurrentLevel();
        StartCoroutine(FadeInLevelScene()); // START BY FADING IN THE LEVEL SCENE
        // IF PLAYER BEAT A LEVEL:
            // DISPLAY LEVEL X WON TEXT, THEN FADE OUT TEXT
            // GENERATE RANDOM NUMBER FROM 1 TO 6
            // MOVE CAMERA UP OR DOWN THAT NUMBER OF LEVELS
            // DISPLAY STATS OF THE LEVEL

        // FADE OUT
        // CHANGE SCENE TO MAINSCENE
    }

    /// <summary>
    /// Before fading in to the scene, the camera must already be centered on the level the player is currently on
    /// </summary>
    private void CenterOnCurrentLevel()
    {
        cameraTransform.position = new Vector3(cameraTransform.position.x,
            levelsGenerator.FindCorrectYPosition(ApplicationManager.instance.playerData.currentLevel - 1),
            cameraTransform.position.z);
    }

    /// <summary>
    /// Slowly fade in the Level Scene by reducing the alpha of the overlay to 0
    /// </summary>
    /// <returns></returns>
    private IEnumerator FadeInLevelScene()
    {
        yield return new WaitForSeconds(0.5f);
        Color overlayColor = canvasOverlayImage.color;
        float initial = 1f;
        float target = 0f;
        float t = 1;

        while (t > 0)
        {
            overlayColor.a = (initial * t) + (target * (1 - t));
            canvasOverlayImage.color = overlayColor;
            t -= sceneFadeInSpeed * Time.deltaTime;
            yield return null;
        }
        overlayColor.a = 0;
        canvasOverlayImage.color = overlayColor;

        print(ApplicationManager.instance.finishedLevelStatus);
        // KEEP ON GOING WITH THE PROCESS OF THE LEVELS SCENE
        if (ApplicationManager.instance.finishedLevelStatus != ApplicationManager.LevelFinishedStatus.No)
        {
            StartCoroutine(FadeInGoodJobText()); // Start fading in good job text if player finished a level
        }
        else
        {
            StartCoroutine(FadeInAndOutWelcomeText()); // Start showing welcome text to player if coming to Level Scene without finishing a level
        }
    }

    /// <summary>
    /// When the player enters the LevelScene without finishing a level, the LevelScene first starts off greeting the player
    /// </summary>
    /// <returns></returns>
    private IEnumerator FadeInAndOutWelcomeText()
    {
        yield return new WaitForSeconds(0.3f);

        // FADING IN THE WELCOME TEXT
        welcomeText.text = "Welcome!\nYou are on level " + ApplicationManager.instance.playerData.currentLevel.ToString();
        float t = 0;

        while (t < 1f)
        {
            welcomeText.alpha = t;
            t += textFadeInSpeed * Time.deltaTime;
            yield return null;
        }
        welcomeText.alpha = 1f;

        yield return new WaitForSeconds(1.5f);

        // FADING OUT THE WELCOME TEXT
        t = 1f;

        while (t > 0f)
        {
            welcomeText.alpha = t;
            t -= textFadeInSpeed * Time.deltaTime;
            yield return null;
        }
        welcomeText.alpha = 0f;

        StartCoroutine(FadeOutLevelScene());
    }

    /// <summary>
    /// This function runs when player enters the Levels Scene after winning a level, displaying a good job text
    /// </summary>
    private IEnumerator FadeInGoodJobText()
    {
        yield return new WaitForSeconds(0.3f);
        float t = 0;

        while (t < 1f)
        {
            goodJobText.alpha = t;
            t += textFadeInSpeed * Time.deltaTime;
            yield return null;
        }
        goodJobText.alpha = 1f;

        StartCoroutine(FadeInGoAheadText());
    }

    private IEnumerator FadeInGoAheadText()
    {
        yield return new WaitForSeconds(0.3f);
        float t = 0;

        while (t < 1f)
        {
            goAheadText.alpha = t;
            t += textFadeInSpeed * Time.deltaTime;
            yield return null;
        }
        goAheadText.alpha = 1f;

        StartCoroutine(FadeInAndHandleAmountText());
    }

    private IEnumerator FadeInAndHandleAmountText()
    {
        int levelJump = Random.Range(1, 7);
        int sum = levelJump;
        amountText.text = levelJump.ToString();

        // FADE IN AND HANDLE AMOUNT TEXT
        yield return new WaitForSeconds(0.3f);
        float t = 0;

        while (t < 1f)
        {
            amountText.alpha = t;
            t += textFadeInSpeed * Time.deltaTime;
            yield return null;
        }
        amountText.alpha = 1f;

        // HANDLE THE LEVELJUMP TEXT UNTIL YOU GET ANOTHER NUMBER THAN 6
        bool loopEntered = false;
        while (levelJump == 6)
        {
            loopEntered = true;
            levelJump = Random.Range(1, 7);
            sum += levelJump;
            amountText.text = amountText.text + " + " + levelJump.ToString();
            yield return new WaitForSeconds(0.5f);
        }

        if (loopEntered)
        {
            yield return new WaitForSeconds(0.3f);
            amountText.text = amountText.text + " = " + sum.ToString();
        }

        ApplicationManager.instance.playerData.currentLevel += sum; // Update the currentLevel stored on disk

        StartCoroutine(FadeInLevelsText());
    }

    private IEnumerator FadeInLevelsText()
    {
        yield return new WaitForSeconds(0.3f);
        float t = 0;

        while (t < 1f)
        {
            levelsText.alpha = t;
            t += textFadeInSpeed * Time.deltaTime;
            yield return null;
        }
        levelsText.alpha = 1f;

        StartCoroutine(LerpCameraToCurrentLevel());
    }

    private IEnumerator LerpCameraToCurrentLevel()
    {
        yield return new WaitForSeconds(0.3f);
        float t = 0;
        float initialY = cameraTransform.position.y;
        float targetY = levelsGenerator.FindCorrectYPosition(ApplicationManager.instance.playerData.currentLevel - 1);

        while (t <= 1f)
        {
            cameraPosition.y = (1f - t) * initialY + (t * targetY);
            cameraTransform.position = cameraPosition;
            t += cameraMovementSpeed * Time.deltaTime;
            yield return null;
        }
        cameraPosition.y = targetY;
        cameraTransform.position = cameraPosition;
    }


    private IEnumerator FadeInLevelStatisticsText()
    {
        yield return new WaitForSeconds(0.3f);
        float t = 0;

        while (t < 1f)
        {
            levelsText.alpha = t;
            t += textFadeInSpeed * Time.deltaTime;
            yield return null;
        }
        levelsText.alpha = 1f;

        StartCoroutine(LerpCameraToCurrentLevel());
    }

    /// <summary>
    /// Slowly fade out the Level Scene by increasing the alpha of the overlay to 1
    /// </summary>
    /// <returns></returns>
    private IEnumerator FadeOutLevelScene()
    {
        yield return new WaitForSeconds(0.5f);
        Color overlayColor = canvasOverlayImage.color;
        float initial = 1f;
        float target = 0f;
        float t = 1;

        while (t > 0)
        {
            overlayColor.a = (initial * t) + (target * (1 - t));
            canvasOverlayImage.color = overlayColor;
            t -= sceneFadeInSpeed * Time.deltaTime;
            yield return null;
        }
        overlayColor.a = 0;
        canvasOverlayImage.color = overlayColor;
    }
}
