using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Button))]
public class SwitchButton : MonoBehaviour
{
    [Header("Bağlantılar")]
    [SerializeField] UIManager uiMgr;      // UIManager objesi
    [SerializeField] Image btnImage;       // Button’un Image component’i
    [SerializeField] TMP_Text label;       // Button’daki yazı

    [Header("Renkler")]
    [SerializeField] Color colorOld = new(0.9f, 0.25f, 0.25f); // Eskiye geçilecekse
    [SerializeField] Color colorNew = new(0.2f, 0.8f, 0.35f);  // Yeniye geçilecekse

    Button _btn;

    void Awake()
    {
        _btn = GetComponent<Button>();

        if (!btnImage) btnImage = GetComponent<Image>();
        if (!label) label = GetComponentInChildren<TMP_Text>();

        UpdateVisual(uiMgr.IsNewActive);

        _btn.onClick.AddListener(OnClick);
    }

    void OnClick()
    {
        bool nextIsNew = !uiMgr.IsNewActive;
        uiMgr.Switch(nextIsNew);
        UpdateVisual(nextIsNew);
    }

    void UpdateVisual(bool nextIsNew)
    {
        label.text = nextIsNew ? "OLD" : "NEW";
        btnImage.color = nextIsNew ? colorNew : colorOld;
    }
}
