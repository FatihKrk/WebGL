using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class MyLoadingBar : MonoBehaviour
{
    [Header("UI Elements")]
    public Slider slider;                // Sahnedeki Slider UI
    public TextMeshProUGUI progressText; // % g�sterecek TMP text
    public GameObject blockerPanel;      // Tam ekran blocker panel

    [Header("Timing Settings")]
    [Tooltip("Her art�� ad�m� aras�nda bekleme s�resi (saniye)")]
    public float stepDelay = 0.05f;
    [Tooltip("Her ad�mda slider'�n artaca�� miktar")]
    [Range(0.001f, 0.1f)]
    public float stepAmount = 0.01f;

    private void Start()
    {
        // Ba�lang��ta ekran� kilitle
        blockerPanel.SetActive(true);
        // Coroutine'i ba�lat
        StartCoroutine(RunLoading());
    }

    private IEnumerator RunLoading()
    {
        float progress = 0f;
        slider.value = 0f;
        progressText.text = "0%";

        // %100�e ula�ana kadar d�ng�
        while (progress < 1f)
        {
            progress += stepAmount;
            slider.value = progress;
            progressText.text = Mathf.RoundToInt(progress * 100f) + "%";

            // Bir sonraki ad�ma kadar bekle
            yield return new WaitForSeconds(stepDelay);
        }

        // Y�kleme tamamland�: blocker paneli kapa
        blockerPanel.SetActive(false);
    }
}
