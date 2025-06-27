using System;
using System.Collections;
using System.Collections.Generic;
using Battlehub.UIControls;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SearchBar : MonoBehaviour
{
    [SerializeField] MouseClick mouseClick;
    [SerializeField] TMP_InputField inputField;
    [SerializeField] TreeViewDemo treeViewDemo;
    Transform first_Parent;
    public Transform loadingPanel;
    List<Transform> searchedObjects = new List<Transform>();
    bool shouldSearch = false;
    int objQueue;
    string searched_Text, lastText;


    void Start()
    {
        first_Parent = GameObject.FindGameObjectWithTag("ParentObject").transform;

    }
    void Update()
    {
        if (inputField.isFocused && Input.anyKeyDown)
        {
            searched_Text = inputField.text.ToLower();
            shouldSearch = true;
        }

        if (shouldSearch && Input.GetKeyDown(KeyCode.Return))
        {
            Invoke("Search", 0.02f);
            shouldSearch = false;
        }
    }
    public void Search()
    {
        if (searched_Text != lastText)
        {
            lastText = searched_Text;
            searchedObjects.Clear();
            objQueue = 0;
            Stack<Transform> stack = new Stack<Transform>();
            stack.Push(first_Parent);

            while (stack.Count > 0)
            {
                var current = stack.Pop();

                if (current.name.IndexOf(searched_Text, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    searchedObjects.Add(current);
                }

                // İlk olarak child'ları stack'e tersten ekleyerek önce ilk child'ın işlenmesini sağlıyoruz.
                for (int i = current.childCount - 1; i >= 0; i--)
                {
                    stack.Push(current.GetChild(i));
                }
            }
            if (searchedObjects.Count != 0)
            {
                mouseClick.currentObject = searchedObjects[objQueue].gameObject;
                mouseClick.Search();
            }
            else StartCoroutine(Notification());
        }
        else
        {
            objQueue++;
            if (searchedObjects.Count > objQueue)
            {
                mouseClick.currentObject = searchedObjects[objQueue].gameObject;
                mouseClick.Search();
            }
            else StartCoroutine(Notification());
        }
    }

    IEnumerator Notification()
    {
        loadingPanel.gameObject.SetActive(true);


        yield return new WaitForSeconds(2f);

        loadingPanel.gameObject.SetActive(false);
    }

}
