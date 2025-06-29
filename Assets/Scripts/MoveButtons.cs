using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MoveButtons : MonoBehaviour
{
    [SerializeField] VisualQueryManager visualQueryManager;
    [SerializeField] ClippingController clippingController;
    [SerializeField] Measure measureScrpt;
    [SerializeField] MouseClick mouseClick;
    [SerializeField] GameObject clipButton, moveButton, scaleButton;
    [SerializeField] Image panT, orbitT, lookAroundT, selectT, measureT, sectionT, avatarT;
    GameObject sectionObject;
    public bool pan, orbit, lookAround, select, measure, section, avatar,visualQuery, buttonControl;

    void Update()
    {
        if(pan) panT.color = new Color(0, 0, 0, 0.39f);
        else panT.color = new Color(255, 255, 255, 0.39f);

        if(orbit) orbitT.color = new Color(0, 0, 0, 0.39f);
        else orbitT.color = new Color(255, 255, 255, 0.39f);

        if(lookAround) lookAroundT.color = new Color(0, 0, 0, 0.39f);
        else lookAroundT.color = new Color(255, 255, 255, 0.39f);

        if(select) selectT.color = new Color(0, 0, 0, 0.39f);
        else selectT.color = new Color(255, 255, 255, 0.39f);

        if(measure) measureT.color = new Color(0, 0, 0, 0.39f);
        else measureT.color = new Color(255, 255, 255, 0.39f);

        if(section) sectionT.color = new Color(0, 0, 0, 0.39f);
        else sectionT.color = new Color(255, 255, 255, 0.39f);

        if(avatar) avatarT.color = new Color(0, 0, 0, 0.39f);
        else avatarT.color = new Color(255, 255, 255, 0.39f);
        
    }
    public void Pan()
    {
        if(pan) pan = false;
        else pan = true;
        measureScrpt.text.text = "";
        measureScrpt.text.gameObject.SetActive(false);
        measureScrpt.lineRenderer.SetPosition(0, new Vector3(0,0,0));
        measureScrpt.lineRenderer.SetPosition(1, new Vector3(0,0,0));
        measureScrpt.lineRenderer.gameObject.layer = 0;
        orbit = false;
        lookAround = false;
        select = false;
        measure = false;
        avatar = false;
    }

    public void Orbit()
    {
        if(orbit) orbit = false;
        else orbit = true;
        measureScrpt.text.text = "";
        measureScrpt.text.gameObject.SetActive(false);
        measureScrpt.lineRenderer.SetPosition(0, new Vector3(0,0,0));
        measureScrpt.lineRenderer.SetPosition(1, new Vector3(0,0,0));
        measureScrpt.lineRenderer.gameObject.layer = 0;
        pan = false;
        lookAround = false;
        select = false;
        measure = false;
        avatar = false;
    }

    public void LookAround()
    {
        if(lookAround) lookAround = false;
        else lookAround = true;
        measureScrpt.text.text = "";
        measureScrpt.text.gameObject.SetActive(false);
        measureScrpt.lineRenderer.SetPosition(0, new Vector3(0,0,0));
        measureScrpt.lineRenderer.SetPosition(1, new Vector3(0,0,0));
        measureScrpt.lineRenderer.gameObject.layer = 0;
        orbit = false;
        pan = false;
        select = false;
        measure = false;
        avatar = false;
    }

    public void Select()
    {
        if(select) select = false;
        else select = true;
        measureScrpt.text.text = "";
        measureScrpt.text.gameObject.SetActive(false);
        measureScrpt.lineRenderer.SetPosition(0, new Vector3(0,0,0));
        measureScrpt.lineRenderer.SetPosition(1, new Vector3(0,0,0));
        measureScrpt.lineRenderer.gameObject.layer = 0;
        lookAround = false;
        orbit = false;
        pan = false;
        measure = false;
        avatar = false;
    }

    public void ActivateButtons()
    {
        if(buttonControl)
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

    public void Section()
    {
        measureScrpt.text.text = "";
        measureScrpt.text.gameObject.SetActive(false);
        measureScrpt.lineRenderer.SetPosition(0, new Vector3(0,0,0));
        measureScrpt.lineRenderer.SetPosition(1, new Vector3(0,0,0));
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
        if(measure) measure = false;
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
        if(avatar)
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
        measureScrpt.lineRenderer.SetPosition(0, new Vector3(0,0,0));
        measureScrpt.lineRenderer.SetPosition(1, new Vector3(0,0,0));
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
        if(visualQuery)
        {
            if(visualQueryManager.mainPanel.activeInHierarchy)
            {
                visualQueryManager.mainPanel.SetActive(false);
            }
            else if(visualQueryManager.groupPanel.activeInHierarchy)
            {
                visualQueryManager.groupPanel.SetActive(false);
                visualQueryManager.DestroyGroupChildren();
                visualQueryManager.ResetColorsWithClipping();
            }
            else if(visualQueryManager.itemPanel.activeInHierarchy)
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
            if(visualQueryManager.mainPanel.activeInHierarchy)
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
}
