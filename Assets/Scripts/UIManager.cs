using UnityEngine;

public class UIManager : MonoBehaviour
{
    public GameObject canvasOld;
    public GameObject canvasNew;

    // Hangi canvas aktif: true = new, false = old
    public bool IsNewActive { get; private set; } = true;

    public void Switch(bool isNew)
    {
        IsNewActive = isNew;

        // Önce ikisini de aktif yap
        canvasOld.SetActive(true);
        canvasNew.SetActive(true);

        // Sonra geçerli olmayanı kapat
        if (isNew)
        {
            canvasOld.SetActive(false);
            canvasNew.SetActive(true);
        }
        else
        {
            canvasOld.SetActive(true);
            canvasNew.SetActive(false);
        }

        Debug.Log($"Canvas switched ➜ {(isNew ? "Canvas_Kopya (NEW)" : "Canvas (OLD)")}");
    }
}
