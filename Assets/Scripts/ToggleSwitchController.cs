using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

[RequireComponent(typeof(Toggle))]
public class ToggleSwitchController : MonoBehaviour
{
    [Header("Otodan bulunur ‑ inspector boş kalabilir")]
    [SerializeField] RectTransform handleRect;        // beyaz knob
    [SerializeField] RectTransform labelRect;         // “OLD / NEW” TMP objesi
    [SerializeField] Image backgroundImage;   // yeşil / kırmızı arka plan
    [SerializeField] TMP_Text label;
    [SerializeField] UIManager uiManager;

    [Header("Renkler")]
    [SerializeField] Color bgOn = new(.20f, .80f, .35f); // NEW (yeşil)
    [SerializeField] Color bgOff = new(.90f, .25f, .25f); // OLD (kırmızı)

    [Header("Anim süreleri")]
    [SerializeField] float moveDur = 0.35f;
    [SerializeField] float colorDur = 0.25f;
    [SerializeField] Ease ease = Ease.InOutBack;

    //‑‑‑ private
    Toggle _tgl;
    Vector2 knobLeft, knobRight;
    Vector2 labelLeft, labelRight;

    void Awake()
    {
        _tgl = GetComponent<Toggle>();

        if (!backgroundImage) backgroundImage = GetComponent<Image>();
        if (!handleRect) handleRect = transform.GetChild(0).GetComponent<RectTransform>();
        if (!labelRect) labelRect = GetComponentInChildren<TMP_Text>().rectTransform;
        if (!label) label = labelRect.GetComponent<TMP_Text>();

        /* -------------------------------------------------
           1)  Ortak anchor / pivot = (0.0 , 0.5)  (solda‑ortada)
        --------------------------------------------------*/
        var bgRT = (RectTransform)backgroundImage.transform;
        handleRect.anchorMin = handleRect.anchorMax = new Vector2(0f, .5f);
        handleRect.pivot = new Vector2(.5f, .5f);

        labelRect.anchorMin = labelRect.anchorMax = new Vector2(0f, .5f);
        labelRect.pivot = new Vector2(.5f, .5f);

        /* -------------------------------------------------
           2)  Pozisyonları dinamik hesapla
        --------------------------------------------------*/
        float bgW = bgRT.rect.width;
        float knobHalf = handleRect.rect.width * 0.5f;
        float labelHalf = labelRect.rect.width * 0.5f;
        float pad = 4f;                 // her iki uçtaki küçük boşluk

        knobLeft = new Vector2(pad + knobHalf, 0);                 // NEW durumunda
        knobRight = new Vector2(bgW - pad - knobHalf, 0);           // OLD  durumunda

        // Etiket knob’un tam karşısında dursun
        labelRight = new Vector2(pad + labelHalf, 0);               // OLD yazısı (kırmızı)
        labelLeft = new Vector2(bgW - pad - labelHalf, 0);         // NEW yazısı (yeşil)

        /* İlk state */
        ApplyState(_tgl.isOn, true);

        _tgl.toggleTransition = Toggle.ToggleTransition.None;  // Unity’nin fade’ini kapat
        _tgl.graphic = null;                          // knob’u saklamasın
        _tgl.onValueChanged.AddListener(ApplyState);


    }

    //---------------------------------------------------------------------------

    void ApplyState(bool isOn) => ApplyState(isOn, false);

    void ApplyState(bool isOn, bool instant)
    {
        /* Knob konumu */
        Vector2 kTarget = isOn ? knobLeft : knobRight;
        if (instant) handleRect.anchoredPosition = kTarget;
        else handleRect.DOAnchorPos(kTarget, moveDur).SetEase(ease);

        /* Label konumu + metni */
        Vector2 lTarget = isOn ? labelLeft : labelRight;
        if (instant) labelRect.anchoredPosition = lTarget;
        else labelRect.DOAnchorPos(lTarget, moveDur).SetEase(ease);

        label.text = isOn ? "OLD" : "NEW";

        /* Arka plan rengi */
        Color bg = isOn ? bgOn : bgOff;
        if (instant) backgroundImage.color = bg;
        else backgroundImage.DOColor(bg, colorDur);

        /* UIManager’a haber ver */
        if (uiManager) uiManager.Switch(isOn);
        label.alignment = isOn

        ? TextAlignmentOptions.Right     // YEŞİL → OLD  (sağa dayalı)
        : TextAlignmentOptions.Left;     // KIRMIZI → NEW (sola dayalı)
    }

    void OnDestroy() => _tgl.onValueChanged.RemoveListener(ApplyState);
}
