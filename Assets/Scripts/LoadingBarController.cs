using UnityEngine;
using UnityEngine.UI;

public class LoadingBarController : MonoBehaviour
{
    public Slider slider; // Yükleme çubuðu referansý
    public Material loadingMaterial; // Yükleme çubuðu materyali

    void Update()
    {
        // Shader’ýn _Progress deðerini, slider’ýn deðerine göre güncelliyoruz
        loadingMaterial.SetFloat("_Progress", slider.value);
    }
}
