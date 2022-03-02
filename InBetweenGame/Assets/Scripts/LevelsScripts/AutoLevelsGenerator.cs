using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoLevelsGenerator : MonoBehaviour
{
    [SerializeField] private GameObject levelsParentObject;

    private void Start()
    {
        GenerateAutoLevels();
    }

    /// <summary>
    /// Goes over all levels not explicitly marked as manual and auto generates the difficulty. Sets the number for all levels as well.
    /// </summary>
    private void GenerateAutoLevels()
    {
        SetLevelNumbers();
    }


    /// <summary>
    /// Make the levels show and know number what they are
    /// </summary>
    private void SetLevelNumbers()
    {
        int limit = levelsParentObject.transform.childCount;

        for (int i = 0; i < limit; i++)
        {
            levelsParentObject.transform.GetChild(i).GetComponent<LevelController>().SetLevelNumber(i+1);
        }
    }
}
