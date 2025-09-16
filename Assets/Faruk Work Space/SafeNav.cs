using UnityEngine;
using UnityEngine.AI;

public static class SafeNav
{
    // origin'e yakýn bir NavMesh noktasý bulur
    public static bool SnapToNavmesh(Vector3 origin, float radius, out Vector3 point)
    {
        if (NavMesh.SamplePosition(origin, out var hit, radius, NavMesh.AllAreas))
        {
            point = hit.position;
            return true;
        }

        point = origin;
        return false;
    }
}
