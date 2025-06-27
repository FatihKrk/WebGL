using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Measure : MonoBehaviour
{
    Ray ray;
    RaycastHit hit;
    public LineRenderer lineRenderer;
    private Vector3 firstPosition, lastPosition;
    public TMP_Text text;
    [SerializeField] MoveButtons moveButtons;
    public bool isOverUI, firstClick, secondClick;
    private float RotationSpeed;
    private GameObject highlightSphere;

    void Start()
    {
        // Highlight için bir sphere oluştur
        highlightSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        highlightSphere.transform.localScale = Vector3.one * 0.1f; // Küçük bir boyut
        highlightSphere.GetComponent<Collider>().enabled = false; // Çarpışma kapalı
        highlightSphere.GetComponent<Renderer>().material.color = Color.red; // Kırmızı renk
        highlightSphere.SetActive(false); // Başlangıçta gizli
    }

    void Update()
    {
        if (moveButtons.measure)
        { 
            isOverUI = UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();

            ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if(Physics.Raycast(ray, out hit))
            { 
                MeshCollider meshCollider = hit.collider as MeshCollider;
                if (meshCollider != null && meshCollider.sharedMesh != null)
                {
                    Mesh mesh = meshCollider.sharedMesh;
                    Vector3[] vertices = mesh.vertices;

                    // Transform vertex to world space
                    Transform hitTransform = hit.collider.transform;
                    Vector3 hitPoint = hit.point;

                    Vector3 closestVertex = Vector3.zero;
                    float minDistance = float.MaxValue;

                    // En yakın vertex'i bul
                    foreach (Vector3 vertex in vertices)
                    {
                        Vector3 worldVertex = hitTransform.TransformPoint(vertex);
                        float distance = Vector3.Distance(worldVertex, hitPoint);
                        if (distance < minDistance)
                        {
                            minDistance = distance;
                            closestVertex = worldVertex;
                        }
                    }

                    // Highlight Sphere'yi en yakın vertex'e taşı ve göster
                    highlightSphere.transform.position = closestVertex;
                    highlightSphere.SetActive(true);

                    if (Input.GetMouseButtonDown(0) && !isOverUI && !firstClick)
                    {  
                        lineRenderer.gameObject.layer = 3;
                        firstPosition = closestVertex;
                        lineRenderer.SetPosition(0, firstPosition);
                        text.gameObject.SetActive(true);
                        Invoke("FirstClick", 0.1f);
                    }

                    if (Input.GetMouseButtonDown(0) && !isOverUI && secondClick)
                    {
                        Invoke("SecondClick", 0.1f);
                    }

                    if (secondClick && !isOverUI)
                    {
                        lastPosition = closestVertex;
                        lineRenderer.SetPosition(1, lastPosition);

                        float x, y, z;

                        x = (float)System.Math.Round(System.Math.Abs(firstPosition.x - lastPosition.x), 2);
                        y = (float)System.Math.Round(System.Math.Abs(firstPosition.y - lastPosition.y), 2);
                        z = (float)System.Math.Round(System.Math.Abs(firstPosition.z - lastPosition.z), 2);

                        float distanceBetween = Vector3.Distance(lastPosition, firstPosition);
                        distanceBetween = (float)System.Math.Round(distanceBetween, 2);

                        text.transform.position = (firstPosition + lastPosition) / 2f;
                        text.transform.rotation = Quaternion.FromToRotation(Vector3.right, (firstPosition - lastPosition).normalized);
                        text.text = "distance: " + distanceBetween + "m\n" + "x: " + x + "m\n" + "y: " + y + "m\n" + "z: " + z + "m\n";
                    }
                }
            }
            else
            {
                highlightSphere.SetActive(false); // Eğer raycast bir şey bulamazsa gizle
            }
            float distanceFromTarget = Vector3.Distance(transform.position, text.transform.position);
            text.transform.localScale = new Vector3(distanceFromTarget / 10, distanceFromTarget / 10, 1);
            Vector3 _direction = (transform.position - text.transform.position).normalized;
            Quaternion _lookRotation = Quaternion.LookRotation(_direction);
            text.transform.rotation = Quaternion.Slerp(transform.rotation, _lookRotation, Time.deltaTime * RotationSpeed);
        }
    }

    void FirstClick()
    {
        secondClick = true;
        firstClick = true;  
    }

    void SecondClick()
    {
        firstClick = false;
        secondClick = false;
        highlightSphere.SetActive(false); // İkinci noktadan sonra highlight'ı gizle
    }
}
