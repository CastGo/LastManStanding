using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using TMPro;
using UnityEngine.SceneManagement;

public class InteractObject : MonoBehaviour
{
    [SerializeField] PolygonCollider2D map;
    CinemachineConfiner confiner;
    public Transform teleportDestination;

    public GameObject interact;
    public GameObject interact2;
    public GameObject interact3;
    public GameObject interactLight;
    private bool playerInRange;
    private InventoryController inventoryController;
    public bool isVendingMachine = false;
    public int itemPrice = 10;
    public GameObject itemPrefab;
    public TMP_Text messageText;
    public float messageDuration = 2f;
    private ItemDictionary itemDictionary;
    public DialogueData dialogueData;
    private bool isDialoguePlaying = false;
    private bool hasExploded = false;

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
                bool hasItem4 = HasItemWithID(4);
                bool hasItem5 = HasItemWithID(5);
                bool hasCutter = HasItemWithID(10);

                if (hasItem4 && hasItem5)
                {
                    // ปิด NPCStudent
                    GameObject[] students = GameObject.FindGameObjectsWithTag("NPCStudent");
                    foreach (GameObject npc in students)
                        npc.SetActive(false);

                    // ✅ เปิด MiniBoss
                    GameObject[] miniBosses = Resources.FindObjectsOfTypeAll<GameObject>();
                    foreach (GameObject boss in miniBosses)
                    {
                        if (boss.CompareTag("MiniBoss") && boss.scene.name == "2-1 Room")
                        {
                            boss.SetActive(true);
                        }
                    }
                }

                if (hasCutter)
                {
                    // ✅ เปิด Boss
                    GameObject[] bossObjects = Resources.FindObjectsOfTypeAll<GameObject>();
                    foreach (GameObject boss in bossObjects)
                    {
                        if (boss.CompareTag("Boss") && boss.scene.name == "2-1 Room")
                        {
                            boss.SetActive(true);
                        }
                    }
                }

                // เปิด resetzombie เสมอ
                GameObject[] allObjectsForZombie = Resources.FindObjectsOfTypeAll<GameObject>();
                foreach (GameObject zombie in allObjectsForZombie)
                {
                    if (zombie.CompareTag("resetzombie") && zombie.scene.name == "2-1 Room")
                    {
                        zombie.SetActive(true);
                    }
                }

                confiner.m_BoundingShape2D = map;
                TeleportPlayer();
            }

            if (interact.CompareTag("BombDoor"))
            {
                if (hasExploded) return; // ✅ กันไม่ให้เล่นซ้ำ

                if (!HasItemWithID(8))
                {
                    ShowVendingMessage("ประตูนี้ล็อคอยู่ แต่ฉันไม่มี KeyCard จะทำยังไงดีนะ");
                    return;
                }

                hasExploded = true; // ✅ ตั้ง flag ว่าระเบิดแล้ว

                RemoveItemByID(8);

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
                    ShowVendingMessage("ฉันต้องการกุญแจเพื่อเปิดห้องนี้");
                    return;
                }

                confiner.m_BoundingShape2D = map;
                TeleportPlayer();
            }

            if (interact.CompareTag("mainkeydoor"))
            {
                interact2.SetActive(true);
                if (!HasItemWithID(7))
                {
                    if (!isDialoguePlaying && dialogueData != null)
                    {
                        StartCoroutine(PlayDialogue());
                        return;
                    }
                }

                if (GameManager.instance != null)
                {
                    GameManager.instance.StartCoroutine(ShowMessageAndReturnToIntro("ประตูถูกเปิดแล้ว"));
                }
                return;
            }
            if (interact.CompareTag("maincutterdoor"))
            {
                interact2.SetActive(true);
                if (!HasItemWithID(10))
                {
                    if (!isDialoguePlaying && dialogueData != null)
                    {
                        StartCoroutine(PlayDialogue());
                        return;
                    }
                }

                if (GameManager.instance != null)
                {
                    GameManager.instance.StartCoroutine(ShowMessageAndReturnToIntro2("ประตูถูกเปิดแล้ว"));
                }
                return;
            }
            if (interact.CompareTag("window"))
            {
                if (!HasItemWithID(9))
                {
                    ShowVendingMessage("ถ้าฉํนมีเชือกฉันน่าจะใช้มันลงไปห้อง 2-3 ได้ ในห้องน้ำซักห้องน่าจะมีเชือกนะ");
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

                if (!hasBattery)
                {
                    interact2.SetActive(true); // ✅ ไม่มีแบตเตอรี่
                }

                if (!hasSugar)
                {
                    interact3.SetActive(true); // ✅ ไม่มีน้ำตาล
                }

                if (hasBattery && hasSugar && hasKNO3)
                {
                    RemoveItemByID(4);
                    RemoveItemByID(5);
                    RemoveItemByID(6);

                    GameObject bombPrefab = itemDictionary.GetItemPrefab(8);
                    if (bombPrefab != null)
                    {
                        bool added = inventoryController.AddItem(bombPrefab);
                        ShowVendingMessage(added ? "You received a bomb!" : "Inventory full, couldn't add bomb.");
                    }

                    GameObject[] miniBosses = GameObject.FindGameObjectsWithTag("MiniBoss");
                    foreach (GameObject boss in miniBosses)
                        boss.SetActive(true);
                }
                else
                {
                    if (!isDialoguePlaying && dialogueData != null)
                    {
                        StartCoroutine(PlayDialogue());
                        return;
                    }
                }
            }
            if (interact.CompareTag("workstation"))
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
                        ShowVendingMessage(added ? "คุณได้รับระเบิด DIY" : "กระเป๋าของคุณเต็ม!!");
                    }

                    GameObject[] miniBosses = GameObject.FindGameObjectsWithTag("MiniBoss");
                    foreach (GameObject boss in miniBosses)
                        boss.SetActive(true);
                }
                else
                {
                    ShowVendingMessage("ต้องการแบตเตอรี่ น้ำตาล และKNO3เพื่อสร้างระเบิด");
                }
            }
            if (interact.CompareTag("janitor"))
            {
                int sushiCount = GetItemQuantity(1);
                if (sushiCount >= 3)
                {
                    RemoveItemByID(1, 3);

                    GameObject keyPrefab = itemDictionary.GetItemPrefab(7);
                    if (keyPrefab != null)
                    {
                        bool added = inventoryController.AddItem(keyPrefab);
                        ShowVendingMessage(added ? "ขอบใจมาก เอากุญแจนี่ไปสิ" : "กระเป๋าของคุณเต็ม!!");
                    }
                }
                if (!isDialoguePlaying && dialogueData != null)
                {
                    StartCoroutine(PlayDialogue());
                    return;
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
                            ShowVendingMessage("ซื้อไอเทมสำเร็จ");
                        }
                        else
                        {
                            ShowVendingMessage("กระเป๋าของคุณเต็ม");
                        }
                    }
                }
                else
                {
                    ShowVendingMessage("คุณมีเงินไม่พอ");
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
            StartCoroutine(TypeAndHideMessage(message));
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

    private int GetItemQuantity(int id)
    {
        int total = 0;
        foreach (Transform slotTransform in inventoryController.slotPanel.transform)
        {
            ItemSlot slot = slotTransform.GetComponent<ItemSlot>();
            if (slot.currentItem != null)
            {
                Item item = slot.currentItem.GetComponent<Item>();
                if (item != null && item.ID == id)
                {
                    total += item.quantity;
                }
            }
        }
        return total;
    }

    private void RemoveItemByID(int itemID, int count = 1)
    {
        foreach (Transform slotTransform in inventoryController.slotPanel.transform)
        {
            ItemSlot slot = slotTransform.GetComponent<ItemSlot>();
            if (slot.currentItem != null)
            {
                Item item = slot.currentItem.GetComponent<Item>();
                if (item != null && item.ID == itemID)
                {
                    int removeAmount = Mathf.Min(count, item.quantity);
                    item.quantity -= removeAmount;
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
    IEnumerator ShowMessageAndReturnToIntro(string message)
    {
        Debug.Log("▶ ShowMessageAndReturnToIntro started");

        if (messageText != null)
        {
            messageText.text = message;
            messageText.gameObject.SetActive(true);
        }

        yield return new WaitForSeconds(messageDuration + 0.5f);

        Destroy(GameManager.instance?.gameObject);
        Destroy(SaveController.instance?.gameObject);

        Debug.Log("▶ Load IntroScene now");
        SceneFader.instance.FadeToScene("KeyEnd");
    }
    IEnumerator ShowMessageAndReturnToIntro2(string message)
    {
        Debug.Log("▶ ShowMessageAndReturnToIntro started");

        if (messageText != null)
        {
            messageText.text = message;
            messageText.gameObject.SetActive(true);
        }

        yield return new WaitForSeconds(messageDuration + 0.5f);

        Destroy(GameManager.instance?.gameObject);
        Destroy(SaveController.instance?.gameObject);

        Debug.Log("▶ Load IntroScene now");
        SceneFader.instance.FadeToScene("CutterEnd");
    }
    IEnumerator PlayDialogue()
    {
        isDialoguePlaying = true;
        messageText.gameObject.SetActive(true);

        foreach (var line in dialogueData.lines)
        {
            string fullText = $"{line.speaker}: {line.text}";
            messageText.text = ""; // เคลียร์ข้อความเก่า

            foreach (char c in fullText)
            {
                messageText.text += c;
                yield return new WaitForSeconds(0.03f); // หน่วงต่ออักษร (ปรับได้)
            }

            yield return new WaitForSeconds(messageDuration); // หน่วงระหว่างบรรทัด
        }

        messageText.gameObject.SetActive(false);
        isDialoguePlaying = false;
    }
    IEnumerator TypeAndHideMessage(string message)
    {
        messageText.gameObject.SetActive(true);
        messageText.text = "";

        foreach (char c in message)
        {
            messageText.text += c;
            yield return new WaitForSeconds(0.03f); // ปรับความเร็วได้
        }

        yield return new WaitForSeconds(messageDuration);
        messageText.gameObject.SetActive(false);
    }
}

