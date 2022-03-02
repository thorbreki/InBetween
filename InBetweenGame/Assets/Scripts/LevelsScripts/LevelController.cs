using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LevelController : MonoBehaviour
{
    // The ID of this level is its level number, which will be stored in this text component
    [SerializeField] private TextMeshPro levelNumberTextComp;

    public void SetLevelNumber(int newNumber)
    {
        levelNumberTextComp.text = newNumber.ToString();
        levelNumberTextComp.gameObject.SetActive(true);
        print("Setting level number for level: " + newNumber.ToString());
    }
}
