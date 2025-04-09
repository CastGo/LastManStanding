using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemSlot : MonoBehaviour
{
    public GameObject selectionBorder;
    public GameObject currentItem;
    public TMP_Text stackText;

    public void Select()
    {   
        if (selectionBorder != null)
            selectionBorder.SetActive(true);
    }

    public void Deselect()
    {
        if (selectionBorder != null)
            selectionBorder.SetActive(false);
    }
    public void UpdateStackText()
    {
        if (stackText == null)
        {
            Debug.LogWarning("❗ StackText is not assigned on: " + gameObject.name);
            return;
        }

        if (currentItem == null)
        {
            stackText.text = "";
            stackText.gameObject.SetActive(false);
            return;
        }

        Item item = currentItem.GetComponent<Item>();
        if (item != null)
        {
            stackText.text = item.quantity.ToString();
            stackText.gameObject.SetActive(true);
        }
    }
}
