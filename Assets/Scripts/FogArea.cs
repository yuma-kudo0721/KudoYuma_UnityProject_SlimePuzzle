using System.Collections;
using UnityEngine;

public class FogArea : MonoBehaviour
{
    [SerializeField] SpriteRenderer targetRenderer;
    [SerializeField] float fadeDuration = 0.5f;
    [SerializeField] float hiddenAlpha = 0f;

    Coroutine fadeCoroutine;

    void Awake()
    {
        if (targetRenderer == null)
        {
            targetRenderer = GetComponent<SpriteRenderer>();
        }
    }


    void OnTriggerStay2D(Collider2D other)
    {
        if (!IsFogTarget(other)) return;
        StartFade(hiddenAlpha);

    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!IsFogTarget(other)) return;
        StartFade(1f);
    }


    bool IsFogTarget(Collider2D other)
    {
        return other.CompareTag("Player")
            || other.CompareTag("PlayerClone")
            || other.CompareTag("PlayerBullet");
    }


    void StartFade(float targetAlpha)
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        fadeCoroutine = StartCoroutine(FadeTo(targetAlpha));
    }

    IEnumerator FadeTo(float targetAlpha)
    {
        Color startColor = targetRenderer.color;
        float startAlpha = startColor.a;
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / fadeDuration);

            Color color = targetRenderer.color;
            color.a = Mathf.Lerp(startAlpha, targetAlpha, t);
            targetRenderer.color = color;

            yield return null;
        }

        Color endColor = targetRenderer.color;
        endColor.a = targetAlpha;
        targetRenderer.color = endColor;
        fadeCoroutine = null;
    }
}
