using UnityEngine;
using UnityEngine.UI;       // Slider ve benzeri UI tipleri i�in
using TMPro;               // TextMeshProUGUI tipi i�in
using System.Collections;  // IEnumerator ve WaitForSeconds i�in

public class LoadingBlockerController : MonoBehaviour
{
    public Slider slider;                    // Y�kleme �ubu�u
    public TextMeshProUGUI progressText;     // Y�zdeyi g�sterecek metin
    public GameObject loadingBlocker;        // Blocker panel (tam ekran)

    IEnumerator StartLoadingBar()
    {
        // Loading ba�larken blocker aktif
        loadingBlocker.SetActive(true);

        float progress = 0f;
        while (progress < 1f)
        {
            progress += 0.01f;
            slider.value = progress;
            progressText.text = (progress * 100f).ToString("F0") + "%";
            yield return new WaitForSeconds(0.05f);
        }

        // Y�kleme bitince blocker'� devre d��� b�rak
        loadingBlocker.SetActive(false);
    }

    void Start()
    {
        // Y�kleme �ubu�u animasyonunu ba�lat�yoruz
        StartCoroutine(StartLoadingBar());
    }
}
