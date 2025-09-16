using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class AgentHardReset : MonoBehaviour
{
    [Tooltip("İstersen ilk hedefi buraya bağlayabilirsin (zorunlu değil).")]
    public Transform initialTarget;

    NavMeshAgent agent;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        // Ajan açık kalsın; yerleştirme bittikten sonra komut alacak.
        agent.autoBraking = true;
    }

    void OnEnable()
    {
        // Sahneye konur konmaz NavMesh'e oturt.
        StartCoroutine(Bootstrap());
    }

    /// <summary>
    /// Ajanı yakın NavMesh'e yerleştirir.
    /// - Önce raycast ile zemine indirir
    /// - Sonra spiral örnekleme ile en yakın NavMesh noktasını bulur
    /// - Bulduğu noktaya Warp eder (uyarı/hata üretmeden)
    /// </summary>
    IEnumerator Bootstrap()
    {
        // --- A) Bulunduğu yerin altına ray at, zemine indir ---
        Vector3 basePos = transform.position;
        if (Physics.Raycast(basePos + Vector3.up * 50f, Vector3.down, out var hit, 200f))
            basePos = hit.point;

        // --- B) Yakın NavMesh noktası ara (spiral tarama) ---
        const float sampleRange = 1f;  // NavMesh.SamplePosition için yarıçap
        const float step = 2f;         // Spiral yarıçap artışı (metre)
        const float max = 200f;        // En fazla ne kadar uzağa bakalım

        Vector3 snapPos = basePos;
        bool found = NavMesh.SamplePosition(basePos, out var nm, sampleRange, NavMesh.AllAreas);
        if (!found)
        {
            for (float r = step; r <= max && !found; r += step)
            {
                for (float a = 0f; a < 360f && !found; a += 15f)
                {
                    float rad = a * Mathf.Deg2Rad;
                    Vector3 p = basePos + new Vector3(Mathf.Cos(rad), 0f, Mathf.Sin(rad)) * r;
                    if (NavMesh.SamplePosition(p, out nm, sampleRange, NavMesh.AllAreas))
                    {
                        snapPos = nm.position;
                        found = true;
                    }
                }
                // Uzun taramada Editor donmasın
                yield return null;
            }
        }
        else
        {
            snapPos = nm.position;
        }

        // --- C) Bulduysak o pozisyona Warp et (NavMesh iç mantığı da güncellenir) ---
        if (found)
        {
            // Çok çok az yukarı al; zemine gömülme riskini azaltır
            Vector3 warpPos = snapPos + Vector3.up * 0.02f;
            agent.Warp(warpPos);
        }
        else
        {
            // Hiç NavMesh bulunamadıysa yerinde bırak (hata fırlatma yok)
            yield break;
        }

        // Bir frame bekle; isOnNavMesh flag'i güncellensin
        yield return null;

        // İsteğe bağlı: başlangıç hedefin varsa tek seferlik ver (TargetChaser yoksa iş görür)
        if (initialTarget && agent.isOnNavMesh)
        {
            agent.SetDestination(initialTarget.position);
        }
    }

    /// <summary>
    /// Dışarıdan çağırıp tekrar yerleştirmek istersen.
    /// </summary>
    public void PlaceOnNavMeshNow()
    {
        StopAllCoroutines();
        StartCoroutine(Bootstrap());
    }
}
