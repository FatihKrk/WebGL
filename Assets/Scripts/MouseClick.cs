using System;
using System.Collections.Generic;
using Battlehub.UIControls;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using System.Collections;
using UnityEngine.EventSystems;


public class MouseClick : MonoBehaviour
{
    [SerializeField] GetAttFromSql getAttFromSql;
    private EventSystem eventSystem;
    public TreeView treeView;
    [SerializeField] SearchBar searchBar;
    TreeViewItem treeViewItem;
    TreeViewExpander treeViewExpander;
    [SerializeField] MoveButtons moveButtons;
    //[SerializeField] Firebase firebase;
    [SerializeField] Hide_n_UnHide hide_N_UnHide;
    [SerializeField] Image hide_Image, hide_Unselected_Image;
    [SerializeField] Camera canvasCam;
    [SerializeField] TMP_Text attributeText, nameText; 
    Ray ray, rayC;
    RaycastHit hit, hitC;
    private float timer = 0.2f , sendBtnTimer;
    private bool doubleClick, doubleClickable, canMove, single, multiple, singleObject, canStartTimer, expanded;
    public bool isOverUI, multiSelect, hideActive, moveUntilArrive;
    public GameObject currentObject, panel, colorChangePanel, attributes_Panel, parentObject;
    [SerializeField] GameObject clickedObject;
    public List<GameObject> clickedItems = new List<GameObject>();
    List<GameObject> objects = new List<GameObject>();
    public Vector3 pozisyon;
    MeshRenderer[] selectedItems;
    
    // Start is called before the first frame update
    void Start()
    {
        parentObject = GameObject.FindGameObjectWithTag("ParentObject");
        MeshRenderer[] allChilds = parentObject.GetComponentsInChildren<MeshRenderer>(true);
        foreach(MeshRenderer mesh in allChilds)
        {
            mesh.gameObject.SetActive(true);
        }

        eventSystem = EventSystem.current;
        Resources.UnloadUnusedAssets();
        GC.Collect();
    }

    // Update is called once per frame
    void Update()
    {
        if(moveUntilArrive)
        {
            Move(3f);
        }
        
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

        //İki kere tıklamayı algılama
        if(doubleClick)
        { 
            if(Input.GetMouseButtonUp(0))
            {
                doubleClickable = true;     
            }           
        }
        if(doubleClickable)
        {
            timer -= Time.deltaTime;

            if(Input.GetMouseButtonDown(0)) 
            {
                canMove = true;
                doubleClick = false;
                doubleClickable = false;
            }

            if(timer <= 0) 
            {
                single = false;
                multiple = false;
                doubleClick = false;
                doubleClickable = false;
            }
        }

        //Hiyararşide tıklanan objeye odaklanma
        if (canMove)
        {
            if(single)
            {
                Move(5f);
            }
            else if(multiple)
            {
                float distance = 0f;
                int length = selectedItems.Length;
                if(length < 10)
                {
                    distance = selectedItems.Length + 5;
                }
                else if(length >= 10 &&  length < 100)
                {
                    distance = 15;
                }
                else if(length >= 100 &&  length < 1000)
                {
                    distance = 30;
                }
                else if(length >= 1000 &&  length < 10000)
                {
                    distance = 50;
                }
                else if(length >= 10000)
                {
                    distance = 80;
                }
                Move(distance);
            }
            else if(singleObject)
            {
                SingleObject();
            }
            else if(!single && !multiple && !singleObject)
            {
                canMove = false;
            }
        }

        /*if(canStartTimer)
        {
            sendBtnTimer -= Time.deltaTime;
            if(sendBtnTimer <= 0)
            {
                nameText.text = "";
                nameText.gameObject.SetActive(false);
                canStartTimer = false;
            }
        }*/

        if(Input.GetKeyDown(KeyCode.LeftArrow))
        {
            MoveInHierarchy();
        }

        if(treeView.canFocus && expanded)
        {
            if (currentObject != null && selectedItems != null)
            {
                ChangeColorBack();
            }
            treeView.SelectedItem = currentObject;
            FindChildrens(currentObject);
            FindPosition();
            ChangeColor();
            ScrollFocus();
            searchBar.loadingPanel.gameObject.SetActive(false);
            objects.Clear();
            treeView.canFocus = false;
            if(attributes_Panel.activeSelf)
            {
                ShowAttributes();
            }
            expanded = false;
        }
    }
    public void MoveInQuery()
    {
        moveUntilArrive = true;
    }

    void PerformHeavyOperations()
    {
        if(eventSystem.enabled)
        {
            isOverUI = UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();

            ray = Camera.main.ScreenPointToRay(Input.mousePosition); 
            
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.CompareTag("RVMObjects") && Input.GetMouseButtonDown(0) && !isOverUI && moveButtons.select)
                {
                    clickedObject = hit.collider.gameObject;
                    MultiObjects();
                } 

                if (hit.collider.CompareTag("RVMObjects") && Input.GetMouseButtonDown(1) && !isOverUI)
                {
                    clickedObject = hit.collider.gameObject;
                    MultiObjects();
                }    

                /*if(hit.collider.CompareTag("RVMObjects") && Input.GetKeyDown(KeyCode.LeftShift) && !isOverUI)
                {
                    objects.Clear();
                    FindParents(hit.transform.gameObject);
                    ShowName();
                }*/

                /*if(hit.collider.tag == null || Input.GetKeyUp(KeyCode.LeftShift))
                {
                    nameText.text = "";
                    nameText.gameObject.SetActive(false);
                }*/
            }
            else
            {
                if(!isOverUI && Input.GetMouseButtonDown(0) && moveButtons.select)
                {
                    ChangeColorBack();
                    selectedItems = parentObject.GetComponentsInChildren<MeshRenderer>();
                    Vector3 plus = new Vector3(0,0,0);

                    FindPosition();
                    treeView.SelectedItem = null;
                }
            }

            if(!isOverUI && Input.GetMouseButtonDown(0))
            {
                panel.SetActive(false);
                colorChangePanel.SetActive(false);
            }

            rayC = canvasCam.ScreenPointToRay(Input.mousePosition);
            
            if(Physics.Raycast(rayC, out hitC))
            {

                if(hitC.collider.CompareTag("Hierarchy") && Input.GetMouseButtonDown(0) && Input.GetKey(KeyCode.LeftShift) || hitC.collider.CompareTag("Hierarchy") && Input.GetMouseButtonDown(0) && Input.GetKey(KeyCode.LeftControl))
                {
                    panel.SetActive(false);
                    colorChangePanel.SetActive(false);
                    multiSelect = true;
                    clickedItems = treeView.m_selectedItems.OfType<GameObject>().ToList();

                    foreach(GameObject clickedItem in clickedItems)
                    {
                        if (clickedItem != null && clickedItem.transform.childCount == 0)
                        {   
                            currentObject = clickedItem;
                            FindPosition();
                            selectedItems = currentObject.GetComponents<MeshRenderer>();
                            ChangeColor();

                            if (!doubleClickable)
                            {
                                timer = 0.2f;
                                single = true;
                                doubleClick = true;
                            }
                        }

                        if(clickedItem != null && clickedItem.transform.childCount > 0)
                        {
                            currentObject = clickedItem;
                                
                            FindChildrens(currentObject);

                            FindPosition();
                                
                            ChangeColor();

                            if (!doubleClickable)
                            {
                                timer = 0.2f;
                                multiple = true;
                                doubleClick = true;
                            }
                        }
                    }   
                }

                //Hiyerarşide tıklanılan objeyi ve childrenlarını gösterme
                else if(hitC.collider.CompareTag("Hierarchy") && Input.GetMouseButtonDown(0))
                {
                    panel.SetActive(false);
                    colorChangePanel.SetActive(false);
                    multiSelect = false;
                    GameObject clickedItem = treeView.SelectedItem as GameObject;

                    if (clickedItem != null && clickedItem.transform.childCount == 0)
                    {
                        if (currentObject != clickedItem)
                        {
                            if (currentObject != null && selectedItems != null)
                            {
                                ChangeColorBack();
                            }    
                            
                            currentObject = clickedItem;
                            FindPosition();
                            selectedItems = currentObject.GetComponents<MeshRenderer>();
                            ChangeColor();
                            if(attributes_Panel.activeSelf)
                            {
                                ShowAttributes();
                            }
                        }

                        if (!doubleClickable)
                        {
                            timer = 0.2f;
                            single = true;
                            doubleClick = true;
                        }
                    }

                    if(clickedItem != null && clickedItem.transform.childCount > 0)
                    {
                        if (currentObject != clickedItem)
                        {
                            if(currentObject != null && selectedItems != null)
                            {
                                ChangeColorBack();
                            }

                            currentObject = clickedItem;
                            FindChildrens(currentObject);

                            FindPosition();
                            
                            ChangeColor();
                            if(attributes_Panel.activeSelf)
                            {
                                ShowAttributes();
                            }
                        }

                        if (!doubleClickable)
                        {
                            timer = 0.2f;
                            multiple = true;
                            doubleClick = true;
                        }
                    }
                }
                
                //Hiyerarşide tıklanılan objenin attributeslarını gösterme
                if(hitC.collider.CompareTag("Hierarchy") && Input.GetMouseButtonDown(1))
                {
                    if(multiSelect)
                    {
                        bool obj_Active = false;
                        foreach(GameObject obj in clickedItems)
                        {
                            if(obj.activeInHierarchy)
                            {
                                obj_Active = true;
                            }
                        }
                        if(obj_Active) hide_Image.gameObject.SetActive(false);
                        else hide_Image.gameObject.SetActive(true);
                    }
                    else if(currentObject.activeInHierarchy)
                    {
                        hide_Image.gameObject.SetActive(false);
                    }
                    else
                    {
                        hide_Image.gameObject.SetActive(true);
                    }
                    hideActive = false;
                    hide_N_UnHide.hide_Unselected_Bool = true;
                    hide_N_UnHide.ControlHideUnSelected();
                    if(hideActive)
                    {
                        hide_Unselected_Image.gameObject.SetActive(false);
                    }
                    else hide_Unselected_Image.gameObject.SetActive(true);

                    panel.SetActive(true);
                    Vector3 mousePos = canvasCam.ScreenToWorldPoint(Input.mousePosition);
                    panel.transform.position = mousePos;
                    panel.transform.localPosition = new Vector3(panel.transform.localPosition.x, panel.transform.localPosition.y, -2.5f);
                    
                }
            }
        }  
    }

    public void MultiObjects()
    {
        if (!doubleClickable)
        {
            timer = 0.2f;
            singleObject = true;
            doubleClick = true;
        }

        if (currentObject != clickedObject)
        {
            if (currentObject != null && selectedItems != null)
            {
                ChangeColorBack();
            }

            panel.SetActive(false);
            currentObject = clickedObject;
            objects.Clear();
            StartCoroutine(HandleMultiObjectFlow(currentObject));
        }
    }

    private IEnumerator HandleMultiObjectFlow(GameObject obj)
    {
        searchBar.loadingPanel.gameObject.SetActive(true);
        yield return StartCoroutine(FindParents(obj));

        currentObject = objects[objects.Count - 6];
        objects.Clear();

        yield return StartCoroutine(FindParents(currentObject));
        yield return StartCoroutine(Expand());

        FindChildrens(currentObject);
        //treeView.SelectedItem = currentObject;
        FindPosition();
        ChangeColor();

        if (attributes_Panel.activeSelf)
        {
            ShowAttributes();
        }
    }



    //Tıklanılan objeyi gösterme
    public void SingleObject()
    {
        if (currentObject != clickedObject)
        {
            if (currentObject != null && selectedItems != null)
            {
                ChangeColorBack();
            }
            panel.SetActive(false);
            currentObject = clickedObject;
            objects.Clear();
            StartCoroutine(FindParents(currentObject));
            FindPosition();
            StartCoroutine(Expand());
            selectedItems = currentObject.GetComponents<MeshRenderer>();
            //treeView.SelectedItem = currentObject;
            ChangeColor();
            singleObject = false;
            if(attributes_Panel.activeSelf)
            {
                ShowAttributes();
            }
        }
    }

    public void FindItemPosition()
    {
        Vector3 pos = new Vector3(0, 0, 0);

        if(currentObject.transform.childCount != 0)
        {
            MeshCollider[] meshColliders = currentObject.GetComponentsInChildren<MeshCollider>();

            foreach(MeshCollider mesh in meshColliders)
            {
                pos += mesh.bounds.center;
            }

            pos /= meshColliders.Length;
            pozisyon = pos;
        }
        else
        {
            pozisyon = currentObject.GetComponent<Collider>().bounds.center;
        }
    }

    void FindPosition()
    {
        if(currentObject.transform.childCount != 0)
        {
            Vector3 plus = new Vector3(0, 0, 0);

            foreach (MeshRenderer pos in selectedItems)
            {
                plus += pos.GetComponentInParent<Collider>().bounds.center;
            }

            plus /= selectedItems.Length;
            pozisyon = plus;
        }
        else
        {
            pozisyon = currentObject.GetComponent<Collider>().bounds.center;
        }
    }

    public void Search()
    {
        objects.Clear();
        StartCoroutine(FindParents(currentObject));
        StartCoroutine(Expand());
    }

    //Çift tıklanıldığında kamerayı objeye yakınlaştırma
    public void Move(float distance)
    {
        Vector3 targetDirection = pozisyon - transform.position;
        float moveSpeed = 1000f * Time.deltaTime;
        float lookSpeed = 100000f * Time.deltaTime;
        float distanceToTarget = Vector3.Distance(transform.position, pozisyon);

        if (distanceToTarget < distance)
        {
            transform.position += new Vector3(-distance+5, 0, -distance+5);
        }

        Vector3 newDirection = Vector3.RotateTowards(transform.forward, targetDirection.normalized, lookSpeed * Mathf.Deg2Rad, 0.0f);
        transform.rotation = Quaternion.LookRotation(newDirection);

        transform.position = Vector3.MoveTowards(transform.position, pozisyon, moveSpeed);
        distanceToTarget = Vector3.Distance(transform.position, pozisyon);

        if (distanceToTarget <= distance)
        {
            moveUntilArrive = false;
            single = false;
            multiple = false;
        }        
    }


    //Rengi değiştirilecek childiren'ları bulma
    void FindChildrens(GameObject parentObject)
    {
        selectedItems = parentObject.GetComponentsInChildren<MeshRenderer>();
    }

    //Tıklanılan objenin parent'larını bulma
    public IEnumerator FindParents(GameObject expandingObject)
    {
        while (expandingObject.transform.parent != null)
        {
            expandingObject = expandingObject.transform.parent.gameObject;
            objects.Add(expandingObject);
            yield return null;
        }
        if(expandingObject.transform.parent == null) objects.Add(expandingObject);
    }

    Dictionary<Renderer, Color> originalColors = new Dictionary<Renderer, Color>();
    Dictionary<Renderer, Color> visualQueryColors = new Dictionary<Renderer, Color>();
    // Materyalleri değiştir
    public void ChangeColor()
    {
        HashSet<Renderer> processedRenderers = new HashSet<Renderer>();
        MaterialPropertyBlock block = new MaterialPropertyBlock();
        if(moveButtons.visualQuery)
        {
            foreach (Renderer renderer in selectedItems)
            {
                renderer.GetPropertyBlock(block);
                if (!visualQueryColors.ContainsKey(renderer))
                {
                    visualQueryColors[renderer] = block.GetColor("_Color");
                }
                block.SetColor("_Color", Color.blue);
                renderer.SetPropertyBlock(block);
                processedRenderers.Add(renderer);
            }
        }
        else
        {
            foreach (Renderer renderer in selectedItems)
            {
                renderer.GetPropertyBlock(block);
                if (!originalColors.ContainsKey(renderer))
                {
                    originalColors[renderer] = renderer.material.color;
                }
                block.SetColor("_Color", Color.blue);
                renderer.SetPropertyBlock(block);
                processedRenderers.Add(renderer);
            }
        }
    }

    // Materyalleri eski haline döndür
    public void ChangeColorBack()
    {
        MaterialPropertyBlock block = new MaterialPropertyBlock();
        HashSet<Renderer> processedRenderers = new HashSet<Renderer>();
        if (clickedItems != null)
        {
            foreach (GameObject obj in clickedItems)
            {
                // FindChildrens fonksiyonunu çağır
                FindChildrens(obj);
            }
        }
        if(moveButtons.visualQuery)
        {
            foreach (var entry in visualQueryColors)
            {
                Renderer item = entry.Key;
                Color originalColor = entry.Value;

                MeshRenderer[] renderers = item.GetComponentsInChildren<MeshRenderer>(true);
                foreach (var renderer in renderers)
                {
                    renderer.GetPropertyBlock(block);
                    if (!processedRenderers.Contains(renderer))
                    {
                        processedRenderers.Add(renderer);
                    }
                    block.SetColor("_Color", originalColor);
                    renderer.SetPropertyBlock(block);
                }
            }
        }
        else
        {
            foreach (var entry in originalColors)
            {
                Renderer item = entry.Key;
                Color originalColor = entry.Value;

                MeshRenderer[] renderers = item.GetComponentsInChildren<MeshRenderer>(true);
                foreach (var renderer in renderers)
                {
                    renderer.GetPropertyBlock(block);
                    if (!processedRenderers.Contains(renderer))
                    {
                        processedRenderers.Add(renderer);
                    }
                    block.SetColor("_Color", originalColor);
                    renderer.SetPropertyBlock(block);
                }
            }
        }
    }

    //Objeye tıklandığında hiyerarşide sayfaların açılması
    public IEnumerator Expand()
    {
        searchBar.loadingPanel.gameObject.SetActive(true);
        searchBar.text.text = "LOADING ...";
        bool isOn = false;
        for (int i = objects.Count - 1; i >= 0; i--)
        {
            treeViewItem = treeView.GetTreeViewItem(objects[i]);
            treeViewExpander = treeViewItem.GetComponentInChildren<TreeViewExpander>();
            isOn = treeViewExpander.IsOn;
            if(!treeViewExpander.IsOn)
            {
                treeViewExpander.IsOn = true;
                // Her bir öğe için bir kare bekle, işlemi zamana yay
                yield return null;
            }
        }
        treeView.SelectedItem = currentObject;
        if(isOn) treeView.canFocus = true;
        expanded = true;
    }

    //Objeye tıklandığında scroll'un odaklanması
    void ScrollFocus()
    {
        GameObject[] itemler = GameObject.FindGameObjectsWithTag("ItemContainer");
        int a = treeView.IndexOf(currentObject);
        int b = itemler.Length;

        float c = Mathf.Clamp01(1f - (float)a / b);
        treeView.scrollRect.verticalNormalizedPosition = c;
    }

    //Shift'e basıldığında objenin isminin gözükmesi
    /*void ShowName()
    {
        nameText.transform.localScale = new Vector3(1,1,1);
        Vector3 mousePosition = Input.mousePosition;
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, Camera.main.nearClipPlane + 1f));

        nameText.gameObject.SetActive(true);
        nameText.transform.position = worldPosition;

        string objectName = clickedObject.name;
        nameText.text = objectName;
    }*/

    /*public void SendMasterTag()
    {
        nameText.transform.localScale = new Vector3(2f,1.5f,1);
        nameText.transform.localPosition = new Vector3(0,0,0);
        nameText.gameObject.SetActive(true);
        nameText.text = "Master tag has been send";
        
        firebase.SendMasterTag(value);
        sendBtn.SetActive(false);

        sendBtnTimer = 2f;
        canStartTimer = true;
    }*/

    void MoveInHierarchy()
    {
        if(currentObject.transform.parent)
        {
            clickedObject = currentObject.transform.parent.gameObject;
            currentObject = currentObject.transform.parent.gameObject;
            Invoke("ScrollFocus", 0.05f);
            FindChildrens(currentObject);
            treeView.SelectedItem = currentObject;
            ChangeColor();
        }
    }
    public void ShowAttributes()
    {
        // Paneli görünür yap
        attributes_Panel.SetActive(true);

        // Veriyi çekmek için GetAttByName'i çağır ve callback fonksiyonunu kullan
        getAttFromSql.GetAttByName(currentObject.name, (att) =>
        {
            if (att != null)
            {
                // Veri geldiyse attributeText'i güncelle
                attributeText.text = att;
            }
            else
            {
                // Hata durumu
                attributeText.text = "Veri alınamadı.";
            }
        });
    }

}
