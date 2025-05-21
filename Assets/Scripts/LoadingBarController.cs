using UnityEngine;
using UnityEngine.UI;

public class LoadingBarController : MonoBehaviour
{
    public Slider slider; // Y�kleme �ubu�u referans�
    public Material loadingMaterial; // Y�kleme �ubu�u materyali

    void Update()
    {
        // Shader��n _Progress de�erini, slider��n de�erine g�re g�ncelliyoruz
        loadingMaterial.SetFloat("_Progress", slider.value);
    }
}
