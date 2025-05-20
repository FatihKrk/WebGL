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
            Debug.LogWarning("Start veya Goal GameObject atanmad�!");
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
                Debug.LogWarning("Goal objesine ula��labilir yol bulunamad�.");
                return;
            }

            step = cameFrom[step];
        }

        path.Add(startObj);

        // Di�erlerini kapat, sadece yoldakileri a��k b�rak
        foreach (GameObject obj in walkableArea)
        {
            obj.SetActive(path.Contains(obj));
        }

        Debug.Log("Yol bulundu. Ad�m say�s�: " + path.Count);
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