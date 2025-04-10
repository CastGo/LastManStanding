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

        dialogueText.text = "A wild" + enemyUnit.unitName + " approaches...";

        playerHUD.SetHUD(playerUnit);
        enemyHUD.SetHUD(enemyUnit);

        yield return new WaitForSeconds(2f);

        state = BattleState.PLAYERTURN;
        PlayerTurn();
    }

    IEnumerator PlayerAttack(int customDamage)
    {
        bool isDead = enemyUnit.TakeDamage(customDamage);

        enemyHUD.SetHP(enemyUnit.currentHP);
        dialogueText.text = "The attack is successful!";

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
        dialogueText.text = enemyUnit.unitName + " attacks!";
        yield return new WaitForSeconds(1f);

        bool isDead = false;

        if (enemyUnit.CompareTag("MiniBoss"))
        {
            miniBossTurnCounter++;
            if (miniBossTurnCounter % 2 == 0)
            {
                dialogueText.text = "MiniBoss uses a POWERFUL ATTACK!";
                yield return new WaitForSeconds(1f);
                isDead = playerUnit.TakeDamage(enemyUnit.damage * 2);
            }
            else
            {
                isDead = playerUnit.TakeDamage(enemyUnit.damage);
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
                dialogueText.text = "Boss regenerates with DARK MAGIC!";
                yield return new WaitForSeconds(1f);
                enemyUnit.Heal(40); // 🔁 ปรับตามต้องการ
                enemyHUD.SetHP(enemyUnit.currentHP);
                bossHealedAtQuarter = true;
            }
            else if (enemyUnit.currentHP <= halfHP && !bossHealedAtHalf)
            {
                dialogueText.text = "Boss uses a healing spell!";
                yield return new WaitForSeconds(1f);
                enemyUnit.Heal(30);
                enemyHUD.SetHP(enemyUnit.currentHP);
                bossHealedAtHalf = true;
            }
            else
            {
                // 2️⃣ Attack logic
                if (bossTurnCounter % 2 == 0)
                {
                    dialogueText.text = "Boss unleashes a POWER STRIKE!";
                    yield return new WaitForSeconds(1f);
                    isDead = playerUnit.TakeDamage(enemyUnit.damage * 2);
                }
                else
                {
                    isDead = playerUnit.TakeDamage(enemyUnit.damage);
                }

                playerHUD.SetHP(playerUnit.currentHP);
            }
        }
        else
        {
            // Enemy ปกติ
            isDead = playerUnit.TakeDamage(enemyUnit.damage);
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
                dialogueText.text = "MiniBoss explodes!";
                bool playerDied = playerUnit.TakeDamage(explosionDamage);
                playerHUD.SetHP(playerUnit.currentHP);

                if (playerDied)
                {
                    state = BattleState.LOST;
                    dialogueText.text = "You both died!";
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
                dialogueText.text = "You Won!!";
                StartCoroutine(ReturnAfterWin());
            }
        }
        else if (state == BattleState.LOST)
        {
            dialogueText.text = "You Lost!!";
            StartCoroutine(ReturnAfterLost());
            StartCoroutine(DelayReEnablePlayerCollider());
        }
    }
    IEnumerator ReturnAfterWin()
    {
        yield return new WaitForSeconds(2f);

        if (enemyUnit.CompareTag("Enemy"))
        {
            GameManager.instance.AddGold(10);
        }
        else if (enemyUnit.CompareTag("MiniBoss"))
        {
            GameManager.instance.AddGold(20);
        }
        else if (enemyUnit.CompareTag("Boss"))
        {
            GameManager.instance.AddGold(50);
        }

        GameManager.instance.UpdateGoldUI();

        GameManager.instance.savedHP = playerUnit.currentHP;
        GameManager.instance.savedEnergy = playerUnit.currentEnergy;
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
        dialogueText.text = "Choose an action";
    }

    IEnumerator PlayerHeal()
    {
        playerUnit.Heal(25);

        playerHUD.SetHP(playerUnit.currentHP);
        dialogueText.text = "you feel strong!!";

        yield return new WaitForSeconds(2f);

        state = BattleState.ENEMYTURN;
        StartCoroutine(EnemyTurn());
    }
    IEnumerator PlayerRun()
    {
        dialogueText.text = "run away you gonna die!!";

        yield return new WaitForSeconds(2f);

        GameManager.instance.savedHP = playerUnit.currentHP;
        GameManager.instance.savedEnergy = playerUnit.currentEnergy;

        GameManager.instance.ReturnToPreviousScene();
    }
    IEnumerator DelayShowActionMessage()
    {
        yield return new WaitForSeconds(2f);
        dialogueText.text = "Choose an action";
    }
    IEnumerator ResetDialogueText()
    {
        yield return new WaitForSeconds(2f);
        if (state == BattleState.PLAYERTURN)
        {
            dialogueText.text = "Choose an action";
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
            dialogueText.text = "You unleashed a POWER ATTACK!";
        }
        else
        {
            dialogueText.text = "Not enough energy!";
            StartCoroutine(ResetDialogueText());
        }
    }
    public void OnHealButton()
    {
        if (state != BattleState.PLAYERTURN)
            return;

        if (hasHealed)
        {
            dialogueText.text = "You can't heal again!";
            return;
        }

        hasHealed = true; // ✅ Mark ว่าใช้ Heal ไปแล้ว
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
            dialogueText.text = "You already used an item this turn!";
            StartCoroutine(ResetDialogueText());
            ReturnToCombat();
            return;
        }

        if (TryUseItemByID(1)) // ID 1 = Snack
        {
            playerUnit.GainEnergy(5);
            playerHUD.SetEnergy(playerUnit.currentEnergy);
            dialogueText.text = "You used a Snack!";
            hasUsedItemThisTurn = true;

            ReturnToCombat();
        }
        else
        {
            dialogueText.text = "You don't have any Snack!";
        }
        StartCoroutine(DelayShowActionMessage());
    }

    public void OnUseFoodButton()
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
        if (TryUseItemByID(2)) // ID 2 = Food
        {
            playerUnit.Heal(10);
            playerUnit.GainEnergy(10);

            playerHUD.SetHP(playerUnit.currentHP);
            playerHUD.SetEnergy(playerUnit.currentEnergy);

            dialogueText.text = "You ate Food! (+10 HP, +10 Energy)";
            hasUsedItemThisTurn = true;

            ReturnToCombat();
        }
        else
        {
            dialogueText.text = "You don't have any Food!";
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
}
