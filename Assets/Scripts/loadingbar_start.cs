    using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class loadingbar_start : MonoBehaviour
{
    public Slider slider;
    public TextMeshProUGUI progressText; 
    public GameObject loadingPanel; 

    void Start()
    {
        loadingPanel.SetActive(true);
        StartCoroutine(LoadBar());
    }

    IEnumerator LoadBar()
    {
        
        float progress = 0f;

        while (progress < 1f)
        {
            progress += 0.01f; 
            slider.value = progress; 
            progressText.text =(progress * 100).ToString("F0") + "%";
            yield return new WaitForSeconds(0.05f); 
        }
        /*
        yield return new WaitForSeconds(5f); 
        */
        loadingPanel.SetActive(false); // Loading panelini gizle
    }
}
