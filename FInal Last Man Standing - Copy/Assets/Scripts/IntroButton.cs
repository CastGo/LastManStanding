using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class IntroButton : MonoBehaviour
{
    public void OnPlayButton()
    {
        PlayerPrefs.SetInt("LoadGame", 0); // ใช้แค่บอกว่าไม่ต้องโหลด
        SceneManager.LoadScene("2-1 Room");
    }

    public void OnLoadButton()
    {
        PlayerPrefs.SetInt("LoadGame", 1); // ใช้แค่บอกว่าให้โหลด
        PlayerPrefs.Save();
        SceneManager.LoadScene("2-1 Room");
    }

    public void OnExitButton()
    {
        Application.Quit();
    }
}
