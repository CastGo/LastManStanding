using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using TMPro;

public class InteractObject : MonoBehaviour
{
    [SerializeField]
    PolygonCollider2D map;
    CinemachineConfiner confiner;
    public Transform teleportDestination;

    public GameObject interact;
    public GameObject interact2;
    public GameObject interactLight;
    private bool playerInRange;
    private InventoryController inventoryController;
    public bool isVendingMachine = false;
    //public int vendingItemID = -1;
    public int itemPrice = 10; // ราคาไอเทม
    public GameObject itemPrefab; // ดึงจาก Inspector ตาม ID
    public TMP_Text messageText;
    public float messageDuration = 2f;

    void Start()
    {
        confiner = FindAnyObjectByType<CinemachineConfiner>();
        inventoryController = FindObjectOfType<InventoryController>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.E) && playerInRange)
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
                    //Add item inventory
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
                
                if (teleportDestination != null)
                {
                    GameObject player = GameObject.FindGameObjectWithTag("Player");
                    if (player != null)
                    {
                        player.transform.position = teleportDestination.position;
                    }
                }
            }
            if (interact.CompareTag("keydoor"))
            {
                // ถ้าต้องการกุญแจ (ID = 7)
                if (!HasItemWithID(7))
                {
                    ShowVendingMessage("You need a key to open this door.");
                    return;
                }

                confiner.m_BoundingShape2D = map;

                if (teleportDestination != null)
                {
                    GameObject player = GameObject.FindGameObjectWithTag("Player");
                    if (player != null)
                    {
                        player.transform.position = teleportDestination.position;
                    }
                }
            }
            if (interact.CompareTag("window"))
            {
                // ถ้าต้องการกุญแจ (ID = 7)
                if (!HasItemWithID(9))
                {
                    ShowVendingMessage("You need a rope to go 2-3 room.");
                    return;
                }

                confiner.m_BoundingShape2D = map;

                if (teleportDestination != null)
                {
                    GameObject player = GameObject.FindGameObjectWithTag("Player");
                    if (player != null)
                    {
                        player.transform.position = teleportDestination.position;
                    }
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

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Player"))
        {
            playerInRange = true;
            interactLight.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if(other.CompareTag("Player"))
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
}
