using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScreenFader : MonoBehaviour
{
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 1f;

    private void Awake()
    {
        if (fadeImage == null)
        {
            fadeImage = GetComponent<Image>();
        }

        SetAlpha(1f); // Start fully black if desired
    }

    public IEnumerator FadeInRoutine(System.Action onComplete = null)
    {
        yield return StartCoroutine(Fade(1f, 0f));
        onComplete?.Invoke();
    }

    public IEnumerator FadeOutRoutine(System.Action onComplete = null)
    {
        yield return StartCoroutine(Fade(0f, 1f));
        onComplete?.Invoke();
    }

    private IEnumerator Fade(float from, float to)
    {
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / fadeDuration);
            SetAlpha(Mathf.Lerp(from, to, t));
            yield return null;
        }

        SetAlpha(to);
    }

    private void SetAlpha(float alpha)
    {
        if (fadeImage != null)
        {
            Color color = fadeImage.color;
            color.a = alpha;
            fadeImage.color = color;
        }
    }
}
