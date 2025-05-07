using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoGridLodSystem : MonoBehaviour
{
    public float distanceThreshold = 20f;
    public float toggleCooldown = 1f;

    private GameObject user;
    private GameObject firstParent;

    private List<MeshRenderer>[] regionRenderers;
    private Vector3[] regionCenters;
    private bool[] regionIsFar;
    private float[] lastToggleTimes;

    private Vector3 lastUserPosition;
    private float moveThreshold = 0.5f;

    private int gridX = 1, gridY = 1, gridZ = 1; // Otomatik belirlenecek
    private const int idealObjectsPerRegion = 750;

    private void Start()
    {
        user = GameObject.FindGameObjectWithTag("MainCamera");
        firstParent = GameObject.FindGameObjectWithTag("ParentObject");

        if (user == null || firstParent == null) return;

        Bounds totalBounds = CalculateTotalBounds();
        Vector3 min = totalBounds.min;
        Vector3 size = totalBounds.size;

        List<MeshRenderer> smallObjects = new List<MeshRenderer>();

        foreach (var rend in firstParent.GetComponentsInChildren<MeshRenderer>(true))
        {
            float volume = rend.bounds.size.x * rend.bounds.size.y * rend.bounds.size.z;
            if (volume < 0.01f)
            {
                smallObjects.Add(rend);
                rend.enabled = false;
            }   
        }

        DetermineGridSize(smallObjects.Count);

        int regionCount = gridX * gridY * gridZ;

        regionRenderers = new List<MeshRenderer>[regionCount];
        regionCenters = new Vector3[regionCount];
        regionIsFar = new bool[regionCount];
        lastToggleTimes = new float[regionCount];

        for (int i = 0; i < regionCount; i++)
            regionRenderers[i] = new List<MeshRenderer>();

        foreach (var rend in smallObjects)
        {
            int index = GetRegionIndex(rend.bounds.center, min, size);
            regionRenderers[index].Add(rend);
        }

        for (int i = 0; i < regionRenderers.Length; i++)
        {
            if (regionRenderers[i].Count == 0) continue;

            Vector3 sum = Vector3.zero;
            foreach (var rend in regionRenderers[i])
                sum += rend.bounds.center;

            regionCenters[i] = sum / regionRenderers[i].Count;
        }

        lastUserPosition = user.transform.position;
        InvokeRepeating(nameof(CheckDistanceAndToggle), 0f, 0.5f);
    }

    private void DetermineGridSize(int objectCount)
    {
        int regionTarget = Mathf.Max(1, objectCount / idealObjectsPerRegion);
        float cubeRoot = Mathf.Pow(regionTarget, 1f / 3f);

        gridX = Mathf.Max(1, Mathf.RoundToInt(cubeRoot));
        gridY = Mathf.Max(1, Mathf.RoundToInt(cubeRoot * 0.66f)); // Y daha kısa olsun (çatı yüksekliği genelde daha az)
        gridZ = Mathf.Max(1, Mathf.RoundToInt(cubeRoot));
    }

    private void CheckDistanceAndToggle()
    {
        Vector3 currentPos = user.transform.position;
        if (Vector3.Distance(currentPos, lastUserPosition) < moveThreshold)
            return;

        float currentTime = Time.time;

        for (int i = 0; i < regionRenderers.Length; i++)
        {
            if (regionRenderers[i].Count == 0) continue;

            if (currentTime - lastToggleTimes[i] < toggleCooldown)
                continue;

            float dist = Vector3.Distance(currentPos, regionCenters[i]);
            bool shouldBeFar = dist >= distanceThreshold;

            if (shouldBeFar != regionIsFar[i])
            {
                regionIsFar[i] = shouldBeFar;
                lastToggleTimes[i] = currentTime;
                StartCoroutine(SetRenderersActive(regionRenderers[i], !shouldBeFar));
            }
        }

        lastUserPosition = currentPos;
    }

    private IEnumerator SetRenderersActive(List<MeshRenderer> renderers, bool state)
    {
        int counter = 0;
        const int chunkSize = 250;

        foreach (var rend in renderers)
        {
            if (rend != null)
                rend.enabled = state;

            counter++;
            if (counter >= chunkSize)
            {
                counter = 0;
                yield return null;
            }
        }
    }

    private Bounds CalculateTotalBounds()
    {
        Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);
        bool initialized = false;

        foreach (var rend in firstParent.GetComponentsInChildren<MeshRenderer>(true))
        {
            if (!initialized)
            {
                bounds = new Bounds(rend.bounds.center, rend.bounds.size);
                initialized = true;
            }
            else
            {
                bounds.Encapsulate(rend.bounds);
            }
        }

        return bounds;
    }

    private int GetRegionIndex(Vector3 pos, Vector3 min, Vector3 size)
    {
        int ix = Mathf.Clamp((int)(((pos.x - min.x) / size.x) * gridX), 0, gridX - 1);
        int iy = Mathf.Clamp((int)(((pos.y - min.y) / size.y) * gridY), 0, gridY - 1);
        int iz = Mathf.Clamp((int)(((pos.z - min.z) / size.z) * gridZ), 0, gridZ - 1);

        return ix + iy * gridX + iz * gridX * gridY;
    }
}
