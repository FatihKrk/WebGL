using UnityEngine;
using UnityEngine.UI; // UI bile�enlerini kullanmak i�in

public class SidebarController : MonoBehaviour
{
    public GameObject leftSidebarPanel;   // Sol panel
    public GameObject toggleOnButton;     // ">>" butonu
    public GameObject toggleOffButton;    // "<<" butonu

    void Start()
    {
        // Ba�lang�� durumlar�n� garanti alt�na alal�m
        leftSidebarPanel.SetActive(false);   // Panel kapal�
        toggleOnButton.SetActive(true);      // A�ma butonu a��k
        toggleOffButton.SetActive(false);    // Kapama butonu kapal�
    }

    public void ShowPanel()
    {
        leftSidebarPanel.SetActive(true);    // Paneli a�
        toggleOnButton.SetActive(false);     // ">>" butonu gizle
        toggleOffButton.SetActive(true);     // "<<" butonu g�ster
    }

    public void HidePanel()
    {
        leftSidebarPanel.SetActive(false);   // Paneli kapa
        toggleOnButton.SetActive(true);      // ">>" butonu g�ster
        toggleOffButton.SetActive(false);    // "<<" butonu gizle
    }
}
