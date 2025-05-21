using UnityEngine;
using UnityEngine.UI;       // Slider ve benzeri UI tipleri için
using TMPro;               // TextMeshProUGUI tipi için
using System.Collections;  // IEnumerator ve WaitForSeconds için

public class LoadingBlockerController : MonoBehaviour
{
    public Slider slider;                    // Yükleme çubuðu
    public TextMeshProUGUI progressText;     // Yüzdeyi gösterecek metin
    public GameObject loadingBlocker;        // Blocker panel (tam ekran)

    IEnumerator StartLoadingBar()
    {
        // Loading baþlarken blocker aktif
        loadingBlocker.SetActive(true);

        float progress = 0f;
        while (progress < 1f)
        {
            progress += 0.01f;
            slider.value = progress;
            progressText.text = (progress * 100f).ToString("F0") + "%";
            yield return new WaitForSeconds(0.05f);
        }

        // Yükleme bitince blocker'ý devre dýþý býrak
        loadingBlocker.SetActive(false);
    }

    void Start()
    {
        // Yükleme çubuðu animasyonunu baþlatýyoruz
        StartCoroutine(StartLoadingBar());
    }
}
