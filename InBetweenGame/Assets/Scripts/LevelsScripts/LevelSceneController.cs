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
    [SerializeField] private Button playLevelButton; // Button to play the level
    [SerializeField] private GameObject GameWonUIObject;

    [Header("Scene Level Attributes")]
    public float sceneFadeInSpeed;
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
        levelStatisticsText.alpha = 0f;
        playLevelButton.gameObject.SetActive(false);
        GameWonUIObject.SetActive(false);

        cameraPosition = cameraTransform.position;
    }


    /// <summary>
    /// This method starts running when the Level Scene loads, and steps through everything to get to the next level
    /// </summary>
    public void GoThroughLevelScene()
    {
        PlayerData playerData = ApplicationManager.instance.GetPlayerData();
        if (playerData.prevLevel == 100 && playerData.levelFinishedStatus == ApplicationManager.LevelFinishedStatus.Win)
        {
            GameWonInitiative();
        }
        else
        {
            CenterOnCurrentLevel();
            InitializeTexts();
            StartCoroutine(FadeInLevelScene()); // START BY FADING IN THE LEVEL SCENE
        }

    }

    private void GameWonInitiative()
    {
        GameWonUIObject.SetActive(true);
    }

    /// <summary>
    /// Initializes all the texts to show correct strings before fading into existence
    /// </summary>
    private void InitializeTexts()
    {
        // GOOD JOB TEXT / BETTER LUCK NEXT TIME TEXT
        goodJobText.text = ApplicationManager.instance.GetPlayerData().levelFinishedStatus == ApplicationManager.LevelFinishedStatus.Win ? "You won! Good job!" : "Better luck next time!";

        // GO AHEAD TEXT / GO BACK TEXT
        goAheadText.text = ApplicationManager.instance.GetPlayerData().levelFinishedStatus == ApplicationManager.LevelFinishedStatus.Win ? "You get to go ahead:" : "You must go back:";

        // LEVELSTATISTICS TEXT
        // TODO: WHEN YOU HAVE FINISHED CALCULATING THE RANDOM AMOUNT OF LEVELS TO GO FORWARD OR BACK YOU RETURN HERE TO SET THE LEVELSTATISTICSTEXT IN THIS METHOD
        // BEFORE FADING IT IN
    }

    /// <summary>
    /// Before fading in to the scene, the camera must already be centered on the level the player is currently on
    /// </summary>
    private void CenterOnCurrentLevel()
    {
        cameraTransform.position = new Vector3(cameraTransform.position.x,
            levelsGenerator.FindCorrectYPosition(ApplicationManager.instance.GetPlayerData().currentLevel - 1),
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

        // KEEP ON GOING WITH THE PROCESS OF THE LEVELS SCENE
        if (ApplicationManager.instance.GetPlayerData().levelFinishedStatus != ApplicationManager.LevelFinishedStatus.No)
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
        welcomeText.text = "Welcome!\nYou are on level " + ApplicationManager.instance.GetPlayerData().currentLevel.ToString();
        StartCoroutine(FadeText(welcomeText, 0f, 1f));

        yield return new WaitForSeconds(1.5f);

        print(ApplicationManager.instance.GetPlayerData().currentLevel - 1);
        print("starting the loop!");
        for (int i = 0; i < levelsGenerator.levelDataArray.arrayOfLevelData.Length; i++)
        {
            print(levelsGenerator.levelDataArray.arrayOfLevelData[i]);
        }
        ApplicationManager.instance.currLevelData = levelsGenerator.levelDataArray.arrayOfLevelData[ApplicationManager.instance.GetPlayerData().currentLevel - 1];

        // FADING OUT THE WELCOME TEXT
        StartCoroutine(FadeText(welcomeText, 1f, 0f, FadeInLevelStatisticsText()));
    }

    /// <summary>
    /// This function runs when player enters the Levels Scene after winning a level, displaying a good job text
    /// </summary>
    private IEnumerator FadeInGoodJobText()
    {
        yield return null;
        StartCoroutine(FadeText(goodJobText, 0f, 1f, FadeInGoAheadText()));
    }

    private IEnumerator FadeInGoAheadText()
    {
        yield return null;
        StartCoroutine(FadeText(goAheadText, 0f, 1f, FadeInAndHandleAmountText()));
    }

    private IEnumerator FadeInAndHandleAmountText()
    {
        int levelJump = Random.Range(1, 7);
        int sum = levelJump;
        amountText.text = levelJump.ToString();

        // FADE IN AND HANDLE AMOUNT TEXT
        StartCoroutine(FadeText(amountText, 0f, 1f));

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

        // Update the currentLevel stored on disk
        PlayerData oldPlayerData = ApplicationManager.instance.GetPlayerData();

        oldPlayerData.prevLevel = oldPlayerData.currentLevel;
        oldPlayerData.currentLevel += ApplicationManager.instance.GetPlayerData().levelFinishedStatus == ApplicationManager.LevelFinishedStatus.Win ? sum : -sum;
        oldPlayerData.currentLevel = Mathf.Min(ApplicationManager.instance.GetPlayerData().currentLevel, 100); // TODO: FIND SOME WAY TO ACCESS CONST IN GENERATOR
        oldPlayerData.currentLevel = Mathf.Max(ApplicationManager.instance.GetPlayerData().currentLevel, 1);
        ApplicationManager.instance.currLevelData = levelsGenerator.levelDataArray.arrayOfLevelData[ApplicationManager.instance.GetPlayerData().currentLevel - 1];
        ApplicationManager.instance.SetPlayerData(oldPlayerData);

        //print("Current level: " + oldPlayerData.currentLevel.ToString());
        //print("Spawn rate: " + ApplicationManager.instance.currLevelData.secondsToSpawn);
        StartCoroutine(FadeInLevelsText());
    }

    private IEnumerator FadeInLevelsText()
    {
        yield return new WaitForSeconds(0.3f);
        StartCoroutine(FadeText(levelsText, 0f, 1f, LerpCameraToCurrentLevel()));
    }

    private IEnumerator LerpCameraToCurrentLevel()
    {
        yield return new WaitForSeconds(0.3f);
        float t = 0;
        float initialY = cameraTransform.position.y;
        float targetY = levelsGenerator.FindCorrectYPosition(ApplicationManager.instance.GetPlayerData().currentLevel - 1);

        while (t <= 1f)
        {
            cameraPosition.y = (1f - t) * initialY + (t * targetY);
            cameraTransform.position = cameraPosition;
            t += cameraMovementSpeed * Time.deltaTime;
            yield return null;
        }
        cameraPosition.y = targetY;
        cameraTransform.position = cameraPosition;
        StartCoroutine(FadeOutAllGoodJobTexts());
    }

    private IEnumerator FadeOutAllGoodJobTexts()
    {
        yield return new WaitForSeconds(0.3f);
        float t = 1f;

        while (t > 0f)
        {
            goodJobText.alpha = t;
            goAheadText.alpha = t;
            amountText.alpha = t;
            levelsText.alpha = t;
            t -= textFadeInSpeed * Time.deltaTime;
            yield return null;
        }
        goodJobText.alpha = 0f;
        goAheadText.alpha = 0f;
        amountText.alpha = 0f;
        levelsText.alpha = 0f;

        StartCoroutine(FadeInLevelStatisticsText());
    }


    private IEnumerator FadeInLevelStatisticsText()
    {
        yield return new WaitForSeconds(0.3f);
        int currLevelIndex = ApplicationManager.instance.GetPlayerData().currentLevel;
        int prevLevelIndex = ApplicationManager.instance.GetPlayerData().prevLevel;
        LevelData currLevel = levelsGenerator.levelDataArray.arrayOfLevelData[currLevelIndex-1];
        LevelData prevLevel = levelsGenerator.levelDataArray.arrayOfLevelData[prevLevelIndex-1];

        //print("currLevelIndex = " + currLevelIndex.ToString());
        //print("prevLevelIndex = " + prevLevelIndex.ToString());
        string differenceString = currLevelIndex > prevLevelIndex ? "+" : "-";
        //print("differenceString = " + differenceString);

        levelStatisticsText.text = "Level " + currLevelIndex.ToString();
        
        // ADD TO THE LEVELSTATISTICSTEXT THE LEVEL ATTRIBUTES THAT HAVE GOTTEN TOUGHER
        if (currLevel.totalAmountOfEnemies != prevLevel.totalAmountOfEnemies)
        {
            levelStatisticsText.text += "\n" + differenceString + " Amount of Enemies";
        } if (currLevel.enemyDamageBoost != prevLevel.enemyDamageBoost)
        {
            levelStatisticsText.text += "\n" + differenceString + " Enemy Damage";
        } if (currLevel.enemyHealthBoost != prevLevel.enemyHealthBoost)
        {
            levelStatisticsText.text += "\n" + differenceString + " Enemy Health";
        } if (currLevel.enemySpeedBoost != prevLevel.enemySpeedBoost)
        {
            levelStatisticsText.text += "\n" + differenceString + " Enemy Speed";
        } if (currLevel.secondsToSpawn != prevLevel.secondsToSpawn)
        {
            levelStatisticsText.text += "\n" + differenceString + " Spawn Rate";
        } if (currLevel.numOfMaxActiveEnemies != prevLevel.numOfMaxActiveEnemies)
        {
            levelStatisticsText.text += "\n" + differenceString + " Number of Active Enemies";
        }

        StartCoroutine(FadeText(levelStatisticsText, 0f, 1f));
        yield return new WaitForSeconds(0.5f);
        playLevelButton.gameObject.SetActive(true);
    }


    /// <summary>
    ///  A general coroutine for lerping the alpha value of the color of a text
    /// </summary>
    /// <param name="text">The input text</param>
    /// <param name="start">The start value of t</param>
    /// <param name="end">The target value of t</param>
    /// <param name="inputCoroutine">The next coroutine that will be started after this one</param>
    /// <returns>Your mom</returns>
    private IEnumerator FadeText(TextMeshProUGUI text, float start, float end, IEnumerator inputCoroutine = null)
    {
        yield return new WaitForSeconds(0.3f);
        float t = start;

        if (start < end)
        {
            while (t < end)
            {
                text.alpha = t;
                t += textFadeInSpeed * Time.deltaTime;
                yield return null;
            }
        }
        else if (start > end)
        {
            while (t > end)
            {
                text.alpha = t;
                t -= textFadeInSpeed * Time.deltaTime;
                yield return null;
            }
        }
        text.alpha = end;

        if (inputCoroutine != null) { StartCoroutine(inputCoroutine); }
    }
}
