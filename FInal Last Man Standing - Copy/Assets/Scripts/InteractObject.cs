using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using TMPro;

public class InteractObject : MonoBehaviour
{
    [SerializeField] PolygonCollider2D map;
    CinemachineConfiner confiner;
    public Transform teleportDestination;

    public GameObject interact;
    public GameObject interact2;
    public GameObject interactLight;
    private bool playerInRange;
    private InventoryController inventoryController;
    public bool isVendingMachine = false;
    public int itemPrice = 10;
    public GameObject itemPrefab;
    public TMP_Text messageText;
    public float messageDuration = 2f;
    private ItemDictionary itemDictionary;

    void Start()
    {
        confiner = FindAnyObjectByType<CinemachineConfiner>();
        inventoryController = FindObjectOfType<InventoryController>();
        itemDictionary = FindObjectOfType<ItemDictionary>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && playerInRange)
        {
            if (interact.CompareTag("savepoint"))
            {
                SaveController saveController = FindObjectOfType<SaveController>();
                if (saveController != null)
                {
                    ShowVendingMessage("Saved");
                    saveController.SaveGame();
                }
            }

            if (interact.CompareTag("item"))
            {
                Item item = interact.GetComponent<Item>();
                if (item != null)
                {
                    bool itemAdded = inventoryController.AddItem(interact.gameObject);
                    if (itemAdded)
                    {
                        interact.SetActive(false);
                        interact2.SetActive(false);
                    }
                }
            }

            if (interact.CompareTag("door"))
            {
                confiner.m_BoundingShape2D = map;
                TeleportPlayer();
            }

            if (interact.CompareTag("BombDoor"))
            {
                if (!HasItemWithID(8))
                {
                    ShowVendingMessage("You need a bomb to open this door.");
                    return;
                }

                RemoveItemByID(8);

                // ✅ เล่นเอฟเฟคระเบิดจาก Boom001
                Transform boom = interact.transform.Find("Boom001");
                if (boom != null)
                {
                    boom.gameObject.SetActive(true);
                    StartCoroutine(DisableObjectAfterSeconds(boom.gameObject, 2f));
                }

                interactLight.SetActive(false);
                StartCoroutine(ChangeToDoorAfterDelay(interact, 1.5f));
            }

            if (interact.CompareTag("keydoor"))
            {
                if (!HasItemWithID(7))
                {
                    ShowVendingMessage("You need a key to open this door.");
                    return;
                }

                if (HasItemWithID(8))
                {
                    GameObject[] npcStudents = GameObject.FindGameObjectsWithTag("NPCStudent");
                    foreach (GameObject npc in npcStudents)
                    {
                        npc.SetActive(false);
                    }
                }

                confiner.m_BoundingShape2D = map;
                TeleportPlayer();
            }

            if (interact.CompareTag("window"))
            {
                if (!HasItemWithID(9))
                {
                    ShowVendingMessage("You need a rope to go 2-3 room.");
                    return;
                }

                confiner.m_BoundingShape2D = map;
                TeleportPlayer();
            }

            if (interact.CompareTag("NPCStudent"))
            {
                bool hasBattery = HasItemWithID(4);
                bool hasSugar = HasItemWithID(5);
                bool hasKNO3 = HasItemWithID(6);

                if (hasBattery && hasSugar && hasKNO3)
                {
                    RemoveItemByID(4);
                    RemoveItemByID(5);
                    RemoveItemByID(6);

                    GameObject bombPrefab = itemDictionary.GetItemPrefab(8);
                    if (bombPrefab != null)
                    {
                        bool added = inventoryController.AddItem(bombPrefab);
                        if (added)
                            ShowVendingMessage("You received a bomb!");
                        else
                            ShowVendingMessage("Inventory full, couldn't add bomb.");
                    }

                    GameObject[] miniBosses = GameObject.FindGameObjectsWithTag("MiniBoss");
                    foreach (GameObject boss in miniBosses)
                    {
                        boss.SetActive(true);
                    }
                }
                else
                {
                    ShowVendingMessage("You need battery, sugar, and KNO3 to make a bomb.");
                }
            }

            if (isVendingMachine)
            {
                if (GameManager.instance.gold >= itemPrice)
                {
                    if (itemPrefab != null)
                    {
                        bool added = inventoryController.AddItem(itemPrefab);

                        if (added)
                        {
                            GameManager.instance.SpendGold(itemPrice);
                            GameManager.instance.UpdateGoldUI();
                            ShowVendingMessage("Item purchased successfully!");
                        }
                        else
                        {
                            ShowVendingMessage("Inventory full or item stack maxed!");
                        }
                    }
                }
                else
                {
                    ShowVendingMessage("Not enough yen!");
                }
            }
        }
    }

    void TeleportPlayer()
    {
        if (teleportDestination != null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                player.transform.position = teleportDestination.position;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            interactLight.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            interactLight.SetActive(false);
        }
    }

    void ShowVendingMessage(string message)
    {
        if (messageText != null)
        {
            messageText.text = message;
            StopAllCoroutines();
            StartCoroutine(HideVendingMessageAfterDelay());
        }
    }

    IEnumerator HideVendingMessageAfterDelay()
    {
        messageText.gameObject.SetActive(true);
        yield return new WaitForSeconds(messageDuration);
        messageText.gameObject.SetActive(false);
    }

    private bool HasItemWithID(int id)
    {
        foreach (Transform slotTransform in inventoryController.slotPanel.transform)
        {
            ItemSlot slot = slotTransform.GetComponent<ItemSlot>();
            if (slot.currentItem != null)
            {
                Item item = slot.currentItem.GetComponent<Item>();
                if (item != null && item.ID == id && item.quantity > 0)
                {
                    return true;
                }
            }
        }
        return false;
    }

    private void RemoveItemByID(int itemID)
    {
        foreach (Transform slotTransform in inventoryController.slotPanel.transform)
        {
            ItemSlot slot = slotTransform.GetComponent<ItemSlot>();
            if (slot.currentItem != null)
            {
                Item item = slot.currentItem.GetComponent<Item>();
                if (item != null && item.ID == itemID)
                {
                    item.quantity--;
                    slot.UpdateStackText();

                    if (item.quantity <= 0)
                    {
                        Destroy(slot.currentItem);
                        slot.currentItem = null;
                    }
                    break;
                }
            }
        }
    }

    IEnumerator ChangeToDoorAfterDelay(GameObject bombDoor, float delay)
    {
        yield return new WaitForSeconds(delay);
        bombDoor.tag = "door";
    }

    IEnumerator DisableObjectAfterSeconds(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        obj.SetActive(false);
    }
}
