using System.Collections;
using UnityEngine;

public class FadeOutObject : MonoBehaviour
{
    [SerializeField] private float fadeDelay = 0.2f;
    [SerializeField] private float fadingTime = 0.45f;

    private Renderer objectRenderer;
    private Color startingColor;

    private void Start()
    {
        objectRenderer = GetComponent<Renderer>();
        startingColor = objectRenderer.material.color;

        Invoke("StartFadeOut", fadeDelay);
    }

    private void StartFadeOut()
    {
        StartCoroutine(FadeOut());
    }

    private IEnumerator FadeOut()
    {
        float elapsedTime = 0f;
        Color targetColor = new Color(startingColor.r, startingColor.g, startingColor.b, 0f);

        while (elapsedTime < fadingTime)
        {
            objectRenderer.material.color = Color.Lerp(startingColor, targetColor, elapsedTime / fadingTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        objectRenderer.material.color = targetColor;

        Destroy(gameObject);
    }
}
