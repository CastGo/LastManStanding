using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class IntroButton : MonoBehaviour
{
    public void OnPlayButton()
    {
        PlayerPrefs.SetInt("LoadGame", 0); // ใช้แค่บอกว่าไม่ต้องโหลด
        PlayerPrefs.SetInt("StartGameDialogue", 1);
        SceneFader.instance.FadeToScene("2-1 Room");
    }

    public void OnLoadButton()
    {
        PlayerPrefs.SetInt("LoadGame", 1); // ใช้แค่บอกว่าให้โหลด
        PlayerPrefs.SetInt("StartGameDialogue", 0);
        PlayerPrefs.Save();
        SceneFader.instance.FadeToScene("2-1 Room");
    }
    public void OnLeaveButton()
    {
        SceneFader.instance.FadeToScene("IntroScene");
    }
    public void OnExitButton()
    {
        Application.Quit();
    }
}
