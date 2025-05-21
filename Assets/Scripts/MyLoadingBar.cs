using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class MyLoadingBar : MonoBehaviour
{
    [Header("UI Elements")]
    public Slider slider;                // Sahnedeki Slider UI
    public TextMeshProUGUI progressText; // % gösterecek TMP text
    public GameObject blockerPanel;      // Tam ekran blocker panel

    [Header("Timing Settings")]
    [Tooltip("Her artýþ adýmý arasýnda bekleme süresi (saniye)")]
    public float stepDelay = 0.05f;
    [Tooltip("Her adýmda slider'ýn artacaðý miktar")]
    [Range(0.001f, 0.1f)]
    public float stepAmount = 0.01f;

    private void Start()
    {
        // Baþlangýçta ekraný kilitle
        blockerPanel.SetActive(true);
        // Coroutine'i baþlat
        StartCoroutine(RunLoading());
    }

    private IEnumerator RunLoading()
    {
        float progress = 0f;
        slider.value = 0f;
        progressText.text = "0%";

        // %100’e ulaþana kadar döngü
        while (progress < 1f)
        {
            progress += stepAmount;
            slider.value = progress;
            progressText.text = Mathf.RoundToInt(progress * 100f) + "%";

            // Bir sonraki adýma kadar bekle
            yield return new WaitForSeconds(stepDelay);
        }

        // Yükleme tamamlandý: blocker paneli kapa
        blockerPanel.SetActive(false);
    }
}
