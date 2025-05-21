using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Basit, bağımsız loading-bar:
///   • Slider + yüzdelik metin günceller
///   • Yükleme tamamlandığında blocker panelini kapatır
///   • Dışarıdan StartLoading() çağrısı ile başlar
/// </summary>
[DisallowMultipleComponent]
public class RealLoadingBar : MonoBehaviour
{
    // ────────── UI ──────────
    [Header("UI")]
    [SerializeField] private Slider slider;
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private GameObject loadingBlocker;

    // ────────── Timing ──────
    [Header("Timing")]
    [Tooltip("İki adım arası bekleme (saniye)")]
    [SerializeField] private float stepDelay = 0.05f;
    [Tooltip("Her adımda ilerleme miktarı")]
    [SerializeField] private float stepAmount = 0.01f;

    Coroutine routine;

    // ──────────────────────────────  API  ──────────────────────────────
    public void StartLoading()
    {
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(FakeProgress());
    }

    // (İsterseniz dışarıdan da çağırabilirsiniz)
    public void CloseImmediately()
    {
        if (routine != null) StopCoroutine(routine);
        loadingBlocker.SetActive(false);
    }

    // ─────────────────────────── Internal ──────────────────────────────
    IEnumerator FakeProgress()
    {
        // Güvenlik: referanslar atandı mı?
        if (!slider || !progressText || !loadingBlocker)
        {
            Debug.LogError("RealLoadingBar → Referans eksik!");
            yield break;
        }

        loadingBlocker.SetActive(true);

        float p = 0f;
        slider.value = 0f;
        progressText.text = "0%";

        while (p < 1f)
        {
            p += stepAmount;
            if (p > 1f) p = 1f;

            slider.value = p;
            progressText.text = Mathf.RoundToInt(p * 100f) + "%";

            yield return new WaitForSeconds(stepDelay);
        }

        // bitti
        loadingBlocker.SetActive(false);
        routine = null;
    }
}
