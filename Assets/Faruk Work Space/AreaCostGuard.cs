using UnityEngine;
using UnityEngine.AI;

[DefaultExecutionOrder(-10000)]
public class AreaCostGuard : MonoBehaviour
{
    void Awake()
    {
        // Uyarý olduðunda script isimlerini görebilmek için stacktrace aç
        Application.SetStackTraceLogType(LogType.Warning, StackTraceLogType.ScriptOnly);

        Debug.LogWarning(
            "[AreaCostGuard] Eðer 'Setting a NavMeshArea cost less than one' tekrar çýkarsa," +
            " konsoldaki stacktrace satýrýna týkla; hangi script SetAreaCost(<1) çaðýrmýþ göreceksin.");
    }

    void OnDestroy()
    {
        // Ýstersen tekrar kapat
        Application.SetStackTraceLogType(LogType.Warning, StackTraceLogType.None);
    }
}
