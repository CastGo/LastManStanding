﻿using System.Collections.Generic;
using System.Collections;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

[System.Serializable]
public class SceneObjectTempData
{
    public string name;
    public bool isActive;
}

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    private Vector3 playerPosition; // บันทึกตำแหน่ง Player
    private Scene previousScene;

    public int gold = 0;
    public TMP_Text goldText;
    public int savedHP;
    public int savedMaxHP;
    public int savedEnergy; 
    public int savedMaxEnergy;
    public int savedDamage;
    public int savedLevel;
    public string savedName;
    public GameObject currentZombie;
    public bool isLoadingFromSave = false;
    public bool sceneWasDisabled = false;
    private bool isLoadingBattleScene = false;
    public HashSet<string> deactivatedZombies = new HashSet<string>();
    public GameObject normalZombieTurnBase;
    public GameObject miniBossTurnBase;
    public GameObject bossTurnBase;
    public GameObject nextEnemyPrefab;
    public List<SceneObjectTempData> sceneObjectStates = new List<SceneObjectTempData>();

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
        if (isLoadingBattleScene) return; // ➕ ป้องกันไม่ให้โหลดซ้ำ
        isLoadingBattleScene = true;      // ✅ mark ว่ากำลังโหลด

        playerPosition = player.position;
        previousScene = SceneManager.GetActiveScene();
        currentZombie = zombie;

        switch (zombie.tag)
        {
            case "Enemy":
            case "resetzombie":
                nextEnemyPrefab = normalZombieTurnBase;
                break;
            case "MiniBoss":
                nextEnemyPrefab = miniBossTurnBase;
                break;
            case "Boss":
                nextEnemyPrefab = bossTurnBase;
                break;
            default:
                Debug.LogWarning("Unknown tag on zombie: " + zombie.tag);
                nextEnemyPrefab = normalZombieTurnBase; // fallback
                break;
        }

        sceneObjectStates.Clear();
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj.scene.IsValid() && obj.scene.name == "2-1 Room" && obj.CompareTag("item"))
            {
                sceneObjectStates.Add(new SceneObjectTempData
                {
                    name = obj.name,
                    isActive = obj.activeSelf
                });
            }
        }
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

        isLoadingBattleScene = false;

        SetSceneActive(previousScene, true);
        SceneManager.SetActiveScene(previousScene);

        GameObject player = GameObject.FindGameObjectWithTag("Player");

        GameObject zombie = GameManager.instance.currentZombie;

        if (player != null)
        {
            player.GetComponent<PlayerController>()?.RefreshStatsFromGameManager();

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
                player.transform.position = playerPosition;
            }
        }

        GameObject[] allZombies = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (GameObject z in allZombies)
        {
            if ((z.CompareTag("Enemy") || z.CompareTag("MiniBoss") || z.CompareTag("Boss"))
                && z.scene.IsValid() && z.scene.name == "2-1 Room")
            {
                if (GameManager.instance.deactivatedZombies.Contains(z.name))
                {
                    z.SetActive(false);
                }
            }
        }

        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj.scene.IsValid() && obj.scene.name == "2-1 Room")
            {
                foreach (var state in sceneObjectStates)
                {
                    if (obj.name.Contains(state.name)) // ป้องกันกรณี clone
                    {
                        obj.SetActive(state.isActive);

                        // ถ้ามี interact2 ให้ Sync ด้วย
                        InteractObject io = obj.GetComponent<InteractObject>();
                        if (io != null && io.interact2 != null)
                        {
                            io.interact2.SetActive(state.isActive);
                        }

                        break;
                    }
                }
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

    public void RestoreSceneObjects()
    {
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj.scene.IsValid() && obj.scene.name == "2-1 Room")
            {
                foreach (var state in sceneObjectStates)
                {
                    if (obj.name.Contains(state.name))
                    {
                        obj.SetActive(state.isActive);

                        InteractObject io = obj.GetComponent<InteractObject>();
                        if (io != null && io.interact2 != null)
                        {
                            io.interact2.SetActive(state.isActive);
                        }

                        break;
                    }
                }
            }
        }
    }
    public void AddGold(int amount)
    {
        gold += amount;
        UpdateGoldUI();
    }

    public bool SpendGold(int amount)
    {
        if (gold >= amount)
        {
            gold -= amount;
            UpdateGoldUI();
            return true;
        }
        return false;
    }

    public void UpdateGoldUI()
    {
        if (goldText != null)
            goldText.text = "¥ " + gold.ToString();
    }
}
