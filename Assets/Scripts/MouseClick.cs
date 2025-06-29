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
    public GameObject currentObject, panel, colorChangePanel, attributes_Panel, parentObject, clickedObject;
    public List<GameObject> clickedItems = new List<GameObject>();
    List<GameObject> objects = new List<GameObject>();
    public Vector3 pozisyon;
    MeshRenderer[] selectedItems;
    [SerializeField] private VisualQueryManager visualQueryManager;
    
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
                if(!isOverUI && Input.GetMouseButtonDown(0) && moveButtons.select && !moveButtons.section && currentObject != null)
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

        currentObject = objects[1];
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
            StartCoroutine(SingleObjectRoutine());
            selectedItems = currentObject.GetComponents<MeshRenderer>();
            
            singleObject = false;

            if (attributes_Panel.activeSelf)
            {
                ShowAttributes();
            }
        }
    }

    private IEnumerator SingleObjectRoutine()
    {
        yield return StartCoroutine(FindParents(currentObject));
        yield return StartCoroutine(Expand());
        FindPosition();
        ChangeColor();
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

    public IEnumerator Search()
    {
        objects.Clear();
        yield return StartCoroutine(FindParents(currentObject));
        FindPosition();
        yield return StartCoroutine(Expand());
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
    public void FindChildrens(GameObject parentObject)
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
    private Dictionary<Renderer, MaterialPropertyBlock> originalBlocks = new Dictionary<Renderer, MaterialPropertyBlock>();
    private Dictionary<Renderer, Color> originalColors = new Dictionary<Renderer, Color>();

    public void ChangeColor()
    {
        if (selectedItems == null || selectedItems.Length == 0) return;

        var highlightBlock = new MaterialPropertyBlock();
        highlightBlock.SetFloat("_UseOverrideColor", 1f);
        highlightBlock.SetColor("_OverrideColor", Color.blue);

        foreach (Renderer renderer in selectedItems)
        {
            // VisualQueryManager'ın kontrolü altındaysa işlem yapma
            if (moveButtons.visualQuery && visualQueryManager != null && 
                visualQueryManager.originalBlocks.ContainsKey(renderer))
            {
                renderer.SetPropertyBlock(highlightBlock);
            }

            // Orijinal değerleri kaydet
            if (!originalBlocks.ContainsKey(renderer))
            {
                var block = new MaterialPropertyBlock();
                renderer.GetPropertyBlock(block);
                originalBlocks[renderer] = block;
            }
            if (!originalColors.ContainsKey(renderer))
            {
                originalColors[renderer] = renderer.sharedMaterial.color;
            }

            renderer.SetPropertyBlock(highlightBlock);
        }
    }
    private Color grayColor = new Color(Color.gray.r, Color.gray.g, Color.gray.b, 0.60f);
    private MaterialPropertyBlock grayBlock;

    public void ChangeColorBack()
    {
        if (selectedItems == null || selectedItems.Length == 0) return;

        foreach (Renderer renderer in selectedItems)
        {
            if (moveButtons.visualQuery && visualQueryManager != null)
            {
                if (visualQueryManager.changedBlocks.ContainsKey(renderer))
                {
                    var visualQueryBlock = visualQueryManager.changedBlocks[renderer];
                    renderer.SetPropertyBlock(visualQueryBlock);
                }
                else
                {
                    grayBlock = new MaterialPropertyBlock();
                    grayBlock.SetFloat("_UseOverrideColor", 1f);
                    grayBlock.SetColor("_OverrideColor", grayColor);
                    renderer.SetPropertyBlock(grayBlock);
                }
            }
            else if (originalBlocks.TryGetValue(renderer, out var originalBlock))
            {
                renderer.SetPropertyBlock(originalBlock);
            }
        }

        selectedItems = null;
    }

    public IEnumerator Expand()
    {
        searchBar.loadingPanel.gameObject.SetActive(true);
        bool isOn = false;

        for (int i = objects.Count - 1; i >= 0; i--)
        {
            if (i >= objects.Count || i < 0)
            yield break;
            TreeViewItem item = null;

            // TreeViewItem oluşana kadar bekle
            yield return new WaitUntil(() =>
            {
                item = treeView.GetTreeViewItem(objects[i]);
                return item != null;
            });

            TreeViewExpander expander = null;

            // Expander oluşana kadar bekle (eğer property ya da field varsa)
            yield return new WaitUntil(() =>
            {
                expander = item.GetComponentInChildren<TreeViewExpander>(); // örnek: field ya da property
                return expander != null;
            });

            treeViewItem = item;
            treeViewExpander = expander;
            isOn = treeViewExpander.IsOn;

            if (!treeViewExpander.IsOn)
            {
                treeViewExpander.IsOn = true;
                yield return null; // UI güncellensin
            }
        }
        treeView.SelectedItem = currentObject;
        if (isOn) treeView.canFocus = true;
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
