using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;

public class LadderLinkTuner : MonoBehaviour
{
    [Header("Area isimleri ve maliyetler")]
    public string ladderAreaName = "Ladder";
    public float ladderAreaCost = 0.05f;   // merdiven çok ucuz
    public float walkableAreaCost = 2.0f;  // normal yüzeyleri pahalý yap

    [Header("Merdiven linkleri runtime oluþtuðu için bekle")]
    public float startDelay = 6f;          // sende ~6. sn

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(startDelay);

        int ladderArea = NavMesh.GetAreaFromName(ladderAreaName);
        int walkableArea = NavMesh.GetAreaFromName("Walkable");

        if (walkableArea >= 0)
            NavMesh.SetAreaCost(walkableArea, walkableAreaCost);

        if (ladderArea >= 0)
        {
            NavMesh.SetAreaCost(ladderArea, ladderAreaCost);
        }
        else
        {
            Debug.LogWarning($"[LadderCost] '{ladderAreaName}' area bulunamadý. Navigation > Areas sekmesinden oluþturun.");
        }

        // Sahnede oluþmuþ tüm NavMeshLink'leri yakala ve ayarla
        var links = FindObjectsOfType<NavMeshLink>(true);
        var agent = FindObjectOfType<NavMeshAgent>(); // tek ajan varsa yeter

        int count = 0, fixedType = 0;
        foreach (var link in links)
        {
            if (ladderArea >= 0)
            {
                link.area = ladderArea;   // link Ladder alanýnda
                link.costModifier = -1f;  // area cost’u kullan
            }
            link.bidirectional = true;

            // Agent type uyuþmuyorsa, ajanýn tipine zorla
            if (agent != null && link.agentTypeID != agent.agentTypeID)
            {
                link.agentTypeID = agent.agentTypeID;
                fixedType++;
            }
            count++;
        }

        Debug.Log($"[LadderCost] {count} link güncellendi. TypeFix={fixedType}, LadderCost={(ladderArea >= 0 ? ladderAreaCost : -1)}, WalkableCost={walkableAreaCost}");
    }
}
