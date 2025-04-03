using System.Collections.Generic;
using System.Collections;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    private Vector3 playerPosition; // บันทึกตำแหน่ง Player
    private Scene previousScene;
    public int savedHP;
    public int savedMaxHP;
    public int savedDamage;
    public int savedLevel;
    public string savedName;
    public GameObject currentZombie;
    public bool isLoadingFromSave = false;
    public bool sceneWasDisabled = false;


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // ทำให้ GameManager คงอยู่ข้าม Scene
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadTurnBase(Transform player, GameObject zombie)
    {
        playerPosition = player.position;
        previousScene = SceneManager.GetActiveScene();
        currentZombie = zombie;
        StartCoroutine(LoadBattleScene());
    }

    private IEnumerator LoadBattleScene()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("TurnBase", LoadSceneMode.Additive);
        while (!asyncLoad.isDone)
            yield return null;

        sceneWasDisabled = true; // ✅ จำไว้ว่า scene ถูกปิด
        SetSceneActive(previousScene, false); // ปิด scene ปัจจุบัน (2-1 Room)
        SceneManager.SetActiveScene(SceneManager.GetSceneByName("TurnBase"));
    }

    public void ReturnToPreviousScene()
    {
        StartCoroutine(UnloadBattleScene());
    }

    private IEnumerator UnloadBattleScene()
    {
        AsyncOperation asyncUnload = SceneManager.UnloadSceneAsync("TurnBase");
        while (!asyncUnload.isDone)
        {
            yield return null;
        }

        SetSceneActive(previousScene, true);
        SceneManager.SetActiveScene(previousScene);

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        GameObject zombie = GameObject.FindGameObjectWithTag("Enemy"); // หา Zombie ในฉาก

        if (player != null)
        {
            // ตรวจสอบตำแหน่งของ Player และ Zombie
            if (zombie != null)
            {
                if (player.transform.position.x > zombie.transform.position.x)
                {
                    player.transform.position = new Vector3(playerPosition.x + 2, playerPosition.y, playerPosition.z);
                }
                else
                {
                    player.transform.position = new Vector3(playerPosition.x - 2, playerPosition.y, playerPosition.z);
                }
            }
            else
            {
                player.transform.position = playerPosition; // ถ้าไม่มี Zombie ก็ใช้ตำแหน่งเดิม
            }
        }
    }

    public void SetSceneActive(Scene scene, bool isActive)
    {
        foreach (GameObject obj in scene.GetRootGameObjects())
        {
            if (obj != null)
            {
                if (isActive && !obj.activeSelf)
                {
                    obj.SetActive(true);
                    Debug.Log("SetActive TRUE: " + obj.name);
                }
                else if (!isActive && obj.activeSelf)
                {
                    obj.SetActive(false);
                    Debug.Log("SetActive FALSE: " + obj.name);
                }
            }
        }
    }


}
