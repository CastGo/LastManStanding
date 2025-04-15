using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StartGameDialogue : MonoBehaviour
{
    public TMP_Text dialogueText;
    public float delay = 0.05f;
    public float messageDuration = 3f;

    private string fullText = "ฉันต้องหาทางออกจากที่นี้...\nต้องไปที่ประตูทางออกชั้น 1 ให้ได้";

    void Start()
    {
        // ✅ เช็คว่าเราต้องแสดง Dialogue มั้ย
        if (PlayerPrefs.GetInt("StartGameDialogue", 0) == 1)
        {
            PlayerPrefs.SetInt("StartGameDialogue", 0); // ✅ แสดงแล้วลบ flag
            PlayerPrefs.Save();
            StartCoroutine(ShowDialogue());
        }
        else
        {
            dialogueText.gameObject.SetActive(false);
        }
    }

    IEnumerator ShowDialogue()
    {
        dialogueText.gameObject.SetActive(true);
        dialogueText.text = "";

        foreach (char c in fullText)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(delay);
        }

        yield return new WaitForSeconds(messageDuration);
        dialogueText.gameObject.SetActive(false);
    }
}
