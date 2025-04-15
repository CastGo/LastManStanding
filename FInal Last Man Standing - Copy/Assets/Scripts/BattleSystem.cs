using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum BattleState { START, PLAYERTURN, ENEMYTURN, WON, LOST }
public class BattleSystem : MonoBehaviour
{
    public GameObject playerPrefab;
    public GameObject combatMenu;        
    public GameObject attackFunction;    
    public GameObject itemFunction;      

    public Transform playerBattleStation;
    public Transform enemyBattleStation;

    Unit playerUnit;
    Unit enemyUnit;

    public Text dialogueText;

    public BattleHUD playerHUD;
    public BattleHUD enemyHUD;

    public BattleState state;
    public bool sceneWasDisabled = false;
    private bool hasHealed = false;
    private bool hasUsedItemThisTurn = false;

    int miniBossTurnCounter = 0;
    int bossTurnCounter = 0;
    bool bossHealedAtHalf = false;
    bool bossHealedAtQuarter = false;
    private int enemyStunTurns = 0;
    private int stunCooldown = 0;
    private const int stunCooldownTurns = 4;
    public Button stunButton;

    void Start()
    {
        state = BattleState.START;
        StartCoroutine(SetupBattle());
    }

    IEnumerator SetupBattle()
    {
        GameObject playerGo = Instantiate(playerPrefab, playerBattleStation);
        playerUnit = playerGo.GetComponent<Unit>();

        playerUnit.currentHP = GameManager.instance.savedHP;
        playerUnit.maxHP = GameManager.instance.savedMaxHP;
        playerUnit.currentEnergy = GameManager.instance.savedEnergy;
        playerUnit.maxEnergy = GameManager.instance.savedMaxEnergy;
        playerUnit.damage = GameManager.instance.savedDamage;
        playerUnit.unitLevel = GameManager.instance.savedLevel;
        playerUnit.unitName = GameManager.instance.savedName;

        GameObject enemyGo = Instantiate(GameManager.instance.nextEnemyPrefab, enemyBattleStation);
        enemyUnit = enemyGo.GetComponent<Unit>();

        dialogueText.text = "คุณเดินมาเจอกับ " + enemyUnit.unitName + " จะทำยังไงต่อ";

        playerHUD.SetHUD(playerUnit);
        enemyHUD.SetHUD(enemyUnit);

        yield return new WaitForSeconds(2f);

        state = BattleState.PLAYERTURN;
        PlayerTurn();
    }

    IEnumerator PlayerAttack(int customDamage)
    {
        bool isDead = enemyUnit.TakeDamage(customDamage);
        StartCoroutine(enemyUnit.FlashRed());

        enemyHUD.SetHP(enemyUnit.currentHP);
        dialogueText.text = "โจมตีสำเร็จ";

        yield return new WaitForSeconds(2f);

        if (isDead)
        {
            state = BattleState.WON;
            EndBattle();
        }
        else
        {
            state = BattleState.ENEMYTURN;
            StartCoroutine(EnemyTurn());
        }
    }
    IEnumerator StunAttack(int damage)
    {
        bool isDead = enemyUnit.TakeDamage(damage);
        StartCoroutine(enemyUnit.FlashRed());

        enemyHUD.SetHP(enemyUnit.currentHP);
        dialogueText.text = "คุณทำให้ศัตรูติดสถานะมึนงง";

        playerHUD.SetEnergy(playerUnit.currentEnergy);
        yield return new WaitForSeconds(2f);

        if (isDead)
        {
            state = BattleState.WON;
            EndBattle();
        }
        else
        {
            state = BattleState.ENEMYTURN;
            StartCoroutine(EnemyTurn());
        }
    }

    IEnumerator EnemyTurn()
    {
        if (enemyStunTurns > 0)
        {
            dialogueText.text = enemyUnit.unitName + " ติดสถานะมึนงงอยู่ไม่สามารถขยับได้";
            enemyStunTurns--; // ✅ ลดระยะ stun ลง
            yield return new WaitForSeconds(2f);

            state = BattleState.PLAYERTURN;
            PlayerTurn();
            yield break;
        }

        dialogueText.text = enemyUnit.unitName + " โจมตี";
        yield return new WaitForSeconds(1f);

        bool isDead = false;

        if (enemyUnit.CompareTag("MiniBoss"))
        {
            miniBossTurnCounter++;
            if (miniBossTurnCounter % 2 == 0)
            {
                dialogueText.text = "ซอมบี้ Chibi โจมตีอย่างรุนแรง";
                yield return new WaitForSeconds(1f);
                isDead = playerUnit.TakeDamage(enemyUnit.damage * 2);
                StartCoroutine(playerUnit.FlashRed());
                playerHUD.SetHP(playerUnit.currentHP);
            }
            else
            {
                isDead = playerUnit.TakeDamage(enemyUnit.damage);
                StartCoroutine(playerUnit.FlashRed());
                playerHUD.SetHP(playerUnit.currentHP);
            }
        }
        else if (enemyUnit.CompareTag("Boss"))
        {
            bossTurnCounter++;

            // 1️⃣ Heal logic
            int halfHP = enemyUnit.maxHP / 2;
            int quarterHP = enemyUnit.maxHP / 4;

            if (enemyUnit.currentHP <= quarterHP && !bossHealedAtQuarter)
            {
                dialogueText.text = "ซอมบี้ ผ.อ ฮีลเลือดตัวเองอีกครั้ง";
                yield return new WaitForSeconds(1f);
                enemyUnit.Heal(15); // 🔁 ปรับตามต้องการ
                enemyHUD.SetHP(enemyUnit.currentHP);
                bossHealedAtQuarter = true;
            }
            else if (enemyUnit.currentHP <= halfHP && !bossHealedAtHalf)
            {
                dialogueText.text = "ซอมบี้ ผ.อ ฮีลเลือดตัวเอง";
                yield return new WaitForSeconds(1f);
                enemyUnit.Heal(10);
                enemyHUD.SetHP(enemyUnit.currentHP);
                bossHealedAtHalf = true;
            }
            else
            {
                // 2️⃣ Attack logic
                if (bossTurnCounter % 2 == 0)
                {
                    dialogueText.text = "ซอมบี้ ผ.อ โจมตีอย่างรุงแรง";
                    yield return new WaitForSeconds(1f);
                    isDead = playerUnit.TakeDamage(enemyUnit.damage * 2);
                    StartCoroutine(playerUnit.FlashRed());
                    playerHUD.SetHP(playerUnit.currentHP);
                }
                else
                {
                    isDead = playerUnit.TakeDamage(enemyUnit.damage);
                    StartCoroutine(playerUnit.FlashRed());
                    playerHUD.SetHP(playerUnit.currentHP);
                }

                playerHUD.SetHP(playerUnit.currentHP);
            }
        }
        else
        {
            // Enemy ปกติ
            isDead = playerUnit.TakeDamage(enemyUnit.damage);
            StartCoroutine(playerUnit.FlashRed());
            playerHUD.SetHP(playerUnit.currentHP);
        }

        yield return new WaitForSeconds(1f);

        if (isDead)
        {
            state = BattleState.LOST;
            EndBattle();
        }
        else
        {
            state = BattleState.PLAYERTURN;
            PlayerTurn();
        }
    }

    void EndBattle()
    {
        if (state == BattleState.WON)
        {
            // 💥 ตรวจสอบว่าเป็น MiniBoss แล้วให้ระเบิดใส่ผู้เล่น
            if (enemyUnit.CompareTag("MiniBoss"))
            {
                int explosionDamage = 15; // หรือจะตั้งไว้ในตัวแปรก็ได้
                dialogueText.text = "ซอมบี้ Chibi ใช้การโจมตีเฮือกสุดท้าย";

                bool playerDied = playerUnit.TakeDamage(explosionDamage);
                StartCoroutine(playerUnit.FlashRed());
                playerHUD.SetHP(playerUnit.currentHP);

                if (playerDied)
                {
                    state = BattleState.LOST;
                    dialogueText.text = "คุณเสียชีวิตจากแรง";
                    StartCoroutine(ReturnAfterLost());
                    StartCoroutine(DelayReEnablePlayerCollider());
                    return; // ออกจากฟังก์ชันไม่ต้องไป ReturnAfterWin()
                }

                StartCoroutine(ReturnAfterWin());
                // ให้รอหน่อยให้เห็น effect การระเบิด
                //StartCoroutine(DelayReturnAfterWin());
            }
            else
            {
                dialogueText.text = "คุณชนะแล้ว!!";
                StartCoroutine(ReturnAfterWin());
            }
        }
        else if (state == BattleState.LOST)
        {
            dialogueText.text = "คุณเสียชีวิต";
            StartCoroutine(ReturnAfterLost());
            StartCoroutine(DelayReEnablePlayerCollider());
        }
    }
    IEnumerator ReturnAfterWin()
    {
        yield return new WaitForSeconds(2f);

        if (enemyUnit.CompareTag("Enemy"))
        {
            GameManager.instance.AddGold(100);
        }
        else if (enemyUnit.CompareTag("MiniBoss"))
        {
            GameManager.instance.AddGold(200);
            ItemDictionary itemDict = FindAnyObjectByType<ItemDictionary>();
            InventoryController inventory = FindAnyObjectByType<InventoryController>();
            GameObject itemPrefab = itemDict.GetItemPrefab(6); // ID 6 = KNO3

            if (itemPrefab != null && inventory != null)
            {
                bool added = inventory.AddItem(itemPrefab);
                if (added)
                {
                    Debug.Log("MiniBoss dropped KNO3 (ID 6)");
                }
                else
                {
                    Debug.Log("Inventory full. Couldn't pick up KNO3");
                }
            }
        }
        else if (enemyUnit.CompareTag("Boss"))
        {
            GameManager.instance.AddGold(500);
        }

        GameManager.instance.UpdateGoldUI();

        GameManager.instance.savedHP = playerUnit.currentHP;
        GameManager.instance.savedEnergy = playerUnit.currentEnergy;

        GameManager.instance.isLoadingBattleScene = false;
        // ปิดฉาก TurnBase แต่ไม่เปลี่ยนตำแหน่ง
        SceneManager.UnloadSceneAsync("TurnBase");

        // ตั้งฉาก "2-1 Room" เป็น active
        GameManager.instance.SetSceneActive(SceneManager.GetSceneByName("2-1 Room"), true);
        SceneManager.SetActiveScene(SceneManager.GetSceneByName("2-1 Room"));

        GameManager.instance.RestoreSceneObjects();

        GameManager.instance.UpdateGoldUI();

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.GetComponent<PlayerController>()?.RefreshStatsFromGameManager();
        }
        // ทำลาย Zombie ที่ถูกสู้ด้วยเท่านั้น (ป้องกันการลบ zombie ทั้งหมด)
        if (GameManager.instance.currentZombie != null)
        {
            string name = GameManager.instance.currentZombie.name;
            GameManager.instance.deactivatedZombies.Add(name); // ✅ จำชื่อ zombie ที่ถูกปิดไว้
            GameManager.instance.currentZombie.SetActive(false);
            GameManager.instance.currentZombie = null;
        }
        GameObject[] allZombies = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (GameObject zombie in allZombies)
        {
            if (zombie.CompareTag("Enemy") && zombie.scene.IsValid() && zombie.scene.name == "2-1 Room")
            {
                if (GameManager.instance.deactivatedZombies.Contains(zombie.name))
                {
                    zombie.SetActive(false);
                }
            }
        }
    }
    IEnumerator ReturnAfterLost()
    {
        yield return new WaitForSeconds(2f);

        GameManager.instance.savedHP = playerUnit.currentHP;
        // ปิดฉาก TurnBase แต่ไม่เปลี่ยนตำแหน่ง
        GameManager.instance.isLoadingBattleScene = false;

        SceneManager.UnloadSceneAsync("TurnBase");

        // ตั้งฉาก "2-1 Room" เป็น active
        GameManager.instance.SetSceneActive(SceneManager.GetSceneByName("2-1 Room"), true);
        SceneManager.SetActiveScene(SceneManager.GetSceneByName("2-1 Room"));

        SaveController.instance.LoadGame();
    }
    IEnumerator DelayReEnablePlayerCollider()
    {
        yield return new WaitForSeconds(1.5f); // ✅ รอสักครู่ให้ player ห่างจาก zombie

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Collider2D col = player.GetComponent<Collider2D>();
            if (col != null)
            {
                col.enabled = false;
                yield return new WaitForSeconds(1f); // ปิด collider ชั่วครู่
                col.enabled = true;
            }
        }
    }

    void PlayerTurn()
    {
        hasUsedItemThisTurn = false;
        if (stunCooldown > 0)
            stunCooldown--; // ลดคูลดาวน์ stun ทุกเทิร์น
        if (stunButton != null)
            stunButton.interactable = stunCooldown <= 0;
        dialogueText.text = "เลือกสิ่งที่คุณจะทำ";
    }

    IEnumerator PlayerHeal()
    {
        playerUnit.Heal(50);

        playerHUD.SetHP(playerUnit.currentHP);
        dialogueText.text = "คุณได้รับการฟื้นฟู";

        yield return new WaitForSeconds(2f);

        state = BattleState.ENEMYTURN;
        StartCoroutine(EnemyTurn());
    }
    IEnumerator PlayerRun()
    {
        dialogueText.text = "คุณรีบหนีออกมา!!";

        yield return new WaitForSeconds(2f);

        GameManager.instance.savedHP = playerUnit.currentHP;
        GameManager.instance.savedEnergy = playerUnit.currentEnergy;

        GameManager.instance.ReturnToPreviousScene();
    }
    IEnumerator DelayShowActionMessage()
    {
        yield return new WaitForSeconds(2f);
        dialogueText.text = "เลือกสิ่งที่คุณจะทำ";
    }
    IEnumerator ResetDialogueText()
    {
        yield return new WaitForSeconds(2f);
        if (state == BattleState.PLAYERTURN)
        {
            dialogueText.text = "เลือกสิ่งที่คุณจะทำ";
        }
    }
    private bool TryUseItemByID(int itemID)
    {
        InventoryController inventory = FindAnyObjectByType<InventoryController>();

        foreach (Transform slotTransform in inventory.slotPanel.transform)
        {
            ItemSlot slot = slotTransform.GetComponent<ItemSlot>();
            if (slot.currentItem != null)
            {
                Item item = slot.currentItem.GetComponent<Item>();
                if (item.ID == itemID)
                {
                    item.quantity--;
                    slot.UpdateStackText();

                    if (item.quantity <= 0)
                    {
                        Destroy(slot.currentItem);
                        slot.currentItem = null;
                    }

                    // ✅ sync GameManager
                    GameManager.instance.savedHP = playerUnit.currentHP;
                    GameManager.instance.savedEnergy = playerUnit.currentEnergy;

                    return true;
                }
            }
        }

        return false;
    }
    public void OnAttackButton()
    {
        if (state != BattleState.PLAYERTURN)
            return;

        ReturnToCombat();
        StartCoroutine(PlayerAttack(playerUnit.damage));
    }
    public void OnPowerAttackButton()
    {
        if (state != BattleState.PLAYERTURN)
            return;

        int energyCost = 10;
        if (playerUnit.UseEnergy(energyCost))
        {
            int powerfulDamage = playerUnit.damage * 3;
            StartCoroutine(PlayerAttack(powerfulDamage));
            playerHUD.SetEnergy(playerUnit.currentEnergy);
            dialogueText.text = "คุณใช้การโจมตีอย่างรุนแรง";
            ReturnToCombat();
        }
        else
        {
            dialogueText.text = "คุณมีเอนเนอร์จี้ไม่พอ";
            StartCoroutine(ResetDialogueText());
        }
    }
    public void OnStunButton()
    {
        if (state != BattleState.PLAYERTURN)
            return;

        if (stunCooldown > 0)
        {
            dialogueText.text = "สกิลสตั้นติดคูลดาวน์ (" + stunCooldown + " เทิร์น)";
            StartCoroutine(ResetDialogueText());
            return;
        }

        int energyCost = 5;
        int stunDamage = playerUnit.damage / 2;

        if (!playerUnit.UseEnergy(energyCost))
        {
            dialogueText.text = "คุณมีเอนเนอร์จี้ไม่พอ";
            StartCoroutine(ResetDialogueText());
            return;
        }

        enemyStunTurns = 2;
        ReturnToCombat();
        stunCooldown = stunCooldownTurns;

        StartCoroutine(StunAttack(stunDamage));
    }
    public void OnHealButton()
    {
        if (state != BattleState.PLAYERTURN)
            return;

        if (hasHealed)
        {
            dialogueText.text = "คุณไม่สามารถฮีลได้อีกรอบ";
            return;
        }

        if (!HasItemID(3)) // ✅ เช็คว่าไม่มี First Aid Kit ID 3
        {
            dialogueText.text = "คุณไม่มี First Aid Kit!";
            StartCoroutine(ResetDialogueText());
            return;
        }

        hasHealed = true;
        StartCoroutine(PlayerHeal());
    }
    public void OnRunButton()
    {
        if (state != BattleState.PLAYERTURN)
            return;

        StartCoroutine(PlayerRun());
    }
    public void OnUseSnackButton()
    {
        if (state != BattleState.PLAYERTURN)
            return;

        if (hasUsedItemThisTurn)
        {
            dialogueText.text = "คุณใช้ไอเทมไปแล้วในเทิร์นนี้";
            StartCoroutine(ResetDialogueText());
            ReturnToCombat();
            return;
        }

        if (TryUseItemByID(1)) // ID 1 = Snack
        {
            playerUnit.GainEnergy(5);
            playerHUD.SetEnergy(playerUnit.currentEnergy);
            dialogueText.text = "คุณกิน Sushi เอนเนอร์จี้เพิ่ม 10 หน่วย";
            hasUsedItemThisTurn = true;

            ReturnToCombat();
        }
        else
        {
            dialogueText.text = "คุณไม่มี Sushi เหลือแล้ว";
        }
        StartCoroutine(DelayShowActionMessage());
    }

    public void OnUseFoodButton()
    {
        if (state != BattleState.PLAYERTURN)
            return;

        if (hasUsedItemThisTurn)
        {
            dialogueText.text = "คุณใช้ไอเทมไปแล้วในเทิร์นนี้";
            StartCoroutine(ResetDialogueText());
            ReturnToCombat();
            return;
        }
        if (TryUseItemByID(2)) // ID 2 = Food
        {
            playerUnit.Heal(10);
            playerUnit.GainEnergy(10);

            playerHUD.SetHP(playerUnit.currentHP);
            playerHUD.SetEnergy(playerUnit.currentEnergy);

            dialogueText.text = "คุณกิน Snack เลือดเพิ่ม 10 หน่วย เอนเนอร์จี้เพิ่ม 10 หน่วย";
            hasUsedItemThisTurn = true;

            ReturnToCombat();
        }
        else
        {
            dialogueText.text = "คุณไม่มี Snack แล้ว";
        }
        StartCoroutine(DelayShowActionMessage());
    }

    public void OnUseFirstAidButton()
    {
        if (state != BattleState.PLAYERTURN)
            return;

        if (hasUsedItemThisTurn)
        {
            dialogueText.text = "You already used an item this turn!";
            StartCoroutine(ResetDialogueText());
            ReturnToCombat();
            return;
        }

        if (TryUseItemByID(3)) // ID 3 = FirstAid
        {
            playerUnit.Heal(40);
            playerHUD.SetHP(playerUnit.currentHP);
            dialogueText.text = "You used FirstAid!";
            hasUsedItemThisTurn = true;

            ReturnToCombat();
        }
        else
        {
            dialogueText.text = "You don't have any FirstAid!";
        }
        StartCoroutine(DelayShowActionMessage());
    }
    public void OpenAttackMenu()
    {
        combatMenu.SetActive(false);
        attackFunction.SetActive(true);
    }

    public void OpenItemMenu()
    {
        combatMenu.SetActive(false);
        itemFunction.SetActive(true);
    }

    public void ReturnToCombat()
    {
        attackFunction.SetActive(false);
        itemFunction.SetActive(false);
        combatMenu.SetActive(true);
    }
    private bool HasItemID(int id)
    {
        InventoryController inventory = FindAnyObjectByType<InventoryController>();

        foreach (Transform slotTransform in inventory.slotPanel.transform)
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
    IEnumerator DisableObjectAfterSeconds(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        obj.SetActive(false);
    }
}
