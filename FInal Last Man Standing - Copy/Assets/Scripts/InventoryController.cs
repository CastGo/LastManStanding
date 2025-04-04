using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryController : MonoBehaviour
{
    public GameObject inventoryPanel;
    public GameObject slotPanel;
    public GameObject slotPrefab;
    public int slotCount;
    public GameObject[] itemPrefabs;
    private bool isInventoryOpen = false;

    // Start is called before the first frame update
    void Start()
    {
        inventoryPanel.SetActive(false);

        for (int i = 0; i < slotCount; i++)
        {
            ItemSlot slot = Instantiate(slotPrefab, slotPanel.transform).GetComponent<ItemSlot>();
            if (i < itemPrefabs.Length)
            {
                GameObject item = Instantiate(itemPrefabs[i], slot.transform);
                item.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                slot.currentItem = item;
            }
        }
    }
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.B))
        {
            isInventoryOpen = !isInventoryOpen;
            inventoryPanel.SetActive(isInventoryOpen);
        }
    }

}