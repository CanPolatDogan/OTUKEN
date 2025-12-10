using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingScript : MonoBehaviour
{
    public Slider slider;               // Slider referans�
    public float fillDuration = 3f;     // Dolma s�resi (saniye)
    public TextMeshProUGUI percentageText; // Y�zde yaz�s�

    private Coroutine fillCoroutine;    // Tekrar ba�lat�lmas�n diye referans

    void OnEnable()
    {
        slider.value = 0;
        UpdatePercentageText(0);

        // �nce �nceki coroutine varsa durdur
        if (fillCoroutine != null)
            StopCoroutine(fillCoroutine);

        // Yeni coroutine ba�lat
        fillCoroutine = StartCoroutine(FillSlider());
    }

    IEnumerator FillSlider()
    {
        float elapsedTime = 0f;

        while (elapsedTime < fillDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsedTime / fillDuration);
            slider.value = progress;
            UpdatePercentageText(progress);
            yield return null;
        }

        slider.value = 1f;
        UpdatePercentageText(1f);
    }

    void UpdatePercentageText(float value)
    {
        int percentage = Mathf.RoundToInt(value * 100f);
        percentageText.text = percentage + "%";
    }
}
