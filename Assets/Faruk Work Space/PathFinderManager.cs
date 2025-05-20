using System.Collections.Generic;
using UnityEngine;

public class PathFinderManager : MonoBehaviour
{
    public List<GameObject> walkableArea;
    public GameObject startObj;
    public GameObject goalObj;
    public float neighborDistance = 1.1f;

    void Start()
    {
        FindAndActivatePath();
    }

    void FindAndActivatePath()
    {
        if (startObj == null || goalObj == null)
        {
            Debug.LogWarning("Start veya Goal GameObject atanmadý!");
            return;
        }

        Dictionary<GameObject, GameObject> cameFrom = new Dictionary<GameObject, GameObject>();
        Queue<GameObject> frontier = new Queue<GameObject>();
        HashSet<GameObject> visited = new HashSet<GameObject>();

        frontier.Enqueue(startObj);
        visited.Add(startObj);

        while (frontier.Count > 0)
        {
            GameObject current = frontier.Dequeue();

            if (current == goalObj)
                break;

            foreach (GameObject neighbor in GetNeighbors(current))
            {
                if (!visited.Contains(neighbor))
                {
                    frontier.Enqueue(neighbor);
                    visited.Add(neighbor);
                    cameFrom[neighbor] = current;
                }
            }
        }

        List<GameObject> path = new List<GameObject>();
        GameObject step = goalObj;

        while (step != startObj)
        {
            path.Add(step);

            if (!cameFrom.ContainsKey(step))
            {
                Debug.LogWarning("Goal objesine ulaþýlabilir yol bulunamadý.");
                return;
            }

            step = cameFrom[step];
        }

        path.Add(startObj);

        // Diðerlerini kapat, sadece yoldakileri açýk býrak
        foreach (GameObject obj in walkableArea)
        {
            obj.SetActive(path.Contains(obj));
        }

        Debug.Log("Yol bulundu. Adým sayýsý: " + path.Count);
    }

    List<GameObject> GetNeighbors(GameObject current)
    {
        List<GameObject> neighbors = new List<GameObject>();
        Vector3 currentPos = current.transform.position;

        foreach (GameObject obj in walkableArea)
        {
            if (obj == current) continue;

            if (Vector3.Distance(obj.transform.position, currentPos) <= neighborDistance)
            {
                neighbors.Add(obj);
            }
        }

        return neighbors;
    }
}