// AgentAreaMaskSetup.cs  → Agent’a ekle
using UnityEngine;
using UnityEngine.AI;

public class AgentAreaMaskSetup : MonoBehaviour
{
    void Start()
    {
        var agent = GetComponent<NavMeshAgent>();
        int walk = NavMesh.GetAreaFromName("Walkable");
        int lad = NavMesh.GetAreaFromName("Ladder");
        int ramp = NavMesh.GetAreaFromName("Ramp"); // yoksa -1 döner

        // Maske: Walkable + Ladder (Ramp hariç)
        int mask = (1 << walk) | (1 << lad);
        agent.areaMask = mask;

        // Maliyetler (güvence)
        if (walk >= 0) agent.SetAreaCost(walk, 5f);
        if (lad >= 0) agent.SetAreaCost(lad, 0.05f);
        if (ramp >= 0) agent.SetAreaCost(ramp, 500f);
    }
}
