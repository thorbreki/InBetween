using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class LevelsGenerator : MonoBehaviour
{
    [SerializeField] private Transform levelNumbersParentObject;
    [SerializeField] private Transform specialLevelNumbersParentObject;

    // CONSTANTS
    private const int AMOUNT_OF_LEVELS = 100;
    private const int DISTANCE_BETWEEN_LEVELS = 4;
    private const string DIR_NAME = "Levels";
    private const string FILE_NAME = "levels.binary";

    // Things needed to know to load data from file
    [HideInInspector] public ArrayOfLevelData levelDataArray;

    private Vector3 levelNumberPosition;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        levelDataArray = new ArrayOfLevelData();
        levelDataArray.arrayOfLevelData = new LevelData[AMOUNT_OF_LEVELS];


        levelNumberPosition = new Vector3(-2.5f, 0, 0);
        SetLevelNumbers();
        GetLevels();
    }

    private void GetLevels()
    {
        if (!Directory.Exists(DIR_NAME))
        {
            Directory.CreateDirectory(DIR_NAME);
            GenerateAndSaveLevelsData();
        } else
        {
            LoadData();
        }
    }

    private void LoadData()
    {
        print("Loading the levels data!");
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream saveFile = File.Open(DIR_NAME + "/" + FILE_NAME, FileMode.Open);

        levelDataArray = (ArrayOfLevelData)formatter.Deserialize(saveFile);

        saveFile.Close();
    }

    private void GenerateAndSaveLevelsData()
    {
        print("Generating and saving the level data!");
        ArrayOfLevelData levelDataArray = new ArrayOfLevelData();
        levelDataArray.arrayOfLevelData = new LevelData[AMOUNT_OF_LEVELS];

        // GENERATE ALL THE DATA TO SERIALIZE INTO THE BINARY FILE
        for (int i = 0; i < AMOUNT_OF_LEVELS; i++)
        {
            /* -- CREATE NEW INSTANCES OF LEVELDATA CLASS AND ADD THEM TO THE LEVELDATAARRAY CLASS -- */
            LevelData currLevelData = new LevelData();
            currLevelData.totalAmountOfEnemies = i * 2;
            currLevelData.secondsToSpawn = 20f / (i + 1);
            currLevelData.enemyDamageBoost = ApplicationManager.instance.playerData.currentLevel > 50 ? 1 : 0; // When player has gotten to over level 50, enemies/projectiles to 1 more damge
            currLevelData.enemySpeedBoost = 0f;
            currLevelData.enemyHealthBoost = ApplicationManager.instance.playerData.currentLevel > 50 ? 2 : 0; // Enemies get 2 more health after reaching level 50
            currLevelData.numOfMaxActiveEnemies = i + 1;
            levelDataArray.arrayOfLevelData[i] = currLevelData;
        }

        // SAVE THE DATA TO A BINARY FILE
        BinaryFormatter binaryFormatter = new BinaryFormatter();
        FileStream saveFile = File.Create(DIR_NAME + "/" + FILE_NAME);
        binaryFormatter.Serialize(saveFile, levelDataArray);

        saveFile.Close();
    }

    private void SetLevelNumbers()
    {
        int i = 0;
        int currChild = 0;
        while (i < AMOUNT_OF_LEVELS)
        {
            if ((i+1) % 5 != 0)
            {
                GameObject currLevelNumber = levelNumbersParentObject.GetChild(currChild).gameObject; // Get a reference to the levelNumberObect

                // Position the level number
                levelNumberPosition.y = FindCorrectYPosition(i);
                currLevelNumber.transform.position = levelNumberPosition;

                // Set the text of the level number to the correct number
                currLevelNumber.GetComponent<TMPro.TextMeshPro>().text = (i + 1).ToString();

                currLevelNumber.SetActive(true); // Set the number as active, now that it is correct
                currChild++;
            }

            i++;
        }

        levelNumberPosition.x = specialLevelNumbersParentObject.transform.position.x;
        int numOfSpecialLevels = AMOUNT_OF_LEVELS / 5;
        for (int j = 0; j < numOfSpecialLevels; j++)
        {
            GameObject currLevelNumber = specialLevelNumbersParentObject.GetChild(j).gameObject;
            levelNumberPosition.y = (j * (DISTANCE_BETWEEN_LEVELS * 5)) + DISTANCE_BETWEEN_LEVELS * 4;
            currLevelNumber.transform.position = levelNumberPosition;
            currLevelNumber.GetComponent<TMPro.TextMeshPro>().text = ((j + 1) * 5).ToString();
            currLevelNumber.SetActive(true);
        }
    }


    public int FindCorrectYPosition(int inputNumber)
    {
        return inputNumber * DISTANCE_BETWEEN_LEVELS;
    }
}
