using System;
using UnityEngine;
using UnityEngine.UI;

public class ClippingController : MonoBehaviour, ICanvasAware
{
    [SerializeField] MoveButtons moveButtons;
    [SerializeField] MouseClick mouseClick;
    [SerializeField] Camera cam;
    [SerializeField] Material cubeMaterial;
    [SerializeField] Image moveT, scaleT;
    Transform parentObject;
    public LayerMask topLayer;
    public Material clippingMaterial;
    private Transform targetObject;     // Hacmi otomatik olarak yerleştirmek istediğiniz obje
    private Vector3 boundsSize, clippingPosition; // Clipping hacminin boyutu
    private Vector3 initialScale, lastWorldPosition, currentWorldPosition;
    private Vector3 initialMousePos;
    private bool isScaling = false, isMoving = false;
    public bool isOverTopLayer;
    private enum ScaleAxis { None, X, Y, Z, ScaleAllAxes }
    private ScaleAxis activeAxis = ScaleAxis.None;
    public Collider centerHandle;
    public Transform player;
    Transform scaleObj;
    public Transform moveObj;
    private GameObject cube, sectionObject; // Küp nesnesi
    private Vector3 lastBound; // Son güncellenen bound
    public bool isScale, isMove = true;
    MeshRenderer[] renderers;

    public void OnCanvasChanged(GameObject activeCanvas)
    {
        var bottom = activeCanvas.transform.Find("Bottompanel");
        if (bottom != null) moveButtons = bottom.GetComponent<MoveButtons>();

        var cameraObj = GameObject.Find("Main Camera");
        if (cameraObj != null) cam = cameraObj.GetComponent<Camera>();
    }

    void Start()
    {
        scaleObj = centerHandle.transform;
        parentObject = GameObject.FindGameObjectWithTag("ParentObject").transform;
        // Küp oluşturma
        cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.parent = transform; // Küpü bu nesneye bağla
        cube.layer = LayerMask.NameToLayer("Ignore Raycast");
        cube.GetComponent<Renderer>().material = cubeMaterial;
        UpdateCubeScale(); // İlk ölçek ayarlaması

        renderers = parentObject.GetComponentsInChildren<MeshRenderer>(true);
    }

    void Update()
    {

        if (System.GC.GetTotalMemory(false) <= 2 * 1024f * 1024 * 1024)
        {
            // Bellek sınırı aşılmamışsa işlemleri sürdür
            PerformHeavyOperations();
        }
        else
        {
            Resources.UnloadUnusedAssets();
            GC.Collect();
        }
    }

    void PerformHeavyOperations()
    {

        if(isScale && !isMove)
        {
            moveObj.gameObject.SetActive(false);
            scaleObj.gameObject.SetActive(true);
            scaleT.color = new Color(0, 0, 0, 0.39f);
            moveT.color = new Color(255, 255, 255, 0.39f);
            scaleObj.position = clippingPosition;
            float distanceFromTarget = Vector3.Distance(player.position, scaleObj.position);
            scaleObj.localScale = new Vector3(distanceFromTarget / 40, distanceFromTarget / 40, distanceFromTarget / 40);
        }
        else
        {
            scaleObj.gameObject.SetActive(false);
            moveObj.gameObject.SetActive(true);
            moveT.color = new Color(0, 0, 0, 0.39f);
            scaleT.color = new Color(255, 255, 255, 0.39f);
            moveObj.position = clippingPosition;
            float distanceFromTarget = Vector3.Distance(player.position, moveObj.position);
            moveObj.localScale = new Vector3(distanceFromTarget / 40, distanceFromTarget / 40, distanceFromTarget / 40);
        }
        if (!moveButtons.section)
        {
            Shader.SetGlobalVector("_Bound", new Vector4(1000000, 1000000, 1000000, 1));
            scaleObj.gameObject.SetActive(false);
            moveObj.gameObject.SetActive(false);
            cube.SetActive(false); // Küpü gizle
        }

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // Fare tıklaması ile tutamaçları kontrol et
        if (Input.GetMouseButtonDown(0) && Physics.Raycast(ray, out hit, Mathf.Infinity, topLayer))
        {
            
            lastWorldPosition = cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, cam.nearClipPlane));
            isOverTopLayer = true;

            if (isMove && hit.collider.tag == "xHandle")
            {
                StartScaling(ScaleAxis.X);
            }
            else if (isMove && hit.collider.tag == "yHandle")
            {
                StartScaling(ScaleAxis.Y);
            }
            else if (isMove && hit.collider.tag == "zHandle")
            {
                StartScaling(ScaleAxis.Z);
            }

            // X, Y veya Z tutamacına tıklanmışsa o eksende ölçekleme yapılacak
            if (isScale && hit.collider.tag == "xHandle")
            {
                StartScaling(ScaleAxis.X);
            }
            else if (isScale && hit.collider.tag == "yHandle")
            {
                StartScaling(ScaleAxis.Y);
            }
            else if (isScale && hit.collider.tag == "zHandle")
            {
                StartScaling(ScaleAxis.Z);
            }
            // Orta objeye tıklandığında, tüm eksenlerde eşit şekilde ölçekleme yapılacak
            else if (isScale && hit.collider == centerHandle)
            {
                StartScaling(ScaleAxis.ScaleAllAxes);
            }
        }
        else isOverTopLayer = false;

        // Fare sürüklendiğinde ölçekleme işlemini devam ettir
        if (isScaling && Input.GetMouseButton(0) || isMoving && Input.GetMouseButton(0))
        {
            if(isScale && !isMove)
            {
                // Mevcut mouse pozisyonunu al
                Vector3 currentMousePosition = Input.mousePosition;

                // Dünya koordinatındaki mouse pozisyonunu hesapla
                Vector3 currentWorldPosition = cam.ScreenToWorldPoint(new Vector3(currentMousePosition.x, currentMousePosition.y, cam.nearClipPlane));
                initialScale = Shader.GetGlobalVector("_Bound");
                Vector4 newBound = new Vector4(initialScale.x, initialScale.y, initialScale.z, 1);
                
                // Seçilen eksene göre clipping bound güncellemesi yapılır
                if (activeAxis == ScaleAxis.X)
                {
                    // X eksenindeki farkı hesapla
                    float deltaX = currentWorldPosition.x - lastWorldPosition.x;
                    newBound.x = initialScale.x + deltaX * 100; // X ekseninde akışkan hareket
                }
                else if (activeAxis == ScaleAxis.Y)
                {
                    // Y eksenindeki farkı hesapla
                    float deltaY = currentWorldPosition.y - lastWorldPosition.y;
                    newBound.y = initialScale.y + deltaY * 100; // Y ekseninde akışkan hareket
                }
                else if (activeAxis == ScaleAxis.Z)
                {
                    // Z eksenindeki farkı hesapla
                    float deltaZ = currentWorldPosition.z - lastWorldPosition.z;
                    newBound.z = initialScale.z + deltaZ * 100; // Z ekseninde akışkan hareket
                }
                else if (activeAxis == ScaleAxis.ScaleAllAxes)
                {
                    // X, Y ve Z eksenlerindeki farkları hesapla
                    float deltaX = currentWorldPosition.x - lastWorldPosition.x;
                    float deltaY = currentWorldPosition.y - lastWorldPosition.y;
                    float deltaZ = currentWorldPosition.z - lastWorldPosition.z;

                    float averageDelta = (deltaX + deltaY + deltaZ) / 3.0f;

                    // Tüm eksenlerde bu farkı uygula
                    float scaleFactor = averageDelta * 100; // Hassasiyeti kontrol eden bir çarpan
                    newBound.x = initialScale.x + scaleFactor;
                    newBound.y = initialScale.y + scaleFactor;
                    newBound.z = initialScale.z + scaleFactor;
                }
                // Clipping materyaline bu yeni bound değeri atanır
                Shader.SetGlobalVector("_Bound", newBound);
                UpdateCubeScale(newBound); // Küpün ölçeğini güncelle
                UpdateRendererLayerBasedOnScale();
                lastWorldPosition = currentWorldPosition;
            }
            else
            {
                // Mevcut mouse pozisyonunu al
                Vector3 currentMousePosition = Input.mousePosition;

                // Dünya koordinatındaki mouse pozisyonunu hesapla
                Vector3 currentWorldPosition = cam.ScreenToWorldPoint(new Vector3(currentMousePosition.x, currentMousePosition.y, cam.nearClipPlane));
                clippingPosition = Shader.GetGlobalVector("_ClippingPosition");
                Vector4 newBound = new Vector4(clippingPosition.x, clippingPosition.y, clippingPosition.z, 1);
                
                // Seçilen eksene göre clipping bound güncellemesi yapılır
                if (activeAxis == ScaleAxis.X)
                {
                    // X eksenindeki farkı hesapla
                    float deltaX = currentWorldPosition.x - lastWorldPosition.x;
                    newBound.x = clippingPosition.x + deltaX * 100; // X ekseninde akışkan hareket
                }
                else if (activeAxis == ScaleAxis.Y)
                {
                    // Y eksenindeki farkı hesapla
                    float deltaY = currentWorldPosition.y - lastWorldPosition.y;
                    newBound.y = clippingPosition.y + deltaY * 100; // Y ekseninde akışkan hareket
                }
                else if (activeAxis == ScaleAxis.Z)
                {
                    // Z eksenindeki farkı hesapla
                    float deltaZ = currentWorldPosition.z - lastWorldPosition.z;
                    newBound.z = clippingPosition.z + deltaZ * 100; // Z ekseninde akışkan hareket
                }
                // Yeni clipping pozisyonunu güncelle
                Shader.SetGlobalVector("_ClippingPosition", newBound);
                cube.transform.position = Shader.GetGlobalVector("_ClippingPosition");

                // Dünya pozisyonunu bir sonraki frame için güncelle
                lastWorldPosition = currentWorldPosition;
            }
            
        }

        // Fareyi bıraktığında ölçekleme işlemini durdur
        if (Input.GetMouseButtonUp(0))
        {
            isScaling = false;
            activeAxis = ScaleAxis.None;
        }
    }

    public void Scale()
    {
        if(isScale && cube.activeSelf) cube.SetActive(false);
        else cube.SetActive(true);
        isMove = false;
        isScale = true;
    }
    public void Move()
    {
        if(isMove && cube.activeSelf) cube.SetActive(false);
        else cube.SetActive(true);
        isScale = false;
        isMove = true;
    }
    private void StartScaling(ScaleAxis axis)
    {
        isScaling = true;
        activeAxis = axis;
        initialScale = Shader.GetGlobalVector("_Bound");
        clippingPosition = Shader.GetGlobalVector("_ClippingPosition");
        initialMousePos = Input.mousePosition;
    }

    private void StartMoving(ScaleAxis axis)
    {
        isMoving = true;
        activeAxis = axis;
        clippingPosition = Shader.GetGlobalVector("_ClippingPosition");
        initialMousePos = Input.mousePosition;
    }

    public void Sectioning()
    {
        cube.SetActive(true);
        targetObject = mouseClick.currentObject.transform;
        // Hedef objenin etrafına clipping hacmini yerleştirmek için objenin pozisyonunu al
        if (targetObject != null)
        {
            MeshRenderer objRenderer = targetObject.GetComponent<MeshRenderer>();
            if (objRenderer != null)
            {
                clippingPosition = objRenderer.bounds.center;
                Shader.SetGlobalVector("_ClippingPosition", new Vector4(clippingPosition.x, clippingPosition.y, clippingPosition.z, 1));
                // Hacim boyutunu ayarla
                Shader.SetGlobalVector("_Bound", new Vector4(objRenderer.bounds.size.x + 0.0001f, objRenderer.bounds.size.y + 0.0001f, objRenderer.bounds.size.z + 0.0001f, 1));
                UpdateCubeScale(); // Küpün ölçeğini güncelle
                UpdateRendererLayerBasedOnScale();
            }
            else
            {
                // Bounds hesaplama
                Bounds totalBounds = CalculateTotalBounds(targetObject);

                // Clipping pozisyonunu ayarlama
                clippingPosition = totalBounds.center;
                centerHandle.transform.position = clippingPosition;
                Shader.SetGlobalVector("_ClippingPosition", new Vector4(clippingPosition.x, clippingPosition.y, clippingPosition.z, 1));

                // Clipping hacmi boyutunu ayarlama
                Shader.SetGlobalVector("_Bound", new Vector4(totalBounds.size.x + 0.0001f, totalBounds.size.y + 0.0001f, totalBounds.size.z + 0.0001f, 1));
                UpdateCubeScale(); // Küpün ölçeğini güncelle
                UpdateRendererLayerBasedOnScale();
            }
        }
        
    }

    Bounds CalculateTotalBounds(Transform obj)
    {
        Bounds totalBounds = new Bounds();
        MeshRenderer[] meshRenderers = obj.GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer meshRenderer in meshRenderers)
        {
            if (meshRenderer != null)
            {
                // Çocuğun bounds'ını al
                Bounds childBounds = meshRenderer.bounds;

                // Eğer totalBounds boşsa, ilk çocuğun bounds'ını ata
                if (totalBounds.size == Vector3.zero)
                {
                    totalBounds = childBounds;
                }
                else
                {
                    // Çocuğun bounds'ını toplam bounds'a ekle
                    totalBounds.Encapsulate(childBounds);
                }
            }
        }

        return totalBounds;
    }

    // Küpün ölçeğini güncelleme metodu
    private void UpdateCubeScale(Vector4? newBound = null)
    {
        if (newBound.HasValue)
        {
            lastBound = newBound.Value; // Yeni bound değerini kaydet
        }
        else
        {
            lastBound = Shader.GetGlobalVector("_Bound"); // Mevcut bound değerini al
        }

        // Küpün ölçeğini ayarla
        cube.transform.localScale = new Vector3(lastBound.x, lastBound.y, lastBound.z);
        cube.transform.position = clippingPosition; // Küpü clipping pozisyonuna yerleştir
    }

    void UpdateRendererLayerBasedOnScale()
    {
        // Küpün Scale'ını al
        Vector3 cubeScale = cube.transform.localScale;

        foreach (Renderer renderer in renderers)
        {
            // Sadece küpün dışında olanları kontrol et
            if (renderer.gameObject != cube && !IsWithinCubeScale(renderer))
            {
                renderer.gameObject.SetActive(false);
            }
            else
            {
                renderer.gameObject.SetActive(true);
            }
        }
    }

    bool IsWithinCubeScale(Renderer renderer)
    {
        // Küpün pozisyonu ve ölçeğini alıyoruz
        Vector3 cubePosition = cube.transform.position;
        Vector3 cubeScale = cube.transform.localScale;

        // Renderer objesinin dünya uzayındaki en küçük ve en büyük noktalarını alıyoruz
        Vector3 rendererMin = renderer.bounds.min;
        Vector3 rendererMax = renderer.bounds.max;

        // Küpün sınırlarını hesaplıyoruz (pozisyon ve scale'i dikkate alarak)
        Vector3 cubeMin = cubePosition - (cubeScale / 2f);
        Vector3 cubeMax = cubePosition + (cubeScale / 2f);

        // Objeyi, küpün içinde olup olmadığını kontrol ediyoruz
        bool withinX = rendererMax.x >= cubeMin.x && rendererMin.x <= cubeMax.x;
        bool withinY = rendererMax.y >= cubeMin.y && rendererMin.y <= cubeMax.y;
        bool withinZ = rendererMax.z >= cubeMin.z && rendererMin.z <= cubeMax.z;

        return withinX && withinY && withinZ;
    }

    public void ChangeDisabled()
    {
        foreach (Renderer renderer in renderers)
        {
            renderer.gameObject.SetActive(true);
        }
    }
}