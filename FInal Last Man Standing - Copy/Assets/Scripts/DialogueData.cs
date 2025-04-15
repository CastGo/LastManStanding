using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Dialogue/DialogueData")]
public class DialogueData : ScriptableObject // ⛳ เปลี่ยนจาก MonoBehaviour เป็น ScriptableObject
{
    public DialogueLine[] lines;
}

[System.Serializable]
public class DialogueLine
{
    public string speaker; // "MC" หรือ "NPC"
    [TextArea]
    public string text;
}