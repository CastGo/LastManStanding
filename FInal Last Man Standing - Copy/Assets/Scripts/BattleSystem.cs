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

    IEnumerator PlayerAttack()
    {
        bool isDead = enemyUnit.TakeDamage(playerUnit.damage);

        enemyHUD.SetHP(enemyUnit.currentHP);
        dialogueText.text = "The attack is successful";

        yield return new WaitForSeconds(2f);

        if(isDead)
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
        dialogueText.text = enemyUnit.unitName + " attack!";

        yield return new WaitForSeconds(1f);

        bool isDead = playerUnit.TakeDamage(enemyUnit.damage);

        playerHUD.SetHP(playerUnit.currentHP);

        yield return new WaitForSeconds(1f);

        if(isDead)
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
            dialogueText.text = "You Won!!";
            StartCoroutine(ReturnAfterWin());
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

        GameManager.instance.savedHP = playerUnit.currentHP;
        GameManager.instance.savedEnergy = playerUnit.currentEnergy;
        // ปิดฉาก TurnBase แต่ไม่เปลี่ยนตำแหน่ง
        SceneManager.UnloadSceneAsync("TurnBase");

        // ตั้งฉาก "2-1 Room" เป็น active
        GameManager.instance.SetSceneActive(SceneManager.GetSceneByName("2-1 Room"), true);
        SceneManager.SetActiveScene(SceneManager.GetSceneByName("2-1 Room"));

        GameManager.instance.RestoreSceneObjects();

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
    public void OnAttackButton()
    {
        if (state != BattleState.PLAYERTURN)
            return;

        ReturnToCombat();
        StartCoroutine(PlayerAttack());
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
