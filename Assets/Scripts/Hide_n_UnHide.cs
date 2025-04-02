using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Battlehub.UIControls;
using UnityEngine;
using UnityEngine.UI;


public class Hide_n_UnHide : MonoBehaviour
{
    [SerializeField] MouseClick mouseClick;
    Transform firstParent;
    public bool hide_Unselected_Bool;

    void Start()
    {
        firstParent = GameObject.FindGameObjectWithTag("ParentObject").transform;
    }

    void Hide()
    {
        if(mouseClick.multiSelect)
        {
            foreach(GameObject obj in mouseClick.clickedItems)
            {
                obj.SetActive(false);
            }
            mouseClick.panel.SetActive(false);
            ChangeColor();
        }
        else 
        {
            mouseClick.currentObject.SetActive(false);
            mouseClick.panel.SetActive(false);
            ChangeColor();
        }
    }

    void UnHide()
    {
        if(mouseClick.multiSelect)
        {
            foreach(GameObject obj in mouseClick.clickedItems)
            {
                obj.SetActive(true);
            }
            mouseClick.panel.SetActive(false);
            ChangeColor();
        }
        else
        {
            mouseClick.currentObject.SetActive(true);
            mouseClick.panel.SetActive(false);
            ChangeColor();
        }
    }

    void Hide_UnSelected()
    {
        if (mouseClick.multiSelect)
        {
            foreach (GameObject clickedItem in mouseClick.clickedItems)
            {
                List<GameObject> parents = FindParents(clickedItem);

                foreach (GameObject parent in parents)
                {
                    Transform parentTransform = parent.transform.parent;
                    if (parentTransform != null)
                    {
                        int siblingCount = parentTransform.childCount;
                        for (int i = 0; i < siblingCount; i++)
                        {
                            GameObject sibling = parentTransform.GetChild(i).gameObject;
                            if (!IsClickedOrParentActive(sibling))
                            {
                                sibling.SetActive(false);
                            }
                        }
                    }
                }
            }
            mouseClick.panel.SetActive(false);
            ChangeColor();
        }
        else
        {
            GameObject currentObject = mouseClick.currentObject;
            Transform parentTransform = currentObject.transform.parent;

            int siblingCount;

            List<GameObject> parents = FindParents(currentObject);

            foreach (GameObject parent in parents)
            {
                parentTransform = parent.transform.parent;
                if (parentTransform != null)
                {
                    siblingCount = parentTransform.childCount;
                    for (int i = 0; i < siblingCount; i++)
                    {
                        GameObject sibling = parentTransform.GetChild(i).gameObject;
                        if (parents.Contains(sibling)) continue;
                        sibling.SetActive(false);
                    }
                }
            }
            
            mouseClick.panel.SetActive(false);
            ChangeColor();
        }
    }

    void Un_Hide_UnSelected()
    {
        GameObject currentObject;
        Transform parentTransform;
        if (mouseClick.multiSelect)
    {
        foreach (GameObject clickedItem in mouseClick.clickedItems)
        {
            List<GameObject> parents = FindParents(clickedItem);

            foreach (GameObject parent in parents)
            {
                parentTransform = parent.transform.parent;
                if (parentTransform != null)
                {
                    int siblingCount = parentTransform.childCount;
                    for (int i = 0; i < siblingCount; i++)
                    {
                        parentTransform.GetChild(i).gameObject.SetActive(true);
                    }
                }
            }
        }

        mouseClick.panel.SetActive(false);
        ChangeColor();
    }
        else
        {
            currentObject = mouseClick.currentObject;
            
            int siblingCount;

            List<GameObject> parents = FindParents(currentObject);

            foreach (GameObject parent in parents)
            {
                parentTransform = parent.transform.parent;
                if (parentTransform != null)
                {
                    siblingCount = parentTransform.childCount;
                    for (int i = 0; i < siblingCount; i++)
                    {
                        GameObject sibling = parentTransform.GetChild(i).gameObject;
                        if (parents.Contains(sibling)) continue;
                        sibling.SetActive(true);
                    }
                }
            }

            mouseClick.panel.SetActive(false);
            ChangeColor();
        }
    }

    public void UnHideAll()
    {
        Queue<Transform> queue = new Queue<Transform>();
        queue.Enqueue(firstParent);
        while (queue.Count > 0)
        {
            var c = queue.Dequeue();
            if(!c.gameObject.activeInHierarchy)
            {
                c.gameObject.SetActive(true);
            }
            foreach(Transform t in c)
                queue.Enqueue(t);
        }
        ChangeColor();
    }

    List<GameObject> FindParents(GameObject child)
    {
        List<GameObject> parents = new List<GameObject>();
        while (child.transform.parent != null)
        {   
            parents.Add(child);
            child = child.transform.parent.gameObject;   
        }
        return parents;
    }

    public void ControlHide()
    {
        if(mouseClick.multiSelect)
        {
            bool obj_Active = false;
            foreach(GameObject obj in mouseClick.clickedItems)
            {
                if(obj.activeInHierarchy)
                {
                    obj_Active = true;
                }
            }
            if(obj_Active) Hide();
            else UnHide();
        }
        else if(mouseClick.currentObject.activeInHierarchy)
        {
            Hide();
        }
        else
        {
            UnHide();
        }
    }
    public void BoolChange()
    {
        hide_Unselected_Bool = false;
    }

    public void ControlHideUnSelected()
    {
        GameObject currentObject = mouseClick.currentObject;
        Transform parentTransform;
        List<GameObject> parents = new List<GameObject>();
        parents = FindParents(currentObject);
        bool anyActiveSibling = false;
        bool anyActiveUnselected = false;

        if (mouseClick.multiSelect)
        {
            foreach(GameObject parent in parents)
            {
                parentTransform = parent.transform.parent;
            
                int siblingCount = parentTransform != null ? parentTransform.childCount : 0;

                for (int i = 0; i < siblingCount; i++)
                {
                    if (i == currentObject.transform.GetSiblingIndex()) {
                        continue; // Skip the current object
                    }

                    GameObject sibling = parentTransform.GetChild(i).gameObject;
                    if (sibling.activeInHierarchy && !IsClickedOrParentActive(sibling))
                    {
                        mouseClick.hideActive = true;
                        anyActiveUnselected = true;
                        break; // Exit loop if an active unselected sibling is found
                    }
                }
                
            }

            if(!hide_Unselected_Bool)
            {
                if (anyActiveUnselected)
                {
                     Hide_UnSelected();
                }
                else
                {
                    Un_Hide_UnSelected();
                }
            }
        }
        else
        {
            foreach(GameObject parent in parents)
            {
                parentTransform = parent.transform.parent;

                int siblingCount = parentTransform != null ? parentTransform.childCount : 0;

                for (int i = 0; i < siblingCount; i++)
                {
                    if (i == parent.transform.GetSiblingIndex()) {
                        continue; // Skip the current object
                    }

                    GameObject sibling = parentTransform.GetChild(i).gameObject;
                    if (sibling.activeInHierarchy)
                    {
                        mouseClick.hideActive = true;
                        anyActiveSibling = true;
                        break; // Exit loop if an active sibling is found
                    }
                }
  
            }

            if(!hide_Unselected_Bool)
            {
                if (anyActiveSibling)
                {
                    Hide_UnSelected();
                }
                else
                {
                    Un_Hide_UnSelected();
                }
            }
        }
    }

    // Helper function to check if a GameObject or its parents are clicked
    bool IsClickedOrParentActive(GameObject obj)
    {
        if (mouseClick.clickedItems.Contains(obj)) {
            return true;
        }
        foreach(GameObject gameObj in mouseClick.clickedItems)
        {
            List<GameObject> objParents = FindParents(gameObj);
            if(objParents.Contains(obj)) return true;
        }

        return false;
    }

    void ChangeColor()
    {
        // Get all ItemContainers
        GameObject[] itemContainers = GameObject.FindGameObjectsWithTag("ItemContainer");

        Image[] images;
        Text text;

        // Collect all texts and images
        foreach (GameObject container in itemContainers)
        {
            images = container.GetComponentsInChildren<Image>();
            text = container.GetComponentInChildren<Text>();

            if (text != null)
            {
                string textString = text.text.Trim();
                if(FindDeepChild(textString))
                {
                    Transform someChild = FindDeepChild(textString);
                    UpdateColor(someChild.gameObject.activeInHierarchy);
                }
                
            }
        }

        // Function to update color based on active state
        void UpdateColor(bool isActive)
        {
                Color color = isActive ? Color.white : Color.gray;
                text.color = color;
                if (images[images.Length-1] != null)
                {
                    images[images.Length-1].color = color;
                }
        }
    }

    Transform FindDeepChild(string aName)
    {
        Queue<Transform> queue = new Queue<Transform>();
        queue.Enqueue(firstParent);
        while (queue.Count > 0)
        {
            var c = queue.Dequeue();
            if (c.name == aName)
                return c;
            foreach(Transform t in c)
                queue.Enqueue(t);
        }
        return null;
    }  

}
