using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MoveButtons : MonoBehaviour
{
    [SerializeField] VisualQueryManager visualQueryManager;
    ClippingController clippingController;
    Measure measureScrpt;
    MouseClick mouseClick;
    [SerializeField] GameObject clipButton, moveButton, scaleButton, tutorialPanel, optionsPanel;
    GameObject sectionObject;
    public bool pan, orbit, lookAround, select, measure, section, avatar, visualQuery, buttonControl;

    void Start()
    {
        measureScrpt = Camera.main.GetComponentInChildren<Measure>();
        mouseClick = Camera.main.GetComponentInChildren<MouseClick>();
        clippingController = GameObject.Find("ScriptsObject").GetComponentInChildren<ClippingController>();
    }

    public void ActivateButtons()
    {
        if (buttonControl)
        {
            clipButton.SetActive(false);
            moveButton.SetActive(false);
            scaleButton.SetActive(false);
            buttonControl = false;
        }
        else
        {
            clipButton.SetActive(true);
            moveButton.SetActive(true);
            scaleButton.SetActive(true);
            buttonControl = true;
        }
    }
    public void Pan()
    {
        if (pan) pan = false;
        else pan = true;
        measureScrpt.text.text = "";
        measureScrpt.text.gameObject.SetActive(false);
        measureScrpt.lineRenderer.SetPosition(0, new Vector3(0, 0, 0));
        measureScrpt.lineRenderer.SetPosition(1, new Vector3(0, 0, 0));
        measureScrpt.lineRenderer.gameObject.layer = 0;
        orbit = false;
        lookAround = false;
        select = false;
        measure = false;
        avatar = false;
    }

    public void Orbit()
    {
        if (orbit) orbit = false;
        else orbit = true;
        measureScrpt.text.text = "";
        measureScrpt.text.gameObject.SetActive(false);
        measureScrpt.lineRenderer.SetPosition(0, new Vector3(0, 0, 0));
        measureScrpt.lineRenderer.SetPosition(1, new Vector3(0, 0, 0));
        measureScrpt.lineRenderer.gameObject.layer = 0;
        pan = false;
        lookAround = false;
        select = false;
        measure = false;
        avatar = false;
    }

    public void LookAround()
    {
        if (lookAround) lookAround = false;
        else lookAround = true;
        measureScrpt.text.text = "";
        measureScrpt.text.gameObject.SetActive(false);
        measureScrpt.lineRenderer.SetPosition(0, new Vector3(0, 0, 0));
        measureScrpt.lineRenderer.SetPosition(1, new Vector3(0, 0, 0));
        measureScrpt.lineRenderer.gameObject.layer = 0;
        orbit = false;
        pan = false;
        select = false;
        measure = false;
        avatar = false;
    }

    public void Select()
    {
        if (select) select = false;
        else select = true;
        measureScrpt.text.text = "";
        measureScrpt.text.gameObject.SetActive(false);
        measureScrpt.lineRenderer.SetPosition(0, new Vector3(0, 0, 0));
        measureScrpt.lineRenderer.SetPosition(1, new Vector3(0, 0, 0));
        measureScrpt.lineRenderer.gameObject.layer = 0;
        lookAround = false;
        orbit = false;
        pan = false;
        measure = false;
        avatar = false;
    }

    public void Section()
    {
        measureScrpt.text.text = "";
        measureScrpt.text.gameObject.SetActive(false);
        measureScrpt.lineRenderer.SetPosition(0, new Vector3(0, 0, 0));
        measureScrpt.lineRenderer.SetPosition(1, new Vector3(0, 0, 0));
        measureScrpt.lineRenderer.gameObject.layer = 0;

        if (section && sectionObject == mouseClick.currentObject)
        {
            clippingController.isScale = false;
            clippingController.isMove = false;
            Shader.SetGlobalVector("_Bound", new Vector4(1000000, 1000000, 1000000, 1));
            clippingController.ChangeDisabled();
            section = false;
        }
        else
        {
            section = true;
            clippingController.isMove = true;
            clippingController.Sectioning();
        }

        sectionObject = mouseClick.currentObject;
        select = false;
        measure = false;
        avatar = false;

    }

    public void MeasureBetween()
    {
        if (measure) measure = false;
        else measure = true;
        mouseClick.ChangeColorBack();
        pan = false;
        orbit = false;
        lookAround = false;
        select = false;
        avatar = false;
    }

    public void Avatar()
    {
        if (avatar)
        {
            avatar = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            avatar = true;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        measureScrpt.text.text = "";
        measureScrpt.text.gameObject.SetActive(false);
        measureScrpt.lineRenderer.SetPosition(0, new Vector3(0, 0, 0));
        measureScrpt.lineRenderer.SetPosition(1, new Vector3(0, 0, 0));
        measureScrpt.lineRenderer.gameObject.layer = 0;
        select = false;
        lookAround = false;
        orbit = false;
        pan = false;
        measure = false;
        section = false;
    }

    public void VisualQuery()
    {
        if (visualQuery)
        {
            if (visualQueryManager.mainPanel.activeInHierarchy)
            {
                visualQueryManager.mainPanel.SetActive(false);
            }
            else if (visualQueryManager.groupPanel.activeInHierarchy)
            {
                visualQueryManager.groupPanel.SetActive(false);
                visualQueryManager.DestroyGroupChildren();
                visualQueryManager.ResetColorsWithClipping();
            }
            else if (visualQueryManager.itemPanel.activeInHierarchy)
            {
                visualQueryManager.itemPanel.SetActive(false);
                visualQueryManager.DestroyGroupChildren();
                visualQueryManager.DestroyItemChildren();
                visualQueryManager.ResetColorsWithClipping();
            }
            visualQuery = false;
        }
        else
        {
            if (visualQueryManager.mainPanel.activeInHierarchy)
            {
                visualQueryManager.mainPanel.SetActive(false);
            }
            else visualQueryManager.mainPanel.SetActive(true);
        }
    }

    public void VisualBoolFalse()
    {
        visualQuery = false;
    }

    public void TutorialPanelControl()
    {
        if (tutorialPanel.activeSelf)
            tutorialPanel.SetActive(false);
        else
            tutorialPanel.SetActive(true);
    }

    public void OptionsPanelControl()
    {
        if (optionsPanel.activeSelf)
            optionsPanel.SetActive(false);
        else
            optionsPanel.SetActive(true);
    }
}
