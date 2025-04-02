using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;

public class GetAttFromSql : MonoBehaviour
{
    // PHP API'nin URL'si
    private string apiUrl = "https://m3muhendislik.com/api/getPcxAtt.php";
    
    [System.Serializable]
    public class TextResponse
    {
        public string att; // Gelen JSON'daki "att" alanı
    }

    // Unity'ye gelen GET isteği
    private IEnumerator SendGetRequest(string nameToSearch, Action<string> onDataReceived)
    {
        string urlWithName = apiUrl + "?name=" + UnityWebRequest.EscapeURL(nameToSearch);

        UnityWebRequest request = UnityWebRequest.Get(urlWithName); // GET isteği oluşturma

        yield return request.SendWebRequest(); // Yanıtı bekleme

        if (request.result == UnityWebRequest.Result.Success)
        {
            string jsonResponse = request.downloadHandler.text;

            try
            {
                // JSON verisini TextResponse objesine çevirme
                var response = JsonUtility.FromJson<TextResponse>(jsonResponse);

                // Callback ile veriyi geri gönder
                onDataReceived?.Invoke(response.att);
            }
            catch (Exception ex)
            {
                Debug.LogError("JSON parse hatası: " + ex.Message); // JSON parse hatası varsa mesajı logla
                onDataReceived?.Invoke(null); // Hata durumunda null döndür
            }
        }
        else
        {
            Debug.LogError("Hata: " + request.error);
            onDataReceived?.Invoke(null); // Hata durumunda null döndür
        }
    }


    // Unity'de çağırılacak örnek fonksiyon
    public void GetAttByName(string name, Action<string> onDataReceived)
    {
        // Coroutine başlat ve callback'i işle
        StartCoroutine(SendGetRequest(name, onDataReceived));
    }
}
