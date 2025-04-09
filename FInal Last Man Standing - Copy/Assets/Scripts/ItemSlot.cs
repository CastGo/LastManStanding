using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemSlot : MonoBehaviour
{
    public GameObject selectionBorder;
    public GameObject currentItem;

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
}
