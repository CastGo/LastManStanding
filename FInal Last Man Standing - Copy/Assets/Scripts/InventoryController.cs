using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;

public class InventoryController : MonoBehaviour
{
    private ItemDictionary itemDictionary;
    [HideInInspector] public ItemSlot selectedSlot;
    [HideInInspector] public bool isInitialized = false;

    public GameObject menuPanel;
    public GameObject mapPanel;
    public GameObject inventoryPanel;
    public GameObject slotPanel;
    public GameObject slotPrefab;
    public TMP_Text itemNameText;
    public TMP_Text itemDescriptionText;
    public int slotCount;
    public GameObject[] itemPrefabs;
    private bool isInventoryOpen = false;
    private bool isMapOpen = false;
    private bool isMainMenuOpen = false;


    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "2-1 Room")
        {
            inventoryPanel = GameObject.Find("Inventory");
            mapPanel = GameObject.Find("Map");
            menuPanel = GameObject.Find("MainMenu");
            slotPanel = GameObject.Find("InventoryItem");

            var nameObj = GameObject.Find("ItemName");
            var descObj = GameObject.Find("ItemDesciption");
            itemNameText = nameObj != null ? nameObj.GetComponent<TMP_Text>() : null;
            itemDescriptionText = descObj != null ? descObj.GetComponent<TMP_Text>() : null;

            if (inventoryPanel == null || mapPanel == null || menuPanel == null || itemNameText == null || itemDescriptionText == null || slotPanel == null)
            {
                Debug.LogWarning("❗ มี UI บางตัวที่หาไม่เจอใน scene 2-1 Room!");
            }

            inventoryPanel?.SetActive(false);
            mapPanel?.SetActive(false);
            menuPanel?.SetActive(false);

            Time.timeScale = 1f;
            CreateEmptySlots();
            isInitialized = true;
        }

        itemDictionary = FindAnyObjectByType<ItemDictionary>();
    }

    void Update()
    {
        if (inventoryPanel == null || mapPanel == null || menuPanel == null)
            return;

        if (Input.GetKeyUp(KeyCode.Escape))
        {
            isMainMenuOpen = !isMainMenuOpen;
            menuPanel.SetActive(isMainMenuOpen);
            Time.timeScale = isMainMenuOpen ? 0f : 1f;

            if (isMainMenuOpen)
            {
                isInventoryOpen = false;
                isMapOpen = false;
                inventoryPanel.SetActive(false);
                mapPanel.SetActive(false);
            }
        }

        if (!isMainMenuOpen)
        {
            if (Input.GetKeyUp(KeyCode.B))
            {
                isInventoryOpen = !isInventoryOpen;
                inventoryPanel.SetActive(isInventoryOpen);

                if (isInventoryOpen)
                {
                    isMapOpen = false;
                    mapPanel.SetActive(false);
                }
            }

            if (Input.GetKeyUp(KeyCode.M))
            {
                isMapOpen = !isMapOpen;
                mapPanel.SetActive(isMapOpen);

                if (isMapOpen)
                {
                    isInventoryOpen = false;
                    inventoryPanel.SetActive(false);
                }
            }

            if (Input.GetMouseButtonDown(0) && isInventoryOpen) DetectSlotClick(false);
            else if (Input.GetMouseButtonDown(1) && isInventoryOpen) DetectSlotClick(true);
        }
    }

    private void DetectSlotClick(bool isRightClick)
    {
        if (slotPanel == null) return;
        Vector2 mousePos = Input.mousePosition;
        bool slotClicked = false;

        foreach (Transform slotTransform in slotPanel.transform)
        {
            RectTransform rect = slotTransform.GetComponent<RectTransform>();
            if (RectTransformUtility.RectangleContainsScreenPoint(rect, mousePos))
            {
                ItemSlot slot = slotTransform.GetComponent<ItemSlot>();

                if (selectedSlot != null)
                    selectedSlot.Deselect();

                selectedSlot = slot;
                selectedSlot.Select();

                if (slot.currentItem != null)
                {
                    if (isRightClick)
                        UseItem(slot);
                    else
                        ShowItemInfo(slot);
                }
                else
                {
                    ClearItemInfo();
                }

                slotClicked = true;
                break;
            }
        }

        if (!slotClicked)
        {
            ClearItemInfo();
            if (selectedSlot != null)
            {
                selectedSlot.Deselect();
                selectedSlot = null;
            }
        }
    }

    public bool AddItem(GameObject itemPrefab)
    {
        if (slotPanel == null) return false;
        Item newItemData = itemPrefab.GetComponent<Item>();

        if (newItemData.isStackable)
        {
            foreach (Transform slotTransform in slotPanel.transform)
            {
                ItemSlot slot = slotTransform.GetComponent<ItemSlot>();
                if (slot.currentItem != null)
                {
                    Item existingItem = slot.currentItem.GetComponent<Item>();
                    if (existingItem.ID == newItemData.ID && existingItem.quantity < existingItem.maxStack)
                    {
                        existingItem.quantity++;
                        slot.UpdateStackText();
                        return true;
                    }
                }
            }
        }

        foreach (Transform slotTransform in slotPanel.transform)
        {
            ItemSlot slot = slotTransform.GetComponent<ItemSlot>();
            if (slot.currentItem == null)
            {
                GameObject newItem = Instantiate(itemPrefab, slotTransform);
                newItem.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

                Item itemComponent = newItem.GetComponent<Item>();
                itemComponent.quantity = 1;

                slot.currentItem = newItem;
                slot.UpdateStackText();
                return true;
            }
        }

        Debug.Log("Inventory is full or cannot stack this item.");
        return false;
    }

    public List<InventorySaveData> GetInventoryItems()
    {
        List<InventorySaveData> invData = new List<InventorySaveData>();
        if (slotPanel == null) return invData;

        foreach (Transform itemslotTranfrom in slotPanel.transform)
        {
            ItemSlot itemSlot = itemslotTranfrom.GetComponent<ItemSlot>();
            if (itemSlot.currentItem != null)
            {
                Item item = itemSlot.currentItem.GetComponent<Item>();
                if (item.quantity > 0)
                {
                    invData.Add(new InventorySaveData
                    {
                        itemID = item.ID,
                        slotIndex = itemslotTranfrom.GetSiblingIndex(),
                        quantity = item.quantity
                    });
                }
            }
        }
        return invData;
    }

    public void SetInventoryItems(List<InventorySaveData> inventorySaveData)
    {
        if (slotPanel == null)
        {
            Debug.LogError("❌ slotPanel is null in SetInventoryItems!");
            return;
        }

        foreach (Transform slotTransform in slotPanel.transform)
        {
            ItemSlot slot = slotTransform.GetComponent<ItemSlot>();
            if (slot.currentItem != null)
            {
                Destroy(slot.currentItem);
                slot.currentItem = null;
            }
        }

        foreach (InventorySaveData data in inventorySaveData)
        {
            if (data.slotIndex < slotCount)
            {
                ItemSlot slot = slotPanel.transform.GetChild(data.slotIndex).GetComponent<ItemSlot>();
                GameObject itemPrefab = itemDictionary.GetItemPrefab(data.itemID);
                if (itemPrefab != null)
                {
                    GameObject item = Instantiate(itemPrefab, slot.transform);
                    item.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

                    Item itemComponent = item.GetComponent<Item>();
                    itemComponent.quantity = data.quantity;

                    slot.currentItem = item;
                    slot.UpdateStackText();
                }
            }
        }
    }

    private void CreateEmptySlots()
    {
        if (slotPanel == null) return;

        foreach (Transform child in slotPanel.transform)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < slotCount; i++)
        {
            Instantiate(slotPrefab, slotPanel.transform);
        }
    }

    private void UseItem(ItemSlot slot)
    {
        if (slot.currentItem == null) return;

        Item item = slot.currentItem.GetComponent<Item>();
        if (item != null && item.isUsable)
        {
            item.Use();

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                Unit unit = player.GetComponent<Unit>();
                GameManager.instance.savedHP = unit.currentHP;
                GameManager.instance.savedEnergy = unit.currentEnergy;
            }

            if (item.ID != 3)
            {
                item.quantity--;

                if (item.quantity <= 0)
                {
                    Destroy(slot.currentItem);
                    slot.currentItem = null;
                }

                slot.UpdateStackText();
            }
            ShowItemInfo(slot);
        }
    }

    public void ShowItemInfo(ItemSlot slot)
    {
        if (slot.currentItem != null)
        {
            Item item = slot.currentItem.GetComponent<Item>();
            if (item != null)
            {
                if (itemNameText != null) itemNameText.text = item.itemName;
                if (itemDescriptionText != null) itemDescriptionText.text = item.itemDescription;
            }
        }
    }

    private void ClearItemInfo()
    {
        if (itemNameText != null) itemNameText.text = "";
        if (itemDescriptionText != null) itemDescriptionText.text = "";
    }

    public void ResumeGame()
    {
        isMainMenuOpen = false;
        menuPanel.SetActive(false);
        Time.timeScale = 1f;
    }

    public void OnResumeButton()
    {
        menuPanel.SetActive(false);
        Time.timeScale = 1f;
        isMainMenuOpen = false;
    }

    public void OnLoadButton()
    {
        SaveController.instance.LoadGame();
        menuPanel.SetActive(false);
        Time.timeScale = 1f;
        isMainMenuOpen = false;
    }

    public void OnLeaveButton()
    {
        menuPanel?.SetActive(false);
        mapPanel?.SetActive(false);
        inventoryPanel?.SetActive(false);

        // ล้าง DontDestroyOnLoad ก่อนกลับ
        Destroy(GameManager.instance?.gameObject);
        Destroy(SaveController.instance?.gameObject);

        Time.timeScale = 1f;
        SceneFader.instance.FadeToScene("IntroScene");
    }
}
