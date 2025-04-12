using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class ZombiePrefabEntry
{
    public string prefabName;
    public GameObject prefab;
}

public class SaveController : MonoBehaviour
{
    public static SaveController instance;
    public GameObject zombiePrefab;
    public List<ZombiePrefabEntry> zombiePrefabs;

    private string saveLocation;
    private InventoryController inventoryController;

    void Start()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        saveLocation = Path.Combine(Application.persistentDataPath, "saveData.json");
        inventoryController = FindAnyObjectByType<InventoryController>();
    }

    public void SaveGame()
    {
        List<SceneObjectSaveData> sceneObjects = new List<SceneObjectSaveData>();
        GameObject[] allSceneObjects = Resources.FindObjectsOfTypeAll<GameObject>();

        foreach (GameObject obj in allSceneObjects)
        {
            if (obj.scene.IsValid() && obj.scene.name == "2-1 Room")
            {
                if (obj.CompareTag("item") || obj.CompareTag("BombDoor"))
                {
                    sceneObjects.Add(new SceneObjectSaveData
                    {
                        objectName = obj.name,
                        isActive = obj.activeSelf,
                        objectTag = obj.tag,
                        position = obj.transform.position
                    });
                }
            }
        }

        List<ZombieSaveData> zombiesData = new List<ZombieSaveData>();
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();

        foreach (GameObject zombie in allObjects)
        {
            if ((zombie.CompareTag("Enemy") || zombie.CompareTag("MiniBoss") || zombie.CompareTag("Boss") || zombie.CompareTag("NPCStudent")) &&
                zombie.scene.IsValid() && zombie.scene.name == "2-1 Room")
            {
                zombiesData.Add(new ZombieSaveData
                {
                    position = zombie.transform.position,
                    zombieName = zombie.name.Replace("(Clone)", "").Trim(),
                    isActive = zombie.activeSelf
                });
            }
        }

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject == null) return;

        Unit playerUnit = playerObject.GetComponent<Unit>();
        if (playerUnit == null) return;

        playerUnit.currentEnergy = playerUnit.maxEnergy;
        GameManager.instance.savedEnergy = playerUnit.maxEnergy;

        SaveData saveData = new SaveData
        {
            playerPosition = playerObject.transform.position,
            playerHP = GameManager.instance.savedHP,
            playerEnergy = GameManager.instance.savedEnergy,
            playerGold = GameManager.instance.gold,
            zombiesData = zombiesData,
            inventorySaveData = inventoryController.GetInventoryItems(),
            sceneObjectData = sceneObjects
        };

        File.WriteAllText(saveLocation, JsonUtility.ToJson(saveData));
        Debug.Log("Game Saved Successfully!");
    }

    public void LoadGame()
    {
        if (!File.Exists(saveLocation)) return;

        SaveData saveData = JsonUtility.FromJson<SaveData>(File.ReadAllText(saveLocation));

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.transform.position = saveData.playerPosition;
            player.GetComponent<Unit>().currentHP = saveData.playerHP;
            player.GetComponent<Unit>().currentEnergy = saveData.playerEnergy;

            GameManager.instance.savedHP = saveData.playerHP;
            GameManager.instance.savedEnergy = saveData.playerEnergy;
            GameManager.instance.gold = saveData.playerGold;
            GameManager.instance.UpdateGoldUI();

            UpdateCameraConfinerToClosest(player.GetComponent<Collider2D>());
        }

        List<GameObject> allZombies = new List<GameObject>();
        allZombies.AddRange(GameObject.FindGameObjectsWithTag("Enemy"));
        allZombies.AddRange(GameObject.FindGameObjectsWithTag("MiniBoss"));
        allZombies.AddRange(GameObject.FindGameObjectsWithTag("Boss"));
        allZombies.AddRange(GameObject.FindGameObjectsWithTag("NPCStudent"));

        foreach (ZombieSaveData zombieData in saveData.zombiesData)
        {
            foreach (GameObject zombie in allZombies)
            {
                string zombieNameInScene = zombie.name.Replace("(Clone)", "").Trim();
                if (zombieNameInScene == zombieData.zombieName)
                {
                    zombie.transform.position = zombieData.position;
                    zombie.SetActive(zombieData.isActive);
                    break;
                }
            }
        }

        GameObject[] allSceneObjects = Resources.FindObjectsOfTypeAll<GameObject>();

        foreach (SceneObjectSaveData objData in saveData.sceneObjectData)
        {
            foreach (GameObject obj in allSceneObjects)
            {
                if (obj.scene.IsValid() && obj.scene.name == "2-1 Room" && obj.name == objData.objectName)
                {
                    if (Vector3.Distance(obj.transform.position, objData.position) < 0.1f)
                    {
                        obj.SetActive(objData.isActive);

                        if (!string.IsNullOrEmpty(objData.objectTag))
                        {
                            obj.tag = objData.objectTag;
                        }

                        InteractObject interactObj = obj.GetComponent<InteractObject>();
                        if (interactObj != null && interactObj.interact2 != null)
                            interactObj.interact2.SetActive(objData.isActive);

                        break;
                    }
                }
            }
        }

        inventoryController.SetInventoryItems(saveData.inventorySaveData);
    }

    void UpdateCameraConfinerToClosest(Collider2D playerCollider)
    {
        CinemachineConfiner confiner = FindAnyObjectByType<CinemachineConfiner>();
        if (confiner == null || playerCollider == null) return;

        PolygonCollider2D[] allPolygons = FindObjectsOfType<PolygonCollider2D>();
        float closestDistance = float.MaxValue;
        PolygonCollider2D closest = null;

        foreach (PolygonCollider2D poly in allPolygons)
        {
            float dist = Vector2.Distance(playerCollider.transform.position, poly.bounds.center);
            if (dist < closestDistance)
            {
                closestDistance = dist;
                closest = poly;
            }
        }

        if (closest != null)
        {
            confiner.m_BoundingShape2D = closest;

            var shape = confiner.m_BoundingShape2D;
            confiner.m_BoundingShape2D = null;
            confiner.m_BoundingShape2D = shape;
        }
    }
    public void NewGame()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Unit unit = player.GetComponent<Unit>();
            if (unit != null)
            {
                unit.ResetToDefault();
                GameManager.instance.savedHP = unit.currentHP;
                GameManager.instance.savedEnergy = unit.currentEnergy;
                GameManager.instance.gold = 0;
                GameManager.instance.UpdateGoldUI();
            }
        }

        InventoryController inventory = FindObjectOfType<InventoryController>();
        if (inventory != null)
        {
            inventory.SetInventoryItems(new List<InventorySaveData>());
        }

        if (File.Exists(saveLocation))
        {
            File.Delete(saveLocation);
            Debug.Log("Old save deleted.");
        }

        Debug.Log("New Game started.");
    }
}


