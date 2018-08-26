using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController
{
    private static string MAIN = "Main";
    private static string ACTIVE = "Active";

    public static void LoadMainMenu()
    {
        SceneManager.LoadScene(MAIN);
    }

    public static void LoadActive()
    {
        SceneManager.LoadScene(ACTIVE);
    }
}
