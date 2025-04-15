using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

public class SceneObjectLister : MonoBehaviour
{
#if UNITY_EDITOR
    [MenuItem("Tools/List All Scene Objects")]
    static void ListAllObjects()
    {
        GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
        foreach (GameObject go in allObjects)
        {
            Debug.Log("Name: " + go.name + " | Active: " + go.activeSelf + " | Tag: " + go.tag);
        }
    }
#endif
}
