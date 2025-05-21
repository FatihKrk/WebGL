using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;

public class Paneloff : MonoBehaviour
{
    [Header("Options Panel Container")]
    public GameObject panelContainer;      

    [Header("Apply & Save Buttons")]
    public Button applyButton;             
    public Button saveButton;             

    [Header("Message Text (TMP)")]
    public TMP_Text messageText;           

    [Header("Custom Blockers")]
    public List<GameObject> blockers;      // Blocker1, Blocker2, Blocker3

    void Start()
    {
        // Panel ba�lang��ta kapal�
        panelContainer.SetActive(false);

        // T�m blocker�lar� kapat
        foreach (var b in blockers)
            b.SetActive(false);

      
        applyButton.onClick.AddListener(OnApply);
        saveButton.onClick.AddListener(OnSave);
    }

 
    public void OpenPanel()
    {
        panelContainer.SetActive(true);

        // Blocker�lar� a�
        foreach (var b in blockers)
            b.SetActive(true);
    }

    private void OnApply()
    {
        messageText.text = "...Settings applied...";
        StartCoroutine(HideMessageAfterDelay(1f));
    }

    private void OnSave()
    {
     
        panelContainer.SetActive(false);

        
        foreach (var b in blockers)
            b.SetActive(false);

        
        messageText.text = "";
    }

    private IEnumerator HideMessageAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        messageText.text = "";
    }

}
