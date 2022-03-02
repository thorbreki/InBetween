using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApplicationManager : MonoBehaviour
{
    public static ApplicationManager instance;
    
    private void Start()
    {
        instance = this;
        Application.targetFrameRate = 60;
    }
}
