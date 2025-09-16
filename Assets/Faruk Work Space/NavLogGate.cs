using UnityEngine;

[DefaultExecutionOrder(-1000)]  // her þeyden önce çalýþsýn
public class NavLogGate : MonoBehaviour
{
    [Header("Konsol gürültüsünü azalt")]
    [Tooltip("Sadece Warning ve Error kalsýn (LOG'lar susar)")]
    public bool keepWarningsAndErrors = true;

    [Tooltip("Her þeyi sustur (Log+Warning+Error)")]
    public bool muteAll = false;

    void Awake()
    {
        if (muteAll)
        {
            Debug.unityLogger.logEnabled = false; // tamamýný kapatýr
        }
        else if (keepWarningsAndErrors)
        {
            // Sadece Log'larý kapatýr; Warning/Error görünür
            Debug.unityLogger.filterLogType = LogType.Warning;
        }

        // Log'lar için stacktrace'i de kapatalým (ekstra hýz)
        Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
    }
}
