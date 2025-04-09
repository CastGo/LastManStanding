using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ItemDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    Transform originalParent;
    CanvasGroup canvasGroup;
    InventoryController inventoryController;

    // Start is called before the first frame update
    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        inventoryController = FindObjectOfType<InventoryController>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent; // Save OG parent
        transform.SetParent(transform.root); // Above other canvas
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.6f; // Semi-transparent during drag
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position; // Follow the mouse
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        ItemSlot dropSlot = null;

        if (eventData.pointerEnter != null)
        {
            dropSlot = eventData.pointerEnter.GetComponent<ItemSlot>();

            if (dropSlot == null)
            {
                GameObject dropObj = eventData.pointerEnter;
                dropSlot = dropObj.GetComponentInParent<ItemSlot>();
            }
        }

        ItemSlot originalSlot = originalParent.GetComponent<ItemSlot>();

        if (dropSlot != null)
        {
            if (dropSlot.currentItem != null)
            {
                // สลับของ
                dropSlot.currentItem.transform.SetParent(originalSlot.transform);
                originalSlot.currentItem = dropSlot.currentItem;
                dropSlot.currentItem.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

                // ✅ อัปเดตจำนวนใน original
                originalSlot.UpdateStackText();
            }
            else
            {
                originalSlot.currentItem = null;
                originalSlot.UpdateStackText(); // ✅ clear จำนวนที่ต้นทาง
            }

            // ย้ายของชิ้นนี้ไปยังช่องใหม่
            transform.SetParent(dropSlot.transform);
            dropSlot.currentItem = gameObject;
            dropSlot.UpdateStackText(); // ✅ อัปเดตจำนวนในช่องใหม่

            // ✅ Sync UI
            if (inventoryController != null)
            {
                if (inventoryController.selectedSlot != null)
                    inventoryController.selectedSlot.Deselect();

                inventoryController.selectedSlot = dropSlot;
                dropSlot.Select();
                inventoryController.ShowItemInfo(dropSlot);
            }
        }

        // จัดให้อยู่ตรงกลางของ parent เสมอ
        GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
    }
}