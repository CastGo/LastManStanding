using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneFader : MonoBehaviour
{
    public static SceneFader instance;

    public Image fadeImage;
    public float fadeDuration = 1f;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        StartCoroutine(FadeFromBlack());
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(FadeFromBlack());
    }

    public void FadeToScene(string sceneName)
    {
        StartCoroutine(FadeAndLoad(sceneName));
    }

    IEnumerator FadeAndLoad(string sceneName)
    {
        yield return StartCoroutine(FadeToBlack());
        SceneManager.LoadScene(sceneName);
    }

    IEnumerator FadeToBlack()
    {
        float t = 0;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float alpha = t / fadeDuration;
            SetAlpha(alpha);
            yield return null;
        }
        SetAlpha(1);
    }

    IEnumerator FadeFromBlack()
    {
        float t = 0;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float alpha = 1 - (t / fadeDuration);
            SetAlpha(alpha);
            yield return null;
        }
        SetAlpha(0);
    }

    void SetAlpha(float a)
    {
        if (fadeImage != null)
        {
            Color c = fadeImage.color;
            c.a = a;
            fadeImage.color = c;
        }
    }
}
