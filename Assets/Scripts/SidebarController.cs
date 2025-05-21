using UnityEngine;
using UnityEngine.UI; // UI bileþenlerini kullanmak için

public class SidebarController : MonoBehaviour
{
    public GameObject leftSidebarPanel;   // Sol panel
    public GameObject toggleOnButton;     // ">>" butonu
    public GameObject toggleOffButton;    // "<<" butonu

    void Start()
    {
        // Baþlangýç durumlarýný garanti altýna alalým
        leftSidebarPanel.SetActive(false);   // Panel kapalý
        toggleOnButton.SetActive(true);      // Açma butonu açýk
        toggleOffButton.SetActive(false);    // Kapama butonu kapalý
    }

    public void ShowPanel()
    {
        leftSidebarPanel.SetActive(true);    // Paneli aç
        toggleOnButton.SetActive(false);     // ">>" butonu gizle
        toggleOffButton.SetActive(true);     // "<<" butonu göster
    }

    public void HidePanel()
    {
        leftSidebarPanel.SetActive(false);   // Paneli kapa
        toggleOnButton.SetActive(true);      // ">>" butonu göster
        toggleOffButton.SetActive(false);    // "<<" butonu gizle
    }
}
